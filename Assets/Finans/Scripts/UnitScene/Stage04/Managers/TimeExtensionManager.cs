using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;

/// <summary>
/// Robust Time Extension Manager that properly integrates with TimerManager and DynamicScoreManager
/// Uses event-driven architecture instead of manual Update() loops for better reliability
/// </summary>
public class TimeExtensionManager : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("Reference to TimerManager - will auto-find if not assigned")]
    public TimerManager timerManager;
    [Tooltip("Reference to MinigameScoreManager - will auto-find if not assigned")]
    public MinigameScoreManager scoreManager;
    [Tooltip("Reference to MiniGameAudioManager - will auto-find if not assigned")]
    public MiniGameAudioManager audioManager;

    [Header("Time Extension Settings")]
    [Range(10, 300)]
    public int adRewardTimeSeconds = 60;
    [Range(10, 600)]
    public int purchaseRewardTimeSeconds = 120;
    [Tooltip("DEPRECATED: Use DynamicScoreManager.warningThreshold instead")]
    [Range(5f, 60f)]
    public float warningTimeThreshold = 30f; // DEPRECATED - Use DynamicScoreManager instead

    [Header("Purchase Settings")]
    [Tooltip("Price to display for time purchase (e.g., $0.99, $1.99, Free)")]
    public string purchasePrice = "$0.99";

    [Header("Text Messages")]
    [Tooltip("Ad reward message template (use {0} for time amount)")]
    public string adRewardMessage = "Watch Ad for +{0} seconds!";
    [Tooltip("Purchase reward message template (use {0} for time amount, {1} for price)")]
    public string purchaseRewardMessage = "Buy +{0} seconds for {1}!";
    [Tooltip("Time remaining message template (use {0} for minutes, {1} for seconds)")]
    public string timeRemainingMessage = "Time Remaining: {0:00}:{1:00}";
    [Tooltip("Ad loading message")]
    public string adLoadingMessage = "Loading Ad...";
    [Tooltip("Ad success message template (use {0} for time amount)")]
    public string adSuccessMessage = "Ad watched! +{0} seconds added!";
    [Tooltip("Ad failed message")]
    public string adFailedMessage = "Ad failed to load. Try again!";
    [Tooltip("Purchase processing message")]
    public string purchaseProcessingMessage = "Processing Purchase...";
    [Tooltip("Purchase success message template (use {0} for time amount)")]
    public string purchaseSuccessMessage = "Purchase successful! +{0} seconds added!";
    [Tooltip("Purchase failed message")]
    public string purchaseFailedMessage = "Purchase failed. Try again!";

    [Header("Configurable TextMeshPro Fields")]
    [Tooltip("TextMeshPro field for ad reward message - will override template if assigned")]
    public TextMeshProUGUI adRewardTextMeshPro;
    [Tooltip("TextMeshPro field for purchase reward message - will override template if assigned")]
    public TextMeshProUGUI purchaseRewardTextMeshPro;
    [Tooltip("TextMeshPro field for time remaining message - will override template if assigned")]
    public TextMeshProUGUI timeRemainingTextMeshPro;
    [Tooltip("TextMeshPro field for ad loading message - will override template if assigned")]
    public TextMeshProUGUI adLoadingTextMeshPro;
    [Tooltip("TextMeshPro field for ad success message - will override template if assigned")]
    public TextMeshProUGUI adSuccessTextMeshPro;
    [Tooltip("TextMeshPro field for ad failed message - will override template if assigned")]
    public TextMeshProUGUI adFailedTextMeshPro;
    [Tooltip("TextMeshPro field for purchase processing message - will override template if assigned")]
    public TextMeshProUGUI purchaseProcessingTextMeshPro;
    [Tooltip("TextMeshPro field for purchase success message - will override template if assigned")]
    public TextMeshProUGUI purchaseSuccessTextMeshPro;
    [Tooltip("TextMeshPro field for purchase failed message - will override template if assigned")]
    public TextMeshProUGUI purchaseFailedTextMeshPro;

    [Header("UI References")]
    [Tooltip("The main time extension panel GameObject")]
    public GameObject timeExtensionPanel;
    [Tooltip("Text showing ad reward information")]
    public TextMeshProUGUI adRewardText;
    [Tooltip("Text showing purchase reward information")]
    public TextMeshProUGUI purchaseRewardText;
    [Tooltip("Text showing remaining time")]
    public TextMeshProUGUI remainingTimeText;
    [Tooltip("Text for status messages")]
    public TextMeshProUGUI statusMessageText;

    [Header("Buttons")]
    [Tooltip("Button to watch ad for time")]
    public UnityEngine.UI.Button watchAdButton;
    [Tooltip("Button to buy time")]
    public UnityEngine.UI.Button buyTimeButton;
    [Tooltip("Button to close panel")]
    public UnityEngine.UI.Button closeButton;

    [Header("Audio")]
    [Tooltip("Sound when time is added")]
    public AudioClip timeAddedSound;
    [Tooltip("Sound when ad is watched")]
    public AudioClip adWatchedSound;
    [Tooltip("Sound when purchase is completed")]
    public AudioClip purchaseCompletedSound;
    [Tooltip("Sound for button clicks")]
    public AudioClip buttonClickSound;

    [Header("Events")]
    [Tooltip("Called when time is successfully added")]
    public UnityEvent<int> OnTimeAdded;
    [Tooltip("Called when ad is successfully watched")]
    public UnityEvent OnAdWatched;
    [Tooltip("Called when purchase is completed")]
    public UnityEvent OnPurchaseCompleted;
    [Tooltip("Called when time extension is requested")]
    public UnityEvent OnTimeExtensionRequested;
    [Tooltip("Called when time extension panel is closed")]
    public UnityEvent OnTimeExtensionClosed;

    [Header("Configuration")]
    [Tooltip("Automatically find managers if not assigned")]
    public bool autoFindManagers = true;
    [Tooltip("Show time extension on warning time (recommended)")]
    public bool showOnWarningTime = true;
    [Tooltip("Show time extension when time runs out")]
    public bool showOnTimeUp = true;
    [Tooltip("Automatically close when game is completed")]
    public bool autoCloseOnGameComplete = true;

    // Private state
    private bool isTimeExtensionActive = false;
    private bool isAdLoading = false;
    private bool isPurchaseProcessing = false;
    private bool isInitialized = false;

    // Audio state management
    private bool mainAudioWasPlaying = false;
    private float mainAudioOriginalVolume = 1f;

    void Awake()
    {
        // Ensure only one instance exists
        if (Object.FindObjectsByType<TimeExtensionManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeTimeExtensionManager();
    }

    /// <summary>
    /// Initialize the time extension manager
    /// </summary>
    private void InitializeTimeExtensionManager()
    {
        if (isInitialized) return;

        //        Debug.Log("TimeExtensionManager: Initializing...");

        // Find managers if not assigned
        if (autoFindManagers)
        {
            FindManagers();
        }

        // Validate required components
        if (!ValidateComponents())
        {
            Debug.LogError("TimeExtensionManager: Critical components missing! Check inspector assignments.");
            return;
        }

        // Setup event listeners
        SetupEventListeners();

        // Initialize UI
        InitializeUI();

        // Configure warning threshold synchronization
        SyncWarningThreshold();

        isInitialized = true;
        Debug.Log("TimeExtensionManager: Initialization complete!");

        // Log current status
        LogCurrentStatus();
    }

    /// <summary>
    /// Find required managers automatically
    /// </summary>
    private void FindManagers()
    {
        // Find TimerManager
        if (timerManager == null)
        {
            timerManager = FindFirstObjectByType<TimerManager>();
            if (timerManager != null)
                Debug.Log("TimeExtensionManager: Auto-found TimerManager");
        }

        // Find DynamicScoreManager
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<MinigameScoreManager>();
            if (scoreManager != null)
                Debug.Log("TimeExtensionManager: Auto-found DynamicScoreManager");
        }

        // Find AudioManager
        if (audioManager == null)
        {
            audioManager = MiniGameAudioManager.Instance;
            if (audioManager != null)
                Debug.Log("TimeExtensionManager: Auto-found MiniGameAudioManager");
        }
    }

    /// <summary>
    /// Validate that all required components are assigned
    /// </summary>
    private bool ValidateComponents()
    {
        bool isValid = true;

        if (timerManager == null)
        {
            Debug.LogError("TimeExtensionManager: TimerManager is required!");
            isValid = false;
        }

        if (timeExtensionPanel == null)
        {
            Debug.LogError("TimeExtensionManager: Time Extension Panel is required!");
            isValid = false;
        }

        if (watchAdButton == null)
        {
            Debug.LogWarning("TimeExtensionManager: Watch Ad Button not assigned - ad functionality disabled");
        }

        if (buyTimeButton == null)
        {
            Debug.LogWarning("TimeExtensionManager: Buy Time Button not assigned - purchase functionality disabled");
        }

        return isValid;
    }

    /// <summary>
    /// Setup event listeners for proper integration
    /// </summary>
    private void SetupEventListeners()
    {
        if (timerManager != null)
        {
            // Listen for warning time event
            if (showOnWarningTime)
            {
                timerManager.OnWarningTime.AddListener(OnTimerWarning);
                //                Debug.Log("TimeExtensionManager: Listening for TimerManager warning events");
            }

            // Listen for time up event
            if (showOnTimeUp)
            {
                timerManager.OnTimeUp.AddListener(OnTimerTimeUp);
                //              Debug.Log("TimeExtensionManager: Listening for TimerManager time up events");
            }
        }

        if (scoreManager != null)
        {
            // Listen for game completion events
            // Note: This would require adding an event to DynamicScoreManager
            //      Debug.Log("TimeExtensionManager: DynamicScoreManager found - will monitor game state");
        }
    }

    /// <summary>
    /// Initialize UI elements and button listeners
    /// </summary>
    private void InitializeUI()
    {
        // Ensure panel starts hidden
        if (timeExtensionPanel != null)
        {
            timeExtensionPanel.SetActive(false);
        }

        // Setup button listeners
        if (watchAdButton != null)
        {
            watchAdButton.onClick.RemoveAllListeners();
            watchAdButton.onClick.AddListener(WatchAdForTime);
        }

        if (buyTimeButton != null)
        {
            buyTimeButton.onClick.RemoveAllListeners();
            buyTimeButton.onClick.AddListener(BuyTime);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseTimeExtension);
        }

        // Update reward texts
        UpdateRewardTexts();
    }

    /// <summary>
    /// Synchronize warning threshold with TimerManager
    /// </summary>
    private void SyncWarningThreshold()
    {
        if (timerManager != null)
        {
            // Use TimerManager's warning threshold as source of truth
            float timerWarningThreshold = timerManager.GetWarningThreshold();
            if (timerWarningThreshold > 0)
            {
                // No longer storing duplicate value - just log for debugging
                Debug.Log($"TimeExtensionManager: Warning threshold synchronized with TimerManager: {timerWarningThreshold}s");
            }
        }
    }

    /// <summary>
    /// Called when TimerManager reaches warning threshold
    /// </summary>
    private void OnTimerWarning()
    {
        Debug.Log("TimeExtensionManager: Timer warning event received!");

        if (!CanShowTimeExtension())
        {
            Debug.Log("TimeExtensionManager: Cannot show time extension - conditions not met");
            return;
        }

        Debug.Log("TimeExtensionManager: Showing time extension panel due to warning time");
        ShowTimeExtensionPanel();
    }

    /// <summary>
    /// Called when TimerManager runs out of time
    /// </summary>
    private void OnTimerTimeUp()
    {
        Debug.Log("TimeExtensionManager: Timer time up event received!");

        if (!CanShowTimeExtension())
        {
            Debug.Log("TimeExtensionManager: Cannot show time extension - conditions not met");
            return;
        }

        Debug.Log("TimeExtensionManager: Showing time extension panel due to time up");
        ShowTimeExtensionPanel();
    }

    /// <summary>
    /// Check if it's safe to show the time extension panel
    /// </summary>
    private bool CanShowTimeExtension()
    {
        // Don't show if already active
        if (isTimeExtensionActive)
        {
            Debug.Log("TimeExtensionManager: Panel already active");
            return false;
        }

        // Don't show if game is completed
        if (IsGameCompleted())
        {
            Debug.Log("TimeExtensionManager: Game is completed");
            return false;
        }

        // Don't show if timer is not running
        if (timerManager == null || !timerManager.IsRunning)
        {
            Debug.Log("TimeExtensionManager: Timer is not running");
            return false;
        }

        // Don't show if panel is not assigned
        if (timeExtensionPanel == null)
        {
            Debug.LogError("TimeExtensionManager: Panel not assigned!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if the game is currently completed
    /// </summary>
    private bool IsGameCompleted()
    {
        // Check score manager first
        if (scoreManager != null)
        {
            return scoreManager.IsGameCompleted();
        }

        // Fallback: check if timer is stopped or time is up
        if (timerManager != null)
        {
            return timerManager.IsTimeUp || !timerManager.IsRunning;
        }

        return false;
    }

    /// <summary>
    /// Show the time extension panel
    /// </summary>
    private void ShowTimeExtensionPanel()
    {
        if (!CanShowTimeExtension()) return;

        Debug.Log("TimeExtensionManager: Showing time extension panel");

        // Set state
        isTimeExtensionActive = true;

        // Show panel
        if (timeExtensionPanel != null)
        {
            timeExtensionPanel.SetActive(true);
        }

        // Update UI
        UpdateRemainingTimeDisplay();

        // Trigger events
        OnTimeExtensionRequested?.Invoke();

        // Play sound
        PlaySound(timeAddedSound);

        Debug.Log("TimeExtensionManager: Panel shown successfully");
    }

    /// <summary>
    /// Close the time extension panel
    /// </summary>
    public void CloseTimeExtension()
    {
        if (!isTimeExtensionActive) return;

        Debug.Log("TimeExtensionManager: Closing time extension panel");

        // Hide panel
        if (timeExtensionPanel != null)
        {
            timeExtensionPanel.SetActive(false);
        }

        // Stop warning sound when panel is closed
        if (timerManager != null)
        {
            timerManager.StopWarningSoundOnTimeExtensionComplete();
        }

        // Reset state
        isTimeExtensionActive = false;
        isAdLoading = false;
        isPurchaseProcessing = false;

        // Trigger events
        OnTimeExtensionClosed?.Invoke();

        Debug.Log("TimeExtensionManager: Panel closed successfully");
    }

    /// <summary>
    /// Watch ad to get time reward
    /// </summary>
    public void WatchAdForTime()
    {
        if (isAdLoading || !enableAdRewards) return;

        Debug.Log("TimeExtensionManager: Starting ad watch process");

        isAdLoading = true;

        // Disable button
        if (watchAdButton != null)
        {
            watchAdButton.interactable = false;
        }

        // Update status
        if (adLoadingTextMeshPro != null)
        {
            adLoadingTextMeshPro.text = adLoadingMessage;
        }
        else if (statusMessageText != null)
        {
            statusMessageText.text = adLoadingMessage;
        }

        // Play sound
        PlaySound(buttonClickSound);

        // Start ad simulation
        StartCoroutine(SimulateAdWatch());
    }

    /// <summary>
    /// Buy time directly
    /// </summary>
    public void BuyTime()
    {
        if (isPurchaseProcessing || !enablePurchaseRewards) return;

        Debug.Log("TimeExtensionManager: Starting purchase process");

        isPurchaseProcessing = true;

        // Disable button
        if (buyTimeButton != null)
        {
            buyTimeButton.interactable = false;
        }

        // Update status
        if (purchaseProcessingTextMeshPro != null)
        {
            purchaseProcessingTextMeshPro.text = purchaseProcessingMessage;
        }
        else if (statusMessageText != null)
        {
            statusMessageText.text = purchaseProcessingMessage;
        }

        // Play sound
        PlaySound(buttonClickSound);

        // Start purchase simulation
        StartCoroutine(SimulatePurchase());
    }

    /// <summary>
    /// Simulate ad watching process
    /// </summary>
    private IEnumerator SimulateAdWatch()
    {
        Debug.Log("TimeExtensionManager: Simulating ad watch...");

        // Simulate ad loading
        yield return new WaitForSeconds(1f);

        // Simulate ad watching
        yield return new WaitForSeconds(3f);

        // Simulate success (80% chance)
        bool success = Random.Range(0f, 1f) < 0.8f;

        if (success)
        {
            Debug.Log("TimeExtensionManager: Ad watched successfully");

            // Add time
            AddTime(adRewardTimeSeconds);

            // Trigger events
            OnAdWatched?.Invoke();

            // Play sound
            PlaySound(adWatchedSound);

            // Update status
            if (adSuccessTextMeshPro != null)
            {
                adSuccessTextMeshPro.text = string.Format(adSuccessMessage, adRewardTimeSeconds);
            }
            else if (statusMessageText != null)
            {
                statusMessageText.text = string.Format(adSuccessMessage, adRewardTimeSeconds);
            }

            // Close panel after delay
            yield return new WaitForSeconds(1.5f);
            CloseTimeExtension();
        }
        else
        {
            Debug.Log("TimeExtensionManager: Ad failed to load");

            // Update status
            if (adFailedTextMeshPro != null)
            {
                adFailedTextMeshPro.text = adFailedMessage;
            }
            else if (statusMessageText != null)
            {
                statusMessageText.text = adFailedMessage;
            }

            // Re-enable button
            if (watchAdButton != null)
            {
                watchAdButton.interactable = true;
            }
        }

        isAdLoading = false;
    }

    /// <summary>
    /// Simulate purchase process
    /// </summary>
    private IEnumerator SimulatePurchase()
    {
        Debug.Log("TimeExtensionManager: Simulating purchase...");

        // Simulate processing
        yield return new WaitForSeconds(1.5f);

        // Simulate success (95% chance)
        bool success = Random.Range(0f, 1f) < 0.95f;

        if (success)
        {
            Debug.Log("TimeExtensionManager: Purchase successful");

            // Add time
            AddTime(purchaseRewardTimeSeconds);

            // Trigger events
            OnPurchaseCompleted?.Invoke();

            // Play sound
            PlaySound(purchaseCompletedSound);

            // Update status
            if (purchaseSuccessTextMeshPro != null)
            {
                purchaseSuccessTextMeshPro.text = string.Format(purchaseSuccessMessage, purchaseRewardTimeSeconds);
            }
            else if (statusMessageText != null)
            {
                statusMessageText.text = string.Format(purchaseSuccessMessage, purchaseRewardTimeSeconds);
            }

            // Close panel after delay
            yield return new WaitForSeconds(1.5f);
            CloseTimeExtension();
        }
        else
        {
            Debug.Log("TimeExtensionManager: Purchase failed");

            // Update status
            if (purchaseFailedTextMeshPro != null)
            {
                purchaseFailedTextMeshPro.text = purchaseFailedMessage;
            }
            else if (statusMessageText != null)
            {
                statusMessageText.text = purchaseFailedMessage;
            }

            // Re-enable button
            if (buyTimeButton != null)
            {
                buyTimeButton.interactable = true;
            }
        }

        isPurchaseProcessing = false;
    }

    /// <summary>
    /// Add time to the timer
    /// </summary>
    private void AddTime(int secondsToAdd)
    {
        if (timerManager == null)
        {
            Debug.LogError("TimeExtensionManager: TimerManager not found! Cannot add time.");
            return;
        }

        Debug.Log($"TimeExtensionManager: Adding {secondsToAdd} seconds to timer");

        // Stop warning sound when time extension is completed
        timerManager.StopWarningSoundOnTimeExtensionComplete();

        // Add time to timer
        timerManager.AddTime(secondsToAdd);

        // Trigger events
        OnTimeAdded?.Invoke(secondsToAdd);

        // Play sound
        PlaySound(timeAddedSound);

        Debug.Log($"TimeExtensionManager: Successfully added {secondsToAdd} seconds");
    }

    /// <summary>
    /// Update the remaining time display
    /// </summary>
    private void UpdateRemainingTimeDisplay()
    {
        if (timerManager == null) return;

        float remainingTime = timerManager.GetRemainingTime();
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        string formattedTime = string.Format(timeRemainingMessage, minutes, seconds);

        // Update TextMeshPro field if assigned, otherwise fall back to legacy field
        if (timeRemainingTextMeshPro != null)
        {
            timeRemainingTextMeshPro.text = formattedTime;
        }
        else if (remainingTimeText != null)
        {
            remainingTimeText.text = formattedTime;
        }
    }

    /// <summary>
    /// Update reward text displays
    /// </summary>
    private void UpdateRewardTexts()
    {
        // Update ad reward text
        if (adRewardTextMeshPro != null)
        {
            adRewardTextMeshPro.text = string.Format(adRewardMessage, adRewardTimeSeconds);
        }
        else if (adRewardText != null)
        {
            adRewardText.text = string.Format(adRewardMessage, adRewardTimeSeconds);
        }

        // Update purchase reward text
        if (purchaseRewardTextMeshPro != null)
        {
            purchaseRewardTextMeshPro.text = string.Format(purchaseRewardMessage, purchaseRewardTimeSeconds, purchasePrice);
        }
        else if (purchaseRewardText != null)
        {
            purchaseRewardText.text = string.Format(purchaseRewardMessage, purchaseRewardTimeSeconds, purchasePrice);
        }
    }

    /// <summary>
    /// Play sound effect
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        // Use unified audio system if available
        if (audioManager != null)
        {
            audioManager.PlaySFX(clip);
        }
        else
        {
            // Fallback to local AudioSource
            AudioSource localSource = GetComponent<AudioSource>();
            if (localSource == null)
            {
                localSource = gameObject.AddComponent<AudioSource>();
            }
            localSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Force show time extension panel (for testing or manual control)
    /// </summary>
    public void ForceShowTimeExtension()
    {
        Debug.Log("TimeExtensionManager: Force showing time extension panel");

        if (isTimeExtensionActive)
        {
            CloseTimeExtension();
        }

        ShowTimeExtensionPanel();
    }

    /// <summary>
    /// Close time extension when game is completed
    /// </summary>
    public void CloseOnGameComplete()
    {
        if (isTimeExtensionActive)
        {
            Debug.Log("TimeExtensionManager: Closing panel due to game completion");
            CloseTimeExtension();
        }
    }

    /// <summary>
    /// Update the purchase price and refresh the UI
    /// </summary>
    public void UpdatePurchasePrice(string newPrice)
    {
        purchasePrice = newPrice;
        Debug.Log($"TimeExtensionManager: Purchase price updated to {newPrice}");

        // Refresh the UI if panel is active
        if (isTimeExtensionActive && purchaseRewardText != null)
        {
            UpdateRewardTexts();
        }
    }

    /// <summary>
    /// Update ad reward message and refresh the UI
    /// </summary>
    public void UpdateAdRewardMessage(string newMessage)
    {
        adRewardMessage = newMessage;
        Debug.Log($"TimeExtensionManager: Ad reward message updated to: {newMessage}");

        // Refresh the UI if panel is active
        if (isTimeExtensionActive && adRewardText != null)
        {
            UpdateRewardTexts();
        }
    }

    /// <summary>
    /// Update purchase reward message and refresh the UI
    /// </summary>
    public void UpdatePurchaseRewardMessage(string newMessage)
    {
        purchaseRewardMessage = newMessage;
        Debug.Log($"TimeExtensionManager: Purchase reward message updated to: {newMessage}");

        // Refresh the UI if panel is active
        if (isTimeExtensionActive && purchaseRewardText != null)
        {
            UpdateRewardTexts();
        }
    }

    /// <summary>
    /// Update time remaining message format
    /// </summary>
    public void UpdateTimeRemainingMessage(string newMessage)
    {
        timeRemainingMessage = newMessage;
        Debug.Log($"TimeExtensionManager: Time remaining message updated to: {newMessage}");

        // Refresh the UI if panel is active
        if (isTimeExtensionActive && remainingTimeText != null)
        {
            UpdateRemainingTimeDisplay();
        }
    }

    /// <summary>
    /// Update ad-related messages
    /// </summary>
    public void UpdateAdMessages(string loadingMessage, string successMessage, string failedMessage)
    {
        adLoadingMessage = loadingMessage;
        adSuccessMessage = successMessage;
        adFailedMessage = failedMessage;
        Debug.Log("TimeExtensionManager: Ad messages updated");
    }

    /// <summary>
    /// Update purchase-related messages
    /// </summary>
    public void UpdatePurchaseMessages(string processingMessage, string successMessage, string failedMessage)
    {
        purchaseProcessingMessage = processingMessage;
        purchaseSuccessMessage = successMessage;
        purchaseFailedMessage = failedMessage;
        Debug.Log("TimeExtensionManager: Purchase messages updated");
    }

    /// <summary>
    /// Update TextMeshPro field assignments
    /// </summary>
    public void UpdateTextMeshProFields(
        TextMeshProUGUI adReward = null,
        TextMeshProUGUI purchaseReward = null,
        TextMeshProUGUI timeRemaining = null,
        TextMeshProUGUI adLoading = null,
        TextMeshProUGUI adSuccess = null,
        TextMeshProUGUI adFailed = null,
        TextMeshProUGUI purchaseProcessing = null,
        TextMeshProUGUI purchaseSuccess = null,
        TextMeshProUGUI purchaseFailed = null)
    {
        if (adReward != null) adRewardTextMeshPro = adReward;
        if (purchaseReward != null) purchaseRewardTextMeshPro = purchaseReward;
        if (timeRemaining != null) timeRemainingTextMeshPro = timeRemaining;
        if (adLoading != null) adLoadingTextMeshPro = adLoading;
        if (adSuccess != null) adSuccessTextMeshPro = adSuccess;
        if (adFailed != null) adFailedTextMeshPro = adFailed;
        if (purchaseProcessing != null) purchaseProcessingTextMeshPro = purchaseProcessing;
        if (purchaseSuccess != null) purchaseSuccessTextMeshPro = purchaseSuccess;
        if (purchaseFailed != null) purchaseFailedTextMeshPro = purchaseFailed;

        Debug.Log("TimeExtensionManager: TextMeshPro fields updated");

        // Refresh UI if panel is active
        if (isTimeExtensionActive)
        {
            UpdateRewardTexts();
            UpdateRemainingTimeDisplay();
        }
    }

    /// <summary>
    /// Get current status for debugging
    /// </summary>
    public string GetStatus()
    {
        string status = "=== TIME EXTENSION STATUS ===\n";
        status += $"Initialized: {isInitialized}\n";
        status += $"Panel Active: {isTimeExtensionActive}\n";
        status += $"TimerManager: {(timerManager != null ? timerManager.name : "NULL")}\n";
        status += $"ScoreManager: {(scoreManager != null ? scoreManager.name : "NULL")}\n";
        status += $"AudioManager: {(audioManager != null ? audioManager.name : "NULL")}\n";
        status += $"Warning Threshold: {(timerManager != null ? timerManager.GetWarningThreshold() : "NOT AVAILABLE")}s (from TimerManager)\n";
        status += $"Show On Warning: {showOnWarningTime}\n";
        status += $"Show On Time Up: {showOnTimeUp}\n";
        status += $"Purchase Price: {purchasePrice}\n";
        status += $"\n--- Text Messages ---\n";
        status += $"Ad Reward: {adRewardMessage}\n";
        status += $"Purchase Reward: {purchaseRewardMessage}\n";
        status += $"Time Remaining: {timeRemainingMessage}\n";
        status += $"Ad Loading: {adLoadingMessage}\n";
        status += $"Ad Success: {adSuccessMessage}\n";
        status += $"Ad Failed: {adFailedMessage}\n";
        status += $"Purchase Processing: {purchaseProcessingMessage}\n";
        status += $"Purchase Success: {purchaseSuccessMessage}\n";
        status += $"Purchase Failed: {purchaseFailedMessage}\n";
        status += $"\n--- TextMeshPro Fields ---\n";
        status += $"Ad Reward TMP: {(adRewardTextMeshPro != null ? adRewardTextMeshPro.name : "NOT ASSIGNED")}\n";
        status += $"Purchase Reward TMP: {(purchaseRewardTextMeshPro != null ? purchaseRewardTextMeshPro.name : "NOT ASSIGNED")}\n";
        status += $"Time Remaining TMP: {(timeRemainingTextMeshPro != null ? timeRemainingTextMeshPro.name : "NOT ASSIGNED")}\n";
        status += $"Ad Loading TMP: {(adLoadingTextMeshPro != null ? adLoadingTextMeshPro.name : "NOT ASSIGNED")}\n";
        status += $"Ad Success TMP: {(adSuccessTextMeshPro != null ? adSuccessTextMeshPro.name : "NOT ASSIGNED")}\n";
        status += $"Ad Failed TMP: {(adFailedTextMeshPro != null ? adFailedTextMeshPro.name : "NOT ASSIGNED")}\n";
        status += $"Purchase Processing TMP: {(purchaseProcessingTextMeshPro != null ? purchaseProcessingTextMeshPro.name : "NOT ASSIGNED")}\n";
        status += $"Purchase Success TMP: {(purchaseSuccessTextMeshPro != null ? purchaseSuccessTextMeshPro.name : "NOT ASSIGNED")}\n";
        status += $"Purchase Failed TMP: {(purchaseFailedTextMeshPro != null ? purchaseFailedTextMeshPro.name : "NOT ASSIGNED")}\n";

        if (timerManager != null)
        {
            status += $"Timer Running: {timerManager.IsRunning}\n";
            status += $"Remaining Time: {timerManager.GetRemainingTime():F1}s\n";
        }

        status += $"Game Completed: {IsGameCompleted()}\n";
        status += $"Can Show: {CanShowTimeExtension()}\n";
        status += "=== END STATUS ===";

        return status;
    }

    /// <summary>
    /// Log current status to console
    /// </summary>
    private void LogCurrentStatus()
    {
        Debug.Log(GetStatus());
    }

    // Public properties for external access
    public bool IsTimeExtensionActive => isTimeExtensionActive;
    public bool IsAdLoading => isAdLoading;
    public bool IsPurchaseProcessing => isPurchaseProcessing;

    // Configuration properties
    public bool enableAdRewards = true;
    public bool enablePurchaseRewards = true;

    // Context menu methods for testing
    [ContextMenu("Test Show Panel")]
    public void TestShowPanel()
    {
        Debug.Log("=== Testing Show Panel ===");
        ForceShowTimeExtension();
    }

    [ContextMenu("Test Close Panel")]
    public void TestClosePanel()
    {
        Debug.Log("=== Testing Close Panel ===");
        CloseTimeExtension();
    }

    /// <summary>
    /// Test different purchase prices
    /// </summary>
    [ContextMenu("Test Price: $0.99")]
    public void TestPrice099()
    {
        UpdatePurchasePrice("$0.99");
    }

    [ContextMenu("Test Price: $1.99")]
    public void TestPrice199()
    {
        UpdatePurchasePrice("$1.99");
    }

    [ContextMenu("Test Price: Free")]
    public void TestPriceFree()
    {
        UpdatePurchasePrice("Free");
    }

    /// <summary>
    /// Test different message formats
    /// </summary>
    [ContextMenu("Test Messages: English")]
    public void TestMessagesEnglish()
    {
        UpdateAdRewardMessage("Watch Ad for +{0} seconds!");
        UpdatePurchaseRewardMessage("Buy +{0} seconds for {1}!");
        UpdateTimeRemainingMessage("Time Remaining: {0:00}:{1:00}");
        UpdateAdMessages("Loading Ad...", "Ad watched! +{0} seconds added!", "Ad failed to load. Try again!");
        UpdatePurchaseMessages("Processing Purchase...", "Purchase successful! +{0} seconds added!", "Purchase failed. Try again!");
    }

    [ContextMenu("Test Messages: Spanish")]
    public void TestMessagesSpanish()
    {
        UpdateAdRewardMessage("¡Mira un anuncio por +{0} segundos!");
        UpdatePurchaseRewardMessage("¡Compra +{0} segundos por {1}!");
        UpdateTimeRemainingMessage("Tiempo Restante: {0:00}:{1:00}");
        UpdateAdMessages("Cargando Anuncio...", "¡Anuncio visto! +{0} segundos añadidos!", "El anuncio falló. ¡Inténtalo de nuevo!");
        UpdatePurchaseMessages("Procesando Compra...", "¡Compra exitosa! +{0} segundos añadidos!", "La compra falló. ¡Inténtalo de nuevo!");
    }

    [ContextMenu("Test Messages: French")]
    public void TestMessagesFrench()
    {
        UpdateAdRewardMessage("Regardez une pub pour +{0} secondes !");
        UpdatePurchaseRewardMessage("Achetez +{0} secondes pour {1} !");
        UpdateTimeRemainingMessage("Temps Restant: {0:00}:{1:00}");
        UpdateAdMessages("Chargement de la Pub...", "Pub regardée ! +{0} secondes ajoutées !", "La pub a échoué. Réessayez !");
        UpdatePurchaseMessages("Traitement de l'Achat...", "Achat réussi ! +{0} secondes ajoutées !", "L'achat a échoué. Réessayez !");
    }

    [ContextMenu("Test Messages: Custom")]
    public void TestMessagesCustom()
    {
        UpdateAdRewardMessage("🎬 Watch Ad → Get +{0}s Bonus!");
        UpdatePurchaseRewardMessage("💎 Premium: +{0}s for {1}");
        UpdateTimeRemainingMessage("⏰ {0:00}:{1:00} left");
        UpdateAdMessages("🔄 Loading...", "✅ +{0}s added!", "❌ Failed! Retry?");
        UpdatePurchaseMessages("💳 Processing...", "✅ +{0}s added!", "❌ Failed! Retry?");
    }

    /// <summary>
    /// Test TextMeshPro field assignments
    /// </summary>
    [ContextMenu("Test TextMeshPro Fields")]
    public void TestTextMeshProFields()
    {
        Debug.Log("=== Testing TextMeshPro Field Assignments ===");

        // Check which fields are assigned
        Debug.Log($"Ad Reward TMP: {(adRewardTextMeshPro != null ? $"✅ {adRewardTextMeshPro.name}" : "❌ NOT ASSIGNED")}");
        Debug.Log($"Purchase Reward TMP: {(purchaseRewardTextMeshPro != null ? $"✅ {purchaseRewardTextMeshPro.name}" : "❌ NOT ASSIGNED")}");
        Debug.Log($"Time Remaining TMP: {(timeRemainingTextMeshPro != null ? $"✅ {timeRemainingTextMeshPro.name}" : "❌ NOT ASSIGNED")}");
        Debug.Log($"Ad Loading TMP: {(adLoadingTextMeshPro != null ? $"✅ {adLoadingTextMeshPro.name}" : "❌ NOT ASSIGNED")}");
        Debug.Log($"Ad Success TMP: {(adSuccessTextMeshPro != null ? $"✅ {adSuccessTextMeshPro.name}" : "❌ NOT ASSIGNED")}");
        Debug.Log($"Ad Failed TMP: {(adFailedTextMeshPro != null ? $"✅ {adFailedTextMeshPro.name}" : "❌ NOT ASSIGNED")}");
        Debug.Log($"Purchase Processing TMP: {(purchaseProcessingTextMeshPro != null ? $"✅ {purchaseProcessingTextMeshPro.name}" : "❌ NOT ASSIGNED")}");
        Debug.Log($"Purchase Success TMP: {(purchaseSuccessTextMeshPro != null ? $"✅ {purchaseSuccessTextMeshPro.name}" : "❌ NOT ASSIGNED")}");
        Debug.Log($"Purchase Failed TMP: {(purchaseFailedTextMeshPro != null ? $"✅ {purchaseFailedTextMeshPro.name}" : "❌ NOT ASSIGNED")}");

        // Test UI updates if panel is active
        if (isTimeExtensionActive)
        {
            Debug.Log("Panel is active - testing UI updates...");
            UpdateRewardTexts();
            UpdateRemainingTimeDisplay();
        }
        else
        {
            Debug.Log("Panel is not active - UI updates will happen when panel opens");
        }

        Debug.Log("=== End TextMeshPro Test ===");
    }

    [ContextMenu("Print Status")]
    public void PrintStatus()
    {
        Debug.Log(GetStatus());
    }

    [ContextMenu("Diagnose Issues")]
    public void DiagnoseIssues()
    {
        Debug.Log("=== DIAGNOSING TIME EXTENSION ISSUES ===");

        // Check initialization
        if (!isInitialized)
        {
            Debug.LogError("❌ Not initialized!");
            return;
        }

        // Check managers
        if (timerManager == null)
        {
            Debug.LogError("❌ TimerManager is NULL!");
        }
        else
        {
            Debug.Log($"✅ TimerManager: {timerManager.name}");
            Debug.Log($"✅ Timer Running: {timerManager.IsRunning}");
            Debug.Log($"✅ Remaining Time: {timerManager.GetRemainingTime():F1}s");
        }

        // Check UI
        if (timeExtensionPanel == null)
        {
            Debug.LogError("❌ Panel is NULL!");
        }
        else
        {
            Debug.Log($"✅ Panel: {timeExtensionPanel.name}");
            Debug.Log($"✅ Panel Active: {timeExtensionPanel.activeInHierarchy}");
        }

        // Check conditions
        Debug.Log($"✅ Can Show: {CanShowTimeExtension()}");
        Debug.Log($"✅ Game Completed: {IsGameCompleted()}");

        Debug.Log("=== END DIAGNOSIS ===");
    }
}