using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class IntroSceneController : MonoBehaviour
{
    private CampaignSceneCatalog catalog;
    private int lineIndex;
    private float lineTimer;
    private string[] lines;

    public void Configure(CampaignSceneCatalog sceneCatalog)
    {
        catalog = sceneCatalog;
        lines = new[]
        {
            "Algo despertó bajo las ruinas.",
            "Tres territorios. Una llave. Ninguna explicación razonable.",
            "Cruza el bosque, las minas y el castillo.",
            "La puerta final espera al otro lado."
        };
    }

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        while (lineIndex < lines.Length)
        {
            lineTimer = 0f;
            while (lineTimer < 2.6f)
            {
                lineTimer += Time.unscaledDeltaTime;
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    lineTimer = 2.6f;
                }

                yield return null;
            }

            lineIndex++;
        }

        LoadFirstLevel();
    }

    private void OnGUI()
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0.01f, 0.01f, 0.02f, 0.96f);
        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
        GUI.color = previousColor;

        string currentLine = lineIndex < lines.Length ? lines[lineIndex] : "";
        GUIStyle lineStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Screen.height / 22, 20, 38),
            wordWrap = true,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(Screen.width * 0.15f, Screen.height * 0.42f, Screen.width * 0.7f, 100f), currentLine, lineStyle);
        if (GUI.Button(new Rect(Screen.width - 180f, Screen.height - 75f, 140f, 42f), "Saltar"))
        {
            StopAllCoroutines();
            LoadFirstLevel();
        }
    }

    private void LoadFirstLevel()
    {
        if (catalog != null)
        {
            SceneManager.LoadScene(catalog.GetCampaignLevelScene(1));
        }
    }
}
