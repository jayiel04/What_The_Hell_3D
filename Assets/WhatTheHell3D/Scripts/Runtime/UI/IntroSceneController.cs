using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class IntroSceneController : MonoBehaviour
{
    [System.Serializable]
    public struct SubtitleLine
    {
        public string text;
        public float duration;
        public AudioClip voice;
    }

    public Text subtitleText;
    public Button skipButton;
    public Image fadeImage;
    public AudioSource voiceSource;
    public SubtitleLine[] lines = System.Array.Empty<SubtitleLine>();

    private CampaignSceneCatalog catalog;
    private bool finished;

    public void Configure(CampaignSceneCatalog sceneCatalog)
    {
        catalog = sceneCatalog;
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(Skip);
        }
    }

    private void Start()
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }

        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        for (int i = 0; i < lines.Length; i++)
        {
            if (subtitleText != null)
            {
                subtitleText.text = lines[i].text;
            }

            if (voiceSource != null && lines[i].voice != null)
            {
                voiceSource.clip = lines[i].voice;
                voiceSource.Play();
            }

            yield return new WaitForSecondsRealtime(Mathf.Max(0.5f, lines[i].duration));
        }

        if (subtitleText != null)
        {
            subtitleText.text = string.Empty;
        }

        yield return FadeOut();
        LoadFirstLevel();
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < 1.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(elapsed / 1.2f);
            fadeImage.color = color;
            yield return null;
        }
    }

    public void Skip()
    {
        StopAllCoroutines();
        StartCoroutine(SkipSequence());
    }

    private IEnumerator SkipSequence()
    {
        yield return FadeOut();
        LoadFirstLevel();
    }

    private void LoadFirstLevel()
    {
        if (finished)
        {
            return;
        }

        finished = true;
        Time.timeScale = 1f;
        if (catalog != null && !string.IsNullOrEmpty(catalog.GetCampaignLevelScene(1)))
        {
            SceneManager.LoadScene(catalog.GetCampaignLevelScene(1));
        }
    }
}
