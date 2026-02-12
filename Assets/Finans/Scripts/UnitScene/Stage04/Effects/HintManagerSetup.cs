using UnityEngine;

/// <summary>
/// Helper script to easily add HintManager to the scene
/// </summary>
public class HintManagerSetup : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(8, 16)]
    public string instructions = @"HOW TO SETUP HINT CONTROLS:

1. Add HintManager to Scene:
   - Create empty GameObject named 'HintManager'
   - Add HintManager component to it
   - Configure hint settings in inspector

2. Add LevelDifficultyManager (Recommended):
   - Create empty GameObject named 'LevelDifficultyManager'
   - Add LevelDifficultyManager component to it
   - Configure difficulty-based hint settings

3. Hint Settings:
   - enableHints: Master switch for all hints
   - hintColor: Color shown when dragging over drop zones
   - hintScaleMultiplier: Scale effect when dragging over drop zones
   - enableColorHints: Enable/disable color hints specifically
   - enableScaleHints: Enable/disable scale hints specifically

4. Difficulty Levels:
   - Easy: enableHints = true (with hints)
   - Medium: enableHints = false (no hints)
   - Hard: enableHints = false (no hints)
   - Custom: Mix and match color/scale hints

5. Runtime Control:
   - Use context menu options on HintManager
   - Use LevelDifficultyManager.SetDifficulty() for automatic settings
   - Or call HintManager.Instance methods from code

Alternative: Use the context menu on this component to auto-create the HintManager.";

    [ContextMenu("Create HintManager")]
    public void CreateHintManager()
    {
        // Check if HintManager already exists
        var existingManager = Object.FindFirstObjectByType<HintManager>();
        if (existingManager != null)
        {
            Debug.Log("HintManager already exists in scene: " + existingManager.name);
            return;
        }

        // Create new GameObject with HintManager
        GameObject managerObj = new GameObject("HintManager");
        HintManager manager = managerObj.AddComponent<HintManager>();
        
        Debug.Log("HintManager created successfully! Configure hint settings in the inspector.");
        Debug.Log("You can also right-click the HintManager component and use context menu options.");
    }

    [ContextMenu("Find HintManager")]
    public void FindHintManager()
    {
        var manager = Object.FindFirstObjectByType<HintManager>();
        if (manager != null)
        {
            Debug.Log("HintManager found: " + manager.name);
            // Select the object in the hierarchy
            #if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = manager.gameObject;
            #endif
        }
        else
        {
            Debug.Log("HintManager not found in scene. Use 'Create HintManager' to add one.");
        }
    }

    [ContextMenu("Enable Hints")]
    public void EnableHints()
    {
        var manager = Object.FindFirstObjectByType<HintManager>();
        if (manager != null)
        {
            manager.EnableHints();
        }
        else
        {
            Debug.Log("HintManager not found. Create one first using 'Create HintManager'.");
        }
    }

    [ContextMenu("Disable Hints")]
    public void DisableHints()
    {
        var manager = Object.FindFirstObjectByType<HintManager>();
        if (manager != null)
        {
            manager.DisableHints();
        }
        else
        {
            Debug.Log("HintManager not found. Create one first using 'Create HintManager'.");
        }
    }

    [ContextMenu("Toggle Hints")]
    public void ToggleHints()
    {
        var manager = Object.FindFirstObjectByType<HintManager>();
        if (manager != null)
        {
            manager.ToggleHints();
        }
        else
        {
            Debug.Log("HintManager not found. Create one first using 'Create HintManager'.");
        }
    }

    [ContextMenu("Show Hint Settings")]
    public void ShowHintSettings()
    {
        var manager = Object.FindFirstObjectByType<HintManager>();
        if (manager != null)
        {
            manager.ShowHintSettings();
        }
        else
        {
            Debug.Log("HintManager not found. Create one first using 'Create HintManager'.");
        }
    }

    void Start()
    {
        // Auto-create HintManager if it doesn't exist
        if (Object.FindFirstObjectByType<HintManager>() == null)
        {
            Debug.Log("No HintManager found in scene. Use 'Create HintManager' context menu option to add one.");
        }
    }
}
