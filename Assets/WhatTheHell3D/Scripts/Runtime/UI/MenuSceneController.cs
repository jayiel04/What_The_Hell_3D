using UnityEngine;
using UnityEngine.UI;

public sealed class MenuSceneController : MonoBehaviour
{
    public Text titleText;
    public Text subtitleText;
    public Text hintLabel;
    public Button newGameButton;
    public Button continueButton;
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;

    private CampaignSceneCatalog catalog;
    private bool wired;

    public void Configure(CampaignSceneCatalog sceneCatalog)
    {
        catalog = sceneCatalog;
        WireButtons();
    }

    private void OnEnable()
    {
        WireButtons();
    }

    private void WireButtons()
    {
        if (wired || catalog == null)
        {
            return;
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(() => CampaignRuntimeState.Ensure(catalog).StartNewGame());
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(() => CampaignRuntimeState.Ensure(catalog).ContinueGame());
        }

        AddLevelListener(level1Button, 1);
        AddLevelListener(level2Button, 2);
        AddLevelListener(level3Button, 3);
        wired = true;
    }

    private void AddLevelListener(Button button, int levelId)
    {
        if (button != null)
        {
            button.onClick.AddListener(() => CampaignRuntimeState.Ensure(catalog).SelectLevel(levelId));
        }
    }
}
