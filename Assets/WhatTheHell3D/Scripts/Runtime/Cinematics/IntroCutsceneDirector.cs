using System.Collections;
using System.IO;
using System.Threading;
using GLTFast;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Réplica del timeline de 7 escenas de la cinemática de Godot
/// (scripts/cinematics/intro_story_3d.gd): cámara, caballero caminando,
/// luces animadas, audio ambiental y voces del narrador con subtítulos.
/// </summary>
public sealed class IntroCutsceneDirector : MonoBehaviour
{
    [System.Serializable]
    public struct VoiceLine
    {
        public string text;
        public float duration;
        public AudioClip clip;
    }

    [Header("Escenario y luces")]
    public Transform player;
    public Light moonLight;
    public Light gateGlow;
    public Light castleLight;
    public Camera cutsceneCamera;

    [Header("UI")]
    public Text subtitleText;
    public Image fadeImage;
    public Button skipButton;

    [Header("Audio")]
    public AudioSource ambience;
    public AudioSource bell;
    public AudioSource breath;
    public AudioSource footsteps;
    public AudioSource voice;

    [Header("Líneas narradas (orden del timeline de Godot)")]
    public VoiceLine[] lines = new VoiceLine[15];

    [Header("Modelo")]
    public string characterModelPath = "Characters/Knight_Male.gltf";
    public string nextScenePath = "Assets/Scenes/CampaignLevel01.unity";

    // Constantes exactas de intro_story_3d.gd
    private const float PlayerCutsceneScale = 0.5f;
    private const float PlayerStopZ = -16.5f;
    private const float CameraFollowDistance = 4.5f;
    private static readonly Vector3 CameraStartPos = new Vector3(0f, 3.5f, 7f);
    private static readonly Vector3 CameraWakePos = new Vector3(0f, 2.2f, 4.5f);
    private static readonly Vector3[] TourPoints =
    {
        new Vector3(1.8f, 2.0f, -1.5f),
        new Vector3(-1.8f, 1.6f, -0.5f),
        new Vector3(2.4f, 2.0f, -4.0f)
    };
    private const float WakeWalkStartZ = 4.2f;
    private const float WakeWalkStopZ = 0f;
    private const float WakeWalkDuration = 2.8f;
    private const float ReferenceWalkSpeed = 2.8f;
    private const float WalkDistance = 16.5f;
    private const float WalkDuration = 12.6f;

    private bool finished;
    private Animation animator;
    private bool modelLoaded;

    public void Configure(CampaignSceneCatalog sceneCatalog)
    {
        if (sceneCatalog != null && !string.IsNullOrEmpty(sceneCatalog.GetCampaignLevelScene(1)))
        {
            nextScenePath = sceneCatalog.GetCampaignLevelScene(1);
        }
    }

    private void Start()
    {
        finished = false;
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }

        if (subtitleText != null)
        {
            subtitleText.text = string.Empty;
            SetSubtitleAlpha(0f);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(Skip);
        }

        if (moonLight != null) moonLight.intensity = 0f;
        if (gateGlow != null) gateGlow.intensity = 0f;
        if (castleLight != null) castleLight.intensity = 0f;

        if (player != null)
        {
            player.localScale = Vector3.one * PlayerCutsceneScale;
            // En Godot el nodo Player tiene basis (−1,0,0 / 0,1,0 / 0,0,−1) = giro Y 180°,
            // necesario para que el caballero mire hacia −Z (hacia la puerta) al caminar.
            player.localRotation = Quaternion.Euler(0f, 180f, 0f);
            player.gameObject.SetActive(false);
        }

        if (cutsceneCamera != null)
        {
            cutsceneCamera.transform.position = CameraStartPos;
        }

        if (ambience != null && ambience.clip != null)
        {
            ambience.loop = true;
            ambience.Play();
        }

        if (bell != null && bell.clip != null)
        {
            bell.loop = true;
            bell.Play();
        }

        StartCoroutine(LoadKnightModel());
        StartCoroutine(RunTimeline());
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        bool enterPressed = (keyboard.enterKey != null && keyboard.enterKey.wasPressedThisFrame)
            || (keyboard.numpadEnterKey != null && keyboard.numpadEnterKey.wasPressedThisFrame);
        bool escapePressed = keyboard.escapeKey != null && keyboard.escapeKey.wasPressedThisFrame;
        if (enterPressed || escapePressed)
        {
            Skip();
        }
    }

    private IEnumerator LoadKnightModel()
    {
        string localPath = Path.Combine(Application.streamingAssetsPath, characterModelPath);
        if (!File.Exists(localPath))
        {
            Debug.LogWarning($"[Intro] Modelo del caballero no encontrado en {localPath}.");
            yield break;
        }

        var importer = new GltfImport();
        var loadTask = importer.LoadFile(localPath, null, null, CancellationToken.None);
        while (!loadTask.IsCompleted)
        {
            yield return null;
        }

        if (!loadTask.Result)
        {
            Debug.LogError($"[Intro] Falló la carga del modelo: {localPath}");
            yield break;
        }

        var instantiateTask = importer.InstantiateMainSceneAsync(player, CancellationToken.None);
        while (!instantiateTask.IsCompleted)
        {
            yield return null;
        }

        animator = player.GetComponent<Animation>() ?? player.gameObject.AddComponent<Animation>();
        animator.playAutomatically = false;
        foreach (AnimationClip clip in importer.GetAnimationClips())
        {
            clip.legacy = true;
            clip.wrapMode = clip.name is "Idle" or "Walk" ? WrapMode.Loop : WrapMode.Once;
            if (animator.GetClip(clip.name) == null)
            {
                animator.AddClip(clip, clip.name);
            }
        }

        modelLoaded = true;
    }

    private void PlayPlayerAnimation(string clipName, float speedScale = 1f)
    {
        if (animator == null || animator.GetClip(clipName) == null)
        {
            return;
        }

        animator[clipName].speed = speedScale;
        animator.CrossFade(clipName, 0.35f);
    }

    // ------------------------------------------------------------- timeline

    private IEnumerator RunTimeline()
    {
        yield return Scene1BlackIntro();
        if (finished) yield break;
        yield return Scene2RevealMoon();
        if (finished) yield break;
        yield return Scene3WakeUp();
        if (finished) yield break;
        yield return Scene4SilentTour();
        if (finished) yield break;
        yield return Scene5WalkAndWatch();
        if (finished) yield break;
        yield return Scene6ArriveAtGate();
        if (finished) yield break;
        yield return Scene7ClosingText();
        if (finished) yield break;
        FinishWithFade();
    }

    private IEnumerator Scene1BlackIntro()
    {
        yield return ShowLine(0);
        if (finished) yield break;
        yield return new WaitForSeconds(0.8f);
        if (finished) yield break;
        yield return ShowLine(1);
    }

    private IEnumerator Scene2RevealMoon()
    {
        Coroutine fadeRoutine = StartCoroutine(TweenFadeAlpha(1f, 0f, 2f));
        yield return fadeRoutine;

        Coroutine reveal = StartCoroutine(TweenRevealMoon());
        yield return ShowLine(2);
        if (finished) yield break;
        yield return ShowLine(3);
        if (finished) yield break;
        yield return ShowLine(4);
        if (reveal != null) StopCoroutine(reveal);
        if (bell != null) bell.Stop();
    }

    private IEnumerator TweenRevealMoon()
    {
        float startEnergy = moonLight != null ? moonLight.intensity : 0f;
        Vector3 startPos = cutsceneCamera != null ? cutsceneCamera.transform.position : CameraStartPos;
        float elapsed = 0f;
        while (elapsed < 6f)
        {
            elapsed += Time.deltaTime;
            float t = SineEase(Mathf.Clamp01(elapsed / 6f));
            if (moonLight != null)
            {
                moonLight.intensity = Mathf.Lerp(startEnergy, 0.65f, t);
            }

            if (cutsceneCamera != null)
            {
                cutsceneCamera.transform.position = Vector3.Lerp(startPos, CameraWakePos, t);
            }

            yield return null;
        }
    }

    private IEnumerator Scene3WakeUp()
    {
        if (player != null)
        {
            Vector3 position = player.position;
            position.z = WakeWalkStartZ;
            player.position = position;
            player.gameObject.SetActive(true);
        }

        if (breath != null && breath.clip != null)
        {
            breath.Play();
        }

        float walkSpeed = (WakeWalkStartZ - WakeWalkStopZ) / WakeWalkDuration;
        PlayPlayerAnimation("Walk", walkSpeed / ReferenceWalkSpeed);
        if (footsteps != null && footsteps.clip != null)
        {
            footsteps.loop = true;
            footsteps.Play();
        }

        Coroutine walk = StartCoroutine(TweenPlayerZ(WakeWalkStopZ, WakeWalkDuration));
        Coroutine zoom = StartCoroutine(TweenCameraZ(3f, WakeWalkDuration));
        yield return walk;
        if (zoom != null) StopCoroutine(zoom);

        if (footsteps != null) footsteps.Stop();
        PlayPlayerAnimation("Idle");

        yield return ShowLine(5);
        if (finished) yield break;
        yield return ShowLine(6);
    }

    private IEnumerator Scene4SilentTour()
    {
        for (int i = 0; i < TourPoints.Length; i++)
        {
            Vector3 start = cutsceneCamera.transform.position;
            float elapsed = 0f;
            while (elapsed < 2.2f)
            {
                elapsed += Time.deltaTime;
                float t = SineEase(Mathf.Clamp01(elapsed / 2.2f));
                if (cutsceneCamera != null)
                {
                    cutsceneCamera.transform.position = Vector3.Lerp(start, TourPoints[i], t);
                }

                yield return null;
            }

            if (finished) yield break;
            int lineIndex = i == 0 ? 7 : (i == 1 ? 8 : -1);
            if (lineIndex >= 0)
            {
                yield return ShowLine(lineIndex);
            }
            else
            {
                yield return new WaitForSeconds(1.4f);
            }

            if (finished) yield break;
        }
    }

    private IEnumerator Scene5WalkAndWatch()
    {
        float walkSpeed = WalkDistance / WalkDuration;
        PlayPlayerAnimation("Walk", walkSpeed / ReferenceWalkSpeed);
        if (footsteps != null && footsteps.clip != null)
        {
            footsteps.loop = true;
            footsteps.Play();
        }

        Coroutine walk = StartCoroutine(TweenPlayerZ(PlayerStopZ, WalkDuration));
        Coroutine follow = StartCoroutine(TweenCameraPosition(
            new Vector3(0f, 2.2f, PlayerStopZ + CameraFollowDistance), WalkDuration));

        // En Godot estos tweens siguen corriendo durante la escena 6 hasta terminar.
        _ = walk;
        _ = follow;

        yield return ShowLine(9);
        if (finished) yield break;
        yield return ShowLine(10);
        if (finished) yield break;
        yield return new WaitForSeconds(3.5f);
    }

    private IEnumerator Scene6ArriveAtGate()
    {
        if (footsteps != null) footsteps.Stop();
        PlayPlayerAnimation("Idle");

        Coroutine glow = StartCoroutine(TweenLightIntensity(gateGlow, 2.5f, 2.5f));
        Coroutine castle = StartCoroutine(TweenLightIntensity(castleLight, 3.0f, 3.5f));
        Coroutine ambienceUp = StartCoroutine(TweenAudioVolume(ambience, Mathf.Pow(10f, -2f / 20f), 2.5f));

        yield return ShowLine(11);
        if (finished) yield break;
        yield return ShowLine(12);
        _ = glow;
        _ = castle;
        _ = ambienceUp;
    }

    private IEnumerator Scene7ClosingText()
    {
        yield return new WaitForSeconds(0.8f);
        if (finished) yield break;
        yield return ShowLine(13);
        if (finished) yield break;
        yield return ShowLine(14);
    }

    // -------------------------------------------------------------- helpers

    private IEnumerator ShowLine(int index)
    {
        if (lines == null || index < 0 || index >= lines.Length)
        {
            yield break;
        }

        VoiceLine line = lines[index];
        if (subtitleText != null)
        {
            subtitleText.text = line.text;
            SetSubtitleAlpha(0f);
        }

        float hold = line.duration;
        if (voice != null && line.clip != null)
        {
            voice.clip = line.clip;
            voice.Play();
            hold = line.clip.length;
        }

        yield return TweenSubtitleAlpha(0f, 1f, 0.6f);
        yield return new WaitForSeconds(hold);
        if (finished) yield break;
        yield return TweenSubtitleAlpha(1f, 0f, 0.6f);
    }

    private void SetSubtitleAlpha(float alpha)
    {
        if (subtitleText != null)
        {
            Color color = subtitleText.color;
            color.a = alpha;
            subtitleText.color = color;
        }
    }

    private IEnumerator TweenSubtitleAlpha(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetSubtitleAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetSubtitleAlpha(to);
    }

    private IEnumerator TweenFadeAlpha(float from, float to, float duration)
    {
        if (fadeImage == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color color = fadeImage.color;
            color.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            fadeImage.color = color;
            yield return null;
        }

        Color finalColor = fadeImage.color;
        finalColor.a = to;
        fadeImage.color = finalColor;
    }

    private IEnumerator TweenPlayerZ(float targetZ, float duration)
    {
        if (player == null)
        {
            yield break;
        }

        float startZ = player.position.z;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 position = player.position;
            position.z = Mathf.Lerp(startZ, targetZ, t);
            player.position = position;
            yield return null;
        }
    }

    private IEnumerator TweenCameraZ(float targetZ, float duration)
    {
        if (cutsceneCamera == null)
        {
            yield break;
        }

        Vector3 start = cutsceneCamera.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = SineEase(Mathf.Clamp01(elapsed / duration));
            Vector3 position = start;
            position.z = Mathf.Lerp(start.z, targetZ, t);
            cutsceneCamera.transform.position = position;
            yield return null;
        }
    }

    private IEnumerator TweenCameraPosition(Vector3 target, float duration)
    {
        if (cutsceneCamera == null)
        {
            yield break;
        }

        Vector3 start = cutsceneCamera.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = SineEase(Mathf.Clamp01(elapsed / duration));
            cutsceneCamera.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
    }

    private IEnumerator TweenLightIntensity(Light light, float target, float duration)
    {
        if (light == null)
        {
            yield break;
        }

        float start = light.intensity;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            light.intensity = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        light.intensity = target;
    }

    private IEnumerator TweenAudioVolume(AudioSource source, float targetLinear, float duration)
    {
        if (source == null)
        {
            yield break;
        }

        float start = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(start, targetLinear, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        source.volume = targetLinear;
    }

    private static float SineEase(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
    }

    // ------------------------------------------------------------ finalización

    public void Skip()
    {
        if (finished)
        {
            return;
        }

        finished = true;
        StopAllCoroutines();
        StopAllAudio();
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextScenePath);
    }

    private void FinishWithFade()
    {
        if (finished)
        {
            return;
        }

        finished = true;
        StartCoroutine(FinishSequence());
    }

    private IEnumerator FinishSequence()
    {
        StopAllAudio();
        yield return TweenFadeAlpha(fadeImage != null ? fadeImage.color.a : 0f, 1f, 0.8f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextScenePath);
    }

    private void StopAllAudio()
    {
        if (ambience != null) ambience.Stop();
        if (bell != null) bell.Stop();
        if (breath != null) breath.Stop();
        if (footsteps != null) footsteps.Stop();
        if (voice != null) voice.Stop();
    }

    public bool IsFinished => finished;
    public bool IsModelLoaded => modelLoaded;
}
