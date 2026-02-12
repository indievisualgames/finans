using UnityEngine;
using UnityEngine.Video;
using System.IO;

[RequireComponent(typeof(VideoPlayer))]
public class LocalVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private string videoFileName = "intro.mp4";
    [SerializeField] private bool useStreamingAssets = true;
    [SerializeField] private bool playOnStart = true;

    private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayLocalVideo();
        }
    }

    public void PlayLocalVideo()
    {
        string fullPath = GetVideoPath();
        
        if (string.IsNullOrEmpty(fullPath))
        {
            Debug.LogError("[LocalVideoPlayer] Failed to resolve video path.");
            return;
        }

        Debug.Log($"[LocalVideoPlayer] Loading video from: {fullPath}");
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = fullPath;
        videoPlayer.Prepare();
        
        videoPlayer.prepareCompleted += (source) => {
            Debug.Log("[LocalVideoPlayer] Video Prepared. Starting playback.");
            source.Play();
        };

        videoPlayer.errorReceived += (source, message) => {
            Debug.LogError($"[LocalVideoPlayer] Video Error: {message}");
        };
    }

    private string GetVideoPath()
    {
        string path = "";

        if (useStreamingAssets)
        {
            // Application.streamingAssetsPath works across platforms for VideoPlayer.url
            path = Path.Combine(Application.streamingAssetsPath, videoFileName);
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, streamingAssetsPath is inside the jar, but VideoPlayer can handle it directly if passed correct URL
            // and often works better with the direct path provided by Unity.
            #endif
        }
        else
        {
            path = Path.Combine(Application.persistentDataPath, videoFileName);
        }

        // Return empty if file not found (optional check, StreamingAssets in Jar/APK can't be checked with File.Exists)
        #if !UNITY_ANDROID || UNITY_EDITOR
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[LocalVideoPlayer] Warning: File not found at {path}. Checking fallback...");
            // Fallback check: if it was StreamingAssets and not found, maybe check persistentDataPath
            if (useStreamingAssets)
            {
                string fallback = Path.Combine(Application.persistentDataPath, videoFileName);
                if (File.Exists(fallback)) return fallback;
            }
        }
        #endif

        return path;
    }

    // --- Control Methods ---

    public void TogglePlayPause()
    {
        if (videoPlayer.isPlaying) videoPlayer.Pause();
        else videoPlayer.Play();
    }

    public void StopVideo()
    {
        videoPlayer.Stop();
    }

    public void Seek(float normalizedTime)
    {
        if (videoPlayer.canStep)
        {
            float targetTime = normalizedTime * (float)videoPlayer.length;
            videoPlayer.time = targetTime;
        }
    }

    public void SetVolume(float volume)
    {
        for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
        {
            videoPlayer.SetDirectAudioVolume(i, volume);
        }
    }

    public void SetPlaybackSpeed(float speed)
    {
        videoPlayer.playbackSpeed = speed;
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        Debug.Log($"[LocalVideoPlayer] Fullscreen toggled to: {Screen.fullScreen}");
    }
}
