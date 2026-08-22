using UnityEngine;

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
