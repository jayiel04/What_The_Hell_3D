using System;
using UnityEngine;

[Serializable]
public sealed class CampaignProgressData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public bool hasKey;
    public bool leverActivated;
    public int seals;
    public int currentLevelId = 1;
    public string checkpointScene = string.Empty;
    public string currentLevelScene = string.Empty;
    public string nextLevelScene = string.Empty;
    public int collected;
    public int totalCollectibles;
    public bool keyCollected;
    public int checkpointIndex;
    public Vector3 checkpointPosition = Vector3.zero;
    public bool levelFinished;

    public static CampaignProgressData CreateNew(string firstLevelScene)
    {
        return new CampaignProgressData
        {
            version = CurrentVersion,
            currentLevelId = 1,
            checkpointScene = firstLevelScene,
            currentLevelScene = firstLevelScene,
            nextLevelScene = string.Empty,
            checkpointPosition = Vector3.zero
        };
    }
}
