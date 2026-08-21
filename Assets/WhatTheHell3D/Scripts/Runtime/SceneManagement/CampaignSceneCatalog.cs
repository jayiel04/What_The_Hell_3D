using UnityEngine;

[CreateAssetMenu(fileName = "CampaignSceneCatalog", menuName = "WhatTheHell3D/Campaign Scene Catalog")]
public sealed class CampaignSceneCatalog : ScriptableObject
{
    public string mainMenuScene = "Assets/WhatTheHell3D/Scenes/MainMenu.unity";
    public string introScene = "Assets/WhatTheHell3D/Scenes/Intro.unity";
    public string[] campaignLevelScenes =
    {
        "Assets/WhatTheHell3D/Scenes/CampaignLevel01.unity",
        "Assets/WhatTheHell3D/Scenes/CampaignLevel02.unity",
        "Assets/WhatTheHell3D/Scenes/CampaignLevel03.unity"
    };
    public string victoryScene = "Assets/WhatTheHell3D/Scenes/Victory.unity";

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
