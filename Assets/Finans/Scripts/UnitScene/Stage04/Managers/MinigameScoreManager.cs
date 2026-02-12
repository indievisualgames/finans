using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.SocialPlatforms;
#endif
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Global Minigame Score Manager - A generic, reusable score tracking system for all minigames.
/// Tracks good moves, mistakes, score, and XP accumulation across different game types.
/// </summary>
[AddComponentMenu("MiniGames/Global Minigame Score Manager")]
[DisallowMultipleComponent]
public class MinigameScoreManager : MonoBehaviour
{
    [Header("Service Registration")]
    [Tooltip("Optional logical id for selecting a specific manager instance")]
    public string serviceId = "";
    [Tooltip("Auto register to MinigameScoreService on enable")] public bool autoRegister = true;
    [Tooltip("Persist this manager across scenes")] public bool persistAcrossScenes = false;

    void Awake()
    {
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        if (autoRegister) MiniGameServices.MinigameScoreService.Register(this);
    }

    void OnDisable()
    {
        if (autoRegister) MiniGameServices.MinigameScoreService.Unregister(this);
    }

    [Header("Global Score Settings")]
    [Tooltip("Points added when score increases")]
    public int scoreIncrement = 1;
    [Tooltip("Points subtracted when score decreases")]
    public int scoreDecrement = 1;

	[Header("Unified Scoring (Final Score)")]
	[Tooltip("Coins collected during the session (base score uses Coins × 100)")]
	public int coinsCollected = 0;
	[Tooltip("Calculated at end of game: sum of base and bonuses")]
	public int finalScore = 0;
	[Tooltip("Stars awarded at the end of game based on performance (1-3)")]
	[Range(0, 3)] public int stars = 0;
	[Tooltip("Time-derived points computed at game end")]
	public int timeBonus = 0;
	[Tooltip("Speed tier label computed from time saved percentage")]
	public string speedTier = "Survivor";
	[Tooltip("(timeGiven - timeTaken)/timeGiven clamped to 0..1")]
	[Range(0f, 1f)] public float timeSavedPercentage = 0f;
	[Tooltip("Streak level grows with consecutive fast completions (caps at 10 for 2.0x)")]
	[Range(0, 10)] public int streakLevel = 0;
	[Tooltip("Best final score across sessions for this player (local)")]
	public int personalBest = 0;
	[Tooltip("Human-readable accuracy tier for the session (Perfect/Near Perfect/Good)")]
	public string accuracyTier = "Good";
	[Tooltip("Unlocked achievement keys for this session")]
	public List<string> achievements = new List<string>();

    [Header("Good Move Rewards")]
    [Tooltip("Base points awarded per good move - fully editable")]
    public int basePointsPerGoodMove = 100;
    [Tooltip("Base XP per good move (no levels)")]
    [Range(1, 200)] public int xpPerGoodMove = 35;
    [Tooltip("XP lost per mistake (will be subtracted as negative value)")]
    [Range(0, 50)] public int xpPerMistake = 8;

    [Header("Performance Multipliers")]
    [Range(0.5f, 5f)] public float timeBonusMultiplier = 2.0f;
    [Range(0.1f, 2f)] public float accuracyMultiplier = 1.5f;

    [Header("Effort-Based Rewards (Speed Bonuses)")]
    [Tooltip("75%+ time saved")] public int quickLearnerBonus = 250;
    [Tooltip("50-74% time saved")] public int efficientBonus = 150;

    [Header("UI Text Targets")]
    [Tooltip("Text components to display score values (numbers only)")]
    public TextMeshProUGUI[] scoreTexts = new TextMeshProUGUI[3];
    [Tooltip("Text components to display XP values (numbers only)")]
    public TextMeshProUGUI[] xpTexts = new TextMeshProUGUI[3];
	[Tooltip("Text components to display accuracy values (percent)")]
	public TextMeshProUGUI[] accuracyTexts = new TextMeshProUGUI[3];
	[Tooltip("Text components to display coin values (numbers only)")]
	public TextMeshProUGUI[] coinTexts = new TextMeshProUGUI[3];
	[Tooltip("Text components to display star values (numbers only)")]
	public TextMeshProUGUI[] starTexts = new TextMeshProUGUI[3];
	[Tooltip("Text components to display final score values (numbers only)")]
	public TextMeshProUGUI[] finalScoreTexts = new TextMeshProUGUI[3];
    
    [Header("UI Text Labels (Optional)")]
    [Tooltip("Custom score label text (leave empty for numbers only)")]
    public string scoreLabel = "";
    [Tooltip("Custom XP label text (leave empty for numbers only)")]
    public string xpLabel = "";
    [Tooltip("Show only numbers without any labels")]
    public bool showNumbersOnly = true;

    [Header("Timer Integration (Optional)")]
    public bool enableTimer = true;
    public TimerManager timerManager;
    public TimeExtensionManager timeExtensionManager;
    public bool autoFindManagers = true;

    [Header("Timer Settings")]
    public GameType gameType = GameType.TimeBased;
    public bool countDownTimer = true;
    [Range(10f, 600f)] public float initialTimeLimit = 60f;
    [Range(5f, 60f)] public float warningThreshold = 10f;
    public bool autoSyncTimerManager = true;
    public bool syncOnTimerStart = true;

    [Header("Audio")] 
    public MiniGameAudioManager audioManager;
    public bool useUnifiedAudio = true;
    public AudioSource scoreAudioSource; // legacy fallback
    public AudioClip scoreUpdateSound;
    public AudioClip scoreDecreaseSound;
    public AudioClip bonusSound;
    public AudioClip xpGainSound;

    [Header("Score/XP Particles Only")]
    public bool enableScoreParticles = true;
    public ParticleSystem scoreAdditionParticles;
    public ParticleSystem scoreSubtractionParticles;
    public ParticleSystem xpGainParticles;
    public ParticleSystem xpLossParticles;
    [Range(0.5f, 5f)] public float particleDuration = 2f;
    public bool autoDestroyParticles = true;
    public Vector3 particleSpawnOffset = new Vector3(0, 50, 0);
    [Range(5, 50)] public int scoreParticleBurstCount = 15;
    [Range(10, 100)] public int xpParticleBurstCount = 25;

    // Events
    public event System.Action<int> OnScoreChanged;
    public event System.Action<int> OnXPChanged;

    // Global State
    private int currentScore = 0;
    private int goodMovesCompleted = 0;
    private int totalGoodMoves = 0;
    private int mistakesMade = 0;
    private bool gameCompleted = false;
    private bool isGameActive = false;
    private float gameStartTime;
    private float totalTimeGiven;
    private float timeTaken;

    // XP (accumulated only)
    private int currentXP = 0;
    private int sessionXP = 0;
    private int totalXP = 0;

    // Speed thresholds
    private const float QUICK_LEARNER_THRESHOLD = 0.75f; // 75%+
    private const float EFFICIENT_THRESHOLD = 0.5f;      // 50-74%

    public enum GameType
    {
        TimeBased,      // Games with time limits (countdown/reverse)
        NonTimeBased    // Games without time limits
    }

    void Start()
    {
        if (autoFindManagers)
        {
            if (timerManager == null) timerManager = FindFirstObjectByType<TimerManager>();
            if (timeExtensionManager == null) timeExtensionManager = FindFirstObjectByType<TimeExtensionManager>();
            if (useUnifiedAudio && audioManager == null) audioManager = MiniGameAudioManager.Instance;
        }
        ValidateTimerSettings();

		// Load streak level if persisted
		streakLevel = PlayerPrefs.GetInt("MG_StreakLevel", streakLevel);
    }

    // Public API
    /// <summary>
    /// Initializes a new game session with the specified number of good moves and time limit.
    /// </summary>
    /// <param name="totalGoodMovesCount">Total number of good moves expected in this game</param>
    /// <param name="timeLimit">Time limit for the game (0 for no time limit)</param>
    public void InitializeGame(int totalGoodMovesCount, float timeLimit)
    {
        // Validate input parameters
        if (totalGoodMovesCount <= 0)
        {
            Debug.LogError($"MinigameScoreManager: Invalid totalGoodMovesCount ({totalGoodMovesCount}). Must be greater than 0.");
            totalGoodMovesCount = 1; // Set minimum valid value
        }

        if (timeLimit < 0)
        {
            Debug.LogWarning($"MinigameScoreManager: Negative time limit ({timeLimit}) provided. Setting to 0 (no time limit).");
            timeLimit = 0;
        }

        // Reset any existing game state
        if (isGameActive)
        {
            Debug.LogWarning("MinigameScoreManager: Initializing new game while previous game was active. Resetting state.");
        }

        totalGoodMoves = totalGoodMovesCount;
        totalTimeGiven = timeLimit;
        gameStartTime = Time.time;
        ResetScoreInternal();
        isGameActive = true;
        
        // Configure timer if enabled and time limit is set
        if (enableTimer && timeLimit > 0 && timerManager != null)
        {
            ConfigureTimer(timeLimit, warningThreshold, countDownTimer);
            StartTimer();
        }
        else if (enableTimer && timeLimit > 0 && timerManager == null)
        {
            Debug.LogWarning("MinigameScoreManager: Timer enabled but no TimerManager found. Timer will not work.");
        }
        
        UpdateUI();
        
        Debug.Log($"MinigameScoreManager: Game initialized - Good Moves: {totalGoodMoves}, Time Limit: {timeLimit}s, Timer: {(enableTimer && timeLimit > 0 ? "Enabled" : "Disabled")}");
    }

    /// <summary>
    /// Records a good move and awards points/XP. This is the main method for tracking successful actions.
    /// </summary>
    public void RecordGoodMove()
    {
        if (!isGameActive)
        {
            Debug.LogWarning("MinigameScoreManager: Cannot record good move - game is not active");
            return;
        }

        goodMovesCompleted++;
        
        // Calculate total score with bonuses
        int baseScore = basePointsPerGoodMove;
        int timeBonus = CalculateTimeBonus();
        int accuracyBonus = CalculateAccuracyBonus();
        int totalScore = baseScore + timeBonus + accuracyBonus;
        
        currentScore += totalScore;
        
        // Calculate XP with speed bonus
        int xpGained = xpPerGoodMove + CalculateSpeedBonusXP();
        AddXP(xpGained);
        
        PlayScoreSound(scoreUpdateSound);
        PlayScoreAdditionParticlesAtTarget();
        PlayXPGainParticlesAtTarget();
        UpdateUI();
        OnScoreChanged?.Invoke(currentScore);
        
        Debug.Log($"MinigameScoreManager: Good move recorded - Score: +{totalScore} (Base: {baseScore}, Time: +{timeBonus}, Accuracy: +{accuracyBonus}), XP: +{xpGained}");
    }

    /// <summary>
    /// Records a mistake and applies XP penalty. Use this for tracking incorrect actions.
    /// </summary>
    public void RecordMistake()
    {
        if (!isGameActive)
        {
            Debug.LogWarning("MinigameScoreManager: Cannot record mistake - game is not active");
            return;
        }

        mistakesMade++;
        AddXP(-xpPerMistake);
        PlayScoreSubtractionParticlesAtTarget();
        PlayXPLossParticlesAtTarget();
        UpdateUI();
        
        Debug.Log($"MinigameScoreManager: Mistake recorded - XP penalty: -{xpPerMistake}, Total mistakes: {mistakesMade}");
    }

    /// <summary>
    /// Legacy method for backward compatibility. Use RecordGoodMove() instead.
    /// </summary>
    [System.Obsolete("Use RecordGoodMove() instead for better clarity")]
    public void AddItemCompleted() => RecordGoodMove();

    public void CompleteGame(float timeTakenToComplete)
    {
        if (gameCompleted)
        {
            Debug.LogWarning("MinigameScoreManager: Game already completed, ignoring duplicate completion");
            return;
        }

        if (!isGameActive)
        {
            Debug.LogWarning("MinigameScoreManager: Cannot complete game - game is not active");
            return;
        }

        gameCompleted = true;
        isGameActive = false;
        timeTaken = Mathf.Max(0, timeTakenToComplete);
        
        // Stop timer if enabled
        if (enableTimer && timerManager != null) 
        {
            timerManager.StopTimer();
        }
        
        // Close time extension UI if available
        if (timeExtensionManager != null) 
        {
            timeExtensionManager.CloseOnGameComplete();
        }
        
		UpdateUI();
		
		// Calculate final statistics
		float finalAccuracy = GetAccuracy();
		float timeEfficiency = GetTimeSaved();
		
		// Compute final score structure
		CalculateFinalScore(finalAccuracy, timeEfficiency);

		// Refresh UI with final values (finalScore, stars, tiers)
		UpdateUI();

		// Update streak progression based on speed performance (50%+ time saved grows streak)
		if (timeSavedPercentage >= 0.50f)
		{
			streakLevel = Mathf.Clamp(streakLevel + 1, 0, 10);
		}
		else
		{
			streakLevel = 0;
		}
		PlayerPrefs.SetInt("MG_StreakLevel", streakLevel);
		
		// Log
		Debug.Log($"MinigameScoreManager: Game Completed - Score: {currentScore}, XP: {currentXP}, FinalScore: {finalScore}, " +
				 $"Accuracy: {finalAccuracy:P0}, Time Saved: {timeSavedPercentage:P0}, Tier: {speedTier}, Streak: {streakLevel}, " +
				 $"Good Moves: {goodMovesCompleted}/{totalGoodMoves}, Mistakes: {mistakesMade}");
		
		// Persist personal best
		if (finalScore > personalBest) personalBest = finalScore;
		
		// Sync database (stub)
		UpdatePlayerScore(finalScore);
    }

	/// <summary>
	/// Calculates the final score using coins, time, accuracy, and streaks.
	/// Applies progressive bonus tiers and sets speedTier.
	/// </summary>
	private void CalculateFinalScore(float accuracyRatio, float timeSavedRatio)
	{
		// Base score = Coins × 100. Use coinsCollected if provided; fallback to good moves as coins proxy
		int coins = Mathf.Max(0, coinsCollected > 0 ? coinsCollected : goodMovesCompleted);
		int baseScore = coins * 100;
		
		// Time saved percentage and speed tier multiplier
		timeSavedPercentage = Mathf.Clamp01(timeSavedRatio);
		float speedMultiplier;
		int flatSpeedBonus;
		if (timeSavedPercentage >= 0.90f) { speedMultiplier = 2.6f; speedTier = "Speed Demon"; flatSpeedBonus = 500; }
		else if (timeSavedPercentage >= 0.75f) { speedMultiplier = 2.3f; speedTier = "Swift"; flatSpeedBonus = 300; }
		else if (timeSavedPercentage >= 0.50f) { speedMultiplier = 2.0f; speedTier = "Fast"; flatSpeedBonus = 150; }
		else if (timeSavedPercentage >= 0.25f) { speedMultiplier = 1.5f; speedTier = "Steady"; flatSpeedBonus = 50; }
		else { speedMultiplier = 1.1f; speedTier = "Survivor"; flatSpeedBonus = 10; }
		
		// Accuracy multiplier based on mistakes
		float accuracyMultiplier;
		if (mistakesMade == 0)
		{
			accuracyMultiplier = 1.5f; // Perfect
			accuracyTier = "Perfect";
		}
		else if (mistakesMade <= 2)
		{
			accuracyMultiplier = 1.2f; // Near Perfect
			accuracyTier = "Near Perfect";
		}
		else
		{
			accuracyMultiplier = 1.0f; // Good
			accuracyTier = "Good";
		}
		
		// Streak multiplier up to 2.0x
		float streakMultiplier = Mathf.Min(1f + (streakLevel * 0.1f), 2.0f);
		
		// Time bonus for telemetry (not directly used in final formula but stored)
		timeBonus = Mathf.RoundToInt(baseScore * (speedMultiplier - 1f));
		
		// Final score
		float rawFinal = (baseScore * speedMultiplier * accuracyMultiplier * streakMultiplier) + flatSpeedBonus;
		finalScore = Mathf.FloorToInt(rawFinal);

		// Stars (1-3) based on time saved and mistakes
		if (timeSavedPercentage >= 0.75f && mistakesMade <= 2) stars = 3;
		else if (timeSavedPercentage >= 0.50f) stars = 2;
		else stars = 1;
		
		// Unlock achievements (stub)
		UnlockAchievements();
	}

	// Coins API to update coinsCollected and refresh UI
	public void SetCoinsCollected(int value)
	{
		coinsCollected = Mathf.Max(0, value);
		UpdateUI();
	}

	public void AddCoins(int delta)
	{
		if (delta == 0) return;
		coinsCollected = Mathf.Max(0, coinsCollected + delta);
		UpdateUI();
	}

	/// <summary>
	/// Unlocks achievements based on time saved thresholds.
	/// Replace with integration to your achievements system.
	/// </summary>
	private void UnlockAchievements()
	{
		if (timeSavedPercentage >= 0.90f) { UnlockAchievement("Speed Demon"); }
		if (timeSavedPercentage >= 0.80f) { UnlockAchievement("Time Master"); }
		if (timeSavedPercentage <= 0.05f) { UnlockAchievement("Last Second Hero"); }
	}

	private void UnlockAchievement(string key)
	{
		if (!achievements.Contains(key)) achievements.Add(key);
		Debug.Log($"Achievement Unlocked: {key}");

#if UNITY_ANDROID || UNITY_IOS
		// Optional: map to platform-specific achievement IDs
		try
		{
			// Example: use a single generic achievement id if available
			// Replace mapping as you add more achievement IDs
			if (Social.localUser != null && Social.localUser.authenticated)
			{
				#if UNITY_ANDROID
				Social.ReportProgress(GPGSIds.achievement_finansachievement, 100.0, _ => { });
				#endif
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"Achievement report failed: {ex.Message}");
		}
#endif
	}

	/// <summary>
	/// Syncs final score to a centralized database. Replace with your implementation.
	/// </summary>
    private void UpdatePlayerScore(int score)
    {
        Debug.Log($"Final Score Synced: {score}");

#if UNITY_ANDROID || UNITY_IOS
        // Report to platform leaderboard if available
        try
        {
            if (Social.localUser != null && Social.localUser.authenticated)
            {
                #if UNITY_ANDROID
                Social.ReportScore(score, GPGSIds.leaderboard_finans_leaderboard, _ => { });
                #endif
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Leaderboard report failed: {ex.Message}");
        }
#endif

        // Firestore sync (best-effort)
        try
        {
            string uid = null;
            try
            {
                uid = Params.__auth != null && Params.__auth.CurrentUser != null ? Params.__auth.CurrentUser.UserId : null;
            }
            catch { uid = null; }

            if (!string.IsNullOrEmpty(uid))
            {
                var data = new Dictionary<string, object>()
                {
                    { "coins", Mathf.Max(0, coinsCollected) },
                    { "finalScore", score },
                    { "timeBonus", timeBonus },
                    { "timeSavedPercentage", Math.Round(timeSavedPercentage, 3) },
                    { "speedTier", speedTier },
                    { "accuracyTier", accuracyTier },
                    { "streakLevel", streakLevel },
                    { "timeTaken", Math.Round(timeTaken, 3) },
                    { "itemsCompleted", goodMovesCompleted },
                    { "personalBest", personalBest },
                    { "achievements", achievements.ToArray() },
                    { "timestampUtc", DateTime.UtcNow.ToString("o") }
                };

                var op = new FirestoreDataOperationManager();
                string collection = IFirestoreEnums.FSCollection.parent.ToString();
                string subcollection = "scores";
                string subdocument = DateTime.UtcNow.Ticks.ToString();
                _ = op.FirestoreDataSave(collection, uid, subcollection, subdocument, data);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Firestore sync skipped/failed: {ex.Message}");
        }
    }

    public int GetCurrentScore() => currentScore;
    public int GetGoodMovesCompleted() => goodMovesCompleted;
    public int GetItemsCompleted() => goodMovesCompleted;
    public int GetTotalGoodMoves() => totalGoodMoves;
    public int GetMistakesMade() => mistakesMade;
    public bool IsGameCompleted() => gameCompleted;
    public float GetTimeTaken() => timeTaken;
    public float GetTimeSavedPercentage() => timeSavedPercentage;
    public string GetAccuracyTier() => accuracyTier;
    public string GetSpeedTier() => speedTier;
    public List<string> GetAchievements() => achievements;
    public float GetTimeSaved() => totalTimeGiven > 0 ? (totalTimeGiven - timeTaken) / totalTimeGiven : 0f;
    public float GetAccuracy() => totalGoodMoves > 0 ? (float)goodMovesCompleted / totalGoodMoves : 0f;

    // XP simple accumulation
    public int GetCurrentXP() => currentXP;
    public int GetSessionXP() => sessionXP;
    public int GetTotalXP() => totalXP;

    // UI Text Management
    /// <summary>
    /// Sets custom score label text.
    /// </summary>
    public void SetScoreLabel(string label)
    {
        scoreLabel = label;
        UpdateUI();
    }

    /// <summary>
    /// Sets custom XP label text.
    /// </summary>
    public void SetXPLabel(string label)
    {
        xpLabel = label;
        UpdateUI();
    }

    /// <summary>
    /// Toggles between showing numbers only or with labels.
    /// </summary>
    public void SetShowNumbersOnly(bool numbersOnly)
    {
        showNumbersOnly = numbersOnly;
        UpdateUI();
    }

    /// <summary>
    /// Gets the current score label text.
    /// </summary>
    public string GetScoreLabel() => scoreLabel;

    /// <summary>
    /// Gets the current XP label text.
    /// </summary>
    public string GetXPLabel() => xpLabel;

    /// <summary>
    /// Comprehensive test method to validate all functionality.
    /// Call this method to test the score manager's robustness.
    /// </summary>
    [ContextMenu("Test Score Manager")]
    public void TestScoreManager()
    {
        Debug.Log("=== MinigameScoreManager Test Started ===");
        
        // Test 1: Initialize game
        Debug.Log("Test 1: Initializing game with 5 good moves, 30s time limit");
        InitializeGame(5, 30f);
        
        // Test 2: Record good moves
        Debug.Log("Test 2: Recording 3 good moves");
        for (int i = 0; i < 3; i++)
        {
            RecordGoodMove();
            Debug.Log($"  Good move {i + 1}: Score={currentScore}, XP={currentXP}");
        }
        
        // Test 3: Record mistakes
        Debug.Log("Test 3: Recording 1 mistake");
        RecordMistake();
        Debug.Log($"  After mistake: Score={currentScore}, XP={currentXP}");
        
        // Test 4: Test UI text formatting
        Debug.Log("Test 4: Testing UI text formatting");
        SetShowNumbersOnly(true);
        Debug.Log($"  Numbers only mode: Score='{FormatScoreText(currentScore)}', XP='{FormatXPText(currentXP)}'");
        
        SetShowNumbersOnly(false);
        SetScoreLabel("Points");
        SetXPLabel("Coins");
        Debug.Log($"  With labels: Score='{FormatScoreText(currentScore)}', XP='{FormatXPText(currentXP)}'");
        
        // Test 5: Complete game
        Debug.Log("Test 5: Completing game");
        CompleteGame(25f);
        
        // Test 6: Test edge cases
        Debug.Log("Test 6: Testing edge cases");
        
        // Test negative values
        InitializeGame(-1, -5f);
        Debug.Log($"  Negative inputs handled: GoodMoves={totalGoodMoves}, TimeLimit={totalTimeGiven}");
        
        // Test zero values
        InitializeGame(0, 0f);
        Debug.Log($"  Zero inputs handled: GoodMoves={totalGoodMoves}, TimeLimit={totalTimeGiven}");
        
        // Test 7: Test bonus calculations
        Debug.Log("Test 7: Testing bonus calculations");
        InitializeGame(10, 60f);
        RecordGoodMove();
        Debug.Log($"  Time bonus: {CalculateTimeBonus()}, Accuracy bonus: {CalculateAccuracyBonus()}, Speed XP bonus: {CalculateSpeedBonusXP()}");
        
        Debug.Log("=== MinigameScoreManager Test Completed ===");
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        PlayScoreSound(scoreUpdateSound);
        UpdateUI();
        OnScoreChanged?.Invoke(currentScore);
    }
    public void AddScore() => AddScore(scoreIncrement);
    public void SubtractScore(int amount)
    {
        currentScore -= amount;
        PlayScoreSound(scoreUpdateSound);
        UpdateUI();
        OnScoreChanged?.Invoke(currentScore);
    }
    public void SubtractScore() => SubtractScore(scoreDecrement);

    // Internals
    private void ResetScoreInternal()
    {
        currentScore = 0;
        goodMovesCompleted = 0;
        mistakesMade = 0;
        gameCompleted = false;
        sessionXP = 0;
        timeTaken = 0f;
        UpdateUI();
    }

    private int CalculateSpeedBonusXP()
    {
        if (totalTimeGiven <= 0 || gameStartTime <= 0f) return 0;
        float elapsed = Time.time - gameStartTime;
        float timeSaved = Mathf.Clamp01((totalTimeGiven - elapsed) / totalTimeGiven);
        if (timeSaved >= QUICK_LEARNER_THRESHOLD) return quickLearnerBonus;
        if (timeSaved >= EFFICIENT_THRESHOLD) return efficientBonus;
        return 0;
    }

    /// <summary>
    /// Calculates time-based bonus points for good moves.
    /// </summary>
    private int CalculateTimeBonus()
    {
        if (totalTimeGiven <= 0 || gameStartTime <= 0f) return 0;
        
        float elapsed = Time.time - gameStartTime;
        float timeRemaining = Mathf.Max(0, totalTimeGiven - elapsed);
        float timeSavedRatio = timeRemaining / totalTimeGiven;
        
        // More time remaining = higher bonus
        return Mathf.RoundToInt(timeSavedRatio * timeBonusMultiplier * basePointsPerGoodMove);
    }

    /// <summary>
    /// Calculates accuracy-based bonus points for good moves.
    /// </summary>
    private int CalculateAccuracyBonus()
    {
        if (totalGoodMoves <= 0) return 0;
        
        float currentAccuracy = (float)goodMovesCompleted / totalGoodMoves;
        
        // Higher accuracy = higher bonus
        return Mathf.RoundToInt(currentAccuracy * accuracyMultiplier * basePointsPerGoodMove);
    }

    // UI
    private void UpdateUI()
    {
        string scoreText = FormatScoreText(currentScore);
        string xpText = FormatXPText(currentXP);
		string accuracyText = FormatAccuracyText(GetAccuracy());
		string coinsText = FormatCoinsText(coinsCollected);
		string starsText = FormatStarsText(stars);
		string finalScoreText = FormatFinalScoreText(finalScore);
        
        UpdateTextArray(scoreTexts, scoreText);
        UpdateTextArray(xpTexts, xpText);
		UpdateTextArray(accuracyTexts, accuracyText);
		UpdateTextArray(coinTexts, coinsText);
		UpdateTextArray(starTexts, starsText);
		UpdateTextArray(finalScoreTexts, finalScoreText);
    }

    /// <summary>
    /// Formats the score text based on current settings.
    /// </summary>
    private string FormatScoreText(int score)
    {
        if (showNumbersOnly)
        {
            return score.ToString();
        }
        
        if (!string.IsNullOrEmpty(scoreLabel))
        {
            return $"{scoreLabel}: {score}";
        }
        
        return $"Score: {score}";
    }

    /// <summary>
    /// Formats the XP text based on current settings.
    /// </summary>
    private string FormatXPText(int xp)
    {
        if (showNumbersOnly)
        {
            return xp.ToString();
        }
        
        if (!string.IsNullOrEmpty(xpLabel))
        {
            return $"{xpLabel}: {xp}";
        }
        
        return $"XP: {xp}";
    }

	/// <summary>
	/// Formats the coins text based on current settings.
	/// </summary>
	private string FormatCoinsText(int coins)
	{
		if (showNumbersOnly)
		{
			return coins.ToString();
		}
		return $"Coins: {coins}";
	}

	/// <summary>
	/// Formats the stars text based on current settings.
	/// </summary>
	private string FormatStarsText(int starsValue)
	{
		if (showNumbersOnly)
		{
			return starsValue.ToString();
		}
		return $"Stars: {starsValue}";
	}

	/// <summary>
	/// Formats the final score text based on current settings.
	/// </summary>
	private string FormatFinalScoreText(int value)
	{
		if (showNumbersOnly)
		{
			return value.ToString();
		}
		return $"Final: {value}";
	}

	/// <summary>
	/// Formats the accuracy text as a percentage (0-100%).
	/// </summary>
	private string FormatAccuracyText(float accuracy)
	{
		return $"{accuracy:P0}";
	}

    private void UpdateTextArray(TextMeshProUGUI[] textArray, string text)
    {
        if (textArray == null) return;
        foreach (var t in textArray)
        {
            if (t != null) t.text = text;
        }
    }

    // XP
    public void AddXP(int xpAmount)
    {
        if (xpAmount == 0) return;
        
        // Store previous XP for logging
        int previousXP = currentXP;
        
        // Apply XP change
        currentXP = Mathf.Max(0, currentXP + xpAmount);
        sessionXP += xpAmount;
        totalXP += xpAmount;
        
        // Play appropriate sound
        if (xpAmount > 0) 
        {
            PlayScoreSound(xpGainSound);
        }
        else if (xpAmount < 0)
        {
            PlayScoreSound(scoreDecreaseSound);
        }
        
        OnXPChanged?.Invoke(currentXP);
        UpdateUI();
        
        // Log XP change for debugging
        if (Mathf.Abs(xpAmount) > 0)
        {
            Debug.Log($"MinigameScoreManager: XP changed by {xpAmount} (from {previousXP} to {currentXP})");
        }
    }

    // Timer
    private void ValidateTimerSettings()
    {
        if (warningThreshold >= initialTimeLimit)
        {
            warningThreshold = Mathf.Max(5f, initialTimeLimit * 0.2f);
        }
        initialTimeLimit = Mathf.Clamp(initialTimeLimit, 10f, 600f);
        warningThreshold = Mathf.Clamp(warningThreshold, 5f, 60f);
    }

    public void ConfigureTimer(float timeLimit, float warningThresh, bool countDown = true)
    {
        if (warningThresh >= timeLimit) warningThresh = Mathf.Max(5f, timeLimit * 0.2f);
        initialTimeLimit = timeLimit;
        warningThreshold = warningThresh;
        countDownTimer = countDown;
        if (autoSyncTimerManager && timerManager != null)
        {
            timerManager.initialTime = initialTimeLimit;
            timerManager.warningThreshold = warningThreshold;
            timerManager.countDown = countDownTimer;
        }
    }

    public void StartTimer()
    {
        if (!enableTimer || timerManager == null) return;
        if (autoSyncTimerManager)
        {
            timerManager.initialTime = initialTimeLimit;
            timerManager.warningThreshold = warningThreshold;
            timerManager.countDown = countDownTimer;
        }
        timerManager.OnTimeUp.AddListener(OnTimerTimeUp);
        timerManager.OnWarningTime.AddListener(OnTimerWarning);
        timerManager.StartTimer();
    }

    private void InitializeTimer()
    {
        if (timerManager == null) return;
        
        // Sync TimerManager with our settings
        timerManager.initialTime = initialTimeLimit;
        timerManager.warningThreshold = warningThreshold;
        timerManager.countDown = countDownTimer;
        
        // Subscribe to timer events
        timerManager.OnTimeUp.AddListener(OnTimerTimeUp);
        timerManager.OnWarningTime.AddListener(OnTimerWarning);
    }

    public void StopTimer()
    {
        if (timerManager == null) return;
        timerManager.StopTimer();
    }

    public void EnableTimer(bool enable)
    {
        enableTimer = enable;
        if (timerManager != null)
        {
            if (enable)
            {
                timerManager.StartTimer();
            }
            else
            {
                timerManager.StopTimer();
            }
        }
    }

    public void SetGameType(GameType newGameType)
    {
        gameType = newGameType;
        enableTimer = (gameType == GameType.TimeBased);
        
        if (timerManager != null)
        {
            if (enableTimer)
            {
                InitializeTimer();
            }
            else
            {
                timerManager.StopTimer();
            }
        }
    }

    // Properties for TimerManager compatibility
    public float MasterTimeLimit => initialTimeLimit;
    public float MasterWarningThreshold => warningThreshold;
    public bool MasterCountDown => countDownTimer;

    // Timer control methods
    public void PauseTimer()
    {
        if (timerManager == null) return;
        timerManager.PauseTimer();
    }

    public void ResumeTimer()
    {
        if (timerManager == null) return;
        timerManager.ResumeTimer();
    }

    public float GetRemainingTime()
    {
        if (timerManager == null) return 0f;
        return timerManager.GetRemainingTime();
    }

    public bool IsTimerRunning()
    {
        if (timerManager == null) return false;
        return timerManager.IsRunning;
    }

    public bool IsTimerEnabled()
    {
        return enableTimer && gameType == GameType.TimeBased;
    }

    private void OnTimerTimeUp()
    {
        if (!gameCompleted) CompleteGame(initialTimeLimit);
        if (timeExtensionManager != null) timeExtensionManager.CloseOnGameComplete();
    }

    private void OnTimerWarning()
    {
        if (gameCompleted && timeExtensionManager != null)
        {
            timeExtensionManager.CloseOnGameComplete();
        }
    }

    // Audio
    private void PlayScoreSound(AudioClip clip)
    {
        if (clip == null) return;
        if (useUnifiedAudio && audioManager != null)
        {
            audioManager.PlaySFX(clip);
        }
        else if (scoreAudioSource != null)
        {
            scoreAudioSource.PlayOneShot(clip);
        }
    }

    // Particles (score/xp only)
    private void PlayScoreParticles(ParticleSystem ps, int burstCount)
    {
        if (!enableScoreParticles || ps == null) return;
        if (burstCount > 0)
        {
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, burstCount) });
        }
        ps.transform.position = GetDefaultParticlePosition() + particleSpawnOffset;
        ps.Play();
        if (autoDestroyParticles) StartCoroutine(DestroyParticlesAfterDelay(ps, particleDuration));
    }

    private void PlayScoreAdditionParticlesAtTarget() => PlayScoreParticles(scoreAdditionParticles, scoreParticleBurstCount);
    private void PlayScoreSubtractionParticlesAtTarget() => PlayScoreParticles(scoreSubtractionParticles, scoreParticleBurstCount);
    private void PlayXPGainParticlesAtTarget() => PlayScoreParticles(xpGainParticles, xpParticleBurstCount);
    private void PlayXPLossParticlesAtTarget() => PlayScoreParticles(xpLossParticles, xpParticleBurstCount);

    private System.Collections.IEnumerator DestroyParticlesAfterDelay(ParticleSystem particles, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (particles != null)
        {
            particles.Stop();
            particles.Clear();
            if (particles.gameObject.scene.IsValid())
            {
                Destroy(particles.gameObject);
            }
        }
    }

    private Vector3 GetDefaultParticlePosition()
    {
        if (scoreTexts != null)
        {
            foreach (var text in scoreTexts)
            {
                if (text != null) return text.transform.position;
            }
        }
        return Camera.main != null ? Camera.main.transform.position + Vector3.forward * 5f : Vector3.zero;
    }
}


