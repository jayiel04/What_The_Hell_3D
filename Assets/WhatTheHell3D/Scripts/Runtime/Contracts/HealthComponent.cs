using System;
using UnityEngine;

public sealed class HealthComponent : MonoBehaviour, IDamageable
{
    [Min(1)] public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public event Action<HealthComponent> HealthChanged;
    public event Action<HealthComponent> Died;

    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;
    public bool IsInvulnerable { get; set; }
    public float DamageMultiplier { get; set; } = 1f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        HealthChanged?.Invoke(this);
    }

    public void TakeDamage(DamageInfo damage)
    {
        if (!IsAlive || IsInvulnerable || damage.amount <= 0)
        {
            return;
        }

        int appliedDamage = Mathf.Max(1, Mathf.CeilToInt(damage.amount * Mathf.Max(0f, DamageMultiplier)));
        currentHealth = Mathf.Max(0, currentHealth - appliedDamage);
        HealthChanged?.Invoke(this);
        if (currentHealth == 0)
        {
            Died?.Invoke(this);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || !IsAlive)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        HealthChanged?.Invoke(this);
    }

    public void Revive(int health = -1)
    {
        currentHealth = health < 0 ? maxHealth : Mathf.Clamp(health, 1, maxHealth);
        HealthChanged?.Invoke(this);
    }
}
