using UnityEngine;

public sealed class MenuSceneController : MonoBehaviour
{
    private CampaignSceneCatalog catalog;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;

    public void Configure(CampaignSceneCatalog sceneCatalog)
    {
        catalog = sceneCatalog;
    }

    private void OnGUI()
    {
        EnsureStyles();
        Color previousColor = GUI.color;
        GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.95f);
        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
        GUI.color = previousColor;

        float width = Mathf.Min(560f, Screen.width * 0.8f);
        float left = (Screen.width - width) * 0.5f;
        GUI.Label(new Rect(left, Screen.height * 0.14f, width, 70f), "WHAT THE HELL? 3D", titleStyle);
        GUI.Label(new Rect(left, Screen.height * 0.25f, width, 35f), "Aventura de plataformas · Migración Unity", bodyStyle);

        float buttonWidth = Mathf.Min(320f, width);
        float buttonLeft = (Screen.width - buttonWidth) * 0.5f;
        float top = Screen.height * 0.4f;
        if (GUI.Button(new Rect(buttonLeft, top, buttonWidth, 48f), "Nueva partida"))
        {
            CampaignRuntimeState.Ensure(catalog).StartNewGame();
        }

        if (GUI.Button(new Rect(buttonLeft, top + 60f, buttonWidth, 48f), "Continuar"))
        {
            CampaignRuntimeState.Ensure(catalog).ContinueGame();
        }

        GUI.Label(new Rect(buttonLeft, top + 128f, buttonWidth, 30f), "Selección directa", bodyStyle);
        for (int levelId = 1; levelId <= 3; levelId++)
        {
            float levelLeft = buttonLeft + (levelId - 1) * (buttonWidth / 3f);
            if (GUI.Button(new Rect(levelLeft, top + 164f, buttonWidth / 3f - 6f, 40f), "Nivel " + levelId))
            {
                CampaignRuntimeState.Ensure(catalog).SelectLevel(levelId);
            }
        }

        GUI.Label(new Rect(0f, Screen.height - 40f, Screen.width, 30f), "WASD mover · Espacio saltar · Clic izquierdo atacar · Q esquivar · Esc pausar", bodyStyle);
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Screen.height / 12, 28, 64),
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.82f, 0.28f) }
        };
        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            normal = { textColor = Color.white }
        };
    }
}
