using UnityEngine;

/// <summary>
/// Helper script to easily add VFXDebugger to the scene
/// </summary>
public class VFXDebuggerSetup : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(5, 10)]
    public string instructions = @"HOW TO ADD VFXDebugger:

1. In Unity, create an empty GameObject (Right-click in Hierarchy → Create Empty)
2. Name it 'VFXDebugger' or 'DebugTools'
3. Add the VFXDebugger component to this GameObject
4. The VFXDebugger will show on-screen controls
5. Press W to test Wrong VFX, C for Correct VFX, P for Pickup VFX

Alternative: Use the context menu on this component to auto-create the debugger.";

    [ContextMenu("Create VFXDebugger")]
    public void CreateVFXDebugger()
    {
        // Check if VFXDebugger already exists
        var existingDebugger = Object.FindFirstObjectByType<VFXDebugger>();
        if (existingDebugger != null)
        {
            Debug.Log("VFXDebugger already exists in scene: " + existingDebugger.name);
            return;
        }

        // Create new GameObject with VFXDebugger
        GameObject debuggerObj = new GameObject("VFXDebugger");
        VFXDebugger debugger = debuggerObj.AddComponent<VFXDebugger>();
        
        Debug.Log("VFXDebugger created successfully! Press W, C, or P keys to test VFX effects.");
        Debug.Log("You can also right-click the VFXDebugger component and use context menu options.");
    }

    [ContextMenu("Find VFXDebugger")]
    public void FindVFXDebugger()
    {
        var debugger = Object.FindFirstObjectByType<VFXDebugger>();
        if (debugger != null)
        {
            Debug.Log("VFXDebugger found: " + debugger.name);
            // Select the object in the hierarchy
            #if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = debugger.gameObject;
            #endif
        }
        else
        {
            Debug.Log("VFXDebugger not found in scene. Use 'Create VFXDebugger' to add one.");
        }
    }

    [ContextMenu("Test All VFX")]
    public void TestAllVFX()
    {
        var debugger = Object.FindFirstObjectByType<VFXDebugger>();
        if (debugger != null)
        {
            debugger.TestAllVFX();
        }
        else
        {
            Debug.Log("VFXDebugger not found. Create one first using 'Create VFXDebugger'.");
        }
    }

    void Start()
    {
        // Auto-create VFXDebugger if it doesn't exist
        if (Object.FindFirstObjectByType<VFXDebugger>() == null)
        {
            Debug.Log("No VFXDebugger found in scene. Use 'Create VFXDebugger' context menu option to add one.");
        }
    }
}
