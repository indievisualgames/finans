using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class VideoControlUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private LocalVideoPlayer localPlayer;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Visibility Toggle")]
    [SerializeField] private GameObject[] uiElements; // Array to assign multiple UI objects
    [SerializeField] private Button visibilityToggleButton;
    private bool isUIVisible = true;

    [Header("UI Controls")]
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Image playPauseImage; // Field to place icon
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Slider timelineSlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown speedDropdown; // Changed to TMP_Dropdown

    private bool isDragging = false;

    private void Start()
    {
        if (playPauseButton) playPauseButton.onClick.AddListener(OnPlayPauseClicked);
        if (stopButton) stopButton.onClick.AddListener(localPlayer.StopVideo);
        if (fullscreenButton) fullscreenButton.onClick.AddListener(localPlayer.ToggleFullscreen);
        if (visibilityToggleButton) visibilityToggleButton.onClick.AddListener(ToggleVisibility);
        
        if (timelineSlider)
        {
            timelineSlider.onValueChanged.AddListener(OnTimelineChanged);
            // Note: For cleaner UX, use EventTrigger for OnPointerDown/Up to set isDragging
        }

        if (volumeSlider)
        {
            volumeSlider.onValueChanged.AddListener(localPlayer.SetVolume);
            volumeSlider.value = 1.0f; // Default
        }

        if (speedDropdown)
        {
            speedDropdown.onValueChanged.AddListener(OnSpeedChanged);
            speedDropdown.RefreshShownValue();
        }
    }

    private void Update()
    {
        if (videoPlayer != null && videoPlayer.isPlaying && !isDragging && timelineSlider != null)
        {
            if (videoPlayer.length > 0)
            {
                timelineSlider.SetValueWithoutNotify((float)(videoPlayer.time / videoPlayer.length));
            }
        }
        UpdatePlayPauseIcon();
    }

    private void OnPlayPauseClicked()
    {
        localPlayer.TogglePlayPause();
        UpdatePlayPauseIcon();
    }

    private void UpdatePlayPauseIcon()
    {
        if (playPauseImage == null) return;

        if (videoPlayer.isPlaying)
        {
            if (pauseIcon != null) playPauseImage.sprite = pauseIcon;
        }
        else
        {
            if (playIcon != null) playPauseImage.sprite = playIcon;
        }
    }

    public void OnPointerDownTimeline()
    {
        isDragging = true;
    }

    public void OnPointerUpTimeline()
    {
        isDragging = false;
        localPlayer.Seek(timelineSlider.value);
    }

    private void ToggleVisibility()
    {
        isUIVisible = !isUIVisible;
        if (uiElements != null)
        {
            foreach (var element in uiElements)
            {
                if (element != null) element.SetActive(isUIVisible);
            }
        }
    }

    private void OnTimelineChanged(float val)
    {
        if (isDragging)
        {
            // Optional: Real-time seeking can be heavy, but if desired:
            // localPlayer.Seek(val);
        }
    }

    private void OnSpeedChanged(int index)
    {
        if (speedDropdown == null || index < 0 || index >= speedDropdown.options.Count) return;

        // Get the text from the selected option (e.g., "1.5x")
        string selectedText = speedDropdown.options[index].text;
        
        // Extract only the numbers and dots (e.g., "1.5x" -> "1.5")
        string numericPart = Regex.Replace(selectedText, "[^0-9.]", "");

        if (float.TryParse(numericPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float speed))
        {
            localPlayer.SetPlaybackSpeed(speed);
            Debug.Log($"[VideoControlUI] Set Speed to: {speed} (from text: {selectedText})");
        }
    }
}
