using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// TutAudioManager centralizes tutorial audio playback. Attach an AudioSource to the same GameObject.
/// Provides play, pause, resume, stop, repeat, volume control, and smooth fades.
/// </summary>
public class TutAudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [Tooltip("AudioSource used for playback. If null, will attempt to GetComponent on Start.")]
    public AudioSource audioSource;

    [Header("Fade Settings (seconds)")]
    [Min(0f)] public float defaultFadeInDuration = 0.2f;
    [Min(0f)] public float defaultFadeOutDuration = 0.2f;

    [Header("Events")]
    public UnityEvent OnClipStarted;
    public UnityEvent OnClipPaused;
    public UnityEvent OnClipResumed;
    public UnityEvent OnClipStopped;
    public UnityEvent OnClipEnded;

    // Internal state
    private Coroutine fadeCoroutine;
    private float targetVolume = 1f;
    private bool isManuallyPaused = false;

    public static TutAudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        targetVolume = audioSource.volume;
    }

    void Update()
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.loop)
        {
            // Detect clip end
            if (!audioSource.isPlaying && !isManuallyPaused)
            {
                // Ensure we only fire once per clip end
                if (Mathf.Approximately(audioSource.time, 0f) || Mathf.Approximately(audioSource.time, audioSource.clip.length))
                {
                    OnClipEnded?.Invoke();
                }
            }
        }
    }

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    public AudioClip GetCurrentClip()
    {
        return audioSource != null ? audioSource.clip : null;
    }

    public void SetVolume(float volume)
    {
        targetVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = targetVolume;
        }
    }

    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : targetVolume;
    }

    public void Play(AudioClip clip, bool loop = false, float? volume = null, float? fadeInDuration = null)
    {
        if (clip == null) return;
        EnsureAudioSource();

        Stop(fadeOutDuration: 0f); // prevent overlap

        audioSource.clip = clip;
        audioSource.loop = loop;
        float startVolume = volume.HasValue ? Mathf.Clamp01(volume.Value) : targetVolume;
        audioSource.volume = 0f;
        isManuallyPaused = false;
        audioSource.Play();
        OnClipStarted?.Invoke();

        float fade = Mathf.Max(0f, fadeInDuration ?? defaultFadeInDuration);
        StartFade(to: startVolume, duration: fade);
    }

    public void Pause()
    {
        if (audioSource == null || !audioSource.isPlaying) return;
        audioSource.Pause();
        isManuallyPaused = true;
        OnClipPaused?.Invoke();
    }

    public void Resume()
    {
        if (audioSource == null || audioSource.clip == null || audioSource.isPlaying) return;
        audioSource.UnPause();
        isManuallyPaused = false;
        OnClipResumed?.Invoke();
    }

    public void Stop(float? fadeOutDuration = null)
    {
        if (audioSource == null) return;
        if (!audioSource.isPlaying && audioSource.clip == null) return;

        float fade = Mathf.Max(0f, fadeOutDuration ?? defaultFadeOutDuration);
        if (fade > 0f)
        {
            StartFade(to: 0f, duration: fade, onComplete: () =>
            {
                audioSource.Stop();
                audioSource.clip = null;
                OnClipStopped?.Invoke();
            });
        }
        else
        {
            audioSource.Stop();
            audioSource.clip = null;
            OnClipStopped?.Invoke();
        }
    }

    public void Repeat(float? fadeInDuration = null)
    {
        if (audioSource == null || audioSource.clip == null) return;
        Play(audioSource.clip, audioSource.loop, audioSource.volume, fadeInDuration);
    }

    private void StartFade(float to, float duration, System.Action onComplete = null)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(to, duration, onComplete));
    }

    private IEnumerator FadeCoroutine(float to, float duration, System.Action onComplete)
    {
        EnsureAudioSource();
        float from = audioSource.volume;
        if (duration <= 0f)
        {
            audioSource.volume = to;
            onComplete?.Invoke();
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            audioSource.volume = Mathf.Lerp(from, to, k);
            yield return null;
        }
        audioSource.volume = to;
        onComplete?.Invoke();
        fadeCoroutine = null;
    }

    private void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }
}


