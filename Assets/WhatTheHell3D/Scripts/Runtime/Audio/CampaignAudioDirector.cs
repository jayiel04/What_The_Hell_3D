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
    private static CampaignAudioDirector activeDirector;

    public void Configure(CampaignLevelConfig level)
    {
        config = level;
    }

    private void OnEnable()
    {
        if (activeDirector != null && activeDirector != this)
        {
            activeDirector.StopAllAudio();
        }

        activeDirector = this;
    }

    private void OnDisable()
    {
        if (activeDirector == this)
        {
            activeDirector = null;
        }
    }

    private void OnDestroy()
    {
        if (activeDirector == this)
        {
            activeDirector = null;
        }
    }

    private void StopAllAudio()
    {
        if (ambientSource != null) ambientSource.Stop();
        if (musicSource != null) musicSource.Stop();
    }

    private void Start()
    {
        if (ambientSource == null)
        {
            Debug.LogError("CampaignAudioDirector necesita un AudioSource creado desde Unity Editor.");
            return;
        }

        // Evita solapamiento si por algún motivo queda un director anterior vivo (p. ej. DontDestroyOnLoad)
        if (activeDirector != null && activeDirector != this)
        {
            activeDirector.StopAllAudio();
            activeDirector = this;
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
        else
        {
            ambientSource.Stop();
            ambientSource.clip = null;
        }

        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.spatialBlend = 0f;
            if (musicClip != null)
            {
                // Si por error ambient y música apuntan al mismo clip, prioriza música
                if (ambientClip != null && ambientClip == musicClip)
                {
                    ambientSource.Stop();
                }

                musicSource.clip = musicClip;
                musicSource.Play();
            }
            else
            {
                musicSource.Stop();
                musicSource.clip = null;
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
