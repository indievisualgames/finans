using UnityEngine;

/// <summary>
/// Simple test script to verify TimeExtensionManager functionality
/// </summary>
public class TimeExtensionTest : MonoBehaviour
{
    [Header("Test References")]
    public TimerManager timerManager;
    public TimeExtensionManager timeExtensionManager;
    public MinigameScoreManager scoreManager;
    
    [Header("Test Settings")]
    [Range(5f, 60f)]
    public float testWarningThreshold = 10f;
    [Range(10f, 120f)]
    public float testTimeLimit = 30f;
    
    void Start()
    {
        // Auto-find managers if not assigned
        if (timerManager == null)
            timerManager = FindFirstObjectByType<TimerManager>();
            
        if (timeExtensionManager == null)
            timeExtensionManager = FindFirstObjectByType<TimeExtensionManager>();
            
        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<MinigameScoreManager>();
            
        Debug.Log("TimeExtensionTest: Ready for testing");
    }
    
    [ContextMenu("Test Time Extension System")]
    public void TestTimeExtensionSystem()
    {
        Debug.Log("=== TESTING TIME EXTENSION SYSTEM ===");
        
        if (timerManager == null)
        {
            Debug.LogError("❌ TimerManager not found!");
            return;
        }
        
        if (timeExtensionManager == null)
        {
            Debug.LogError("❌ TimeExtensionManager not found!");
            return;
        }
        
        Debug.Log("✅ All managers found");
        
        // Test 1: Check current status
        Debug.Log("--- Test 1: Current Status ---");
        Debug.Log($"Timer Running: {timerManager.IsRunning}");
        Debug.Log($"Time Extension Active: {timeExtensionManager.IsTimeExtensionActive}");
        Debug.Log($"Remaining Time: {timerManager.GetRemainingTime():F1}s");
        Debug.Log($"Warning Threshold: {timerManager.GetWarningThreshold()}s");
        
        // Test 2: Force show time extension panel
        Debug.Log("--- Test 2: Force Show Panel ---");
        timeExtensionManager.ForceShowTimeExtension();
        
        // Test 3: Check status after showing
        Debug.Log("--- Test 3: Status After Show ---");
        Debug.Log($"Time Extension Active: {timeExtensionManager.IsTimeExtensionActive}");
        
        // Test 4: Close panel
        Debug.Log("--- Test 4: Close Panel ---");
        timeExtensionManager.CloseTimeExtension();
        
        // Test 5: Final status
        Debug.Log("--- Test 5: Final Status ---");
        Debug.Log($"Time Extension Active: {timeExtensionManager.IsTimeExtensionActive}");
        
        Debug.Log("=== TEST COMPLETE ===");
    }
    
    [ContextMenu("Test Warning Time Trigger")]
    public void TestWarningTimeTrigger()
    {
        Debug.Log("=== TESTING WARNING TIME TRIGGER ===");
        
        if (timerManager == null)
        {
            Debug.LogError("❌ TimerManager not found!");
            return;
        }
        
        // Set timer to just above warning threshold
        float currentTime = timerManager.GetRemainingTime();
        float testTime = testWarningThreshold + 1f;
        
        Debug.Log($"Current time: {currentTime:F1}s");
        Debug.Log($"Setting timer to: {testTime:F1}s (just above warning threshold: {testWarningThreshold}s)");
        
        timerManager.SetTime(testTime);
        
        // Wait a moment for the warning to trigger
        StartCoroutine(WaitAndCheckWarning());
    }
    
    private System.Collections.IEnumerator WaitAndCheckWarning()
    {
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log("--- Checking Warning Status ---");
        Debug.Log($"Remaining Time: {timerManager.GetRemainingTime():F1}s");
        Debug.Log($"Time Extension Active: {timeExtensionManager.IsTimeExtensionActive}");
        
        if (timeExtensionManager.IsTimeExtensionActive)
        {
            Debug.Log("✅ SUCCESS: Time extension panel opened automatically!");
        }
        else
        {
            Debug.LogWarning("⚠️ Time extension panel did not open automatically");
        }
    }
    
    [ContextMenu("Reset Timer")]
    public void ResetTimer()
    {
        if (timerManager != null)
        {
            timerManager.ResetTimer();
            Debug.Log("Timer reset to initial time");
        }
    }
    
    [ContextMenu("Start Timer")]
    public void StartTimer()
    {
        if (timerManager != null)
        {
            timerManager.StartTimer(testTimeLimit);
            Debug.Log($"Timer started with {testTimeLimit}s");
        }
    }
    
    [ContextMenu("Stop Timer")]
    public void StopTimer()
    {
        if (timerManager != null)
        {
            timerManager.StopTimer();
            Debug.Log("Timer stopped");
        }
    }
    
    [ContextMenu("Print All Status")]
    public void PrintAllStatus()
    {
        Debug.Log("=== ALL MANAGER STATUS ===");
        
        if (timerManager != null)
        {
            Debug.Log($"TimerManager: {timerManager.name}");
            Debug.Log($"  Running: {timerManager.IsRunning}");
            Debug.Log($"  Time: {timerManager.GetRemainingTime():F1}s");
            Debug.Log($"  Warning Threshold: {timerManager.GetWarningThreshold()}s");
        }
        
        if (timeExtensionManager != null)
        {
            Debug.Log($"TimeExtensionManager: {timeExtensionManager.name}");
            Debug.Log($"  Active: {timeExtensionManager.IsTimeExtensionActive}");
            Debug.Log($"  Ad Loading: {timeExtensionManager.IsAdLoading}");
            Debug.Log($"  Purchase Processing: {timeExtensionManager.IsPurchaseProcessing}");
        }
        
        if (scoreManager != null)
        {
            Debug.Log($"DynamicScoreManager: {scoreManager.name}");
            Debug.Log($"  Game Completed: {scoreManager.IsGameCompleted()}");
            Debug.Log($"  Timer Enabled: {scoreManager.IsTimerEnabled()}");
        }
        
        Debug.Log("=== END STATUS ===");
    }
}
