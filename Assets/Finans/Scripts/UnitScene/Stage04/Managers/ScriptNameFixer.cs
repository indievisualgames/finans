using UnityEngine;

/// <summary>
/// Utility script to help fix scene references to renamed scripts.
/// This script will help identify GameObjects that still have the old EducationalPerformanceTracker component.
/// </summary>
public class ScriptNameFixer : MonoBehaviour
{
    [Header("Script Name Fixer")]
    [TextArea(3, 10)]
    public string instructions = @"This script helps identify GameObjects with old script references.

To fix the 'EducationalPerformanceTracker' reference error:

1. Look for GameObjects in your scene named 'EducationalPerformanceTracker'
2. Remove the old 'EducationalPerformanceTracker' component
3. Add the new 'MiniGameEducationalPerformanceTracker' component
4. Reassign any inspector values that were lost

This script will log any GameObjects it finds with the old component.";

    [Header("Auto Check")]
    public bool checkOnStart = true;
    public bool logFoundObjects = true;

    void Start()
    {
        if (checkOnStart)
        {
            CheckForOldScriptReferences();
        }
    }

    [ContextMenu("Check for Old Script References")]
    public void CheckForOldScriptReferences()
    {
        Debug.Log("=== Script Name Fixer: Checking for old script references ===");
        
        // Look for GameObjects with the old component name
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        bool foundOldReferences = false;
        
        foreach (var obj in allObjects)
        {
            // Check if the GameObject name contains the old script name
            if (obj.name.Contains("EducationalPerformanceTracker"))
            {
                Debug.LogWarning($"Found GameObject '{obj.name}' that might have old script reference!");
                foundOldReferences = true;
            }
        }
        
        if (!foundOldReferences)
        {
            Debug.Log("✅ No GameObjects with old script names found.");
        }
        
        Debug.Log("=== Script Name Fixer: Check Complete ===");
        Debug.Log("If you see warnings above, you need to update those GameObjects in your scene.");
    }

    [ContextMenu("Print Instructions")]
    public void PrintInstructions()
    {
        Debug.Log("=== HOW TO FIX SCRIPT REFERENCE ERRORS ===");
        Debug.Log("1. Open your Unity scene");
        Debug.Log("2. Find any GameObject with 'EducationalPerformanceTracker' component");
        Debug.Log("3. Remove the old component");
        Debug.Log("4. Add 'MiniGameEducationalPerformanceTracker' component");
        Debug.Log("5. Reassign any inspector values");
        Debug.Log("6. Save the scene");
        Debug.Log("=== END INSTRUCTIONS ===");
    }
} 