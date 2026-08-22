using UnityEngine;

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
