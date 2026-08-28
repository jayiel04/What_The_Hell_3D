using UnityEngine;

[CreateAssetMenu(fileName = "CampaignSceneCatalog", menuName = "WhatTheHell3D/Campaign Scene Catalog")]
public sealed class CampaignSceneCatalog : ScriptableObject
{
    public string mainMenuScene = "Assets/Scenes/MainMenu.unity";
    public string introScene = "Assets/Scenes/Intro.unity";
    public string[] campaignLevelScenes =
    {
        "Assets/Scenes/CampaignLevel01.unity",
        "Assets/Scenes/CampaignLevel02.unity",
        "Assets/Scenes/CampaignLevel03.unity"
    };
    public string victoryScene = "Assets/Scenes/Victory.unity";

    public string GetCampaignLevelScene(int levelId)
    {
        int index = levelId - 1;
        if (campaignLevelScenes == null || index < 0 || index >= campaignLevelScenes.Length)
        {
            return string.Empty;
        }

        return campaignLevelScenes[index];
    }

    public string GetNextScene(int levelId)
    {
        string nextLevel = GetCampaignLevelScene(levelId + 1);
        return string.IsNullOrEmpty(nextLevel) ? victoryScene : nextLevel;
    }
}
