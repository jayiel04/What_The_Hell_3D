using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class VictorySceneController : MonoBehaviour
{
    public Text titleText;
    public Text bodyText;
    public Button menuButton;

    private CampaignSceneCatalog catalog;

    public void Configure(CampaignSceneCatalog sceneCatalog)
    {
        catalog = sceneCatalog;
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(() =>
            {
                if (catalog != null && !string.IsNullOrEmpty(catalog.mainMenuScene))
                {
                    SceneManager.LoadScene(catalog.mainMenuScene);
                }
            });
        }
    }
}
