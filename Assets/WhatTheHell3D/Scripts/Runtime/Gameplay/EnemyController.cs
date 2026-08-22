using System.Collections;
using System.IO;
using System.Threading;
using GLTFast;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HealthComponent))]
public sealed class EnemyController : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        WindUp,
        Strike,
        Recover,
        Stunned
    }

    public CampaignEnemyKind kind;
    public float patrolDistance = 4f;
    public float moveSpeed = 2.8f;
    public float chaseSpeed = 4f;
    public float detectionRange = 14f;
    public float attackRange = 1.9f;
    public float attackCooldown = 1.15f;
    public int attackDamage = 12;
    public float leashDistance = 18f;

    [Header("Attack states")]
    public float windUpTime = 0.45f;
    public float recoverTime = 0.55f;
    public float hitStunDuration = 0.28f;
    public float strikeRadius = 2.1f;

    [Header("Knockback")]
    public float knockbackForce = 5f;

    [Header("Witch projectile")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip attackClip;
    public AudioClip hurtClip;
    private Color baseColor = Color.white;

    [Header("Visual Model")]
    [Tooltip("Ruta relativa a StreamingAssets; si está vacía se resuelve por kind.")]
    public string characterModelPath = string.Empty;
    public bool loadCharacterModel = true;
    public float modelScale = 1f;
    public float modelYawOffsetDegrees = 0f;
    public string[] loopClips = { "Idle", "Walk", "Run" };

    public HealthComponent Health { get; private set; }

    private CharacterController controller;
    private Transform target;
    private Vector3 homePosition;
    private Vector3 velocity;
    private Vector3 knockbackVelocity;
    private float nextAttackTime;
    private float stateTimer;
    private bool dying;
    private EnemyState state = EnemyState.Patrol;
    private Renderer[] renderers;
    private bool modelStarted;
    private bool modelLoaded;
    private Transform modelRoot;
    private Animation animator;
    private string currentClip;
    private float actionClipEnd;

    public void Configure(CampaignEnemyKind enemyKind, float patrol, Transform player)
    {
        kind = enemyKind;
        patrolDistance = Mathf.Max(0.5f, patrol);
        target = player;
        Initialize();
        ApplyDefaults();
        if (loadCharacterModel && !modelStarted)
        {
            modelStarted = true;
            StartCoroutine(LoadCharacterModel());
        }
    }

    private void Awake()
    {
        Initialize();
        ApplyDefaults();
    }

    private void Start()
    {
        if (loadCharacterModel && !modelStarted && !string.IsNullOrEmpty(GetModelPath()))
        {
            modelStarted = true;
            StartCoroutine(LoadCharacterModel());
        }
    }

    private void Initialize()
    {
        controller = GetComponent<CharacterController>();
        Health = GetComponent<HealthComponent>();
        homePosition = transform.position;
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0 && renderers[0].sharedMaterial != null)
        {
            baseColor = renderers[0].sharedMaterial.color;
        }

        Health.Died -= OnDied;
        Health.Died += OnDied;
        Health.Damaged -= OnDamaged;
        Health.Damaged += OnDamaged;
    }

    private void Update()
    {
        if (dying || Health == null || !Health.IsAlive)
        {
            return;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            target = player == null ? null : player.transform;
        }

        switch (state)
        {
            case EnemyState.WindUp:
                stateTimer -= Time.deltaTime;
                FaceTarget();
                if (stateTimer <= 0f)
                {
                    Strike();
                }

                break;
            case EnemyState.Strike:
                state = EnemyState.Recover;
                stateTimer = recoverTime;
                break;
            case EnemyState.Recover:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    SetTint(Color.white);
                    state = EnemyState.Patrol;
                    nextAttackTime = Time.time + attackCooldown * 0.35f;
                }

                break;
            case EnemyState.Stunned:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    SetTint(Color.white);
                    state = EnemyState.Patrol;
                }

                break;
            default:
                UpdateCombat();
                break;
        }

        UpdateAnimation();
    }

    private void UpdateCombat()
    {
        Vector3 destination = PatrolDestination();
        float speed = moveSpeed;
        bool chasing = false;
        if (target != null)
        {
            float targetDistance = Vector3.Distance(transform.position, target.position);
            float homeDistance = Vector3.Distance(transform.position, homePosition);
            if (targetDistance <= detectionRange && homeDistance <= leashDistance)
            {
                destination = target.position;
                speed = chaseSpeed;
                chasing = true;
                if (targetDistance <= attackRange && Time.time >= nextAttackTime)
                {
                    BeginWindUp();
                    return;
                }
            }
        }

        MoveTowards(destination, speed, chasing);
    }

    private void BeginWindUp()
    {
        state = EnemyState.WindUp;
        stateTimer = windUpTime;
        SetTint(new Color(1f, 0.45f, 0.25f));
    }

    private void Strike()
    {
        nextAttackTime = Time.time + attackCooldown;
        PlayFeedback(attackClip);
        if (kind == CampaignEnemyKind.Witch && target != null)
        {
            SpawnProjectile();
        }
        else if (target != null)
        {
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude <= strikeRadius
                && Vector3.Dot(toTarget.normalized, transform.forward) > 0.35f)
            {
                IDamageable damageable = target.GetComponent<HealthComponent>();
                damageable?.TakeDamage(new DamageInfo(attackDamage, transform.position, gameObject));
            }
        }

        state = EnemyState.Recover;
        stateTimer = recoverTime;
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab == null)
        {
            projectilePrefab = new GameObject("WitchProjectile", typeof(SphereCollider), typeof(Rigidbody), typeof(WitchProjectileRuntime));
            SphereCollider collider = projectilePrefab.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.28f;
            Rigidbody body = projectilePrefab.GetComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(mesh.GetComponent<Collider>());
            mesh.transform.SetParent(projectilePrefab.transform, false);
            mesh.transform.localScale = Vector3.one * 0.56f;
            MeshRenderer renderer = mesh.GetComponent<MeshRenderer>();
            renderer.material.color = new Color(0.65f, 0.2f, 0.9f);
            projectilePrefab.tag = "Projectile";
            projectilePrefab.hideFlags = HideFlags.None;
        }

        Vector3 spawnPosition = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position + transform.forward * 0.8f + Vector3.up * 1.1f;
        Quaternion facing = Quaternion.LookRotation((target.position - spawnPosition).normalized);
        GameObject instance = Instantiate(projectilePrefab, spawnPosition, facing);
        instance.tag = "Projectile";
        WitchProjectileRuntime projectile = instance.GetComponent<WitchProjectileRuntime>();
        if (projectile == null)
        {
            projectile = instance.AddComponent<WitchProjectileRuntime>();
        }

        projectile.damage = attackDamage;
        projectile.Launch(target.position);
    }

    private void OnDamaged(DamageInfo damage)
    {
        if (dying || !Health.IsAlive)
        {
            return;
        }

        PlayFeedback(hurtClip);
        SetTint(new Color(1f, 0.85f, 0.3f));
        state = EnemyState.Stunned;
        stateTimer = hitStunDuration;
        Vector3 direction = transform.position - damage.sourcePosition;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            knockbackVelocity += direction.normalized * knockbackForce;
        }
    }

    private void ApplyDefaults()
    {
        switch (kind)
        {
            case CampaignEnemyKind.Zombie:
                Health.maxHealth = 150;
                moveSpeed = 2.2f;
                chaseSpeed = 3.2f;
                attackDamage = 18;
                detectionRange = 11f;
                break;
            case CampaignEnemyKind.Witch:
                Health.maxHealth = 80;
                moveSpeed = 1.8f;
                chaseSpeed = 2.6f;
                attackDamage = 22;
                detectionRange = 16f;
                break;
            default:
                Health.maxHealth = 100;
                moveSpeed = 2.8f;
                chaseSpeed = 4f;
                attackDamage = 12;
                detectionRange = 14f;
                break;
        }

        Health.Revive();
    }

    private Vector3 PatrolDestination()
    {
        float phase = Mathf.PingPong(Time.time * moveSpeed * 0.25f, patrolDistance * 2f) - patrolDistance;
        return homePosition + transform.right * phase;
    }

    private void FaceTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
        }
    }

    private void MoveTowards(Vector3 destination, float speed, bool chasing)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.05f)
        {
            direction.Normalize();
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 8f * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += -22f * Time.deltaTime;
        controller.Move(((chasing ? direction * speed : direction * speed * 0.5f) + knockbackVelocity + velocity) * Time.deltaTime);
        knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, 14f * Time.deltaTime);
    }

    private void PlayFeedback(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SetTint(Color color)
    {
        if (renderers == null)
        {
            return;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
    }

    private void OnDied(HealthComponent _)
    {
        dying = true;
        if (animator != null && animator.GetClip("Death"))
        {
            animator.CrossFade("Death", 0.12f);
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            renderer.material.color = Color.gray;
        }

        Destroy(gameObject, 1.2f);
    }

    private string GetModelPath()
    {
        if (!string.IsNullOrEmpty(characterModelPath))
        {
            return characterModelPath;
        }

        switch (kind)
        {
            case CampaignEnemyKind.Goblin:
                // Alterna variante por posición para mostrar variedad sin cambiar el enum.
                int g = Mathf.FloorToInt(Mathf.Abs(transform.position.x * 7.3f) + Mathf.Abs(transform.position.z * 3.7f));
                return (g % 2 == 0)
                    ? "Characters/Enemies/Goblin_Male.gltf"
                    : "Characters/Enemies/Goblin_Female.gltf";
            case CampaignEnemyKind.Zombie:
                int z = Mathf.FloorToInt(Mathf.Abs(transform.position.x * 5.1f) + Mathf.Abs(transform.position.z * 4.2f));
                return (z % 2 == 0)
                    ? "Characters/Enemies/Zombie_Male.gltf"
                    : "Characters/Enemies/Zombie_Female.gltf";
            case CampaignEnemyKind.Witch:
                return "Characters/Enemies/Witch.gltf";
            default:
                return "Characters/Enemies/Goblin_Male.gltf";
        }
    }

    private IEnumerator LoadCharacterModel()
    {
        string rel = GetModelPath();
        if (string.IsNullOrEmpty(rel))
        {
            yield break;
        }

        string localPath = Path.Combine(Application.streamingAssetsPath, rel);
        if (!File.Exists(localPath))
        {
            Debug.LogWarning($"[Enemy:{kind}] Modelo no encontrado en {localPath}");
            yield break;
        }

        var importer = new GltfImport();
        var loadTask = importer.LoadFile(localPath, null, null, CancellationToken.None);
        while (!loadTask.IsCompleted) yield return null;
        if (!loadTask.Result)
        {
            Debug.LogError($"[Enemy:{kind}] Falló carga de {localPath}");
            yield break;
        }

        modelRoot = new GameObject($"{kind}Model").transform;
        modelRoot.SetParent(transform, false);
        modelRoot.localPosition = Vector3.zero;
        modelRoot.localRotation = Quaternion.Euler(0f, modelYawOffsetDegrees, 0f);
        modelRoot.localScale = Vector3.one * modelScale;

        var instTask = importer.InstantiateMainSceneAsync(modelRoot, CancellationToken.None);
        while (!instTask.IsCompleted) yield return null;

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
        renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0 && renderers[0].sharedMaterial != null)
        {
            baseColor = renderers[0].sharedMaterial.color;
        }

        Transform placeholder = transform.Find("EnemyVisual");
        if (placeholder != null)
        {
            var mr = placeholder.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null || !modelLoaded || dying) return;

        string desired;
        switch (state)
        {
            case EnemyState.Stunned:
                desired = "RecieveHit";
                break;
            case EnemyState.WindUp:
                desired = kind == CampaignEnemyKind.Witch ? "Shoot_OneHanded" : "Punch";
                break;
            case EnemyState.Strike:
                desired = kind == CampaignEnemyKind.Witch ? "Shoot_OneHanded" : "SwordSlash";
                break;
            case EnemyState.Recover:
                desired = "Idle";
                break;
            default:
                if (target != null)
                {
                    float dist = Vector3.Distance(transform.position, target.position);
                    float homeDist = Vector3.Distance(transform.position, homePosition);
                    bool chasing = dist <= detectionRange && homeDist <= leashDistance;
                    desired = chasing ? "Run" : "Walk";
                    if (!chasing && Vector3.Distance(transform.position, PatrolDestination()) < 0.3f)
                    {
                        desired = "Idle";
                    }
                }
                else
                {
                    desired = "Walk";
                }

                break;
        }

        bool desiredIsLoop = System.Array.IndexOf(loopClips, desired) >= 0;
        if (animator.GetClip(desired) == null)
        {
            desired = desiredIsLoop ? "Idle" : currentClip;
            if (desired == null || animator.GetClip(desired) == null) return;
            desiredIsLoop = System.Array.IndexOf(loopClips, desired) >= 0;
        }

        if (desired == currentClip) return;
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
}
