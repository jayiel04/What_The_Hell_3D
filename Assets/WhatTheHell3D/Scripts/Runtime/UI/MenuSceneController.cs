using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Menú principal réplica de main_menu.gd: estados Principal/Capítulos/Créditos,
/// CONTINUAR ligado al guardado real, SFX de clic en cada botón, música ambiente
/// en bucle y ESC para volver de los popups.
/// </summary>
public sealed class MenuSceneController : MonoBehaviour
{
    [Header("Textos")]
    public Text titleText;
    public Text subtitleText;
    public Text hintLabel;

    [Header("Botones principales")]
    public GameObject mainButtonsPanel;
    public Button newGameButton;
    public Button continueButton;
    public Button chaptersButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Popups")]
    public GameObject chaptersPanel;
    public GameObject creditsPanel;
    public Button chapter1Button;
    public Button chapter2Button;
    public Button chapter3Button;
    public Button chaptersBackButton;
    public Button creditsBackButton;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioSource clickSource;

    private CampaignSceneCatalog catalog;
    private bool wired;

    public void Configure(CampaignSceneCatalog sceneCatalog)
    {
        catalog = sceneCatalog;
        Wire();
        RefreshContinueButton();
    }

    private void OnEnable()
    {
        Wire();
        ShowMain();
    }

    private void Start()
    {
        Wire();
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }

        RefreshContinueButton();
        ShowMain();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (IsPopupOpen())
            {
                PlayClick();
                ShowMain();
            }
        }
    }

    public void ShowMain()
    {
        SetPanel(mainButtonsPanel, true);
        SetPanel(chaptersPanel, false);
        SetPanel(creditsPanel, false);
    }

    public void ShowChapters()
    {
        SetPanel(mainButtonsPanel, false);
        SetPanel(chaptersPanel, true);
        SetPanel(creditsPanel, false);
    }

    public void ShowCredits()
    {
        SetPanel(mainButtonsPanel, false);
        SetPanel(chaptersPanel, false);
        SetPanel(creditsPanel, true);
    }

    public void StartNewGame()
    {
        CampaignRuntimeState.Ensure(catalog).StartNewGame();
    }

    public void ContinueGame()
    {
        CampaignRuntimeState.Ensure(catalog).ContinueGame();
    }

    public void SelectChapter(int levelId)
    {
        CampaignRuntimeState.Ensure(catalog).SelectLevel(levelId);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(0);
#endif
    }

    public void PlayClick()
    {
        if (clickSource != null && clickSource.clip != null)
        {
            clickSource.PlayOneShot(clickSource.clip);
        }
    }

    public bool IsPopupOpen()
    {
        return (chaptersPanel != null && chaptersPanel.activeSelf)
            || (creditsPanel != null && creditsPanel.activeSelf);
    }

    public bool IsContinueAvailable()
    {
        return new JsonCampaignProgressStore().Load() != null;
    }

    private void RefreshContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.interactable = IsContinueAvailable();
        }
    }

    private void Wire()
    {
        if (wired || catalog == null)
        {
            return;
        }

        AddListener(newGameButton, () =>
        {
            PlayClick();
            StartNewGame();
        });
        AddListener(continueButton, () =>
        {
            PlayClick();
            ContinueGame();
        });
        AddListener(chaptersButton, () =>
        {
            PlayClick();
            ShowChapters();
        });
        AddListener(creditsButton, () =>
        {
            PlayClick();
            ShowCredits();
        });
        AddListener(quitButton, () =>
        {
            PlayClick();
            QuitGame();
        });

        AddListener(chapter1Button, () =>
        {
            PlayClick();
            SelectChapter(1);
        });
        AddListener(chapter2Button, () =>
        {
            PlayClick();
            SelectChapter(2);
        });
        AddListener(chapter3Button, () =>
        {
            PlayClick();
            SelectChapter(3);
        });
        AddListener(chaptersBackButton, () =>
        {
            PlayClick();
            ShowMain();
        });
        AddListener(creditsBackButton, () =>
        {
            PlayClick();
            ShowMain();
        });

        wired = true;
    }

    private static void AddListener(Button button, System.Action action)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }
    }

    private static void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}
