using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HealthComponent))]
public sealed class EnemyController : MonoBehaviour
{
    public CampaignEnemyKind kind;
    public float patrolDistance = 4f;
    public float moveSpeed = 2.8f;
    public float chaseSpeed = 4f;
    public float detectionRange = 14f;
    public float attackRange = 1.9f;
    public float attackCooldown = 1.15f;
    public int attackDamage = 12;
    public float leashDistance = 18f;

    public HealthComponent Health { get; private set; }

    private CharacterController controller;
    private Transform target;
    private Vector3 homePosition;
    private Vector3 velocity;
    private float nextAttackTime;
    private bool dying;

    public void Configure(CampaignEnemyKind enemyKind, float patrol, Transform player)
    {
        kind = enemyKind;
        patrolDistance = Mathf.Max(0.5f, patrol);
        target = player;
        homePosition = transform.position;
        controller = GetComponent<CharacterController>();
        Health = GetComponent<HealthComponent>();
        ApplyDefaults();
        Health.Died -= OnDied;
        Health.Died += OnDied;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Health = GetComponent<HealthComponent>();
        homePosition = transform.position;
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
                if (targetDistance <= attackRange)
                {
                    TryAttack();
                    destination = transform.position;
                }
            }
        }

        MoveTowards(destination, speed, chasing);
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
        controller.Move(((chasing ? direction * speed : direction * speed * 0.5f) + velocity) * Time.deltaTime);
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime || target == null)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        IDamageable damageable = target.GetComponent<HealthComponent>();
        if (damageable != null && damageable.IsAlive)
        {
            damageable.TakeDamage(new DamageInfo(attackDamage, transform.position, gameObject));
        }
    }

    private void OnDied(HealthComponent _)
    {
        dying = true;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = Color.gray;
        }

        Destroy(gameObject, 1.2f);
    }
}
