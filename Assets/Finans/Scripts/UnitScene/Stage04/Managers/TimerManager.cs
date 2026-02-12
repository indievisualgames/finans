using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

/// <summary>
/// Enhanced timer manager that combines LevelTimer features with TimeExtensionManager compatibility
/// Supports time extension, audio, VFX, and advanced warning systems
/// </summary>
public class TimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("DEPRECATED: Use DynamicScoreManager.initialTimeLimit instead")]
    public float initialTime = 120f; // Initial time in seconds - DEPRECATED
    [Tooltip("DEPRECATED: Use DynamicScoreManager.countDownTimer instead")]
    public bool countDown = true; // Whether to count down or up - DEPRECATED
    public bool autoStart = true; // Whether to start automatically
    
    [Header("Time Format")]
    public string timeFormat = "mm:ss"; // Format: mm:ss, ss, mm:ss.ms
    
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI[] additionalTimerTexts; // Array for multiple timer text elements
    public TextMeshProUGUI warningText;
    
    [Header("Warning Auto-Hide")]
    public bool autoHideWarning = true; // Whether to automatically hide warning text
    public float warningHideDelay = 3f; // Time in seconds before warning text is hidden
    
    [Header("Audio")]
    [Tooltip("Reference to the main audio manager for unified audio control")]
    public MiniGameAudioManager audioManager;
    [Tooltip("Whether to use MiniGameAudioManager for all audio (recommended)")]
    public bool useUnifiedAudio = true;
    
    [Header("Legacy Audio (deprecated - use MiniGameAudioManager instead)")]
    [Tooltip("DEPRECATED: Use MiniGameAudioManager instead")]
    public AudioSource audioSource; // Main audio source
    [Tooltip("DEPRECATED: Use MiniGameAudioManager instead")]
    public AudioSource warningAudioSource; // Separate AudioSource for looping warning sound
    [Tooltip("Audio clips for timer events")]
    public AudioClip timeWarningSound; // Play when warning threshold reached
    public AudioClip timeUpSound; // Play when time runs out
    
    [Header("VFX")]
    public GameObject timeUpVFX; // Assign VFX prefab in Inspector
    public Transform vfxSpawnPoint; // Where to spawn VFX (optional)
    
    [Header("UI Panels")]
    public GameObject timeUpPanel; // Assign the time-up UI panel in Inspector
    public bool hideTimerOnTimeUp = true; // Whether to hide the timer text when time is up
    
    [Header("Events")]
    public UnityEvent OnTimeUp; // Called when time runs out
    public UnityEvent OnWarningTime; // Called when warning threshold is reached
    public UnityEvent<float> OnTimeChanged; // Called when time changes
    
    [Header("Warning Settings")]
    [Tooltip("DEPRECATED: Use DynamicScoreManager.warningThreshold instead")]
    [Range(5f, 60f)]
    public float warningThreshold = 30f; // When to show warning - DEPRECATED
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public bool enableWarningFlash = true; // Enable flashing text during warning
    public float warningFlashInterval = 0.2f; // Flash interval in seconds
    
    [Header("MinigameScoreManager Integration")]
    [Tooltip("Reference to MinigameScoreManager for centralized timer configuration")]
    public MinigameScoreManager scoreManager;
    [Tooltip("Automatically find DynamicScoreManager if not assigned")]
    public bool autoFindScoreManager = true;
    
    // Private variables
    private float currentTime;
    private bool isRunning = false;
    private bool warningShown = false;
    private bool timeUpTriggered = false;
    private Coroutine warningCoroutine;
    private Coroutine warningHideCoroutine;
    private bool warningSoundPlaying = false;
    
    // Audio source for looping warning sound (unified audio system)
    private AudioSource loopingWarningSource = null;
    
    // Public properties that read from DynamicScoreManager when available
    public float TimeLeft => currentTime;
    public bool IsRunning => isRunning;
    public bool IsTimeUp => currentTime <= 0;
    public bool IsTimeRunningLow => GetCountDownMode() && currentTime <= GetWarningThreshold() && currentTime > 0;
    
    // Properties that read from DynamicScoreManager (master source of truth)
    public float MasterInitialTime => (scoreManager != null) ? scoreManager.MasterTimeLimit : initialTime;
    public float MasterWarningThreshold => (scoreManager != null) ? scoreManager.MasterWarningThreshold : warningThreshold;
    public bool MasterCountDown => (scoreManager != null) ? scoreManager.MasterCountDown : countDown;
    
    void Start()
    {
        // Find score manager if not assigned
        if (autoFindScoreManager && scoreManager == null)
        {
            FindScoreManager();
        }
        
        // Find audio manager if not assigned
        if (useUnifiedAudio && audioManager == null)
        {
            FindAudioManager();
        }
        
        ResetTimer();
        if (autoStart)
            StartTimer();
    }
    
    /// <summary>
    /// Finds the DynamicScoreManager automatically
    /// </summary>
    private void FindScoreManager()
    {
        scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
        if (scoreManager != null)
        {
            Debug.Log("TimerManager: Found DynamicScoreManager automatically");
        }
        else
        {
            Debug.LogWarning("TimerManager: DynamicScoreManager not found! Using local timer settings.");
        }
    }
    
    /// <summary>
    /// Finds the MiniGameAudioManager automatically
    /// </summary>
    private void FindAudioManager()
    {
        audioManager = MiniGameAudioManager.Instance;
        if (audioManager != null)
        {
            Debug.Log("TimerManager: Found MiniGameAudioManager automatically");
        }
        else
        {
            Debug.LogWarning("TimerManager: MiniGameAudioManager not found! Audio will be disabled.");
            useUnifiedAudio = false;
        }
    }
    
    void Update()
    {
        if (isRunning)
        {
            UpdateTimer();
        }
    }
    
    /// <summary>
    /// Start the timer
    /// </summary>
    public void StartTimer(float customTime = -1f)
    {
        currentTime = customTime > 0 ? customTime : MasterInitialTime;
        isRunning = true;
        warningShown = false;
        timeUpTriggered = false;
        
        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);
            
        Debug.Log($"Timer started with {currentTime} seconds (Master: {MasterInitialTime}s)");
    }
    
    /// <summary>
    /// Stop the timer
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);
            
        // Stop warning sound if playing
        if (useUnifiedAudio && audioManager != null && loopingWarningSource != null)
        {
            audioManager.StopLoopingSFX(loopingWarningSource);
            loopingWarningSource = null;
            warningSoundPlaying = false;
        }
        else if (warningAudioSource != null && warningSoundPlaying)
        {
            warningAudioSource.Stop();
            warningSoundPlaying = false;
        }
        
        Debug.Log("Timer stopped");
    }
    
    /// <summary>
    /// Pause the timer
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;
        Debug.Log("Timer paused");
    }
    
    /// <summary>
    /// Resume the timer
    /// </summary>
    public void ResumeTimer()
    {
        isRunning = true;
        Debug.Log("Timer resumed");
    }
    
    /// <summary>
    /// Reset the timer to initial time
    /// </summary>
    public void ResetTimer()
    {
        currentTime = MasterInitialTime;
        warningShown = false;
        timeUpTriggered = false;
        
        // Don't stop warning sound here - let it continue if warning is active
        // Only stop if we're resetting during a warning period and want to clear it
        
        UpdateTimerDisplay();
        Debug.Log($"Timer reset to {currentTime}s (Master: {MasterInitialTime}s)");
    }
    
    /// <summary>
    /// Add time to the timer
    /// </summary>
    public void AddTime(float secondsToAdd)
    {
        currentTime += secondsToAdd;
        UpdateTimerDisplay();
        OnTimeChanged?.Invoke(currentTime);
        Debug.Log($"Added {secondsToAdd} seconds to timer. New time: {currentTime}");
    }
    
    /// <summary>
    /// Subtract time from the timer
    /// </summary>
    public void SubtractTime(float secondsToSubtract)
    {
        currentTime -= secondsToSubtract;
        if (currentTime < 0) currentTime = 0;
        UpdateTimerDisplay();
        OnTimeChanged?.Invoke(currentTime);
        Debug.Log($"Subtracted {secondsToSubtract} seconds from timer. New time: {currentTime}");
    }
    
    /// <summary>
    /// Get remaining time
    /// </summary>
    public float GetRemainingTime()
    {
        return currentTime;
    }
    
    /// <summary>
    /// Get current time as formatted string
    /// </summary>
    public string GetFormattedTime()
    {
        return FormatTime(currentTime);
    }
    
    /// <summary>
    /// Get the master initial time from DynamicScoreManager
    /// </summary>
    public float GetMasterInitialTime()
    {
        return MasterInitialTime;
    }
    
    /// <summary>
    /// Get the master warning threshold from DynamicScoreManager
    /// </summary>
    public float GetMasterWarningThreshold()
    {
        return MasterWarningThreshold;
    }
    
    /// <summary>
    /// Get the master countdown mode from DynamicScoreManager
    /// </summary>
    public bool GetMasterCountDown()
    {
        return MasterCountDown;
    }
    
    /// <summary>
    /// Update timer logic
    /// </summary>
    private void UpdateTimer()
    {
        if (GetCountDownMode())
        {
            currentTime -= Time.deltaTime;
            
            // Check for warning threshold
            if (currentTime <= GetWarningThreshold() && !warningShown && currentTime > 0)
            {
                ShowWarning();
            }
            
            // Ensure warning sound continues during warning period
            if (warningShown && currentTime <= GetWarningThreshold() && currentTime > 0)
            {
                EnsureWarningSoundContinues();
            }
            
            // Check if time is up
            if (currentTime <= 0 && !timeUpTriggered)
            {
                currentTime = 0;
                TriggerTimeUp();
            }
        }
        else
        {
            currentTime += Time.deltaTime;
        }
        
        UpdateTimerDisplay();
        OnTimeChanged?.Invoke(currentTime);
    }
    
    /// <summary>
    /// Update timer display
    /// </summary>
    private void UpdateTimerDisplay()
    {
        string formattedTime = FormatTime(currentTime);
        
        // Update main timer text
        if (timerText != null)
        {
            timerText.text = formattedTime;
            
            // Change color based on warning state
            if (currentTime <= GetWarningThreshold() && GetCountDownMode())
            {
                timerText.color = warningColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }
        
        // Update additional timer texts
        if (additionalTimerTexts != null)
        {
            foreach (TextMeshProUGUI additionalText in additionalTimerTexts)
            {
                if (additionalText != null)
                {
                    additionalText.text = formattedTime;
                    
                    // Apply same color logic to additional texts
                    if (currentTime <= GetWarningThreshold() && GetCountDownMode())
                    {
                        additionalText.color = warningColor;
                    }
                    else
                    {
                        additionalText.color = normalColor;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Format time based on selected format
    /// </summary>
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

        switch (timeFormat.ToLower())
        {
            case "mm:ss":
                return string.Format("{0:00}:{1:00}", minutes, seconds);
            case "ss":
                return Mathf.CeilToInt(timeInSeconds).ToString();
            case "mm:ss.ms":
                int milliseconds = Mathf.FloorToInt((timeInSeconds % 1f) * 100f);
                return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
            default:
                return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    /// <summary>
    /// Show warning when time is running low
    /// </summary>
    private void ShowWarning()
    {
        warningShown = true;
        OnWarningTime?.Invoke();
        
        // Use unified audio system if available
        if (useUnifiedAudio && audioManager != null)
        {
            // Play looping warning sound through MiniGameAudioManager
            if (timeWarningSound != null)
            {
                // Stop any existing looping warning sound
                if (loopingWarningSource != null)
                {
                    audioManager.StopLoopingSFX(loopingWarningSource);
                    loopingWarningSource = null;
                }
                
                // Start new looping warning sound
                loopingWarningSource = audioManager.PlayLoopingSFX(timeWarningSound);
                warningSoundPlaying = (loopingWarningSource != null);
                Debug.Log("TimerManager: Looping warning sound started through MiniGameAudioManager");
            }
        }
        // Legacy audio system (deprecated)
        else if (warningAudioSource != null && timeWarningSound != null && !warningSoundPlaying)
        {
            warningAudioSource.clip = timeWarningSound;
            warningAudioSource.loop = true;
            warningAudioSource.Play();
            warningSoundPlaying = true;
            Debug.LogWarning("TimerManager: Using legacy audio system - consider switching to MiniGameAudioManager");
        }
        
        if (warningText != null)
        {
            warningText.text = "Time is running low!";
            warningText.gameObject.SetActive(true);
            
            // Start auto-hide coroutine if enabled
            if (autoHideWarning)
            {
                if (warningHideCoroutine != null)
                    StopCoroutine(warningHideCoroutine);
                warningHideCoroutine = StartCoroutine(AutoHideWarning());
            }
        }
        
        // Start continuous flashing for the warning period
        if (enableWarningFlash && timerText != null && warningCoroutine == null)
        {
            warningCoroutine = StartCoroutine(ContinuousFlashTimerText());
        }
        
        Debug.Log($"Warning: Time is running low! (Threshold: {GetWarningThreshold()}s)");
    }
    
    /// <summary>
    /// Check if warning sound should be playing and ensure it continues
    /// This method ensures the warning sound continues during the warning period
    /// </summary>
    private void EnsureWarningSoundContinues()
    {
        // Only check if we're in warning state
        if (!warningShown || currentTime <= 0) return;
        
        // Unified audio system
        if (useUnifiedAudio && audioManager != null)
        {
            // If warning sound should be playing but isn't, restart it
            if (!warningSoundPlaying && currentTime <= GetWarningThreshold() && currentTime > 0)
            {
                if (timeWarningSound != null)
                {
                    // Stop any existing looping warning sound
                    if (loopingWarningSource != null)
                    {
                        audioManager.StopLoopingSFX(loopingWarningSource);
                        loopingWarningSource = null;
                    }
                    
                    // Start new looping warning sound
                    loopingWarningSource = audioManager.PlayLoopingSFX(timeWarningSound);
                    warningSoundPlaying = (loopingWarningSource != null);
                    Debug.Log("TimerManager: Restarted looping warning sound through MiniGameAudioManager");
                }
            }
        }
        // Legacy audio system (deprecated)
        else if (warningAudioSource != null && timeWarningSound != null)
        {
            // If warning sound should be playing but isn't, restart it
            if (!warningSoundPlaying && currentTime <= GetWarningThreshold() && currentTime > 0)
            {
                warningAudioSource.clip = timeWarningSound;
                warningAudioSource.loop = true;
                warningAudioSource.Play();
                warningSoundPlaying = true;
                Debug.Log("TimerManager: Restarted warning sound to ensure continuity");
            }
        }
    }
    
    /// <summary>
    /// Auto-hide warning text after specified delay
    /// </summary>
    private IEnumerator AutoHideWarning()
    {
        yield return new WaitForSeconds(warningHideDelay);
        
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
            Debug.Log("Warning text auto-hidden");
        }
        
        warningHideCoroutine = null;
    }
    
    /// <summary>
    /// Trigger time up event
    /// </summary>
    private void TriggerTimeUp()
    {
        timeUpTriggered = true;
        isRunning = false;
        Debug.Log("Time's up!");
        
        // Don't stop warning sound immediately - let TimeExtensionManager handle it
        // The warning sound should continue until time extension is completed or closed
        Debug.Log("TimerManager: Warning sound continues for time extension");
        
        // Play time up sound through unified audio system
        if (useUnifiedAudio && audioManager != null)
        {
            if (timeUpSound != null)
            {
                audioManager.PlaySFX(timeUpSound);
                Debug.Log("TimerManager: Time up sound played through MiniGameAudioManager");
            }
        }
        // Legacy audio system (deprecated)
        else if (audioSource != null && timeUpSound != null)
        {
            audioSource.PlayOneShot(timeUpSound);
            Debug.LogWarning("TimerManager: Using legacy audio system for time up sound");
        }
        
        // Spawn VFX
        if (timeUpVFX != null)
        {
            Vector3 spawnPosition = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
            GameObject vfxInstance = Instantiate(timeUpVFX, spawnPosition, Quaternion.identity);
            
            // Ensure VFX is properly set up and visible
            if (vfxInstance != null)
            {
                // Get ParticleSystem component
                ParticleSystem particleSystem = vfxInstance.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    // Ensure the particle system is active and playing
                    particleSystem.gameObject.SetActive(true);
                    particleSystem.Play();
                    
                    // Set proper sorting order for visibility
                    ParticleSystemRenderer renderer = vfxInstance.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        renderer.sortingOrder = 1000;
                        renderer.sortingLayerName = "UI";
                    }
                    
                    Debug.Log($"TimerManager: Time up VFX spawned at {spawnPosition} and started playing");
                }
                else
                {
                    Debug.LogWarning("TimerManager: Time up VFX prefab has no ParticleSystem component!");
                }
                
                // Auto-destroy VFX after a reasonable time if it doesn't destroy itself
                Destroy(vfxInstance, 10f);
            }
            else
            {
                Debug.LogError("TimerManager: Failed to instantiate time up VFX!");
            }
        }
        else
        {
            Debug.LogWarning("TimerManager: No time up VFX assigned in inspector!");
        }
        
        // Show time-up UI panel
        if (timeUpPanel != null)
        {
            timeUpPanel.SetActive(true);
            Debug.Log("Time-up panel activated");
        }
        
        // Hide timer text if requested
        if (hideTimerOnTimeUp && timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
        
        // Hide additional timer texts if requested
        if (hideTimerOnTimeUp && additionalTimerTexts != null)
        {
            foreach (TextMeshProUGUI additionalText in additionalTimerTexts)
            {
                if (additionalText != null)
                {
                    additionalText.gameObject.SetActive(false);
                }
            }
        }
        
        // Trigger time up event
        OnTimeUp?.Invoke();
        
        // Stop warning flash if running
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }
        
        // Stop warning hide coroutine if running
        if (warningHideCoroutine != null)
        {
            StopCoroutine(warningHideCoroutine);
            warningHideCoroutine = null;
        }
        
        // Optional: Final flash effect
        if (timerText != null)
        {
            StartCoroutine(FinalFlashEffect());
        }
    }
    
    /// <summary>
    /// Continuous flash effect during warning
    /// </summary>
    private IEnumerator ContinuousFlashTimerText()
    {
        Color originalColor = timerText.color;
        Color warningColor = Color.red;
        bool isRed = false;
        
        while (isRunning && GetCountDownMode() && currentTime > 0 && currentTime <= GetWarningThreshold())
        {
            timerText.color = isRed ? warningColor : originalColor;
            isRed = !isRed;
            yield return new WaitForSeconds(warningFlashInterval);
        }
        timerText.color = originalColor;
        warningCoroutine = null;
    }
    
    /// <summary>
    /// Final flash effect when time is up
    /// </summary>
    private IEnumerator FinalFlashEffect()
    {
        Color originalColor = timerText.color;
        Color finalColor = Color.red;
        float flashDuration = 2f;
        float flashInterval = 0.2f;

        float elapsed = 0f;
        bool isRed = false;

        while (elapsed < flashDuration)
        {
            timerText.color = isRed ? finalColor : originalColor;
            isRed = !isRed;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        timerText.color = finalColor; // Keep it red at the end
    }
    
    /// <summary>
    /// Hide warning text
    /// </summary>
    public void HideWarning()
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
        
        // Stop auto-hide coroutine if running
        if (warningHideCoroutine != null)
        {
            StopCoroutine(warningHideCoroutine);
            warningHideCoroutine = null;
        }
    }
    
    /// <summary>
    /// Set the timer to a specific time
    /// </summary>
    public void SetTime(float time)
    {
        currentTime = time;
        UpdateTimerDisplay();
        OnTimeChanged?.Invoke(currentTime);
    }
    
    /// <summary>
    /// Get the initial time setting (from master source)
    /// </summary>
    public float GetInitialTime()
    {
        return MasterInitialTime;
    }
    
    /// <summary>
    /// Set the initial time (for next reset) - DEPRECATED: Use DynamicScoreManager instead
    /// </summary>
    [System.Obsolete("Use DynamicScoreManager.ConfigureTimer() instead")]
    public void SetInitialTime(float time)
    {
        Debug.LogWarning("TimerManager.SetInitialTime is deprecated. Use DynamicScoreManager.ConfigureTimer() instead.");
        initialTime = time; // Only affects local fallback
    }
    
    /// <summary>
    /// Get the warning threshold (from master source)
    /// </summary>
    public float GetWarningThreshold()
    {
        return MasterWarningThreshold;
    }
    
    /// <summary>
    /// Set the warning threshold - DEPRECATED: Use DynamicScoreManager instead
    /// </summary>
    [System.Obsolete("Use DynamicScoreManager.ConfigureTimer() instead")]
    public void SetWarningThreshold(float threshold)
    {
        Debug.LogWarning("TimerManager.SetWarningThreshold is deprecated. Use DynamicScoreManager.ConfigureTimer() instead.");
        warningThreshold = threshold; // Only affects local fallback
    }
    
    /// <summary>
    /// Get time percentage (0-1)
    /// </summary>
    public float GetTimePercentage()
    {
        if (GetCountDownMode())
        {
            return Mathf.Clamp01(currentTime / MasterInitialTime);
        }
        else
        {
            return Mathf.Clamp01(currentTime / MasterInitialTime);
        }
    }
    
    /// <summary>
    /// Hide the time-up panel
    /// </summary>
    public void HideTimeUpPanel()
    {
        if (timeUpPanel != null)
        {
            timeUpPanel.SetActive(false);
            Debug.Log("Time-up panel hidden");
        }
        
        // Show timer text again if it was hidden
        if (hideTimerOnTimeUp && timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }
        
        // Show additional timer texts again if they were hidden
        if (hideTimerOnTimeUp && additionalTimerTexts != null)
        {
            foreach (TextMeshProUGUI additionalText in additionalTimerTexts)
            {
                if (additionalText != null)
                {
                    additionalText.gameObject.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Stop the warning sound loop
    /// </summary>
    public void StopWarningSound()
    {
        // Stop unified audio system looping warning sound
        if (useUnifiedAudio && audioManager != null && loopingWarningSource != null)
        {
            audioManager.StopLoopingSFX(loopingWarningSource);
            loopingWarningSource = null;
            warningSoundPlaying = false;
            Debug.Log("TimerManager: Looping warning sound stopped through MiniGameAudioManager");
        }
        // Stop legacy warning sound if playing
        else if (warningAudioSource != null && warningSoundPlaying)
        {
            warningAudioSource.Stop();
            warningSoundPlaying = false;
            Debug.Log("TimerManager: Warning sound stopped");
        }
    }
    
    /// <summary>
    /// Stop the warning sound when time extension is completed
    /// This should be called when time is added or time extension panel is closed
    /// </summary>
    public void StopWarningSoundOnTimeExtensionComplete()
    {
        // Stop unified audio system looping warning sound
        if (useUnifiedAudio && audioManager != null && loopingWarningSource != null)
        {
            audioManager.StopLoopingSFX(loopingWarningSource);
            loopingWarningSource = null;
            warningSoundPlaying = false;
            Debug.Log("TimerManager: Looping warning sound stopped - time extension completed");
        }
        // Stop legacy warning sound if playing
        else if (warningAudioSource != null && warningSoundPlaying)
        {
            warningAudioSource.Stop();
            warningSoundPlaying = false;
            Debug.Log("TimerManager: Warning sound stopped - time extension completed");
        }
        
        // Reset warning state
        warningShown = false;
    }
    
    /// <summary>
    /// Legacy compatibility: Get time limit (same as master initial time)
    /// </summary>
    public float timeLimit => MasterInitialTime;
    
    /// <summary>
    /// Get countdown mode from master source
    /// </summary>
    private bool GetCountDownMode()
    {
        return MasterCountDown;
    }
    
    /// <summary>
    /// Configure the unified audio system
    /// </summary>
    public void ConfigureUnifiedAudio(bool useUnified, MiniGameAudioManager audioMgr = null)
    {
        useUnifiedAudio = useUnified;
        
        if (audioMgr != null)
        {
            audioManager = audioMgr;
        }
        else if (useUnifiedAudio && audioManager == null)
        {
            FindAudioManager();
        }
        
        Debug.Log($"TimerManager: Unified audio {(useUnifiedAudio ? "enabled" : "disabled")}");
        if (useUnifiedAudio)
        {
            Debug.Log($"TimerManager: Audio manager: {(audioManager != null ? audioManager.name : "NOT FOUND")}");
        }
    }
    
    /// <summary>
    /// Test the time up VFX
    /// </summary>
    [ContextMenu("Test Time Up VFX")]
    public void TestTimeUpVFX()
    {
        Debug.Log("=== Testing Time Up VFX ===");
        
        if (timeUpVFX != null)
        {
            Vector3 spawnPosition = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
            Debug.Log($"Spawning VFX at position: {spawnPosition}");
            
            GameObject vfxInstance = Instantiate(timeUpVFX, spawnPosition, Quaternion.identity);
            
            if (vfxInstance != null)
            {
                ParticleSystem particleSystem = vfxInstance.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.gameObject.SetActive(true);
                    particleSystem.Play();
                    
                    ParticleSystemRenderer renderer = vfxInstance.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        renderer.sortingOrder = 1000;
                        renderer.sortingLayerName = "UI";
                    }
                    
                    Debug.Log("✅ VFX spawned and playing successfully!");
                    Destroy(vfxInstance, 10f);
                }
                else
                {
                    Debug.LogError("❌ VFX prefab has no ParticleSystem component!");
                }
            }
            else
            {
                Debug.LogError("❌ Failed to instantiate VFX!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No time up VFX assigned in inspector!");
        }
        
        Debug.Log("=== End VFX Test ===");
    }
    
    /// <summary>
    /// Manually spawn time up VFX at a specific position
    /// </summary>
    public void SpawnTimeUpVFX(Vector3? customPosition = null)
    {
        if (timeUpVFX == null)
        {
            Debug.LogWarning("TimerManager: No time up VFX assigned!");
            return;
        }
        
        Vector3 spawnPosition = customPosition ?? (vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position);
        
        GameObject vfxInstance = Instantiate(timeUpVFX, spawnPosition, Quaternion.identity);
        
        if (vfxInstance != null)
        {
            ParticleSystem particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.Play();
                
                ParticleSystemRenderer renderer = vfxInstance.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 1000;
                    renderer.sortingLayerName = "UI";
                }
                
                Debug.Log($"TimerManager: Time up VFX manually spawned at {spawnPosition}");
                Destroy(vfxInstance, 10f);
            }
        }
    }
    
    /// <summary>
    /// Test the unified audio system
    /// </summary>
    [ContextMenu("Test Unified Audio")]
    public void TestUnifiedAudio()
    {
        Debug.Log("=== Testing Unified Audio System ===");
        
        if (useUnifiedAudio)
        {
            Debug.Log("✅ Unified audio is enabled");
            
            if (audioManager != null)
            {
                Debug.Log($"✅ Audio Manager: {audioManager.name}");
                
                // Test warning sound
                if (timeWarningSound != null)
                {
                    audioManager.PlaySFX(timeWarningSound);
                    Debug.Log("✅ Warning sound test played through MiniGameAudioManager");
                }
                else
                {
                    Debug.LogWarning("⚠️ No warning sound assigned");
                }
                
                // Test time up sound
                if (timeUpSound != null)
                {
                    audioManager.PlaySFX(timeUpSound);
                    Debug.Log("✅ Time up sound test played through MiniGameAudioManager");
                }
                else
                {
                    Debug.LogWarning("⚠️ No time up sound assigned");
                }
            }
            else
            {
                Debug.LogError("❌ Audio Manager is null!");
            }
        }
        else
        {
            Debug.Log("⚠️ Unified audio is disabled - using legacy system");
            Debug.Log($"Legacy Audio Source: {(audioSource != null ? audioSource.name : "NULL")}");
            Debug.Log($"Legacy Warning Audio Source: {(warningAudioSource != null ? warningAudioSource.name : "NULL")}");
        }
        
        Debug.Log("=== End Audio Test ===");
    }
    
    /// <summary>
    /// Get VFX system status
    /// </summary>
    public string GetVFXStatus()
    {
        string status = "=== TIMER VFX STATUS ===\n";
        status += $"Time Up VFX: {(timeUpVFX != null ? timeUpVFX.name : "NOT ASSIGNED")}\n";
        status += $"VFX Spawn Point: {(vfxSpawnPoint != null ? vfxSpawnPoint.name : "NOT ASSIGNED")}\n";
        
        if (timeUpVFX != null)
        {
            ParticleSystem particleSystem = timeUpVFX.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                status += $"Particle System: ✅ FOUND\n";
                status += $"  Duration: {particleSystem.main.duration}s\n";
                status += $"  Start Lifetime: {particleSystem.main.startLifetime.constant}s\n";
                status += $"  Max Particles: {particleSystem.main.maxParticles}\n";
            }
            else
            {
                status += $"Particle System: ❌ NOT FOUND\n";
            }
            
            ParticleSystemRenderer renderer = timeUpVFX.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                status += $"Renderer: ✅ FOUND\n";
                status += $"  Sorting Order: {renderer.sortingOrder}\n";
                status += $"  Sorting Layer: {renderer.sortingLayerName}\n";
            }
            else
            {
                status += $"Renderer: ❌ NOT FOUND\n";
            }
        }
        
        status += "=== END VFX STATUS ===";
        return status;
    }
    
    /// <summary>
    /// Print VFX status to console (for debugging)
    /// </summary>
    [ContextMenu("Print VFX Status")]
    public void PrintVFXStatus()
    {
        Debug.Log(GetVFXStatus());
    }
    
    /// <summary>
    /// Get audio system status
    /// </summary>
    public string GetAudioStatus()
    {
        string status = "=== TIMER AUDIO STATUS ===\n";
        status += $"Unified Audio: {(useUnifiedAudio ? "ENABLED" : "DISABLED")}\n";
        
        if (useUnifiedAudio)
        {
            status += $"Audio Manager: {(audioManager != null ? audioManager.name : "NULL")}\n";
            status += $"Warning Sound: {(timeWarningSound != null ? timeWarningSound.name : "NOT ASSIGNED")}\n";
            status += $"Time Up Sound: {(timeUpSound != null ? timeUpSound.name : "NOT ASSIGNED")}\n";
            status += $"Looping Warning Source: {(loopingWarningSource != null ? "ACTIVE" : "INACTIVE")}\n";
            status += $"Warning Sound Playing: {warningSoundPlaying}\n";
        }
        else
        {
            status += $"Legacy Audio Source: {(audioSource != null ? audioSource.name : "NULL")}\n";
            status += $"Legacy Warning Audio Source: {(warningAudioSource != null ? warningAudioSource.name : "NULL")}\n";
            status += $"Warning Sound Playing: {warningSoundPlaying}\n";
        }
        
        status += "=== END STATUS ===";
        return status;
    }
    
    /// <summary>
    /// Get timer configuration status from master source
    /// </summary>
    public string GetTimerConfigurationStatus()
    {
        string status = "=== TIMER CONFIGURATION STATUS ===\n";
        status += $"DynamicScoreManager: {(scoreManager != null ? scoreManager.name : "NOT FOUND")}\n";
        status += $"Master Initial Time: {MasterInitialTime}s\n";
        status += $"Master Warning Threshold: {MasterWarningThreshold}s\n";
        status += $"Master Count Down: {MasterCountDown}\n";
        status += $"Local Initial Time (Fallback): {initialTime}s\n";
        status += $"Local Warning Threshold (Fallback): {warningThreshold}s\n";
        status += $"Local Count Down (Fallback): {countDown}\n";
        status += $"Current Time: {currentTime:F1}s\n";
        status += $"Timer Running: {isRunning}\n";
        status += $"Warning Shown: {warningShown}\n";
        status += $"Warning Sound Playing: {warningSoundPlaying}\n";
        status += $"In Warning Period: {currentTime <= GetWarningThreshold() && currentTime > 0}\n";
        status += "=== END CONFIGURATION STATUS ===";
        return status;
    }
    
    /// <summary>
    /// Print timer configuration status to console (for debugging)
    /// </summary>
    [ContextMenu("Print Timer Configuration Status")]
    public void PrintTimerConfigurationStatus()
    {
        Debug.Log(GetTimerConfigurationStatus());
    }
    
    /// <summary>
    /// Test warning sound functionality
    /// </summary>
    [ContextMenu("Test Warning Sound")]
    public void TestWarningSound()
    {
        Debug.Log("=== Testing Warning Sound ===");
        
        if (timeWarningSound == null)
        {
            Debug.LogWarning("No warning sound assigned!");
            return;
        }
        
        // Test unified audio system
        if (useUnifiedAudio && audioManager != null)
        {
            // Stop any existing looping warning sound
            if (loopingWarningSource != null)
            {
                audioManager.StopLoopingSFX(loopingWarningSource);
                loopingWarningSource = null;
            }
            
            // Start new looping warning sound
            loopingWarningSource = audioManager.PlayLoopingSFX(timeWarningSound);
            warningSoundPlaying = (loopingWarningSource != null);
            Debug.Log("✅ Looping warning sound test started through MiniGameAudioManager");
        }
        // Test legacy audio system
        else if (warningAudioSource != null)
        {
            warningAudioSource.clip = timeWarningSound;
            warningAudioSource.loop = true;
            warningAudioSource.Play();
            warningSoundPlaying = true;
            Debug.Log("✅ Warning sound test started with legacy audio system");
        }
        else
        {
            Debug.LogError("❌ No audio system available for warning sound test");
        }
        
        Debug.Log("=== End Warning Sound Test ===");
    }
    
    /// <summary>
    /// Force start warning sound (for testing)
    /// </summary>
    [ContextMenu("Force Start Warning Sound")]
    public void ForceStartWarningSound()
    {
        Debug.Log("=== Force Starting Warning Sound ===");
        
        if (timeWarningSound == null)
        {
            Debug.LogWarning("No warning sound assigned!");
            return;
        }
        
        // Force start warning sound
        if (useUnifiedAudio && audioManager != null)
        {
            // Stop any existing looping warning sound
            if (loopingWarningSource != null)
            {
                audioManager.StopLoopingSFX(loopingWarningSource);
                loopingWarningSource = null;
            }
            
            // Start new looping warning sound
            loopingWarningSource = audioManager.PlayLoopingSFX(timeWarningSound);
            warningSoundPlaying = (loopingWarningSource != null);
            Debug.Log("✅ Warning sound force started through MiniGameAudioManager");
        }
        else if (warningAudioSource != null)
        {
            warningAudioSource.clip = timeWarningSound;
            warningAudioSource.loop = true;
            warningAudioSource.Play();
            warningSoundPlaying = true;
            Debug.Log("✅ Warning sound force started with legacy audio system");
        }
        
        // Set warning state
        warningShown = true;
        
        Debug.Log("=== End Force Start Warning Sound ===");
    }
    
    /// <summary>
    /// Test the looping warning sound system
    /// </summary>
    [ContextMenu("Test Looping Warning Sound")]
    public void TestLoopingWarningSound()
    {
        Debug.Log("=== Testing Looping Warning Sound System ===");
        
        if (timeWarningSound == null)
        {
            Debug.LogWarning("No warning sound assigned!");
            return;
        }
        
        if (useUnifiedAudio && audioManager != null)
        {
            Debug.Log("✅ Testing unified audio system looping warning sound");
            
            // Start looping warning sound
            if (loopingWarningSource != null)
            {
                audioManager.StopLoopingSFX(loopingWarningSource);
                loopingWarningSource = null;
            }
            
            loopingWarningSource = audioManager.PlayLoopingSFX(timeWarningSound);
            warningSoundPlaying = (loopingWarningSource != null);
            
            if (warningSoundPlaying)
            {
                Debug.Log("✅ Looping warning sound started successfully");
                Debug.Log($"✅ Audio Source: {loopingWarningSource.name}");
                Debug.Log($"✅ Is Playing: {loopingWarningSource.isPlaying}");
                Debug.Log($"✅ Loop: {loopingWarningSource.loop}");
                
                // Stop after 3 seconds for testing
                StartCoroutine(StopWarningSoundAfterDelay(3f));
            }
            else
            {
                Debug.LogError("❌ Failed to start looping warning sound");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Unified audio system not available - test skipped");
        }
        
        Debug.Log("=== End Looping Warning Sound Test ===");
    }
    
    /// <summary>
    /// Coroutine to stop warning sound after a delay (for testing)
    /// </summary>
    private IEnumerator StopWarningSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (useUnifiedAudio && audioManager != null && loopingWarningSource != null)
        {
            audioManager.StopLoopingSFX(loopingWarningSource);
            loopingWarningSource = null;
            warningSoundPlaying = false;
            Debug.Log("✅ Test warning sound stopped after delay");
        }
    }
    
    /// <summary>
    /// Verify warning sound system is working correctly
    /// </summary>
    [ContextMenu("Verify Warning Sound System")]
    public void VerifyWarningSoundSystem()
    {
        Debug.Log("=== VERIFYING WARNING SOUND SYSTEM ===");
        
        // Check basic setup
        Debug.Log($"✅ Time Warning Sound: {(timeWarningSound != null ? timeWarningSound.name : "NOT ASSIGNED")}");
        Debug.Log($"✅ Use Unified Audio: {useUnifiedAudio}");
        Debug.Log($"✅ Audio Manager: {(audioManager != null ? audioManager.name : "NOT FOUND")}");
        Debug.Log($"✅ Warning Threshold: {GetWarningThreshold()}s");
        
        // Check current state
        Debug.Log($"✅ Warning Shown: {warningShown}");
        Debug.Log($"✅ Warning Sound Playing: {warningSoundPlaying}");
        Debug.Log($"✅ Looping Warning Source: {(loopingWarningSource != null ? "ACTIVE" : "INACTIVE")}");
        
        // Test audio system
        if (useUnifiedAudio && audioManager != null)
        {
            Debug.Log("✅ Unified audio system is ready");
            
            // Test if we can create a looping audio source
            if (timeWarningSound != null)
            {
                AudioSource testSource = audioManager.PlayLoopingSFX(timeWarningSound);
                if (testSource != null)
                {
                    Debug.Log("✅ Looping audio source creation successful");
                    Debug.Log($"✅ Test Source: {testSource.name}");
                    Debug.Log($"✅ Test Source Loop: {testSource.loop}");
                    Debug.Log($"✅ Test Source Playing: {testSource.isPlaying}");
                    
                    // Stop test source
                    audioManager.StopLoopingSFX(testSource);
                    Debug.Log("✅ Test audio source stopped and returned to pool");
                }
                else
                {
                    Debug.LogError("❌ Failed to create looping audio source");
                }
            }
        }
        else if (warningAudioSource != null)
        {
            Debug.Log("✅ Legacy audio system is ready");
            Debug.Log($"✅ Warning Audio Source: {warningAudioSource.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ No audio system available");
        }
        
        Debug.Log("=== END VERIFICATION ===");
    }
} 