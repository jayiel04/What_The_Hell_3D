using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class VictorySceneController : MonoBehaviour
{
    private CampaignSceneCatalog catalog;

    public void Configure(CampaignSceneCatalog sceneCatalog)
    {
        catalog = sceneCatalog;
    }

    private void OnGUI()
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0.05f, 0.1f, 0.06f, 0.96f);
        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
        GUI.color = previousColor;

        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Screen.height / 10, 32, 72),
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.85f, 0.25f) }
        };
        GUIStyle body = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(0f, Screen.height * 0.25f, Screen.width, 80f), "¡VICTORIA!", title);
        GUI.Label(new Rect(Screen.width * 0.15f, Screen.height * 0.43f, Screen.width * 0.7f, 80f), "La campaña de What the Hell? 3D ha terminado.", body);
        if (GUI.Button(new Rect(Screen.width * 0.5f - 140f, Screen.height * 0.62f, 280f, 48f), "Volver al menú"))
        {
            if (catalog != null)
            {
                SceneManager.LoadScene(catalog.mainMenuScene);
            }
        }
    }
}
