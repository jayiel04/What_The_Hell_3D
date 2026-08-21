using UnityEngine;

public sealed class CampaignHudController : MonoBehaviour
{
    private CampaignLevelConfig config;
    private PlayerController player;
    private GUIStyle labelStyle;
    private GUIStyle objectiveStyle;

    public void Configure(CampaignLevelConfig level, PlayerController playerController)
    {
        config = level;
        player = playerController;
    }

    private void OnGUI()
    {
        if (config == null || player == null)
        {
            return;
        }

        EnsureStyles();
        CampaignProgressData progress = CampaignRuntimeState.Instance == null ? null : CampaignRuntimeState.Instance.Progress;
        int currentHealth = player.Health == null ? 0 : player.Health.CurrentHealth;
        float healthWidth = 220f * (currentHealth / 100f);

        GUI.Label(new Rect(24f, 18f, 320f, 30f), config.title, objectiveStyle);
        GUI.Label(new Rect(24f, 51f, 440f, 30f), config.objective, labelStyle);
        GUI.Label(new Rect(24f, 86f, 180f, 24f), "SALUD " + currentHealth + "/100", labelStyle);
        Color previousColor = GUI.color;
        GUI.color = new Color(0.65f, 0.08f, 0.08f);
        GUI.DrawTexture(new Rect(24f, 112f, 220f, 14f), Texture2D.whiteTexture);
        GUI.color = new Color(0.15f, 0.8f, 0.25f);
        GUI.DrawTexture(new Rect(24f, 112f, Mathf.Clamp(healthWidth, 0f, 220f), 14f), Texture2D.whiteTexture);
        GUI.color = previousColor;

        int collected = progress == null ? 0 : progress.collected;
        int total = progress == null ? 0 : progress.totalCollectibles;
        bool key = progress != null && progress.keyCollected;
        int checkpoint = progress == null ? 0 : progress.checkpointIndex;
        GUI.Label(new Rect(Screen.width - 300f, 20f, 270f, 28f), "Monedas: " + collected + "/" + total, labelStyle);
        GUI.Label(new Rect(Screen.width - 300f, 50f, 270f, 28f), "Llave: " + (key ? "obtenida" : "pendiente"), labelStyle);
        GUI.Label(new Rect(Screen.width - 300f, 80f, 270f, 28f), "Checkpoint: " + checkpoint, labelStyle);
        GUI.Label(new Rect(Screen.width - 300f, Screen.height - 36f, 270f, 24f), "Esc pausa · E interactuar", labelStyle);
    }

    private void EnsureStyles()
    {
        if (labelStyle != null)
        {
            return;
        }

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            normal = { textColor = Color.white }
        };
        objectiveStyle = new GUIStyle(labelStyle)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.82f, 0.3f) }
        };
    }
}
