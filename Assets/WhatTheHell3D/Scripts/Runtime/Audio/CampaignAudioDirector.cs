using UnityEngine;

public sealed class CampaignAudioDirector : MonoBehaviour
{
    public AudioClip ambientClip;
    public float ambientVolume = 0.35f;
    public AudioSource ambientSource;
    public AudioSource musicSource;
    public AudioClip musicClip;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    public AudioSource sfxSource;

    private CampaignLevelConfig config;

    public void Configure(CampaignLevelConfig level)
    {
        config = level;
    }

    private void Start()
    {
        if (ambientSource == null)
        {
            Debug.LogError("CampaignAudioDirector necesita un AudioSource creado desde Unity Editor.");
            return;
        }

        ambientSource.playOnAwake = false;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.spatialBlend = 0f;
        if (ambientClip != null)
        {
            ambientSource.clip = ambientClip;
            ambientSource.Play();
        }

        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.spatialBlend = 0f;
            if (musicClip != null)
            {
                musicSource.clip = musicClip;
                musicSource.Play();
            }
        }
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, volume);
    }
}
