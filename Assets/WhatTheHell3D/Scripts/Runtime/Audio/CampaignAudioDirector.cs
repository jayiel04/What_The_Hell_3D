using UnityEngine;

public sealed class CampaignAudioDirector : MonoBehaviour
{
    public AudioClip ambientClip;
    public float ambientVolume = 0.35f;
    private CampaignLevelConfig config;
    private AudioSource ambientSource;

    public void Configure(CampaignLevelConfig level)
    {
        config = level;
    }

    private void Start()
    {
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.playOnAwake = false;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.spatialBlend = 0f;
        if (ambientClip != null)
        {
            ambientSource.clip = ambientClip;
            ambientSource.Play();
        }
    }
}
