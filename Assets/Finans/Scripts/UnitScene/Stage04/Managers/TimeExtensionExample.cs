using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Example script showing how to integrate TimeExtensionManager with a mini-game
/// This demonstrates the complete setup for warning time and time up cases
/// </summary>
public class TimeExtensionExample : MonoBehaviour
{
    [Header("Manager References")]
    public TimerManager timerManager;
    public TimeExtensionManager timeExtensionManager;
    
    [Header("Game UI")]
    public GameObject gameUI;
    public GameObject gameOverUI;
    public GameObject pauseUI;
    
    [Header("Game State")]
    public bool isGameActive = false;
    public bool isGamePaused = false;
    
    [Header("Audio")]
    public AudioSource gameAudioSource;
    public AudioClip gameOverSound;
    public AudioClip warningSound;
    
    void Start()
    {
        // Find managers if not assigned
        if (timerManager == null)
            timerManager = FindFirstObjectByType<TimerManager>();
            
        if (timeExtensionManager == null)
            timeExtensionManager = FindFirstObjectByType<TimeExtensionManager>();
        
        // Connect the managers
        ConnectManagers();
        
        // Setup event listeners
        SetupEventListeners();
        
        // Initialize game state
        InitializeGame();
    }
    
    /// <summary>
    /// Connect timer manager to time extension manager
    /// </summary>
    private void ConnectManagers()
    {
        if (timeExtensionManager != null && timerManager != null)
        {
            // TimeExtensionManager now auto-finds managers, no need to set manually
            Debug.Log("TimeExtensionExample: TimeExtensionManager will auto-find TimerManager");
        }
        else
        {
            Debug.LogError("TimeExtensionExample: Missing TimerManager or TimeExtensionManager!");
        }
    }
    
    /// <summary>
    /// Setup event listeners for timer and time extension events
    /// </summary>
    private void SetupEventListeners()
    {
        if (timerManager != null)
        {
            // Listen for time up event
            timerManager.OnTimeUp.AddListener(OnTimeUp);
            
            // Listen for warning time event
            timerManager.OnWarningTime.AddListener(OnWarningTime);
            
            // Listen for time changes
            timerManager.OnTimeChanged.AddListener(OnTimeChanged);
        }
        
        if (timeExtensionManager != null)
        {
            // Listen for time extension events
            timeExtensionManager.OnTimeAdded.AddListener(OnTimeAdded);
            timeExtensionManager.OnAdWatched.AddListener(OnAdWatched);
            timeExtensionManager.OnPurchaseCompleted.AddListener(OnPurchaseCompleted);
            timeExtensionManager.OnTimeExtensionRequested.AddListener(OnTimeExtensionRequested);
            timeExtensionManager.OnTimeExtensionClosed.AddListener(OnTimeExtensionClosed);
        }
    }
    
    /// <summary>
    /// Initialize the game state
    /// </summary>
    private void InitializeGame()
    {
        isGameActive = true;
        isGamePaused = false;
        
        // Show game UI, hide others
        if (gameUI != null) gameUI.SetActive(true);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(false);
        
        Debug.Log("TimeExtensionExample: Game initialized");
    }
    
    /// <summary>
    /// Called when time runs out
    /// </summary>
    private void OnTimeUp()
    {
        isGameActive = false;
        Debug.Log("TimeExtensionExample: Game Over - Time's up!");
        
        // Play game over sound
        if (gameAudioSource != null && gameOverSound != null)
        {
            gameAudioSource.PlayOneShot(gameOverSound);
        }
        
        // Show time extension option
        if (timeExtensionManager != null)
        {
            // TimeExtensionManager now automatically shows panel on time up events
            Debug.Log("TimeExtensionExample: TimeExtensionManager will auto-show panel on time up");
        }
        else
        {
            // If no time extension manager, show game over UI directly
            ShowGameOverUI();
        }
    }
    
    /// <summary>
    /// Called when warning threshold is reached
    /// </summary>
    private void OnWarningTime()
    {
        Debug.Log("TimeExtensionExample: Warning - Time is running low!");
        
        // Play warning sound
        if (gameAudioSource != null && warningSound != null)
        {
            gameAudioSource.PlayOneShot(warningSound);
        }
        
        // Show time extension option
        if (timeExtensionManager != null)
        {
            // TimeExtensionManager now automatically shows panel on warning events
            Debug.Log("TimeExtensionExample: TimeExtensionManager will auto-show panel on warning");
        }
    }
    
    /// <summary>
    /// Called when time changes
    /// </summary>
    private void OnTimeChanged(float remainingTime)
    {
        // Update any time-dependent UI elements
        Debug.Log($"TimeExtensionExample: Time remaining: {remainingTime:F1}s");
    }
    
    /// <summary>
    /// Called when time is successfully added
    /// </summary>
    private void OnTimeAdded(int secondsAdded)
    {
        Debug.Log($"TimeExtensionExample: Added {secondsAdded} seconds to timer!");
        
        // Resume game if it was paused
        if (isGamePaused)
        {
            ResumeGame();
        }
        
        // Update game state
        isGameActive = true;
        
        // Show success message
        ShowMessage($"Added {secondsAdded} seconds!");
    }
    
    /// <summary>
    /// Called when ad is successfully watched
    /// </summary>
    private void OnAdWatched()
    {
        Debug.Log("TimeExtensionExample: Ad watched successfully!");
        
        // Track analytics here
        // Analytics.TrackEvent("AdWatched", "TimeExtension");
        
        // Show success message
        ShowMessage("Ad watched! Time added!");
    }
    
    /// <summary>
    /// Called when purchase is completed
    /// </summary>
    private void OnPurchaseCompleted()
    {
        Debug.Log("TimeExtensionExample: Purchase completed!");
        
        // Track analytics here
        // Analytics.TrackEvent("PurchaseCompleted", "TimeExtension");
        
        // Show success message
        ShowMessage("Purchase completed! Time added!");
    }
    
    /// <summary>
    /// Called when time extension is requested
    /// </summary>
    private void OnTimeExtensionRequested()
    {
        Debug.Log("TimeExtensionExample: Time extension requested");
        
        // Pause the game
        PauseGame();
    }
    
    /// <summary>
    /// Called when time extension panel is closed
    /// </summary>
    private void OnTimeExtensionClosed()
    {
        Debug.Log("TimeExtensionExample: Time extension closed");
        
        // Resume game if it was paused
        if (isGamePaused)
        {
            ResumeGame();
        }
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    private void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        
        // Show pause UI if available
        if (pauseUI != null)
        {
            pauseUI.SetActive(true);
        }
        
        Debug.Log("TimeExtensionExample: Game paused");
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    private void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        
        // Hide pause UI if available
        if (pauseUI != null)
        {
            pauseUI.SetActive(false);
        }
        
        Debug.Log("TimeExtensionExample: Game resumed");
    }
    
    /// <summary>
    /// Show game over UI
    /// </summary>
    private void ShowGameOverUI()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }
        
        Debug.Log("TimeExtensionExample: Game over UI shown");
    }
    
    /// <summary>
    /// Show a temporary message to the player
    /// </summary>
    private void ShowMessage(string message)
    {
        Debug.Log($"TimeExtensionExample: {message}");
        
        // You can implement a message system here
        // For example, show a popup or update a status text
    }
    
    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartNewGame()
    {
        // Reset timer
        if (timerManager != null)
        {
            timerManager.ResetTimer();
            timerManager.StartTimer();
        }
        
        // Reset game state
        isGameActive = true;
        isGamePaused = false;
        Time.timeScale = 1f;
        
        // Show game UI, hide others
        if (gameUI != null) gameUI.SetActive(true);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(false);
        
        Debug.Log("TimeExtensionExample: New game started");
    }
    
    /// <summary>
    /// Configure time extension settings
    /// </summary>
    public void ConfigureTimeExtension(int adTime, int purchaseTime, float warningThreshold)
    {
        if (timeExtensionManager != null)
        {
            // TimeExtensionManager now uses inspector values, but we can update them at runtime
            Debug.Log($"TimeExtensionExample: Time extension configured - Ad: {adTime}s, Purchase: {purchaseTime}s, Warning: {warningThreshold}s");
        }
    }
    
    /// <summary>
    /// Enable or disable time extension features
    /// </summary>
    public void SetTimeExtensionEnabled(bool enabled)
    {
        if (timeExtensionManager != null)
        {
            // TimeExtensionManager now uses inspector values for enable/disable
            Debug.Log($"TimeExtensionExample: Time extension {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Get current game state
    /// </summary>
    public (bool isActive, bool isPaused, float remainingTime) GetGameState()
    {
        float remainingTime = 0f;
        if (timerManager != null)
        {
            remainingTime = timerManager.GetRemainingTime();
        }
        
        return (isGameActive, isGamePaused, remainingTime);
    }
    
    /// <summary>
    /// Test time extension (for debugging)
    /// </summary>
    [ContextMenu("Test Time Extension")]
    public void TestTimeExtension()
    {
        if (timeExtensionManager != null)
        {
            timeExtensionManager.ForceShowTimeExtension();
            Debug.Log("TimeExtensionExample: Testing time extension");
        }
    }
    
    /// <summary>
    /// Test game over time extension (for debugging)
    /// </summary>
    [ContextMenu("Test Game Over Time Extension")]
    public void TestGameOverTimeExtension()
    {
        if (timeExtensionManager != null)
        {
            timeExtensionManager.ForceShowTimeExtension();
            Debug.Log("TimeExtensionExample: Testing game over time extension");
        }
    }
    
    /// <summary>
    /// Add time manually (for testing)
    /// </summary>
    [ContextMenu("Add 30 Seconds")]
    public void AddThirtySeconds()
    {
        if (timerManager != null)
        {
            timerManager.AddTime(30f);
            Debug.Log("TimeExtensionExample: Added 30 seconds manually");
        }
    }
} 