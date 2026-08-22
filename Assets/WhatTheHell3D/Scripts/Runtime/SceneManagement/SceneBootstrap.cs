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
                MenuSceneController menu = GetComponent<MenuSceneController>();
                if (menu == null)
                {
                    Debug.LogError("La escena de menú debe contener un MenuSceneController creado desde Unity Editor.");
                }
                else
                {
                    menu.Configure(sceneCatalog);
                }
                break;
            case RuntimeSceneRole.Intro:
                IntroSceneController intro = GetComponent<IntroSceneController>();
                if (intro == null)
                {
                    Debug.LogError("La escena de intro debe contener un IntroSceneController creado desde Unity Editor.");
                }
                else
                {
                    intro.Configure(sceneCatalog);
                }
                break;
            case RuntimeSceneRole.CampaignLevel:
                CampaignLevelRuntime level = GetComponent<CampaignLevelRuntime>();
                if (level == null)
                {
                    Debug.LogError("La escena de campaña debe contener un CampaignLevelRuntime creado desde Unity Editor.");
                }
                else
                {
                    level.Configure(levelConfig, inputActions);
                }
                break;
            case RuntimeSceneRole.Victory:
                VictorySceneController victory = GetComponent<VictorySceneController>();
                if (victory == null)
                {
                    Debug.LogError("La escena de victoria debe contener un VictorySceneController creado desde Unity Editor.");
                }
                else
                {
                    victory.Configure(sceneCatalog);
                }
                break;
        }
    }
}
