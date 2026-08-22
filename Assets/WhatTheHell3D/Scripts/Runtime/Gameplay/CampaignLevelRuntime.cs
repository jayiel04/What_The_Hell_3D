using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CampaignLevelRuntime : MonoBehaviour
{
    [Header("Scene-authored references")]
    public PlayerController player;
    public CameraController cameraController;
    public Light keyLight;
    public Light fillLight;
    public CampaignHudController hud;
    public PauseController pause;
    public CampaignAudioDirector audioDirector;

    private CampaignLevelConfig config;
    private InputActionAsset inputActions;

    public Transform PlayerTransform => player == null ? null : player.transform;

    public void Configure(CampaignLevelConfig level, InputActionAsset actions)
    {
        config = level;
        inputActions = actions;
    }

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("CampaignLevelRuntime necesita una CampaignLevelConfig.");
            return;
        }

        if (player == null || cameraController == null || hud == null || pause == null || audioDirector == null)
        {
            Debug.LogError("La escena de campaña debe tener jugador, cámara, HUD, pausa y audio configurados desde Unity Editor.");
            return;
        }

        CampaignRuntimeState.Ensure(null).BeginLevel(config);
        ConfigureEnvironment();
        cameraController.Configure(player.transform, inputActions, config);
        player.Configure(inputActions, cameraController.transform, config.playerStart);
        hud.Configure(config, player);
        pause.Configure(config, player);
        audioDirector.Configure(config);
    }

    private void ConfigureEnvironment()
    {
        RenderSettings.fog = config.fogDensity > 0f;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = config.fogDensity;
        RenderSettings.fogColor = config.backgroundColor;
        RenderSettings.ambientLight = config.ambientLightColor;

        if (keyLight != null)
        {
            keyLight.color = config.ambientLightColor;
            keyLight.intensity = config.keyLightEnergy;
        }

        if (fillLight != null)
        {
            fillLight.color = config.fillLightColor;
            fillLight.intensity = config.fillLightEnergy;
        }
    }
}
