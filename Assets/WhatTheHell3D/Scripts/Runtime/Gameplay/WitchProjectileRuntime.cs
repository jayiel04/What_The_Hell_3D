using UnityEngine;

public sealed class WitchProjectileRuntime : MonoBehaviour
{
    public int damage = 22;
    public float speed = 9f;
    public float lifetime = 4f;
    private Vector3 direction;
    private bool launched;
    private float spawnedAt;

    public void Launch(Vector3 targetPosition)
    {
        Vector3 delta = targetPosition + Vector3.up * 0.9f - transform.position;
        delta.y *= 0.35f;
        direction = delta.sqrMagnitude > 0.001f ? delta.normalized : transform.forward;
        launched = true;
        spawnedAt = Time.time;
    }

    private void Update()
    {
        if (!launched)
        {
            return;
        }

        transform.position += direction * (speed * Time.deltaTime);
        if (Time.time - spawnedAt >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IDamageable damageable = FindDamageable(other);
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(new DamageInfo(damage, transform.position, gameObject));
            }

            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger && !other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    private static IDamageable FindDamageable(Collider other)
    {
        MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
    }
}
