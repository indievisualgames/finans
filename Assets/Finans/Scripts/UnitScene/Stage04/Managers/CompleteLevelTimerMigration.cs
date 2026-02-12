using UnityEngine;
using System.Reflection;
using System.Linq;

/// <summary>
/// Complete migration script to replace LevelTimer with TimerManager
/// This script handles all dependencies and removes the old LevelTimer script
/// </summary>
public class CompleteLevelTimerMigration : MonoBehaviour
{
    [Header("Migration Settings")]
    [Tooltip("Automatically perform complete migration on Start")]
    public bool autoMigrate = true;
    
    [Tooltip("Show detailed migration logs")]
    public bool showDetailedLogs = true;
    
    [Tooltip("Remove LevelTimer script after migration")]
    public bool removeLevelTimerScript = true;
    
    void Start()
    {
        if (autoMigrate)
        {
            PerformCompleteMigration();
        }
    }
    
    /// <summary>
    /// Perform complete migration from LevelTimer to TimerManager
    /// </summary>
    public void PerformCompleteMigration()
    {
        Log("Starting complete LevelTimer to TimerManager migration...");
        
        // Step 1: Find or create TimerManager
        TimerManager timerManager = FindOrCreateTimerManager();
        
        // Step 2: Find and migrate LevelTimer settings (if LevelTimer still exists)
        var oldLevelTimer = FindFirstObjectByType<MonoBehaviour>();
        if (oldLevelTimer != null && oldLevelTimer.GetType().Name == "LevelTimer")
        {
            MigrateLevelTimerSettings(oldLevelTimer, timerManager);
        }
        
        // Step 3: Update all script references
        UpdateAllScriptReferences(timerManager);
        
        // Step 4: Remove old LevelTimer components
        if (removeLevelTimerScript)
        {
            RemoveAllLevelTimerComponents();
        }
        
        Log("Complete migration finished successfully!");
    }
    
    /// <summary>
    /// Find existing TimerManager or create new one
    /// </summary>
    private TimerManager FindOrCreateTimerManager()
    {
        TimerManager timerManager = FindFirstObjectByType<TimerManager>();
        
        if (timerManager == null)
        {
            GameObject timerManagerGO = new GameObject("TimerManager");
            timerManager = timerManagerGO.AddComponent<TimerManager>();
            Log("Created new TimerManager GameObject");
        }
        else
        {
            Log("Found existing TimerManager");
        }
        
        return timerManager;
    }
    
    /// <summary>
    /// Migrate settings from LevelTimer to TimerManager
    /// </summary>
    private void MigrateLevelTimerSettings(MonoBehaviour oldTimer, TimerManager newTimer)
    {
        Log("Migrating LevelTimer settings...");
        
        // Migrate basic settings using reflection
        var timeLimitField = oldTimer.GetType().GetField("timeLimit");
        var autoStartField = oldTimer.GetType().GetField("autoStart");
        var countdownModeField = oldTimer.GetType().GetField("countdownMode");
        var warningTimeField = oldTimer.GetType().GetField("warningTime");
        
        if (timeLimitField != null) newTimer.initialTime = (float)timeLimitField.GetValue(oldTimer);
        if (autoStartField != null) newTimer.autoStart = (bool)autoStartField.GetValue(oldTimer);
        if (countdownModeField != null) newTimer.countDown = (bool)countdownModeField.GetValue(oldTimer);
        if (warningTimeField != null) newTimer.warningThreshold = (float)warningTimeField.GetValue(oldTimer);
        
        // Migrate UI references
        var timerTextField = oldTimer.GetType().GetField("timerText");
        var timeFormatField = oldTimer.GetType().GetField("timeFormat");
        if (timerTextField != null) newTimer.timerText = (TMPro.TextMeshProUGUI)timerTextField.GetValue(oldTimer);
        if (timeFormatField != null) newTimer.timeFormat = (string)timeFormatField.GetValue(oldTimer);
        
        // Migrate audio settings
        var audioSourceField = oldTimer.GetType().GetField("audioSource");
        var warningAudioSourceField = oldTimer.GetType().GetField("warningAudioSource");
        var timeWarningSoundField = oldTimer.GetType().GetField("timeWarningSound");
        var timeUpSoundField = oldTimer.GetType().GetField("timeUpSound");
        
        if (audioSourceField != null) newTimer.audioSource = (AudioSource)audioSourceField.GetValue(oldTimer);
        if (warningAudioSourceField != null) newTimer.warningAudioSource = (AudioSource)warningAudioSourceField.GetValue(oldTimer);
        if (timeWarningSoundField != null) newTimer.timeWarningSound = (AudioClip)timeWarningSoundField.GetValue(oldTimer);
        if (timeUpSoundField != null) newTimer.timeUpSound = (AudioClip)timeUpSoundField.GetValue(oldTimer);
        
        // Migrate VFX settings
        var timeUpVFXField = oldTimer.GetType().GetField("timeUpVFX");
        var vfxSpawnPointField = oldTimer.GetType().GetField("vfxSpawnPoint");
        if (timeUpVFXField != null) newTimer.timeUpVFX = (GameObject)timeUpVFXField.GetValue(oldTimer);
        if (vfxSpawnPointField != null) newTimer.vfxSpawnPoint = (Transform)vfxSpawnPointField.GetValue(oldTimer);
        
        // Migrate UI panel settings
        var timeUpPanelField = oldTimer.GetType().GetField("timeUpPanel");
        var hideTimerOnTimeUpField = oldTimer.GetType().GetField("hideTimerOnTimeUp");
        if (timeUpPanelField != null) newTimer.timeUpPanel = (GameObject)timeUpPanelField.GetValue(oldTimer);
        if (hideTimerOnTimeUpField != null) newTimer.hideTimerOnTimeUp = (bool)hideTimerOnTimeUpField.GetValue(oldTimer);
        
        // Migrate events
        var onTimeUpField = oldTimer.GetType().GetField("onTimeUp");
        var onTimeWarningField = oldTimer.GetType().GetField("onTimeWarning");
        if (onTimeUpField != null) newTimer.OnTimeUp = (UnityEngine.Events.UnityEvent)onTimeUpField.GetValue(oldTimer);
        if (onTimeWarningField != null) newTimer.OnWarningTime = (UnityEngine.Events.UnityEvent)onTimeWarningField.GetValue(oldTimer);
        
        Log("Successfully migrated all LevelTimer settings to TimerManager");
    }
    
    /// <summary>
    /// Update all script references from LevelTimer to TimerManager
    /// </summary>
    private void UpdateAllScriptReferences(TimerManager timerManager)
    {
        Log("Updating script references...");
        
        // Update CoinTemplateController references
        var coinControllers = FindObjectsByType<MasterCoinGameManager>(FindObjectsSortMode.None);
        foreach (var controller in coinControllers)
        {
            UpdateScriptReference(controller, "levelTimer", timerManager);
        }
        
        // Update any other scripts that might reference LevelTimer
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in allMonoBehaviours)
        {
            if (mb != null && mb.GetType().GetField("levelTimer") != null)
            {
                UpdateScriptReference(mb, "levelTimer", timerManager);
            }
        }
        
        Log($"Updated references in {coinControllers.Length} coin controllers");
    }
    
    /// <summary>
    /// Update a specific script reference
    /// </summary>
    private void UpdateScriptReference(MonoBehaviour script, string fieldName, TimerManager timerManager)
    {
        var field = script.GetType().GetField(fieldName);
        if (field != null)
        {
            field.SetValue(script, timerManager);
            Log($"Updated {fieldName} reference in {script.name}");
        }
    }
    
    /// <summary>
    /// Remove all LevelTimer components from the scene
    /// </summary>
    private void RemoveAllLevelTimerComponents()
    {
        Log("Removing LevelTimer components...");
        
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        int removedCount = 0;
        
        foreach (var mb in allMonoBehaviours)
        {
            if (mb != null && mb.GetType().Name == "LevelTimer")
            {
                DestroyImmediate(mb);
                removedCount++;
            }
        }
        
        Log($"Removed {removedCount} LevelTimer components");
    }
    
    /// <summary>
    /// Check migration status
    /// </summary>
    [ContextMenu("Check Migration Status")]
    public void CheckMigrationStatus()
    {
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var oldTimers = allMonoBehaviours.Where(mb => mb.GetType().Name == "LevelTimer").ToArray();
        var newTimers = FindObjectsByType<TimerManager>(FindObjectsSortMode.None);
        
        Log($"Migration Status:");
        Log($"- LevelTimer components found: {oldTimers.Length}");
        Log($"- TimerManager components found: {newTimers.Length}");
        
        if (oldTimers.Length == 0 && newTimers.Length > 0)
        {
            Log("✅ Migration appears complete - no LevelTimer components found");
        }
        else if (oldTimers.Length > 0 && newTimers.Length > 0)
        {
            Log("⚠️ Both LevelTimer and TimerManager found - consider removing LevelTimer");
        }
        else if (oldTimers.Length > 0 && newTimers.Length == 0)
        {
            Log("❌ LevelTimer found but no TimerManager - migration needed");
        }
        else
        {
            Log("ℹ️ No timer components found in scene");
        }
    }
    
    /// <summary>
    /// Complete migration with context menu
    /// </summary>
    [ContextMenu("Complete Migration")]
    public void CompleteMigration()
    {
        PerformCompleteMigration();
    }
    
    /// <summary>
    /// Log message with prefix
    /// </summary>
    private void Log(string message)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"[LevelTimerMigration] {message}");
        }
    }
} 