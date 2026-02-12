using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Example script demonstrating how to assign and use TimerManager events
/// This shows a complete game system with timer integration
/// </summary>
public class TimerEventExample : MonoBehaviour
{
    [Header("Timer Manager")]
    public TimerManager timerManager;
    
    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public GameObject warningPanel;
    public TextMeshProUGUI timeDisplay;
    public TextMeshProUGUI warningText;
    public Slider timeProgressBar;
    public Image timeBarFill;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gameOverSound;
    public AudioClip warningSound;
    
    [Header("Game Settings")]
    public float gameTime = 120f; // 2 minutes
    public float warningThreshold = 30f; // Show warning at 30 seconds
    
    void Start()
    {
        // Find TimerManager if not assigned
        if (timerManager == null)
        {
            timerManager = FindFirstObjectByType<TimerManager>();
        }
        
        // Configure timer
        if (timerManager != null)
        {
            timerManager.initialTime = gameTime;
            timerManager.warningThreshold = warningThreshold;
            
            // Assign events
            AssignTimerEvents();
            
            // Start the timer
            timerManager.StartTimer();
        }
        else
        {
            Debug.LogError("TimerEventExample: No TimerManager found!");
        }
    }
    
    /// <summary>
    /// Assign all timer events
    /// </summary>
    private void AssignTimerEvents()
    {
        // Time Up Event
        timerManager.OnTimeUp.AddListener(OnGameTimeUp);
        
        // Warning Time Event
        timerManager.OnWarningTime.AddListener(OnWarningTriggered);
        
        // Time Changed Event
        timerManager.OnTimeChanged.AddListener(OnTimeUpdated);
        
        Debug.Log("TimerEventExample: All events assigned successfully!");
    }
    
    /// <summary>
    /// Called when time runs out
    /// </summary>
    public void OnGameTimeUp()
    {
        Debug.Log("TimerEventExample: Game Over - Time's up!");
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Show game over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
        
        // Save final score
        SaveFinalScore();
        
        // Show restart options
        ShowRestartOptions();
    }
    
    /// <summary>
    /// Called when warning threshold is reached
    /// </summary>
    public void OnWarningTriggered()
    {
        Debug.Log("TimerEventExample: Warning - Time is running low!");
        
        // Show warning UI
        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
        }
        
        // Update warning text
        if (warningText != null)
        {
            warningText.text = "Hurry up! Time is running out!";
        }
        
        // Play warning sound
        if (audioSource != null && warningSound != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
        
        // Start flashing effect
        StartCoroutine(FlashWarningUI());
        
        // Show time extension options (if available)
        ShowTimeExtensionOptions();
    }
    
    /// <summary>
    /// Called when time changes
    /// </summary>
    public void OnTimeUpdated(float currentTime)
    {
        // Update time display
        if (timeDisplay != null)
        {
            timeDisplay.text = timerManager.GetFormattedTime();
        }
        
        // Update progress bar
        if (timeProgressBar != null)
        {
            float progress = currentTime / timerManager.initialTime;
            timeProgressBar.value = progress;
        }
        
        // Change color based on time remaining
        if (timeBarFill != null)
        {
            if (currentTime <= 30f)
            {
                timeBarFill.color = Color.red;
            }
            else if (currentTime <= 60f)
            {
                timeBarFill.color = Color.yellow;
            }
            else
            {
                timeBarFill.color = Color.green;
            }
        }
        
        // Update warning panel visibility
        if (warningPanel != null)
        {
            warningPanel.SetActive(currentTime <= warningThreshold && currentTime > 0);
        }
    }
    
    /// <summary>
    /// Flash warning UI elements
    /// </summary>
    private IEnumerator FlashWarningUI()
    {
        if (warningText == null) yield break;
        
        Color originalColor = warningText.color;
        Color warningColor = Color.red;
        
        while (timerManager.IsTimeRunningLow)
        {
            warningText.color = warningColor;
            yield return new WaitForSeconds(0.5f);
            warningText.color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }
        
        // Reset color when warning ends
        warningText.color = originalColor;
    }
    
    /// <summary>
    /// Save final score
    /// </summary>
    private void SaveFinalScore()
    {
        // Get current score from your game logic
        int finalScore = GetCurrentScore();
        
        // Save to PlayerPrefs
        PlayerPrefs.SetInt("LastScore", finalScore);
        
        // Update high score if needed
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (finalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", finalScore);
            Debug.Log($"TimerEventExample: New high score! {finalScore}");
        }
        
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Show restart options
    /// </summary>
    private void ShowRestartOptions()
    {
        // This would typically show UI buttons for restart, main menu, etc.
        Debug.Log("TimerEventExample: Show restart options");
    }
    
    /// <summary>
    /// Show time extension options
    /// </summary>
    private void ShowTimeExtensionOptions()
    {
        // Check if TimeExtensionManager is available
        var timeExtensionManager = FindFirstObjectByType<TimeExtensionManager>();
        if (timeExtensionManager != null)
        {
            // Connect to time added event to stop warning sound
            timeExtensionManager.OnTimeAdded.AddListener(OnTimeExtended);
            
            timeExtensionManager.ForceShowTimeExtension();
        }
        else
        {
            Debug.Log("TimerEventExample: TimeExtensionManager not found");
        }
    }
    
    /// <summary>
    /// Called when time is extended through ads or purchases
    /// </summary>
    private void OnTimeExtended(int secondsAdded)
    {
        Debug.Log($"TimerEventExample: Time extended by {secondsAdded} seconds - Warning sound stopped");
        
        // Hide warning panel since time was extended
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
        
        // Reset warning text color
        if (warningText != null)
        {
            warningText.color = Color.white;
        }
        
        // Show success message
        StartCoroutine(ShowTimeExtensionSuccess(secondsAdded));
    }
    
    /// <summary>
    /// Show success message when time is extended
    /// </summary>
    private IEnumerator ShowTimeExtensionSuccess(int secondsAdded)
    {
        if (warningText != null)
        {
            string originalText = warningText.text;
            warningText.text = $"+{secondsAdded} seconds added!";
            warningText.color = Color.green;
            
            yield return new WaitForSeconds(2f);
            
            warningText.text = originalText;
            warningText.color = Color.white;
        }
    }
    
    /// <summary>
    /// Get current score (replace with your game logic)
    /// </summary>
    private int GetCurrentScore()
    {
        // Replace this with your actual score calculation
        return Random.Range(100, 1000);
    }
    
    /// <summary>
    /// Restart the game
    /// </summary>
    public void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1f;
        
        // Hide UI panels
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (warningPanel != null) warningPanel.SetActive(false);
        
        // Reset timer
        if (timerManager != null)
        {
            timerManager.ResetTimer();
            timerManager.StartTimer();
        }
        
        // Reset game state (implement your game reset logic)
        ResetGameState();
    }
    
    /// <summary>
    /// Reset game state
    /// </summary>
    private void ResetGameState()
    {
        // Implement your game reset logic here
        Debug.Log("TimerEventExample: Game state reset");
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (timerManager != null)
        {
            timerManager.PauseTimer();
        }
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        if (timerManager != null)
        {
            timerManager.ResumeTimer();
        }
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Add time to the timer
    /// </summary>
    public void AddTime(float seconds)
    {
        if (timerManager != null)
        {
            timerManager.AddTime(seconds);
            Debug.Log($"TimerEventExample: Added {seconds} seconds to timer");
        }
    }
    
    /// <summary>
    /// Clean up event listeners
    /// </summary>
    void OnDestroy()
    {
        if (timerManager != null)
        {
            timerManager.OnTimeUp.RemoveListener(OnGameTimeUp);
            timerManager.OnWarningTime.RemoveListener(OnWarningTriggered);
            timerManager.OnTimeChanged.RemoveListener(OnTimeUpdated);
        }
        
        // Clean up TimeExtensionManager event listener
                    var timeExtensionManager = FindFirstObjectByType<TimeExtensionManager>();
        if (timeExtensionManager != null)
        {
            timeExtensionManager.OnTimeAdded.RemoveListener(OnTimeExtended);
        }
    }
    
    /// <summary>
    /// Test method to simulate time up
    /// </summary>
    [ContextMenu("Test Time Up")]
    public void TestTimeUp()
    {
        if (timerManager != null)
        {
            timerManager.SetTime(0f);
        }
    }
    
    /// <summary>
    /// Test method to simulate warning
    /// </summary>
    [ContextMenu("Test Warning")]
    public void TestWarning()
    {
        if (timerManager != null)
        {
            timerManager.SetTime(warningThreshold);
        }
    }
    
    /// <summary>
    /// Test method to simulate time extension
    /// </summary>
    [ContextMenu("Test Time Extension")]
    public void TestTimeExtension()
    {
        if (timerManager != null)
        {
            // Simulate adding time (this will stop warning sound)
            timerManager.AddTime(60f);
            Debug.Log("TimerEventExample: Test time extension - 60 seconds added");
        }
    }
} 