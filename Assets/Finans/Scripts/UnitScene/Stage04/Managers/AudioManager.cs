using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages audio playback for the currency building game
/// Handles SFX, music, and audio feedback
/// </summary>
public class MiniGameAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;
    
    [Header("Default Audio Clips")]
    public AudioClip defaultPickupSFX;
    public AudioClip defaultDropSFX;
    public AudioClip defaultSnapSFX;
    public AudioClip defaultCorrectSFX;
    public AudioClip defaultWrongSFX;
    public AudioClip defaultCompletionSFX;
    [Tooltip("Played when score increases")] public AudioClip scoreUpdateSFX;
    [Tooltip("Played when score decreases or mistake happens")] public AudioClip scoreDecreaseSFX;
    [Tooltip("Played when a bonus is awarded")] public AudioClip bonusSFX;
    [Tooltip("Played when XP is gained")] public AudioClip xpGainSFX;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    [Range(0f, 1f)]
    public float voiceVolume = 0.9f;
    
    // Singleton instance
    public static MiniGameAudioManager Instance { get; private set; }
    
    // Private variables
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private const int MAX_AUDIO_SOURCES = 10;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeAudioManager()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            var musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            var sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        if (voiceSource == null)
        {
            var voiceObj = new GameObject("VoiceSource");
            voiceObj.transform.SetParent(transform);
            voiceSource = voiceObj.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.playOnAwake = false;
        }
        
        // Set initial volumes
        UpdateVolumes();
        
        // Pre-populate audio source pool
        for (int i = 0; i < MAX_AUDIO_SOURCES; i++)
        {
            var poolObj = new GameObject($"PooledAudioSource_{i}");
            poolObj.transform.SetParent(transform);
            var audioSource = poolObj.AddComponent<AudioSource>();
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSourcePool.Enqueue(audioSource);
        }
    }
    
    void UpdateVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume;
        }
        
        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }
        
        if (voiceSource != null)
        {
            voiceSource.volume = masterVolume * voiceVolume;
        }
    }
    
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource == null || musicClip == null) return;
        
        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.Play();
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }
    
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxSource == null || sfxClip == null) return;
        
        sfxSource.PlayOneShot(sfxClip);
    }
    
    public void PlaySFX(AudioClip sfxClip, float volumeScale)
    {
        if (sfxSource == null || sfxClip == null) return;
        
        sfxSource.PlayOneShot(sfxClip, volumeScale);
    }
    
    /// <summary>
    /// Play a looping SFX sound using a pooled audio source
    /// Returns the AudioSource so it can be stopped later
    /// </summary>
    public AudioSource PlayLoopingSFX(AudioClip sfxClip, float volumeScale = 1f)
    {
        if (sfxClip == null) return null;
        
        AudioSource pooledSource = GetPooledAudioSource();
        if (pooledSource != null)
        {
            pooledSource.clip = sfxClip;
            pooledSource.loop = true;
            pooledSource.volume = masterVolume * sfxVolume * volumeScale;
            pooledSource.Play();
        }
        
        return pooledSource;
    }
    
    /// <summary>
    /// Stop a looping SFX sound and return the audio source to the pool
    /// </summary>
    public void StopLoopingSFX(AudioSource audioSource)
    {
        if (audioSource == null) return;
        
        ReturnAudioSourceToPool(audioSource);
    }
    
    public void PlayVoice(AudioClip voiceClip)
    {
        if (voiceSource == null || voiceClip == null) return;
        
        voiceSource.clip = voiceClip;
        voiceSource.Play();
    }
    
    public void StopVoice()
    {
        if (voiceSource != null)
        {
            voiceSource.Stop();
        }
    }
    
    // Convenience methods for common SFX
    public void PlayPickupSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? defaultPickupSFX);
    }
    
    public void PlayDropSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? defaultDropSFX);
    }
    
    public void PlaySnapSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? defaultSnapSFX);
    }
    
    public void PlayCorrectSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? defaultCorrectSFX);
    }
    
    public void PlayWrongSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? defaultWrongSFX);
    }
    
    public void PlayCompletionSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? defaultCompletionSFX);
    }
    
    // Convenience methods for score/xp/bonus events
    public void PlayScoreUpdateSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? scoreUpdateSFX);
    }
    
    public void PlayScoreDecreaseSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? scoreDecreaseSFX);
    }
    
    public void PlayBonusSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? bonusSFX);
    }
    
    public void PlayXPGainSFX(AudioClip customClip = null)
    {
        PlaySFX(customClip ?? xpGainSFX);
    }
    
    // Pooled audio source for multiple simultaneous sounds
    public AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }
        
        // Create new audio source if pool is empty
        var newObj = new GameObject("DynamicAudioSource");
        newObj.transform.SetParent(transform);
        var audioSource = newObj.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = masterVolume * sfxVolume;
        
        return audioSource;
    }
    
    public void ReturnAudioSourceToPool(AudioSource audioSource)
    {
        if (audioSource == null) return;
        
        audioSource.Stop();
        audioSource.clip = null;
        
        if (audioSourcePool.Count < MAX_AUDIO_SOURCES)
        {
            audioSourcePool.Enqueue(audioSource);
        }
        else
        {
            Destroy(audioSource.gameObject);
        }
    }
    
    // Volume control methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    // Mute methods
    public void MuteAll()
    {
        masterVolume = 0f;
        UpdateVolumes();
    }
    
    public void UnmuteAll()
    {
        masterVolume = 1f;
        UpdateVolumes();
    }
    
    public void MuteMusic()
    {
        musicVolume = 0f;
        UpdateVolumes();
    }
    
    public void UnmuteMusic()
    {
        musicVolume = 0.7f;
        UpdateVolumes();
    }
    
    public void MuteSFX()
    {
        sfxVolume = 0f;
        UpdateVolumes();
    }
    
    public void UnmuteSFX()
    {
        sfxVolume = 0.8f;
        UpdateVolumes();
    }
    
    // Audio clip management
    public void RegisterAudioClip(string name, AudioClip clip)
    {
        if (!string.IsNullOrEmpty(name) && clip != null)
        {
            audioClips[name] = clip;
        }
    }
    
    public AudioClip GetAudioClip(string name)
    {
        audioClips.TryGetValue(name, out var clip);
        return clip;
    }
    
    public void PlayRegisteredSFX(string name)
    {
        var clip = GetAudioClip(name);
        if (clip != null)
        {
            PlaySFX(clip);
        }
    }
    
    // Fade methods
    public void FadeMusicIn(float duration = 1f)
    {
        StartCoroutine(FadeAudioSource(musicSource, 0f, masterVolume * musicVolume, duration));
    }
    
    public void FadeMusicOut(float duration = 1f)
    {
        StartCoroutine(FadeAudioSource(musicSource, musicSource.volume, 0f, duration));
    }
    
    private System.Collections.IEnumerator FadeAudioSource(AudioSource audioSource, float startVolume, float endVolume, float duration)
    {
        if (audioSource == null) yield break;
        
        float currentTime = 0f;
        audioSource.volume = startVolume;
        
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, currentTime / duration);
            yield return null;
        }
        
        audioSource.volume = endVolume;
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
} 