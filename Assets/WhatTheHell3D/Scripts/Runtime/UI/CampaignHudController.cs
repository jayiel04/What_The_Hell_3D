using UnityEngine;
using UnityEngine.UI;

public sealed class CampaignHudController : MonoBehaviour
{
    public Text objectiveTitle;
    public Text objectiveText;
    public Image healthFill;
    public Text healthLabel;
    public Text coinsLabel;
    public Text keyLabel;
    public Text checkpointLabel;
    public Text hintLabel;

    private CampaignLevelConfig config;
    private PlayerController player;

    public void Configure(CampaignLevelConfig level, PlayerController playerController)
    {
        config = level;
        player = playerController;
        if (objectiveTitle != null)
        {
            objectiveTitle.text = config.title;
        }

        if (objectiveText != null)
        {
            objectiveText.text = config.objective;
        }

        if (hintLabel != null)
        {
            hintLabel.text = "Esc pausa · E interactuar";
        }
    }

    private void Update()
    {
        if (config == null || player == null)
        {
            return;
        }

        CampaignProgressData progress = CampaignRuntimeState.Instance == null ? null : CampaignRuntimeState.Instance.Progress;
        int currentHealth = player.Health == null ? 0 : player.Health.CurrentHealth;
        int maxHealth = player.Health == null ? 100 : player.Health.maxHealth;
        if (healthLabel != null)
        {
            healthLabel.text = "SALUD " + currentHealth + "/" + maxHealth;
        }

        if (healthFill != null)
        {
            healthFill.fillAmount = Mathf.Clamp01((float)currentHealth / Mathf.Max(1, maxHealth));
        }

        if (coinsLabel != null)
        {
            int collected = progress == null ? 0 : progress.collected;
            int total = progress == null ? 0 : progress.totalCollectibles;
            coinsLabel.text = "Monedas: " + collected + "/" + total;
        }

        if (keyLabel != null)
        {
            bool key = progress != null && progress.keyCollected;
            keyLabel.text = "Llave: " + (key ? "obtenida" : "pendiente");
        }

        if (checkpointLabel != null)
        {
            int checkpoint = progress == null ? 0 : progress.checkpointIndex;
            checkpointLabel.text = "Checkpoint: " + checkpoint;
        }
    }
}
