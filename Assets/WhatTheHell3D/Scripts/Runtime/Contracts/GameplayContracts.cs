using System;
using UnityEngine;

[Serializable]
public struct DamageInfo
{
    public int amount;
    public Vector3 sourcePosition;
    public GameObject source;

    public DamageInfo(int amount, Vector3 sourcePosition, GameObject source = null)
    {
        this.amount = amount;
        this.sourcePosition = sourcePosition;
        this.source = source;
    }
}

public interface IDamageable
{
    bool IsAlive { get; }
    void TakeDamage(DamageInfo damage);
}

public interface IInteractable
{
    bool CanInteract(GameObject interactor);
    void Interact(GameObject interactor);
}

public interface ICheckpoint
{
    int Index { get; }
    Vector3 RespawnPosition { get; }
}

public interface ICampaignCollectible
{
    CampaignPickupKind Kind { get; }
    bool IsCollected { get; }
}

public interface ICampaignGoal
{
    bool CanFinish { get; }
    void Finish();
}
