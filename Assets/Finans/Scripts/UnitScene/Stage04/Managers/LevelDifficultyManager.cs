using UnityEngine;

/// <summary>
/// Manages difficulty levels and automatically configures hint settings
/// </summary>
public class LevelDifficultyManager : MonoBehaviour
{
    public static LevelDifficultyManager Instance;
    
    [Header("Difficulty Settings")]
    [Tooltip("Current difficulty level")]
    public DifficultyLevel currentDifficulty = DifficultyLevel.Medium;
    
    [Header("Hint Configuration")]
    [Tooltip("Enable hints for Easy difficulty")]
    public bool enableHintsEasy = true;
    [Tooltip("Enable hints for Medium difficulty")]
    public bool enableHintsMedium = false;
    [Tooltip("Enable hints for Hard difficulty")]
    public bool enableHintsHard = false;
    
    [Header("Advanced Hint Settings")]
    [Tooltip("Enable color hints for Easy difficulty")]
    public bool enableColorHintsEasy = true;
    [Tooltip("Enable scale hints for Easy difficulty")]
    public bool enableScaleHintsEasy = true;
    
    [Tooltip("Enable color hints for Medium difficulty")]
    public bool enableColorHintsMedium = false;
    [Tooltip("Enable scale hints for Medium difficulty")]
    public bool enableScaleHintsMedium = false;
    
    [Tooltip("Enable color hints for Hard difficulty")]
    public bool enableColorHintsHard = false;
    [Tooltip("Enable scale hints for Hard difficulty")]
    public bool enableScaleHintsHard = false;
    
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Custom
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Apply initial difficulty settings
        ApplyDifficultySettings(currentDifficulty);
    }
    
    /// <summary>
    /// Set difficulty level and apply corresponding hint settings
    /// </summary>
    public void SetDifficulty(DifficultyLevel difficulty)
    {
        currentDifficulty = difficulty;
        ApplyDifficultySettings(difficulty);
        
        Debug.Log($"Difficulty set to: {difficulty}");
    }
    
    /// <summary>
    /// Apply hint settings based on difficulty level
    /// </summary>
    private void ApplyDifficultySettings(DifficultyLevel difficulty)
    {
        if (HintManager.Instance == null)
        {
            Debug.LogWarning("HintManager not found! Create one first.");
            return;
        }
        
        switch (difficulty)
        {
            case DifficultyLevel.Easy:
                ApplyEasySettings();
                break;
            case DifficultyLevel.Medium:
                ApplyMediumSettings();
                break;
            case DifficultyLevel.Hard:
                ApplyHardSettings();
                break;
            case DifficultyLevel.Custom:
                // Custom settings are managed manually
                Debug.Log("Custom difficulty - hint settings not automatically changed");
                break;
        }
    }
    
    private void ApplyEasySettings()
    {
        HintManager.Instance.enableHints = enableHintsEasy;
        HintManager.Instance.enableColorHints = enableColorHintsEasy;
        HintManager.Instance.enableScaleHints = enableScaleHintsEasy;
        Debug.Log("Applied Easy difficulty settings");
    }
    
    private void ApplyMediumSettings()
    {
        HintManager.Instance.enableHints = enableHintsMedium;
        HintManager.Instance.enableColorHints = enableColorHintsMedium;
        HintManager.Instance.enableScaleHints = enableScaleHintsMedium;
        Debug.Log("Applied Medium difficulty settings");
    }
    
    private void ApplyHardSettings()
    {
        HintManager.Instance.enableHints = enableHintsHard;
        HintManager.Instance.enableColorHints = enableColorHintsHard;
        HintManager.Instance.enableScaleHints = enableScaleHintsHard;
        Debug.Log("Applied Hard difficulty settings");
    }
    
    /// <summary>
    /// Quick methods for changing difficulty
    /// </summary>
    [ContextMenu("Set Easy Difficulty")]
    public void SetEasyDifficulty()
    {
        SetDifficulty(DifficultyLevel.Easy);
    }
    
    [ContextMenu("Set Medium Difficulty")]
    public void SetMediumDifficulty()
    {
        SetDifficulty(DifficultyLevel.Medium);
    }
    
    [ContextMenu("Set Hard Difficulty")]
    public void SetHardDifficulty()
    {
        SetDifficulty(DifficultyLevel.Hard);
    }
    
    /// <summary>
    /// Get current hint status
    /// </summary>
    public string GetCurrentHintStatus()
    {
        if (HintManager.Instance == null)
            return "HintManager not found";
            
        return $"Hints: {HintManager.Instance.enableHints}, " +
               $"Color: {HintManager.Instance.enableColorHints}, " +
               $"Scale: {HintManager.Instance.enableScaleHints}";
    }
    
    /// <summary>
    /// Toggle hints on/off for current difficulty
    /// </summary>
    public void ToggleHints()
    {
        if (HintManager.Instance != null)
        {
            HintManager.Instance.ToggleHints();
            Debug.Log($"Hints toggled. Current status: {GetCurrentHintStatus()}");
        }
    }
    
    /// <summary>
    /// Force enable hints regardless of difficulty
    /// </summary>
    public void ForceEnableHints()
    {
        if (HintManager.Instance != null)
        {
            HintManager.Instance.EnableHints();
            Debug.Log("Hints forcefully enabled");
        }
    }
    
    /// <summary>
    /// Force disable hints regardless of difficulty
    /// </summary>
    public void ForceDisableHints()
    {
        if (HintManager.Instance != null)
        {
            HintManager.Instance.DisableHints();
            Debug.Log("Hints forcefully disabled");
        }
    }
    
    [ContextMenu("Show Current Settings")]
    public void ShowCurrentSettings()
    {
        Debug.Log($"=== Level Difficulty Manager ===");
        Debug.Log($"Current Difficulty: {currentDifficulty}");
        Debug.Log($"Hint Status: {GetCurrentHintStatus()}");
        Debug.Log($"Easy Hints: {enableHintsEasy}");
        Debug.Log($"Medium Hints: {enableHintsMedium}");
        Debug.Log($"Hard Hints: {enableHintsHard}");
    }
}
