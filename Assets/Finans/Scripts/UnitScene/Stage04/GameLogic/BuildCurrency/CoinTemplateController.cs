using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MasterCoinGameManager : MonoBehaviour
{
    public static MasterCoinGameManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    [Header("Coin Prefixes (e.g., Penny_Head_, Dime_Tail_)")]
    public List<string> coinPrefixes; // Assign in Inspector or fill at runtime

    [Header("Session Settings")]
    [Range(1, 12)]
    public int coinsPerSession = 3; // Number of coins to play in one session
    [Range(0.5f, 5f)]
    public float coinSpawnDelay = 1f; // Delay between coin spawns in seconds

    [Header("Panels")]
    public List<GameObject> coinTextPanels;      // All CoinTextPanel objects in scene
    public List<GameObject> referencePanels;     // All ReferencePanel objects in scene
    public List<GameObject> dropzonePanels;      // All DropzonePanel objects in scene
    public List<GameObject> trayPanels;          // All TrayPanel objects in scene

    [Header("UI")]
    public GameObject winPanel; // Assign the win UI panel in Inspector

    [Header("Coin Sounds")]
    public List<CoinSound> coinSounds; // Assign in Inspector: one per prefix
    public AudioSource audioSource; // Assign in Inspector or add at runtime

    [Header("Sound Delay Settings")]
    [Range(0f, 3f)]
    public float loadSoundDelay = 0.5f; // Delay before playing load sound
    [Range(0f, 3f)]
    public float completeSoundDelay = 0.2f; // Delay before playing complete sound
    [Range(0f, 2f)]
    public float defaultLoadDelay = 0.3f; // Default delay for load sounds
    [Range(0f, 2f)]
    public float defaultCompleteDelay = 0.1f; // Default delay for complete sounds

    [System.Serializable]
    public class CoinSound
    {
        public string prefix;
        public AudioClip loadSound;
        public AudioClip completeSound;
        [Range(0f, 3f)]
        public float customLoadDelay = -1f; // -1 means use default, otherwise use this value
        [Range(0f, 3f)]
        public float customCompleteDelay = -1f; // -1 means use default, otherwise use this value
    }

    private List<string> sessionPrefixes = new List<string>();
    private int currentIndex = 0;
    private float gameStartTime;

    public TimerManager timerManager;

    void Start()
    {
        // Validate coin prefixes first
        ValidateCoinPrefixes();

        if (timerManager != null)
        {
            timerManager.OnTimeUp.AddListener(OnGameTimeUp);
        }

        // Initialize scoring system
        var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
        if (scoreManager != null)
        {
            scoreManager.InitializeGame(coinsPerSession, timerManager != null ? timerManager.initialTime : 300f);
        }

        // Initialize educational performance tracking
        if (EducationalPerformanceTracker_Enhanced.Instance != null)
        {
            string difficulty = GetDifficultyLevel();
            EducationalPerformanceTracker_Enhanced.DifficultyLevel difficultyLevel = EducationalPerformanceTracker_Enhanced.DifficultyLevel.Medium;

            // Convert string difficulty to enum
            switch (difficulty.ToLower())
            {
                case "easy": difficultyLevel = EducationalPerformanceTracker_Enhanced.DifficultyLevel.Easy; break;
                case "medium": difficultyLevel = EducationalPerformanceTracker_Enhanced.DifficultyLevel.Medium; break;
                case "hard": difficultyLevel = EducationalPerformanceTracker_Enhanced.DifficultyLevel.Hard; break;
                case "expert": difficultyLevel = EducationalPerformanceTracker_Enhanced.DifficultyLevel.Expert; break;
            }

            EducationalPerformanceTracker_Enhanced.Instance.InitializeLearningSession(
                EducationalPerformanceTracker_Enhanced.ActivityType.CoinGame,
                coinsPerSession,
                timerManager != null ? timerManager.initialTime : 300f,
                difficultyLevel
            );
        }

        // Validate coinsPerSession
        if (coinsPerSession <= 0)
        {
            Debug.LogWarning("coinsPerSession must be greater than 0. Setting to 1.");
            coinsPerSession = 1;
        }

        // The system automatically adjusts if you set too many coins:
        if (coinsPerSession > coinPrefixes.Count)
        {
            coinsPerSession = coinPrefixes.Count;  // Limits to available prefixes
        }

        // Pick unique coins for this session
        sessionPrefixes = new List<string>(coinPrefixes);
        Shuffle(sessionPrefixes);
        sessionPrefixes = sessionPrefixes.GetRange(0, Mathf.Min(coinsPerSession, sessionPrefixes.Count));
        currentIndex = 0;

        /*        Debug.Log($"Session started with {sessionPrefixes.Count} coins: {string.Join(", ", sessionPrefixes)}");
                Debug.Log($"Available coin prefixes: {string.Join(", ", coinPrefixes)}");
                Debug.Log($"Coins per session setting: {coinsPerSession}");
        */
        // Print detailed panel setup for debugging
        PrintPanelSetup();

        gameStartTime = Time.time;
        ShowCurrentCoin();
    }

    // Validate coin prefixes and auto-populate if needed
    private void ValidateCoinPrefixes()
    {
        // Check if coinPrefixes is empty or null
        if (coinPrefixes == null || coinPrefixes.Count == 0)
        {
            Debug.LogWarning("Coin prefixes list is empty! Attempting to auto-populate from scene objects...");
            AutoPopulateCoinPrefixes();
        }

        // Remove any null or empty entries
        coinPrefixes.RemoveAll(prefix => string.IsNullOrEmpty(prefix));

        // Validate that we have at least some prefixes
        if (coinPrefixes.Count == 0)
        {
            Debug.LogError("No valid coin prefixes found! Please add coin prefixes in the inspector or ensure coin panels exist in the scene.");
            return;
        }

        Debug.Log($"Validated {coinPrefixes.Count} coin prefixes: {string.Join(", ", coinPrefixes)}");
    }

    // Auto-populate coin prefixes from scene objects
    private void AutoPopulateCoinPrefixes()
    {
        coinPrefixes = new List<string>();

        // Method 1: Try to find prefixes from coin text panels
        if (coinTextPanels != null && coinTextPanels.Count > 0)
        {
            foreach (var panel in coinTextPanels)
            {
                if (panel != null)
                {
                    string prefix = ExtractPrefixFromName(panel.name);
                    if (!string.IsNullOrEmpty(prefix) && !coinPrefixes.Contains(prefix))
                    {
                        coinPrefixes.Add(prefix);
                        Debug.Log($"Auto-added coin prefix from text panel: {prefix}");
                    }
                }
            }
        }

        // Method 2: Try reference panels
        if (coinPrefixes.Count == 0 && referencePanels != null && referencePanels.Count > 0)
        {
            foreach (var panel in referencePanels)
            {
                if (panel != null)
                {
                    string prefix = ExtractPrefixFromName(panel.name);
                    if (!string.IsNullOrEmpty(prefix) && !coinPrefixes.Contains(prefix))
                    {
                        coinPrefixes.Add(prefix);
                        Debug.Log($"Auto-added coin prefix from reference panel: {prefix}");
                    }
                }
            }
        }

        // Method 3: Try dropzone panels
        if (coinPrefixes.Count == 0 && dropzonePanels != null && dropzonePanels.Count > 0)
        {
            foreach (var panel in dropzonePanels)
            {
                if (panel != null)
                {
                    string prefix = ExtractPrefixFromName(panel.name);
                    if (!string.IsNullOrEmpty(prefix) && !coinPrefixes.Contains(prefix))
                    {
                        coinPrefixes.Add(prefix);
                        Debug.Log($"Auto-added coin prefix from dropzone panel: {prefix}");
                    }
                }
            }
        }

        // Method 4: Try tray panels
        if (coinPrefixes.Count == 0 && trayPanels != null && trayPanels.Count > 0)
        {
            foreach (var panel in trayPanels)
            {
                if (panel != null)
                {
                    string prefix = ExtractPrefixFromName(panel.name);
                    if (!string.IsNullOrEmpty(prefix) && !coinPrefixes.Contains(prefix))
                    {
                        coinPrefixes.Add(prefix);
                        Debug.Log($"Auto-added coin prefix from tray panel: {prefix}");
                    }
                }
            }
        }

        // Method 5: Search all GameObjects in scene for coin prefixes
        if (coinPrefixes.Count == 0)
        {
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            string[] coinTypes = { "Penny", "Nickel", "Dime", "Quarter", "Dollar" };

            foreach (var obj in allObjects)
            {
                foreach (string coinType in coinTypes)
                {
                    if (obj.name.Contains(coinType) && obj.name.Contains("_"))
                    {
                        string prefix = ExtractPrefixFromName(obj.name);
                        if (!string.IsNullOrEmpty(prefix) && !coinPrefixes.Contains(prefix))
                        {
                            coinPrefixes.Add(prefix);
                            Debug.Log($"Auto-added coin prefix from scene object: {prefix} (from {obj.name})");
                        }
                    }
                }
            }
        }

        // Method 6: Add default coin prefixes if still empty
        if (coinPrefixes.Count == 0)
        {
            string[] defaultPrefixes = { "Penny_Head_", "Penny_Tail_", "Nickel_Head_", "Nickel_Tail_", "Dime_Head_", "Dime_Tail_", "Quarter_Head_", "Quarter_Tail_", "Dollar_Head_", "Dollar_Tail_" };
            foreach (string prefix in defaultPrefixes)
            {
                coinPrefixes.Add(prefix);
            }
            Debug.Log("No coin prefixes found in scene. Using default prefixes.");
        }
    }

    // Extract prefix from panel name based on actual project naming conventions
    private string ExtractPrefixFromName(string panelName)
    {
        // Based on actual scene analysis, the naming patterns are:
        // - Coin prefixes: "Dollar_Tail_", "Penny_Head_", etc.
        // - Panel names: "Tray_Panel", "Ref_Panel", "Text (TMP)", etc.

        // First, check if it's a coin prefix (ends with underscore)
        if (panelName.EndsWith("_"))
        {
            return panelName;
        }

        // Check for common panel suffixes and extract the coin prefix
        string[] suffixes = {
            "_TextPanel", "_ReferencePanel", "_DropzonePanel", "_TrayPanel", "_Panel",
            "_Text", "_Ref", "_Dropzone", "_Tray"
        };

        foreach (string suffix in suffixes)
        {
            if (panelName.EndsWith(suffix))
            {
                string prefix = panelName.Substring(0, panelName.Length - suffix.Length);
                // If the prefix ends with underscore, it's likely a coin prefix
                if (prefix.EndsWith("_"))
                {
                    return prefix;
                }
            }
        }

        // Check if the panel name contains coin prefixes
        string[] coinTypes = { "Penny", "Nickel", "Dime", "Quarter", "Dollar" };
        foreach (string coinType in coinTypes)
        {
            if (panelName.Contains(coinType))
            {
                // Try to extract the full prefix (e.g., "Penny_Head_")
                int coinIndex = panelName.IndexOf(coinType);
                int underscoreIndex = panelName.IndexOf("_", coinIndex);
                if (underscoreIndex != -1)
                {
                    int secondUnderscore = panelName.IndexOf("_", underscoreIndex + 1);
                    if (secondUnderscore != -1)
                    {
                        return panelName.Substring(coinIndex, secondUnderscore + 1);
                    }
                    else
                    {
                        return panelName.Substring(coinIndex, underscoreIndex + 1);
                    }
                }
                else
                {
                    return coinType + "_";
                }
            }
        }

        // If no pattern found, return the name as is
        return panelName;
    }

    // Get difficulty level based on session settings
    private string GetDifficultyLevel()
    {
        if (coinsPerSession <= 2) return "Easy";
        if (coinsPerSession <= 4) return "Medium";
        if (coinsPerSession <= 6) return "Hard";
        return "Expert";
    }

    void ShowCurrentCoin()
    {
        if (currentIndex >= sessionPrefixes.Count)
        {
            Debug.LogError($"Current index ({currentIndex}) is out of bounds for session prefixes ({sessionPrefixes.Count})");
            return;
        }

        string prefix = sessionPrefixes[currentIndex];
        Debug.Log($"Showing coin {currentIndex + 1}/{sessionPrefixes.Count}: {prefix}");

        // Deactivate all panels first
        DeactivateAllPanels();

        // Activate panels for current coin
        SetPanelActive(coinTextPanels, prefix);
        SetPanelActive(referencePanels, prefix);
        SetPanelActive(dropzonePanels, prefix);
        SetPanelActive(trayPanels, prefix);

        // Play coin load sound
        PlayCoinSound(prefix, false);

        // Find the tray panel for the current prefix
        var trayPanel = FindPanelWithPrefix(trayPanels, prefix);
        if (trayPanel != null)
        {
            Debug.Log($"Found tray panel for {prefix}: {trayPanel.name}");
            // Collect all child objects as required parts, regardless of their active state
            var requiredParts = new List<GameObject>();
            foreach (Transform child in trayPanel.transform)
            {
                requiredParts.Add(child.gameObject);
            }
            // Deactivate all children in the trayPanel to avoid duplicates in the scene
            foreach (Transform child in trayPanel.transform)
            {
                child.gameObject.SetActive(false);
            }

            if (TrayManager.Instance != null)
            {
                TrayManager.Instance.SetupTray(requiredParts.ToArray());
                Debug.Log($"Setup tray with {requiredParts.Count} parts for {prefix}");
            }
            else
            {
                Debug.LogWarning("TrayManager.Instance is null!");
            }
        }
        else
        {
            Debug.LogWarning($"No tray panel found for prefix: {prefix}");
        }

        var dropzone = FindPanelWithPrefix(dropzonePanels, prefix);
        if (dropzone != null)
        {
            //            Debug.Log($"Found dropzone panel for {prefix}: {dropzone.name}");
            var controller = dropzone.GetComponent<DropzonePanelController>();
            if (controller != null)
            {
                controller.OnComplete -= OnCoinCompleteCoroutine;
                controller.OnComplete += OnCoinCompleteCoroutine;
                // Wire up sound for completion
                controller.OnComplete -= () => PlayCoinSound(prefix, true);
                controller.OnComplete += () => PlayCoinSound(prefix, true);
            }
            else
            {
                Debug.LogWarning($"DropzonePanelController not found on {dropzone.name}");
            }
        }
        else
        {
            Debug.LogWarning($"No dropzone panel found for prefix: {prefix}");
        }
    }

    void OnCoinComplete()
    {
        // Old method, now unused
    }

    private void OnCoinCompleteCoroutine()
    {
        StartCoroutine(HandleCoinCompleteCoroutine());
    }

    private System.Collections.IEnumerator HandleCoinCompleteCoroutine()
    {
        var dropzone = FindPanelWithPrefix(dropzonePanels, sessionPrefixes[currentIndex]);
        if (dropzone != null)
        {
            var controller = dropzone.GetComponent<DropzonePanelController>();
            if (controller != null)
                controller.OnComplete -= OnCoinCompleteCoroutine;
        }

        // Add score for completed coin
        var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
        if (scoreManager != null)
        {
            scoreManager.RecordGoodMove();
        }

        // Record educational metrics for completed coin
        if (EducationalPerformanceTracker_Enhanced.Instance != null)
        {
            string conceptName = sessionPrefixes[currentIndex];
            bool isCorrect = true; // Coin was completed successfully
            float responseTime = Time.time - gameStartTime; // Approximate response time
            bool isTimePressure = responseTime < 5f; // Less than 5 seconds is time pressure

            EducationalPerformanceTracker_Enhanced.Instance.RecordItemAttempt(
                conceptName,
                isCorrect,
                responseTime,
                "", // no wrong answer type since it's correct
                isTimePressure,
                false, // no hint used
                false  // not skipped
            );
        }

        yield return new WaitForSeconds(coinSpawnDelay);
        currentIndex++;
        if (currentIndex < sessionPrefixes.Count)
        {
            ShowCurrentCoin();
        }
        else
        {
            Debug.Log("All coins completed!");

            // Complete the game in scoring system
            var sm = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (sm != null)
            {
                sm.CompleteGame(Time.time - gameStartTime);
            }

            // Complete educational learning session
            if (EducationalPerformanceTracker_Enhanced.Instance != null)
            {
                var sm2 = MiniGameServices.MinigameScoreService.GetClosest(transform);
                int finalScore = sm2 != null ? sm2.GetCurrentScore() : 0;
                EducationalPerformanceTracker_Enhanced.Instance.CompleteLearningSession(finalScore);
            }

            // Stop the timer and its sounds immediately
            if (timerManager != null)
            {
                timerManager.StopTimer();
                timerManager.StopWarningSound();
                Debug.Log("Timer stopped - all coins completed!");
            }

            // Show win panel with star rating system
            if (winPanel != null)
            {
                winPanel.SetActive(true);
                Debug.Log("Win panel activated!");

                // Trigger star rating calculation and display
                var winPanelController = winPanel.GetComponent<WinPanelController>();
                if (winPanelController != null)
                {
                    winPanelController.ShowWinPanel();
                }
                else
                {
                    Debug.LogWarning("WinPanelController not found on win panel! Please add WinPanelController component.");
                }
            }
        }
    }

    void DeactivateAllPanels()
    {
        // Deactivate all coin text panels
        if (coinTextPanels != null)
        {
            foreach (var panel in coinTextPanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        // Deactivate all reference panels
        if (referencePanels != null)
        {
            foreach (var panel in referencePanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        // Deactivate all dropzone panels
        if (dropzonePanels != null)
        {
            foreach (var panel in dropzonePanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        // Deactivate all tray panels
        if (trayPanels != null)
        {
            foreach (var panel in trayPanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }
    }

    void SetPanelActive(List<GameObject> panels, string prefix)
    {
        if (panels == null) return;

        int activatedCount = 0;
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                bool shouldActivate = panel.name.StartsWith(prefix);
                panel.SetActive(shouldActivate);
                if (shouldActivate)
                {
                    activatedCount++;
                    //           Debug.Log($"Activated panel: {panel.name} for prefix: {prefix}");
                }
            }
        }

        if (activatedCount == 0)
        {
            Debug.LogWarning($"No panels found for prefix: {prefix}");
        }
        else
        {
            Debug.Log($"Activated {activatedCount} panels for prefix: {prefix}");
        }
    }

    GameObject FindPanelWithPrefix(List<GameObject> panels, string prefix)
    {
        foreach (var panel in panels)
        {
            if (panel != null && panel.name.StartsWith(prefix))
                return panel;
        }
        return null;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public string GetCurrentPrefix()
    {
        return sessionPrefixes[currentIndex];
    }

    public List<string> GetAllSessionPrefixes()
    {
        return new List<string>(sessionPrefixes);
    }

    void PlayCoinSound(string prefix, bool isComplete)
    {
        if (audioSource == null) return;

        foreach (var cs in coinSounds)
        {
            if (cs.prefix == prefix)
            {
                var clip = isComplete ? cs.completeSound : cs.loadSound;
                if (clip != null)
                {
                    // Calculate delay based on sound type and custom settings
                    float delay = CalculateSoundDelay(cs, isComplete);
                    StartCoroutineSafely(PlayDelayedSound(clip, delay));
                }
                break;
            }
        }
    }

    private void StartCoroutineSafely(System.Collections.IEnumerator routine)
    {
        if (routine == null) return;
        if (isActiveAndEnabled)
        {
            StartCoroutine(routine);
            return;
        }

        // Try unified audio manager as host if available
        if (MiniGameAudioManager.Instance != null && MiniGameAudioManager.Instance.isActiveAndEnabled)
        {
            MiniGameAudioManager.Instance.StartCoroutine(routine);
            return;
        }

        // Fallback: create or reuse a lightweight runner in the scene
        GetFallbackRunner().StartCoroutine(routine);
    }

    private static MonoBehaviour GetFallbackRunner()
    {
        if (fallbackRunner != null) return fallbackRunner;
        var go = GameObject.Find("_CoinTemplateCoroutineRunner");
        if (go == null)
        {
            go = new GameObject("_CoinTemplateCoroutineRunner");
            GameObject.DontDestroyOnLoad(go);
        }
        fallbackRunner = go.GetComponent<CoroutineProxy>();
        if (fallbackRunner == null) fallbackRunner = go.AddComponent<CoroutineProxy>();
        return fallbackRunner;
    }

    private static CoroutineProxy fallbackRunner;

    private class CoroutineProxy : MonoBehaviour { }

    private float CalculateSoundDelay(CoinSound coinSound, bool isComplete)
    {
        if (isComplete)
        {
            // Use custom complete delay if set, otherwise use global complete delay, otherwise use default
            if (coinSound.customCompleteDelay >= 0f)
                return coinSound.customCompleteDelay;
            else if (completeSoundDelay > 0f)
                return completeSoundDelay;
            else
                return defaultCompleteDelay;
        }
        else
        {
            // Use custom load delay if set, otherwise use global load delay, otherwise use default
            if (coinSound.customLoadDelay >= 0f)
                return coinSound.customLoadDelay;
            else if (loadSoundDelay > 0f)
                return loadSoundDelay;
            else
                return defaultLoadDelay;
        }
    }

    private System.Collections.IEnumerator PlayDelayedSound(AudioClip clip, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log($"Playing {(delay > 0f ? "delayed " : "")}sound: {clip.name} (delay: {delay}s)");
        }
    }

    public void OnGameTimeUp()
    {
        Debug.Log("Game Over - Time's up!");
        Debug.Log($"Session Progress: {currentIndex}/{sessionPrefixes.Count} coins completed");
        // Add your game over logic here (stop input, show results, etc.)
    }

    // Public method to get session information
    public int GetCurrentSessionProgress()
    {
        return currentIndex;
    }

    public int GetTotalSessionCoins()
    {
        return sessionPrefixes.Count;
    }

    public float GetSessionProgressPercentage()
    {
        if (sessionPrefixes.Count == 0) return 0f;
        return (float)currentIndex / sessionPrefixes.Count;
    }

    public List<string> GetCurrentSessionCoins()
    {
        return new List<string>(sessionPrefixes);
    }

    // Change session length based on difficulty
    public void SetDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 1: coinsPerSession = 2; break; // Easy
            case 2: coinsPerSession = 4; break; // Medium  
            case 3: coinsPerSession = 6; break; // Hard
        }

        Debug.Log($"Difficulty set to {difficulty}, coins per session: {coinsPerSession}");
    }

    // Public method to set coins per session directly
    public void SetCoinsPerSession(int coins)
    {
        if (coins <= 0)
        {
            Debug.LogWarning($"Invalid coins per session: {coins}. Must be greater than 0.");
            return;
        }

        if (coins > coinPrefixes.Count)
        {
            Debug.LogWarning($"Requested coins per session ({coins}) is greater than available prefixes ({coinPrefixes.Count}). Setting to {coinPrefixes.Count}.");
            coins = coinPrefixes.Count;
        }

        coinsPerSession = coins;
        Debug.Log($"Coins per session set to: {coinsPerSession}");
    }

    // Public method to get current coins per session setting
    public int GetCoinsPerSession()
    {
        return coinsPerSession;
    }

    // Public method to get available coin prefixes count
    public int GetAvailableCoinPrefixesCount()
    {
        return coinPrefixes != null ? coinPrefixes.Count : 0;
    }

    // Public method to manually add coin prefixes
    public void AddCoinPrefix(string prefix)
    {
        if (!string.IsNullOrEmpty(prefix) && !coinPrefixes.Contains(prefix))
        {
            coinPrefixes.Add(prefix);
            Debug.Log($"Manually added coin prefix: {prefix}");
        }
    }

    // Public method to clear and reset coin prefixes
    public void ResetCoinPrefixes()
    {
        coinPrefixes.Clear();
        ValidateCoinPrefixes();
        Debug.Log("Coin prefixes reset and re-validated.");
    }

    // Public method to print current panel setup for debugging
    public void PrintPanelSetup()
    {
        /*       Debug.Log("=== Current Panel Setup ===");
               Debug.Log($"Coin Text Panels: {coinTextPanels?.Count ?? 0}");
               Debug.Log($"Reference Panels: {referencePanels?.Count ?? 0}");
               Debug.Log($"Dropzone Panels: {dropzonePanels?.Count ?? 0}");
               Debug.Log($"Tray Panels: {trayPanels?.Count ?? 0}");
               Debug.Log($"Coin Prefixes: {coinPrefixes?.Count ?? 0}");
       */
        if (coinPrefixes != null && coinPrefixes.Count > 0)
        {
            Debug.Log($"Available prefixes: {string.Join(", ", coinPrefixes)}");
        }

        //   Debug.Log($"Coins per session: {coinsPerSession}");
        //   Debug.Log("==========================");
    }

    // Restart Session
    public void RestartSession()
    {
        Debug.Log($"Restarting session with {coinsPerSession} coins per session");

        // Validate coin prefixes again in case they were changed
        ValidateCoinPrefixes();

        currentIndex = 0;
        sessionPrefixes = new List<string>(coinPrefixes);
        Shuffle(sessionPrefixes);
        sessionPrefixes = sessionPrefixes.GetRange(0, Mathf.Min(coinsPerSession, sessionPrefixes.Count));

        Debug.Log($"Restarted session with {sessionPrefixes.Count} coins: {string.Join(", ", sessionPrefixes)}");

        // Reset game start time
        gameStartTime = Time.time;

        ShowCurrentCoin();
    }

    // Public method to hide the win panel
    public void HideWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
            Debug.Log("Win panel hidden");
        }
    }

    // Fast-paced mode
    public void SetFastMode()
    {
        coinSpawnDelay = 0.8f;
    }

    // Relaxed mode
    public void SetRelaxedMode()
    {
        coinSpawnDelay = 2.5f;
    }

    // Tutorial mode (longer delays)
    public void SetTutorialMode()
    {
        coinSpawnDelay = 3f;
    }

    // Adjust delay based on audio clip length
    public void AdjustDelayForAudio(AudioClip clip)
    {
        if (clip != null)
        {
            // Set delay to 1.5x the audio length for comfortable listening
            coinSpawnDelay = Mathf.Clamp(clip.length * 1.5f, 0.5f, 5f);
        }
    }

    // Public methods to control sound delays
    public void SetLoadSoundDelay(float delay)
    {
        loadSoundDelay = Mathf.Clamp(delay, 0f, 3f);
        Debug.Log($"Load sound delay set to: {loadSoundDelay}s");
    }

    public void SetCompleteSoundDelay(float delay)
    {
        completeSoundDelay = Mathf.Clamp(delay, 0f, 3f);
        Debug.Log($"Complete sound delay set to: {completeSoundDelay}s");
    }

    public void SetDefaultLoadDelay(float delay)
    {
        defaultLoadDelay = Mathf.Clamp(delay, 0f, 2f);
        Debug.Log($"Default load delay set to: {defaultLoadDelay}s");
    }

    public void SetDefaultCompleteDelay(float delay)
    {
        defaultCompleteDelay = Mathf.Clamp(delay, 0f, 2f);
        Debug.Log($"Default complete delay set to: {defaultCompleteDelay}s");
    }

    // Get current delay settings
    public float GetLoadSoundDelay() => loadSoundDelay;
    public float GetCompleteSoundDelay() => completeSoundDelay;
    public float GetDefaultLoadDelay() => defaultLoadDelay;
    public float GetDefaultCompleteDelay() => defaultCompleteDelay;

    // Set custom delays for specific coin sounds
    public void SetCustomLoadDelay(string prefix, float delay)
    {
        foreach (var cs in coinSounds)
        {
            if (cs.prefix == prefix)
            {
                cs.customLoadDelay = Mathf.Clamp(delay, 0f, 3f);
                Debug.Log($"Custom load delay for {prefix} set to: {cs.customLoadDelay}s");
                return;
            }
        }
        Debug.LogWarning($"Coin sound with prefix '{prefix}' not found!");
    }

    public void SetCustomCompleteDelay(string prefix, float delay)
    {
        foreach (var cs in coinSounds)
        {
            if (cs.prefix == prefix)
            {
                cs.customCompleteDelay = Mathf.Clamp(delay, 0f, 3f);
                Debug.Log($"Custom complete delay for {prefix} set to: {cs.customCompleteDelay}s");
                return;
            }
        }
        Debug.LogWarning($"Coin sound with prefix '{prefix}' not found!");
    }

    // Reset all delays to defaults
    public void ResetAllDelays()
    {
        loadSoundDelay = 0.5f;
        completeSoundDelay = 0.2f;
        defaultLoadDelay = 0.3f;
        defaultCompleteDelay = 0.1f;

        foreach (var cs in coinSounds)
        {
            cs.customLoadDelay = -1f;
            cs.customCompleteDelay = -1f;
        }

        Debug.Log("All sound delays reset to defaults");
    }

    // Disable all delays (set to 0)
    public void DisableAllDelays()
    {
        loadSoundDelay = 0f;
        completeSoundDelay = 0f;
        defaultLoadDelay = 0f;
        defaultCompleteDelay = 0f;

        foreach (var cs in coinSounds)
        {
            cs.customLoadDelay = 0f;
            cs.customCompleteDelay = 0f;
        }

        Debug.Log("All sound delays disabled (set to 0)");
    }
}