using UnityEngine;

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
