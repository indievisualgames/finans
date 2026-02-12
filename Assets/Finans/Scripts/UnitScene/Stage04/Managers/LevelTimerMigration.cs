using UnityEngine;
using TMPro;
using System.Linq;

/// <summary>
/// Migration script to help transition from LevelTimer to enhanced TimerManager
/// This script provides compatibility methods and automatic migration
/// </summary>
public class LevelTimerMigration : MonoBehaviour
{
    [Header("Migration Settings")]
    [Tooltip("Automatically migrate LevelTimer references to TimerManager")]
    public bool autoMigrate = true;
    
    [Tooltip("Show migration warnings in console")]
    public bool showMigrationWarnings = true;
    
    [Header("Legacy LevelTimer References")]
    [Tooltip("Old LevelTimer component (will be migrated)")]
    public MonoBehaviour oldLevelTimer;
    
    [Tooltip("New TimerManager component (target for migration)")]
    public TimerManager newTimerManager;
    
    void Start()
    {
        if (autoMigrate)
        {
            PerformMigration();
        }
    }
    
    /// <summary>
    /// Perform automatic migration from LevelTimer to TimerManager
    /// </summary>
    public void PerformMigration()
    {
        // Find old LevelTimer in scene (for migration purposes)
        if (oldLevelTimer == null)
        {
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            oldLevelTimer = allMonoBehaviours.FirstOrDefault(mb => mb.GetType().Name == "LevelTimer");
        }
        
        // Find or create new TimerManager
        if (newTimerManager == null)
        {
            newTimerManager = FindFirstObjectByType<TimerManager>();
            
            if (newTimerManager == null)
            {
                // Create new TimerManager GameObject
                GameObject timerManagerGO = new GameObject("TimerManager");
                newTimerManager = timerManagerGO.AddComponent<TimerManager>();
                
                if (showMigrationWarnings)
                {
                    Debug.Log("LevelTimerMigration: Created new TimerManager GameObject");
                }
            }
        }
        
        // Migrate settings if old LevelTimer exists
        if (oldLevelTimer != null && newTimerManager != null)
        {
            MigrateSettings();
            
            if (showMigrationWarnings)
            {
                Debug.Log("LevelTimerMigration: Successfully migrated LevelTimer to TimerManager");
            }
        }
        else
        {
            if (showMigrationWarnings)
            {
                Debug.LogWarning("LevelTimerMigration: Could not find LevelTimer or TimerManager for migration");
            }
        }
    }
    
    /// <summary>
    /// Migrate settings from LevelTimer to TimerManager
    /// </summary>
    private void MigrateSettings()
    {
        if (oldLevelTimer != null && oldLevelTimer.GetType().Name == "LevelTimer")
        {
            // Migrate basic settings using reflection
            var timeLimitField = oldLevelTimer.GetType().GetField("timeLimit");
            var autoStartField = oldLevelTimer.GetType().GetField("autoStart");
            var countdownModeField = oldLevelTimer.GetType().GetField("countdownMode");
            var warningTimeField = oldLevelTimer.GetType().GetField("warningTime");
            
            if (timeLimitField != null) newTimerManager.initialTime = (float)timeLimitField.GetValue(oldLevelTimer);
            if (autoStartField != null) newTimerManager.autoStart = (bool)autoStartField.GetValue(oldLevelTimer);
            if (countdownModeField != null) newTimerManager.countDown = (bool)countdownModeField.GetValue(oldLevelTimer);
            if (warningTimeField != null) newTimerManager.warningThreshold = (float)warningTimeField.GetValue(oldLevelTimer);
            
            // Migrate UI references
            var timerTextField = oldLevelTimer.GetType().GetField("timerText");
            var timeFormatField = oldLevelTimer.GetType().GetField("timeFormat");
            if (timerTextField != null) newTimerManager.timerText = (TMPro.TextMeshProUGUI)timerTextField.GetValue(oldLevelTimer);
            if (timeFormatField != null) newTimerManager.timeFormat = (string)timeFormatField.GetValue(oldLevelTimer);
            
            // Migrate audio settings
            var audioSourceField = oldLevelTimer.GetType().GetField("audioSource");
            var warningAudioSourceField = oldLevelTimer.GetType().GetField("warningAudioSource");
            var timeWarningSoundField = oldLevelTimer.GetType().GetField("timeWarningSound");
            var timeUpSoundField = oldLevelTimer.GetType().GetField("timeUpSound");
            
            if (audioSourceField != null) newTimerManager.audioSource = (AudioSource)audioSourceField.GetValue(oldLevelTimer);
            if (warningAudioSourceField != null) newTimerManager.warningAudioSource = (AudioSource)warningAudioSourceField.GetValue(oldLevelTimer);
            if (timeWarningSoundField != null) newTimerManager.timeWarningSound = (AudioClip)timeWarningSoundField.GetValue(oldLevelTimer);
            if (timeUpSoundField != null) newTimerManager.timeUpSound = (AudioClip)timeUpSoundField.GetValue(oldLevelTimer);
            
            // Migrate VFX settings
            var timeUpVFXField = oldLevelTimer.GetType().GetField("timeUpVFX");
            var vfxSpawnPointField = oldLevelTimer.GetType().GetField("vfxSpawnPoint");
            if (timeUpVFXField != null) newTimerManager.timeUpVFX = (GameObject)timeUpVFXField.GetValue(oldLevelTimer);
            if (vfxSpawnPointField != null) newTimerManager.vfxSpawnPoint = (Transform)vfxSpawnPointField.GetValue(oldLevelTimer);
            
            // Migrate UI panel settings
            var timeUpPanelField = oldLevelTimer.GetType().GetField("timeUpPanel");
            var hideTimerOnTimeUpField = oldLevelTimer.GetType().GetField("hideTimerOnTimeUp");
            if (timeUpPanelField != null) newTimerManager.timeUpPanel = (GameObject)timeUpPanelField.GetValue(oldLevelTimer);
            if (hideTimerOnTimeUpField != null) newTimerManager.hideTimerOnTimeUp = (bool)hideTimerOnTimeUpField.GetValue(oldLevelTimer);
            
            // Migrate events
            var onTimeUpField = oldLevelTimer.GetType().GetField("onTimeUp");
            var onTimeWarningField = oldLevelTimer.GetType().GetField("onTimeWarning");
            if (onTimeUpField != null) newTimerManager.OnTimeUp = (UnityEngine.Events.UnityEvent)onTimeUpField.GetValue(oldLevelTimer);
            if (onTimeWarningField != null) newTimerManager.OnWarningTime = (UnityEngine.Events.UnityEvent)onTimeWarningField.GetValue(oldLevelTimer);
            
            if (showMigrationWarnings)
            {
                Debug.Log("LevelTimerMigration: Migrated all settings from LevelTimer to TimerManager");
            }
        }
    }
    
    /// <summary>
    /// Update references in scripts that use LevelTimer
    /// </summary>
    public void UpdateScriptReferences()
    {
        // Find all scripts that might reference LevelTimer
        var coinControllers = FindObjectsByType<MasterCoinGameManager>(FindObjectsSortMode.None);
        
        foreach (var controller in coinControllers)
        {
            // Use reflection to update LevelTimer references
            var levelTimerField = controller.GetType().GetField("levelTimer");
            if (levelTimerField != null)
            {
                levelTimerField.SetValue(controller, newTimerManager);
                
                if (showMigrationWarnings)
                {
                    Debug.Log($"LevelTimerMigration: Updated LevelTimer reference in {controller.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Remove old LevelTimer component
    /// </summary>
    public void RemoveOldLevelTimer()
    {
        if (oldLevelTimer != null)
        {
            DestroyImmediate(oldLevelTimer);
            
            if (showMigrationWarnings)
            {
                Debug.Log("LevelTimerMigration: Removed old LevelTimer component");
            }
        }
    }
    
    /// <summary>
    /// Complete migration process
    /// </summary>
    [ContextMenu("Complete Migration")]
    public void CompleteMigration()
    {
        PerformMigration();
        UpdateScriptReferences();
        RemoveOldLevelTimer();
        
        Debug.Log("LevelTimerMigration: Migration completed successfully!");
    }
    
    /// <summary>
    /// Check if migration is needed
    /// </summary>
    [ContextMenu("Check Migration Status")]
    public void CheckMigrationStatus()
    {
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var oldTimer = allMonoBehaviours.FirstOrDefault(mb => mb.GetType().Name == "LevelTimer"); // This will be null after migration
        var newTimer = FindFirstObjectByType<TimerManager>();
        
        if (oldTimer != null && newTimer == null)
        {
            Debug.LogWarning("LevelTimerMigration: LevelTimer found but no TimerManager. Migration needed.");
        }
        else if (oldTimer != null && newTimer != null)
        {
            Debug.Log("LevelTimerMigration: Both LevelTimer and TimerManager found. Consider removing LevelTimer.");
        }
        else if (oldTimer == null && newTimer != null)
        {
            Debug.Log("LevelTimerMigration: TimerManager found. Migration appears complete.");
        }
        else
        {
            Debug.Log("LevelTimerMigration: No timers found in scene.");
        }
    }
} 