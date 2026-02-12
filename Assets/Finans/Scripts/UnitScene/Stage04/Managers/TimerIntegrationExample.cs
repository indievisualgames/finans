using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Example script demonstrating how to use the timer integration features
/// in DynamicScoreManager for both time-based and non-time-based games
/// </summary>
public class TimerIntegrationExample : MonoBehaviour
{
    [Header("Minigame Score Manager")]
    public MinigameScoreManager scoreManager;
    
    [Header("UI Controls")]
    public Button enableTimerButton;
    public Button disableTimerButton;
    public Button setTimeBasedButton;
    public Button setNonTimeBasedButton;
    public Button configureTimerButton;
    public Button startTimerButton;
    public Button stopTimerButton;
    public Button pauseTimerButton;
    public Button resumeTimerButton;
    
    [Header("Timer Configuration")]
    public Slider timeLimitSlider;
    public Slider warningThresholdSlider;
    public Toggle countDownToggle;
    public TextMeshProUGUI timeLimitText;
    public TextMeshProUGUI warningThresholdText;
    
    [Header("Status Display")]
    public TextMeshProUGUI gameTypeText;
    public TextMeshProUGUI timerStatusText;
    public TextMeshProUGUI remainingTimeText;
    
    void Start()
    {
        // Find score manager if not assigned
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<MinigameScoreManager>();
        }
        
        // Setup UI controls
        SetupUI();
        
        // Update display
        UpdateDisplay();
    }
    
    void Update()
    {
        // Update remaining time display
        if (scoreManager != null && scoreManager.IsTimerEnabled())
        {
            float remainingTime = scoreManager.GetRemainingTime();
            if (remainingTimeText != null)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                remainingTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
    }
    
    void SetupUI()
    {
        // Enable/Disable Timer buttons
        if (enableTimerButton != null)
            enableTimerButton.onClick.AddListener(() => EnableTimer(true));
            
        if (disableTimerButton != null)
            disableTimerButton.onClick.AddListener(() => EnableTimer(false));
        
        // Game Type buttons
        if (setTimeBasedButton != null)
            setTimeBasedButton.onClick.AddListener(() => SetGameType(MinigameScoreManager.GameType.TimeBased));
            
        if (setNonTimeBasedButton != null)
            setNonTimeBasedButton.onClick.AddListener(() => SetGameType(MinigameScoreManager.GameType.NonTimeBased));
        
        // Timer Control buttons
        if (configureTimerButton != null)
            configureTimerButton.onClick.AddListener(ConfigureTimer);
            
        if (startTimerButton != null)
            startTimerButton.onClick.AddListener(() => scoreManager?.StartTimer());
            
        if (stopTimerButton != null)
            stopTimerButton.onClick.AddListener(() => scoreManager?.StopTimer());
            
        if (pauseTimerButton != null)
            pauseTimerButton.onClick.AddListener(() => scoreManager?.PauseTimer());
            
        if (resumeTimerButton != null)
            resumeTimerButton.onClick.AddListener(() => scoreManager?.ResumeTimer());
        
        // Sliders
        if (timeLimitSlider != null)
        {
            timeLimitSlider.minValue = 10f;
            timeLimitSlider.maxValue = 600f;
            timeLimitSlider.value = 120f;
            timeLimitSlider.onValueChanged.AddListener(OnTimeLimitChanged);
        }
        
        if (warningThresholdSlider != null)
        {
            warningThresholdSlider.minValue = 5f;
            warningThresholdSlider.maxValue = 60f;
            warningThresholdSlider.value = 30f;
            warningThresholdSlider.onValueChanged.AddListener(OnWarningThresholdChanged);
        }
        
        // Toggle
        if (countDownToggle != null)
        {
            countDownToggle.isOn = true;
            countDownToggle.onValueChanged.AddListener(OnCountDownChanged);
        }
    }
    
    void EnableTimer(bool enable)
    {
        if (scoreManager != null)
        {
            scoreManager.EnableTimer(enable);
            UpdateDisplay();
        }
    }
    
    void SetGameType(MinigameScoreManager.GameType gameType)
    {
        if (scoreManager != null)
        {
            scoreManager.SetGameType(gameType);
            UpdateDisplay();
        }
    }
    
    void ConfigureTimer()
    {
        if (scoreManager != null && scoreManager.IsTimerEnabled())
        {
            float timeLimit = timeLimitSlider != null ? timeLimitSlider.value : 120f;
            float warningThresh = warningThresholdSlider != null ? warningThresholdSlider.value : 30f;
            bool countDown = countDownToggle != null ? countDownToggle.isOn : true;
            
            scoreManager.ConfigureTimer(timeLimit, warningThresh, countDown);
            Debug.Log($"Timer configured: {timeLimit}s, Warning: {warningThresh}s, CountDown: {countDown}");
        }
    }
    
    void OnTimeLimitChanged(float value)
    {
        if (timeLimitText != null)
        {
            timeLimitText.text = $"Time Limit: {value:F0}s";
        }
    }
    
    void OnWarningThresholdChanged(float value)
    {
        if (warningThresholdText != null)
        {
            warningThresholdText.text = $"Warning: {value:F0}s";
        }
    }
    
    void OnCountDownChanged(bool countDown)
    {
        Debug.Log($"Count Down: {(countDown ? "ON" : "OFF")}");
    }
    
    void UpdateDisplay()
    {
        if (scoreManager == null) return;
        
        // Update game type display
        if (gameTypeText != null)
        {
            gameTypeText.text = $"Game Type: {scoreManager.gameType}";
        }
        
        // Update timer status
        if (timerStatusText != null)
        {
            string status = scoreManager.IsTimerEnabled() ? "ENABLED" : "DISABLED";
            if (scoreManager.IsTimerEnabled())
            {
                status += scoreManager.IsTimerRunning() ? " (Running)" : " (Stopped)";
            }
            timerStatusText.text = $"Timer: {status}";
        }
        
        // Update button states
        UpdateButtonStates();
    }
    
    void UpdateButtonStates()
    {
        if (scoreManager == null) return;
        
        bool timerEnabled = scoreManager.IsTimerEnabled();
        bool timerRunning = scoreManager.IsTimerRunning();
        
        // Enable/Disable buttons based on timer state
        if (enableTimerButton != null)
            enableTimerButton.interactable = !timerEnabled;
            
        if (disableTimerButton != null)
            disableTimerButton.interactable = timerEnabled;
        
        // Game type buttons
        if (setTimeBasedButton != null)
            setTimeBasedButton.interactable = scoreManager.gameType != MinigameScoreManager.GameType.TimeBased;
            
        if (setNonTimeBasedButton != null)
            setNonTimeBasedButton.interactable = scoreManager.gameType != MinigameScoreManager.GameType.NonTimeBased;
        
        // Timer control buttons
        if (configureTimerButton != null)
            configureTimerButton.interactable = timerEnabled;
            
        if (startTimerButton != null)
            startTimerButton.interactable = timerEnabled && !timerRunning;
            
        if (stopTimerButton != null)
            stopTimerButton.interactable = timerEnabled && timerRunning;
            
        if (pauseTimerButton != null)
            pauseTimerButton.interactable = timerEnabled && timerRunning;
            
        if (resumeTimerButton != null)
            resumeTimerButton.interactable = timerEnabled && !timerRunning;
        
        // Sliders and toggle
        if (timeLimitSlider != null)
            timeLimitSlider.interactable = timerEnabled;
            
        if (warningThresholdSlider != null)
            warningThresholdSlider.interactable = timerEnabled;
            
        if (countDownToggle != null)
            countDownToggle.interactable = timerEnabled;
    }
    
    // Public methods for external calls
    public void TestTimeBasedGame()
    {
        if (scoreManager != null)
        {
            SetGameType(MinigameScoreManager.GameType.TimeBased);
            EnableTimer(true);
            ConfigureTimer();
            StartTimer();
        }
    }
    
    public void TestNonTimeBasedGame()
    {
        if (scoreManager != null)
        {
            SetGameType(MinigameScoreManager.GameType.NonTimeBased);
            EnableTimer(false);
        }
    }
    
    public void StartTimer()
    {
        if (scoreManager != null)
        {
            scoreManager.StartTimer();
            UpdateDisplay();
        }
    }
}
