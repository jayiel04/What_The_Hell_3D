using System.Collections;
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

    public bool IsGuarding { get; private set; }
    public HealthComponent Health { get; private set; }

    private CharacterController controller;
    private InputReader input;
    private Transform cameraTransform;
    private Vector3 velocity;
    private Vector3 planarVelocity;
    private Vector3 spawnPosition;
    private float nextAttackTime;
    private float stamina;
    private float lastGroundedTime = -100f;
    private float jumpBufferExpires;
    private int jumpCount;
    private bool isDodging;
    private bool respawning;

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
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Health = GetComponent<HealthComponent>();
        input = new InputReader();
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

        IsGuarding = input.GuardHeld && !isDodging;
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
        controller.Move((planarVelocity + velocity) * Time.deltaTime);
    }

    private void Attack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        Vector3 origin = transform.position + transform.forward * attackRange;
        Collider[] hits = Physics.OverlapSphere(origin, attackRadius, LayerMask.GetMask("Enemy"));
        foreach (Collider hit in hits)
        {
            IDamageable damageable = FindDamageable(hit);
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(new DamageInfo(attackDamage, transform.position, gameObject));
            }
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackRadius);
    }
}
