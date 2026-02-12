using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Example integration script showing how to connect TimeExtensionManager with existing game systems
/// </summary>
public class GameTimeIntegration : MonoBehaviour
{
    [Header("Manager References")]
    public TimerManager timerManager;
    public TimeExtensionManager timeExtensionManager;
    
    [Header("Game State")]
    public bool isGameActive = false;
    public bool isGameOver = false;
    
    [Header("Integration Settings")]
    public bool enableTimeExtension = true;
    public bool showExtensionOnWarning = true;
    public bool showExtensionOnGameOver = true;
    
    void Start()
    {
        // Find managers if not assigned
        if (timerManager == null)
            timerManager = FindFirstObjectByType<TimerManager>();
            
        if (timeExtensionManager == null)
            timeExtensionManager = FindFirstObjectByType<TimeExtensionManager>();
            
        // Connect timer manager to time extension manager
        if (timeExtensionManager != null && timerManager != null)
        {
            // TimeExtensionManager now auto-finds managers, no need to set manually
            Debug.Log("GameTimeIntegration: TimeExtensionManager will auto-find TimerManager");
        }
        
        // Setup event listeners
        SetupEventListeners();
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
    /// Called when time runs out
    /// </summary>
    private void OnTimeUp()
    {
        isGameOver = true;
        Debug.Log("Game Over - Time's up!");
        
        // Show time extension option if enabled
        if (enableTimeExtension && showExtensionOnGameOver && timeExtensionManager != null)
        {
            // TimeExtensionManager now automatically shows panel on time up events
            Debug.Log("GameTimeIntegration: TimeExtensionManager will auto-show panel on time up");
        }
        else
        {
            // Handle game over without time extension
            HandleGameOver();
        }
    }
    
    /// <summary>
    /// Called when warning threshold is reached
    /// </summary>
    private void OnWarningTime()
    {
        Debug.Log("Warning: Time is running low!");
        
        // Show time extension option if enabled
        if (enableTimeExtension && showExtensionOnWarning && timeExtensionManager != null)
        {
            // TimeExtensionManager now automatically shows panel on warning events
            Debug.Log("GameTimeIntegration: TimeExtensionManager will auto-show panel on warning");
        }
    }
    
    /// <summary>
    /// Called when time is successfully added
    /// </summary>
    private void OnTimeAdded(int secondsAdded)
    {
        Debug.Log($"Time added successfully: +{secondsAdded} seconds");
        
        // Resume game if it was paused due to time
        if (isGameOver)
        {
            isGameOver = false;
            ResumeGame();
        }
        
        // Update game state
        UpdateGameState();
    }
    
    /// <summary>
    /// Called when ad is successfully watched
    /// </summary>
    private void OnAdWatched()
    {
        Debug.Log("Ad watched successfully!");
        
        // You can add analytics here
        // AnalyticsManager.Instance.RecordAdWatched();
        
        // Update player preferences
        PlayerPrefs.SetInt("AdsWatched", PlayerPrefs.GetInt("AdsWatched", 0) + 1);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Called when purchase is completed
    /// </summary>
    private void OnPurchaseCompleted()
    {
        Debug.Log("Purchase completed successfully!");
        
        // You can add analytics here
        // AnalyticsManager.Instance.RecordPurchase();
        
        // Update player preferences
        PlayerPrefs.SetInt("PurchasesMade", PlayerPrefs.GetInt("PurchasesMade", 0) + 1);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Called when time extension is requested
    /// </summary>
    private void OnTimeExtensionRequested()
    {
        Debug.Log("Time extension requested");
        
        // Pause game if needed
        if (isGameActive)
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// Called when time extension panel is closed
    /// </summary>
    private void OnTimeExtensionClosed()
    {
        Debug.Log("Time extension panel closed");
        
        // Resume game if it was paused
        if (isGameActive && !isGameOver)
        {
            ResumeGame();
        }
    }
    
    /// <summary>
    /// Handle game over without time extension
    /// </summary>
    private void HandleGameOver()
    {
        Debug.Log("Handling game over...");
        
        // Stop the game
        isGameActive = false;
        isGameOver = true;
        
        // Show game over UI
        ShowGameOverUI();
        
        // Save game results
        SaveGameResults();
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    private void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Game paused");
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    private void ResumeGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Game resumed");
    }
    
    /// <summary>
    /// Update game state based on current conditions
    /// </summary>
    private void UpdateGameState()
    {
        if (timerManager != null)
        {
            float remainingTime = timerManager.GetRemainingTime();
            
            // Update UI based on remaining time
            if (remainingTime <= 10f)
            {
                // Critical time - show urgent warning
                ShowUrgentWarning();
            }
            else if (remainingTime <= 30f)
            {
                // Low time - show normal warning
                ShowLowTimeWarning();
            }
            else
            {
                // Normal time - hide warnings
                HideWarnings();
            }
        }
    }
    
    /// <summary>
    /// Show urgent warning for very low time
    /// </summary>
    private void ShowUrgentWarning()
    {
        Debug.Log("URGENT: Very little time remaining!");
        // Add your urgent warning UI logic here
    }
    
    /// <summary>
    /// Show warning for low time
    /// </summary>
    private void ShowLowTimeWarning()
    {
        Debug.Log("Warning: Low time remaining");
        // Add your low time warning UI logic here
    }
    
    /// <summary>
    /// Hide all time warnings
    /// </summary>
    private void HideWarnings()
    {
        Debug.Log("Time warnings hidden");
        // Add your warning hiding logic here
    }
    
    /// <summary>
    /// Show game over UI
    /// </summary>
    private void ShowGameOverUI()
    {
        Debug.Log("Showing game over UI");
        // Add your game over UI logic here
    }
    
    /// <summary>
    /// Save game results
    /// </summary>
    private void SaveGameResults()
    {
        Debug.Log("Saving game results");
        // Add your game results saving logic here
    }
    
    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartNewGame()
    {
        isGameActive = true;
        isGameOver = false;
        
        if (timerManager != null)
        {
            timerManager.ResetTimer();
            timerManager.StartTimer();
        }
        
        // Close any active time extension
        if (timeExtensionManager != null)
        {
            timeExtensionManager.CloseTimeExtension();
        }
        
        Debug.Log("New game started");
    }
    
    /// <summary>
    /// Configure time extension settings
    /// </summary>
    public void ConfigureTimeExtension(int adTime, int purchaseTime, float warningThreshold)
    {
        if (timeExtensionManager != null)
        {
            // TimeExtensionManager now uses inspector values, but we can update them at runtime
            Debug.Log($"GameTimeIntegration: Time extension configured - Ad: {adTime}s, Purchase: {purchaseTime}s, Warning: {warningThreshold}s");
        }
    }
    
    /// <summary>
    /// Enable or disable time extension features
    /// </summary>
    public void SetTimeExtensionEnabled(bool enabled)
    {
        enableTimeExtension = enabled;
        
        if (timeExtensionManager != null)
        {
            // TimeExtensionManager now uses inspector values for enable/disable
            Debug.Log($"GameTimeIntegration: Time extension features {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Get current game state information
    /// </summary>
    public (bool isActive, bool isGameOver, float remainingTime) GetGameState()
    {
        float remainingTime = 0f;
        if (timerManager != null)
        {
            remainingTime = timerManager.GetRemainingTime();
        }
        
        return (isGameActive, isGameOver, remainingTime);
    }
    
    /// <summary>
    /// Manual trigger for time extension (for testing)
    /// </summary>
    [ContextMenu("Test Time Extension")]
    public void TestTimeExtension()
    {
        if (timeExtensionManager != null)
        {
            timeExtensionManager.ForceShowTimeExtension();
        }
    }
    
    /// <summary>
    /// Manual trigger for game over time extension (for testing)
    /// </summary>
    [ContextMenu("Test Game Over Time Extension")]
    public void TestGameOverTimeExtension()
    {
        if (timeExtensionManager != null)
        {
            timeExtensionManager.ForceShowTimeExtension();
        }
    }
} 