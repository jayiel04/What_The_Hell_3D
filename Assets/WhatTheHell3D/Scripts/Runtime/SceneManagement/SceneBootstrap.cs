using UnityEngine;
using UnityEngine.InputSystem;

public enum RuntimeSceneRole
{
    MainMenu,
    Intro,
    CampaignLevel,
    Victory
}

public sealed class SceneBootstrap : MonoBehaviour
{
    public RuntimeSceneRole role;
    public CampaignLevelConfig levelConfig;
    public CampaignSceneCatalog sceneCatalog;
    public InputActionAsset inputActions;

    private void Awake()
    {
        CampaignRuntimeState.Ensure(sceneCatalog);

        switch (role)
        {
            case RuntimeSceneRole.MainMenu:
                gameObject.AddComponent<MenuSceneController>().Configure(sceneCatalog);
                break;
            case RuntimeSceneRole.Intro:
                gameObject.AddComponent<IntroSceneController>().Configure(sceneCatalog);
                break;
            case RuntimeSceneRole.CampaignLevel:
                CampaignLevelRuntime level = gameObject.AddComponent<CampaignLevelRuntime>();
                level.Configure(levelConfig, inputActions);
                break;
            case RuntimeSceneRole.Victory:
                gameObject.AddComponent<VictorySceneController>().Configure(sceneCatalog);
                break;
        }
    }
}
