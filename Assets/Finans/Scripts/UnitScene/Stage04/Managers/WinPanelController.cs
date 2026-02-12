using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Win Panel Controller - Manages the win panel display and integrates with star rating system
/// This script should be attached to the win panel GameObject
/// </summary>
[AddComponentMenu("MiniGames/Win Panel Controller")]
[DisallowMultipleComponent]
public class WinPanelController : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Star rating system component")]
    public StarRatingSystem starRatingSystem;
    
    [Tooltip("Score display text")]
    public TextMeshProUGUI scoreText;
    
    [Tooltip("XP display text")]
    public TextMeshProUGUI xpText;
    
    [Tooltip("Accuracy display text")]
    public TextMeshProUGUI accuracyText;
    
    [Tooltip("Time taken display text")]
    public TextMeshProUGUI timeText;
    
    [Tooltip("Performance message text")]
    public TextMeshProUGUI performanceMessageText;
    
    [Tooltip("Continue/Next button")]
    public Button continueButton;
    
    [Tooltip("Restart button")]
    public Button restartButton;

    [Header("Animation Settings")]
    [Tooltip("Delay before showing win panel content")]
    public float showDelay = 0.5f;
    
    [Tooltip("Fade in duration for win panel")]
    public float fadeInDuration = 1f;
    
    [Tooltip("Delay before showing stars")]
    public float starShowDelay = 1f;

    [Header("Performance Messages")]
    [Tooltip("Messages for different star ratings")]
    public string[] performanceMessages = {
        "Keep practicing!",
        "Good job!",
        "Excellent work!"
    };

    [Header("Message Timing")]
    [Tooltip("Maximum time to wait for star rating to settle before updating message")]
    public float starsReadyTimeout = 2f;
    [Tooltip("Polling interval while waiting for star rating to settle")]
    public float starsPollInterval = 0.05f;

    [Header("Integration")]
    [Tooltip("Reference to MinigameScoreManager")]
    public MinigameScoreManager scoreManager;
    
    [Tooltip("Reference to MasterCoinGameManager")]
    public MasterCoinGameManager coinGameManager;
    
    [Tooltip("Auto-find managers if not assigned")]
    public bool autoFindManagers = true;

    [Header("Star Source Preference")]
    [Tooltip("If true, prefer stars computed by MinigameScoreManager over StarRatingSystem thresholds")]
    public bool preferScoreManagerStars = true;

    [Header("Events")]
    [Tooltip("Called when win panel is fully displayed")]
    public UnityEngine.Events.UnityEvent OnWinPanelFullyShown;
    
    [Tooltip("Called when continue button is clicked")]
    public UnityEngine.Events.UnityEvent OnContinueClicked;
    
    [Tooltip("Called when restart button is clicked")]
    public UnityEngine.Events.UnityEvent OnRestartClicked;

    private CanvasGroup canvasGroup;
    private bool isShowing = false;

    void Awake()
    {
        // Get or add CanvasGroup for fade effects
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Initially hide the panel
        gameObject.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        // Auto-find managers if needed
        if (autoFindManagers)
        {
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<MinigameScoreManager>();
            
            if (coinGameManager == null)
                coinGameManager = FindFirstObjectByType<MasterCoinGameManager>();
            
            if (starRatingSystem == null)
                starRatingSystem = FindFirstObjectByType<StarRatingSystem>();
        }
        
        // Set up button listeners
        SetupButtonListeners();
    }

    /// <summary>
    /// Set up button event listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }

    /// <summary>
    /// Show the win panel with game results
    /// </summary>
    public void ShowWinPanel()
    {
        if (isShowing) return;
        
        isShowing = true;
        gameObject.SetActive(true);
        
        // Update display with current game data
        UpdateDisplayData();
        
        // Start the show sequence
        StartCoroutine(ShowWinPanelSequence());
    }

    /// <summary>
    /// Update display data with current game statistics
    /// </summary>
    private void UpdateDisplayData()
    {
        if (scoreManager == null) return;
        
        // Update score display
        if (scoreText != null)
        {
            scoreText.text = scoreManager.GetCurrentScore().ToString();
        }
        
        // Update XP display
        if (xpText != null)
        {
            xpText.text = scoreManager.GetCurrentXP().ToString();
        }
        
        // Update accuracy display
        if (accuracyText != null)
        {
            float accuracy = scoreManager.GetAccuracy();
            accuracyText.text = $"{accuracy:P0}";
        }
        
        // Update time display
        if (timeText != null)
        {
            float timeTaken = scoreManager.GetTimeTaken();
            int minutes = Mathf.FloorToInt(timeTaken / 60f);
            int seconds = Mathf.FloorToInt(timeTaken % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    /// <summary>
    /// Show win panel sequence with animations
    /// </summary>
    private IEnumerator ShowWinPanelSequence()
    {
        // Wait for initial delay
        yield return new WaitForSeconds(showDelay);
        
        // Ensure score manager has completed the game before reading stars
        float waitElapsed = 0f;
        float waitTimeout = 2f;
        while (scoreManager != null && !scoreManager.IsGameCompleted() && waitElapsed < waitTimeout)
        {
            waitElapsed += Time.deltaTime;
            yield return null;
        }

        // Fade in the panel
        yield return StartCoroutine(FadeInPanel());
        
        // Wait before showing stars
        yield return new WaitForSeconds(starShowDelay);
        
        // Calculate and show stars
        if (starRatingSystem != null)
        {
            bool usedScoreManagerStars = false;
            if (preferScoreManagerStars && scoreManager != null && scoreManager.stars > 0)
            {
                starRatingSystem.ShowStarsImmediate(Mathf.Clamp(scoreManager.stars, 0, starRatingSystem.stars.Length));
                usedScoreManagerStars = true;
            }
            else
            {
                starRatingSystem.CalculateAndDisplayStars();
            }

            if (usedScoreManagerStars)
            {
                UpdatePerformanceMessage(scoreManager.stars);
            }
            else
            {
                // Wait for star rating to settle and then update the performance message
                yield return StartCoroutine(WaitForStarsAndUpdateMessage());
            }
        }
        
        // Panel is fully shown
        OnWinPanelFullyShown?.Invoke();
    }

    /// <summary>
    /// Fade in the win panel
    /// </summary>
    private IEnumerator FadeInPanel()
    {
        float elapsedTime = 0f;
        float startAlpha = 0f;
        float targetAlpha = 1f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeInDuration;
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }

    /// <summary>
    /// Update performance message based on star rating
    /// </summary>
    private void UpdatePerformanceMessage()
    {
        if (performanceMessageText == null) return;
        int resolvedStars = GetResolvedStarRating();
        UpdatePerformanceMessage(resolvedStars);
    }

    private void UpdatePerformanceMessage(int forcedStars)
    {
        if (performanceMessageText == null) return;
        int starRating = Mathf.Clamp(forcedStars, 1, 3);
        int messageIndex = Mathf.Clamp(starRating - 1, 0, performanceMessages.Length - 1);
        performanceMessageText.text = performanceMessages[messageIndex];
    }

    private int GetResolvedStarRating()
    {
        int starFromSystem = starRatingSystem != null ? starRatingSystem.GetCurrentStarRating() : 0;
        int starFromScore = scoreManager != null ? Mathf.Clamp(scoreManager.stars, 0, 3) : 0;
        return starFromSystem > 0 ? starFromSystem : (starFromScore > 0 ? starFromScore : 1);
    }

    private IEnumerator WaitForStarsAndUpdateMessage()
    {
        float elapsed = 0f;
        int lastStars = GetResolvedStarRating();
        int stableCount = 0;
        while (elapsed < starsReadyTimeout)
        {
            yield return new WaitForSeconds(starsPollInterval);
            elapsed += starsPollInterval;
            int currentStars = GetResolvedStarRating();
            if (currentStars == lastStars && currentStars > 0)
            {
                stableCount++;
            }
            else
            {
                stableCount = 0;
                lastStars = currentStars;
            }
            if (stableCount >= 2) break; // stable for two polls
        }
        UpdatePerformanceMessage(lastStars);
    }

    /// <summary>
    /// Handle continue button click
    /// </summary>
    private void OnContinueButtonClicked()
    {
        Debug.Log("WinPanelController: Continue button clicked");
        OnContinueClicked?.Invoke();
        
        // Hide the panel
        HideWinPanel();
    }

    /// <summary>
    /// Handle restart button click
    /// </summary>
    private void OnRestartButtonClicked()
    {
        Debug.Log("WinPanelController: Restart button clicked");
        OnRestartClicked?.Invoke();
        
        // Hide the panel
        HideWinPanel();
        
        // Restart the game
        if (coinGameManager != null)
        {
            coinGameManager.RestartSession();
        }
    }

    /// <summary>
    /// Hide the win panel
    /// </summary>
    public void HideWinPanel()
    {
        if (!isShowing) return;
        
        isShowing = false;
        gameObject.SetActive(false);
        
        // Reset stars
        if (starRatingSystem != null)
        {
            starRatingSystem.ResetStars();
        }
    }

    /// <summary>
    /// Force show the win panel (for testing)
    /// </summary>
    public void ForceShowWinPanel()
    {
        ShowWinPanel();
    }

    /// <summary>
    /// Set custom performance messages
    /// </summary>
    public void SetPerformanceMessages(string[] messages)
    {
        performanceMessages = messages;
    }

    /// <summary>
    /// Set star rating system reference
    /// </summary>
    public void SetStarRatingSystem(StarRatingSystem system)
    {
        starRatingSystem = system;
    }

    /// <summary>
    /// Set score manager reference
    /// </summary>
    public void SetScoreManager(MinigameScoreManager manager)
    {
        scoreManager = manager;
    }

    /// <summary>
    /// Set coin game manager reference
    /// </summary>
    public void SetCoinGameManager(MasterCoinGameManager manager)
    {
        coinGameManager = manager;
    }
}
