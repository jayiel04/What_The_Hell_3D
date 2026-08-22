using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class PauseController : MonoBehaviour
{
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartButton;
    public Button menuButton;

    private CampaignLevelConfig config;
    private PlayerController player;
    private bool paused;

    public void Configure(CampaignLevelConfig level, PlayerController playerController)
    {
        config = level;
        player = playerController;
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(TogglePause);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartFromCheckpoint);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(ReturnToMenu);
        }
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

    public void TogglePause()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        if (pausePanel != null)
        {
            pausePanel.SetActive(paused);
        }
    }

    private void RestartFromCheckpoint()
    {
        Time.timeScale = 1f;
        paused = false;
        CampaignRuntimeState.Instance?.RestartLevel();
    }

    private void ReturnToMenu()
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
