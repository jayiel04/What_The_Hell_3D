using UnityEngine;

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
