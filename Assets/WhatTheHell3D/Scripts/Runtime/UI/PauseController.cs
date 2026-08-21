using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class PauseController : MonoBehaviour
{
    private CampaignLevelConfig config;
    private PlayerController player;
    private bool paused;
    private GUIStyle titleStyle;

    public void Configure(CampaignLevelConfig level, PlayerController playerController)
    {
        config = level;
        player = playerController;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void OnGUI()
    {
        if (!paused)
        {
            return;
        }

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.78f);
        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
        GUI.color = previousColor;
        GUI.Label(new Rect(Screen.width * 0.5f - 160f, Screen.height * 0.25f, 320f, 60f), "PAUSA", titleStyle);

        float left = Screen.width * 0.5f - 130f;
        if (GUI.Button(new Rect(left, Screen.height * 0.45f, 260f, 46f), "Continuar"))
        {
            TogglePause();
        }

        if (GUI.Button(new Rect(left, Screen.height * 0.45f + 58f, 260f, 46f), "Reiniciar checkpoint"))
        {
            Time.timeScale = 1f;
            paused = false;
            CampaignRuntimeState.Instance?.RestartLevel();
        }

        if (GUI.Button(new Rect(left, Screen.height * 0.45f + 116f, 260f, 46f), "Volver al menú"))
        {
            Time.timeScale = 1f;
            paused = false;
            string menuScene = CampaignRuntimeState.Instance == null || CampaignRuntimeState.Instance.Catalog == null
                ? string.Empty
                : CampaignRuntimeState.Instance.Catalog.mainMenuScene;
            if (!string.IsNullOrEmpty(menuScene))
            {
                SceneManager.LoadScene(menuScene);
            }
        }
    }

    private void TogglePause()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
    }
}
