using UnityEngine;

public sealed class HazardRuntime : MonoBehaviour
{
    public CampaignHazardKind kind;
    public int damage = 30;
    public float repeatDelay = 0.65f;
    private float nextDamageTime;

    public void Configure(CampaignHazardKind hazardKind)
    {
        kind = hazardKind;
        damage = kind == CampaignHazardKind.Lava ? 40 : 30;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        IDamageable damageable = FindDamageable(other);
        if (damageable != null && damageable.IsAlive)
        {
            damageable.TakeDamage(new DamageInfo(damage, transform.position, gameObject));
            nextDamageTime = Time.time + repeatDelay;
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
