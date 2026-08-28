using System.Collections;
using System.IO;
using System.Threading;
using GLTFast;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HealthComponent))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float acceleration = 12f;
    public float gravity = -22f;
    public float jumpHeight = 1.8f;
    public int maximumJumps = 2;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.14f;

    [Header("Combat")]
    public int attackDamage = 35;
    public float attackRange = 2.3f;
    public float attackRadius = 0.85f;
    public float attackCooldown = 0.55f;
    public float dodgeDistance = 3.5f;
    public float dodgeDuration = 0.22f;
    public float maximumStamina = 100f;
    public float sprintStaminaPerSecond = 18f;
    public float dodgeStaminaCost = 26f;
    public float staminaRecoveryPerSecond = 28f;

    [Header("Combo")]
    public int maxComboSteps = 3;
    public float comboWindow = 0.9f;
    public float[] comboDamageMultipliers = { 1f, 1.25f, 1.7f };

    [Header("Parry")]
    public float parryWindow = 0.28f;
    public int parryDamage = 15;
    public float parryKnockback = 4.5f;

    [Header("Knockback")]
    public float knockbackResistance = 1f;

    [Header("Feedback")]
    public AudioSource combatAudioSource;
    public AudioClip attackClip;
    public AudioClip parryClip;
    public AudioClip hurtClip;
    public Transform swordSocket;
    public ParticleSystem attackVfx;
    public ParticleSystem parryVfx;

    [Header("Visual Model")]
    [Tooltip("Ruta relativa a StreamingAssets del glTF del caballero exportado desde Godot/Mixamo.")]
    public string characterModelPath = "Characters/Knight_Male.gltf";
    public bool loadCharacterModel = true;
    public float modelScale = 1f;
    public float modelYawOffsetDegrees = 0f;
    public string[] loopClips = { "Idle", "Walk", "Run" };

    public bool IsGuarding { get; private set; }
    public HealthComponent Health { get; private set; }
    public int ComboStep => comboStep;

    private CharacterController controller;
    private InputReader input;
    private Transform cameraTransform;
    private Vector3 velocity;
    private Vector3 planarVelocity;
    private Vector3 knockbackVelocity;
    private Vector3 spawnPosition;
    private float nextAttackTime;
    private float lastAttackTime;
    private float guardStartTime = -100f;
    private float stamina;
    private float lastGroundedTime = -100f;
    private float jumpBufferExpires;
    private int jumpCount;
    private int comboStep;
    private bool isDodging;
    private bool respawning;
    private bool modelStarted;
    private bool modelLoaded;
    private Transform modelRoot;
    private Animation animator;
    private string currentClip;
    private float actionClipEnd;
    private float attackAnimUntil;
    private float hurtAnimUntil;

    public void Configure(InputActionAsset inputActions, Transform cameraTarget, Vector3 initialSpawn)
    {
        controller = GetComponent<CharacterController>();
        Health = GetComponent<HealthComponent>();
        input = new InputReader();
        input.Configure(inputActions);
        cameraTransform = cameraTarget;
        spawnPosition = initialSpawn;
        stamina = maximumStamina;
        Health.Died -= OnDied;
        Health.Died += OnDied;
        Health.Damaged -= OnDamaged;
        Health.Damaged += OnDamaged;
        if (loadCharacterModel && !modelStarted)
        {
            modelStarted = true;
            StartCoroutine(LoadCharacterModel());
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Health = GetComponent<HealthComponent>();
        input = new InputReader();

        // Rigidbody cinemático necesario para que los triggers (recogidas, metas y
        // checkpoints) detecten al jugador vía OnTriggerEnter con el CharacterController.
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        // Asegura que el jugador tenga la espada (la crea desde Resources si la escena
        // no fue reconstruida con el builder). AttachSwordToHand la coloca en la mano.
        EnsureSwordChild();
    }

    private void EnsureSwordChild()
    {
        if (transform.Find("Sword") != null)
        {
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("Sword");
        if (prefab == null)
        {
            return;
        }

        GameObject sword = Instantiate(prefab, transform);
        sword.name = "Sword";
    }

    private void Update()
    {
        if (respawning)
        {
            return;
        }

        if (input == null)
        {
            input = new InputReader();
        }

        bool guardWasHeld = IsGuarding;
        IsGuarding = input.GuardHeld && !isDodging;
        if (IsGuarding && !guardWasHeld)
        {
            guardStartTime = Time.time;
        }

        Health.IsInvulnerable = isDodging;
        Health.DamageMultiplier = IsGuarding ? 0.25f : 1f;
        if (!isDodging)
        {
            Move();
            if (input.AttackPressed || input.AttackHeld)
            {
                Attack();
            }

            if (input.DodgePressed)
            {
                StartCoroutine(Dodge());
            }

            if (input.InteractPressed)
            {
                Interact();
            }
        }

        UpdateAnimation();
    }

    private void Move()
    {
        Vector2 inputVector = Vector2.ClampMagnitude(input.Move, 1f);
        Vector3 forward = cameraTransform == null ? Vector3.forward : cameraTransform.forward;
        Vector3 right = cameraTransform == null ? Vector3.right : cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        bool sprinting = input.SprintHeld && inputVector.sqrMagnitude > 0.01f && stamina > 0f;
        Vector3 desired = (forward * inputVector.y + right * inputVector.x) * (sprinting ? sprintSpeed : walkSpeed);
        if (sprinting)
        {
            stamina = Mathf.Max(0f, stamina - sprintStaminaPerSecond * Time.deltaTime);
        }
        else
        {
            stamina = Mathf.Min(maximumStamina, stamina + staminaRecoveryPerSecond * Time.deltaTime);
        }

        planarVelocity = Vector3.MoveTowards(planarVelocity, desired, acceleration * Time.deltaTime);
        if (desired.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desired, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12f * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
            jumpCount = 0;
            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }
        }

        if (input.JumpPressed)
        {
            jumpBufferExpires = Time.time + jumpBufferTime;
        }

        bool canUseCoyote = Time.time <= lastGroundedTime + coyoteTime;
        bool canJump = jumpCount < maximumJumps && (controller.isGrounded || canUseCoyote || jumpCount > 0);
        if (Time.time <= jumpBufferExpires && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCount++;
            jumpBufferExpires = 0f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move((planarVelocity + knockbackVelocity + velocity) * Time.deltaTime);
        knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, 18f * Time.deltaTime);
    }

    private void Attack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        if (Time.time - lastAttackTime <= comboWindow && comboStep < Mathf.Max(1, maxComboSteps))
        {
            comboStep++;
        }
        else
        {
            comboStep = 0;
        }

        lastAttackTime = Time.time;
        nextAttackTime = Time.time + attackCooldown;
        attackAnimUntil = Time.time + 0.5f;
        float multiplier = comboDamageMultipliers != null && comboStep < comboDamageMultipliers.Length
            ? comboDamageMultipliers[comboStep]
            : 1f;
        int damage = Mathf.Max(1, Mathf.RoundToInt(attackDamage * multiplier));
        PlayFeedback(attackClip, attackVfx);
        Vector3 origin = transform.position + transform.forward * attackRange;
        Collider[] hits = Physics.OverlapSphere(origin, attackRadius, LayerMask.GetMask("Enemy"));
        foreach (Collider hit in hits)
        {
            IDamageable damageable = FindDamageable(hit);
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(new DamageInfo(damage, transform.position, gameObject));
            }
        }
    }

    private void OnDamaged(DamageInfo damage)
    {
        bool parried = IsGuarding && Time.time - guardStartTime <= parryWindow
            && Vector3.Dot((damage.sourcePosition - transform.position).normalized, transform.forward) > 0.25f;
        if (parried)
        {
            PlayFeedback(parryClip, parryVfx);
            IDamageable attacker = FindDamageableFromSource(damage.source);
            attacker?.TakeDamage(new DamageInfo(parryDamage, transform.position, gameObject));
            return;
        }

        PlayFeedback(hurtClip, null);
        hurtAnimUntil = Time.time + 0.35f;
        Vector3 direction = transform.position - damage.sourcePosition;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            knockbackVelocity += direction.normalized * (6f / Mathf.Max(0.25f, knockbackResistance));
        }

        comboStep = 0;
    }

    private void PlayFeedback(AudioClip clip, ParticleSystem vfx)
    {
        if (clip != null && combatAudioSource != null)
        {
            combatAudioSource.PlayOneShot(clip);
        }

        if (vfx != null)
        {
            vfx.transform.position = transform.position + transform.forward * attackRange;
            vfx.Play();
        }
    }

    private IDamageable FindDamageableFromSource(GameObject source)
    {
        if (source == null)
        {
            return null;
        }

        MonoBehaviour[] behaviours = source.GetComponentsInParent<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDamageable damageable && !ReferenceEquals(damageable, Health))
            {
                return damageable;
            }
        }

        return null;
    }

    private IEnumerator Dodge()
    {
        if (stamina < dodgeStaminaCost)
        {
            yield break;
        }

        stamina -= dodgeStaminaCost;
        isDodging = true;
        Health.IsInvulnerable = true;
        Vector3 direction = planarVelocity.sqrMagnitude > 0.1f ? planarVelocity.normalized : transform.forward;
        float elapsed = 0f;
        while (elapsed < dodgeDuration)
        {
            controller.Move(direction * (dodgeDistance / dodgeDuration) * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDodging = false;
        Health.IsInvulnerable = false;
    }

    private void Interact()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.4f, 1.4f);
        foreach (Collider hit in hits)
        {
            IInteractable interactable = FindInteractable(hit);
            if (interactable != null && interactable.CanInteract(gameObject))
            {
                interactable.Interact(gameObject);
                return;
            }
        }
    }

    private static IDamageable FindDamageable(Collider hit)
    {
        MonoBehaviour[] behaviours = hit.GetComponentsInParent<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
    }

    private static IInteractable FindInteractable(Collider hit)
    {
        MonoBehaviour[] behaviours = hit.GetComponentsInParent<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }

    private void OnDied(HealthComponent _)
    {
        if (!respawning)
        {
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        respawning = true;
        planarVelocity = Vector3.zero;
        velocity = Vector3.zero;
        knockbackVelocity = Vector3.zero;
        comboStep = 0;
        yield return new WaitForSecondsRealtime(1.1f);

        Vector3 respawnPosition = CampaignRuntimeState.Instance == null
            ? spawnPosition
            : CampaignRuntimeState.Instance.GetRespawnPosition(spawnPosition);
        controller.enabled = false;
        transform.position = respawnPosition;
        controller.enabled = true;
        Health.Revive();
        Health.IsInvulnerable = false;
        Health.DamageMultiplier = 1f;
        stamina = maximumStamina;
        respawning = false;
    }

    public void SetSpawnPosition(Vector3 position)
    {
        spawnPosition = position;
    }

    private IEnumerator LoadCharacterModel()
    {
        string localPath = Path.Combine(Application.streamingAssetsPath, characterModelPath);
        if (!File.Exists(localPath))
        {
            Debug.LogWarning($"[Player] Modelo de personaje no encontrado en {localPath}; se usa marcador de posición.");
            CreatePlaceholder();
            yield break;
        }

        var importer = new GltfImport();
        var loadTask = importer.LoadFile(localPath, null, null, CancellationToken.None);
        while (!loadTask.IsCompleted)
        {
            yield return null;
        }

        if (!loadTask.Result)
        {
            Debug.LogError($"[Player] Falló la carga del modelo de personaje: {localPath}");
            CreatePlaceholder();
            yield break;
        }

        modelRoot = new GameObject("KnightModel").transform;
        modelRoot.SetParent(transform, false);
        modelRoot.localPosition = Vector3.zero;
        modelRoot.localRotation = Quaternion.Euler(0f, modelYawOffsetDegrees, 0f);
        modelRoot.localScale = Vector3.one * modelScale;

        var instantiateTask = importer.InstantiateMainSceneAsync(modelRoot, CancellationToken.None);
        while (!instantiateTask.IsCompleted)
        {
            yield return null;
        }

        animator = modelRoot.GetComponent<Animation>() ?? modelRoot.gameObject.AddComponent<Animation>();
        animator.playAutomatically = false;
        foreach (AnimationClip clip in importer.GetAnimationClips())
        {
            clip.legacy = true;
            bool loop = System.Array.IndexOf(loopClips, clip.name) >= 0;
            clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            if (animator.GetClip(clip.name) == null)
            {
                animator.AddClip(clip, clip.name);
            }
        }

        if (animator.GetClip("Idle"))
        {
            animator.Play("Idle");
            currentClip = "Idle";
        }

        modelLoaded = true;

        AttachSwordToHand(modelRoot);

        Transform placeholder = transform.Find("PlayerCapsule");
        if (placeholder != null)
        {
            MeshRenderer mr = placeholder.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = false;
            }
        }
    }

    private void AttachSwordToHand(Transform modelRoot)
    {
        Transform sword = transform.Find("Sword");
        if (sword == null)
        {
            return;
        }

        string[] candidates = { "LowerArm.R", "Hand.R", "RightHand", "mixamorig:RightHand", "RightArm" };
        Transform handBone = null;
        foreach (string candidate in candidates)
        {
            handBone = FindBone(modelRoot, candidate);
            if (handBone != null)
            {
                break;
            }
        }

        if (handBone != null)
        {
            sword.SetParent(handBone, false);
            sword.localPosition = new Vector3(0f, 0f, 0f);
            sword.localRotation = Quaternion.identity;
            // Se conserva la escala natural del FBX (raíz ~100) para que la espada
            // mida ~2.3u; no sobreescribir localScale (la volvería invisible).
        }
        else
        {
            // Fallback: cerca de la mano derecha a la altura del pecho.
            sword.SetParent(transform, false);
            sword.localPosition = new Vector3(0.45f, 1.0f, 0.15f);
            sword.localRotation = Quaternion.identity;
        }
    }

    private static Transform FindBone(Transform root, string name)
    {
        foreach (Transform bone in root.GetComponentsInChildren<Transform>(true))
        {
            if (bone.name == name)
            {
                return bone;
            }
        }
        return null;
    }

    private void CreatePlaceholder()
    {
        if (transform.Find("PlayerCapsule") != null)
        {
            return;
        }

        GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        placeholder.name = "PlayerCapsule";
        placeholder.transform.SetParent(transform, false);
        placeholder.transform.localPosition = Vector3.up * 1f;
        placeholder.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        Object.Destroy(placeholder.GetComponent<Collider>());
    }

    private void UpdateAnimation()
    {
        if (animator == null || !modelLoaded)
        {
            return;
        }

        bool alive = Health.IsAlive;
        string desired;
        if (!alive)
        {
            desired = animator.GetClip("Death") ? "Death" : "Idle";
        }
        else if (isDodging)
        {
            desired = "Roll";
        }
        else if (Time.time < attackAnimUntil)
        {
            desired = "SwordSlash";
        }
        else if (Time.time < hurtAnimUntil)
        {
            desired = "RecieveHit";
        }
        else if (!controller.isGrounded)
        {
            desired = "Jump";
        }
        else
        {
            float speed = planarVelocity.magnitude;
            desired = speed > sprintSpeed * 0.55f ? "Run" : (speed > 0.15f ? "Walk" : "Idle");
        }

        bool desiredIsLoop = System.Array.IndexOf(loopClips, desired) >= 0;
        if (!animator.GetClip(desired))
        {
            desired = desiredIsLoop ? "Idle" : (currentClip ?? "Idle");
        }

        if (desired == currentClip)
        {
            return;
        }

        if (!desiredIsLoop && currentClip != null && System.Array.IndexOf(loopClips, currentClip) < 0 && Time.time < actionClipEnd)
        {
            return;
        }

        AnimationClip clip = animator.GetClip(desired);
        if (clip != null)
        {
            animator.CrossFade(desired, 0.12f);
            currentClip = desired;
            actionClipEnd = Time.time + clip.length;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackRadius);
    }
}
