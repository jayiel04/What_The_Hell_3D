using UnityEngine;

public sealed class MovingPlatformRuntime : MonoBehaviour
{
    private Vector3 start;
    private Vector3 travel;
    private float duration;

    public void Configure(Vector3 movement, float seconds)
    {
        start = transform.position;
        travel = movement;
        duration = Mathf.Max(0.1f, seconds);
    }

    private void Update()
    {
        float progress = Mathf.PingPong(Time.time / duration, 1f);
        transform.position = start + travel * progress;
    }
}

public sealed class FallingPlatformRuntime : MonoBehaviour
{
    private Vector3 start;
    private bool activated;
    private float activatedAt;

    public void Configure()
    {
        start = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activated = true;
            activatedAt = Time.time;
        }
    }

    private void Update()
    {
        if (!activated)
        {
            return;
        }

        float elapsed = Time.time - activatedAt;
        if (elapsed < 0.45f)
        {
            transform.position = start + Vector3.down * (elapsed * elapsed * 8f);
        }
        else if (elapsed > 3.5f)
        {
            transform.position = start;
            activated = false;
        }
    }
}

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

public sealed class PickupRuntime : MonoBehaviour, IInteractable, ICampaignCollectible
{
    public CampaignPickupKind kind;
    public int heartValue = 35;

    public CampaignPickupKind Kind => kind;
    public bool IsCollected { get; private set; }

    public void Configure(CampaignPickupKind pickupKind)
    {
        kind = pickupKind;
    }

    private void Update()
    {
        if (!IsCollected)
        {
            transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Interact(other.gameObject);
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return !IsCollected && interactor != null && interactor.CompareTag("Player");
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        IsCollected = true;
        CampaignRuntimeState.Instance?.Collect(kind);
        if (kind == CampaignPickupKind.Heart)
        {
            HealthComponent health = interactor.GetComponentInParent<HealthComponent>();
            health?.Heal(heartValue);
        }

        gameObject.SetActive(false);
    }
}

public sealed class CheckpointRuntime : MonoBehaviour, ICheckpoint
{
    public int index;
    public Vector3 respawnPosition;
    private bool activated;

    public int Index => index;
    public Vector3 RespawnPosition => respawnPosition;

    public void Configure(int checkpointIndex, Vector3 position)
    {
        index = checkpointIndex;
        respawnPosition = position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.CompareTag("Player"))
        {
            return;
        }

        activated = CampaignRuntimeState.Instance != null && CampaignRuntimeState.Instance.SetCheckpoint(index, respawnPosition);
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (activated && renderer != null)
        {
            renderer.material.color = Color.green;
        }
    }
}

public sealed class GoalRuntime : MonoBehaviour, ICampaignGoal, IInteractable
{
    public bool CanFinish => CampaignRuntimeState.Instance != null && CampaignRuntimeState.Instance.CanFinishLevel();
    public int levelId;

    public void Configure(int campaignLevelId)
    {
        levelId = campaignLevelId;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Finish();
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return interactor != null && interactor.CompareTag("Player");
    }

    public void Interact(GameObject interactor)
    {
        Finish();
    }

    public void Finish()
    {
        if (CanFinish)
        {
            CampaignRuntimeState.Instance.FinishLevel(levelId);
        }
    }
}

public sealed class BonusCacheRuntime : MonoBehaviour, IInteractable
{
    private bool opened;

    public void Configure()
    {
        opened = false;
    }

    private void Update()
    {
        if (!opened)
        {
            transform.Rotate(Vector3.up, 28f * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Interact(other.gameObject);
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return !opened && interactor != null && interactor.CompareTag("Player");
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        opened = true;
        CampaignRuntimeState.Instance?.Collect(CampaignPickupKind.Coin);
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.95f, 0.75f, 0.15f);
        }
    }
}
