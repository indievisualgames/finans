using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to fix common canvas and VFX visibility issues
/// </summary>
public class CanvasVFXHelper : MonoBehaviour
{
    [Header("Canvas Settings")]
    public Canvas targetCanvas;
    public Camera uiCamera;
    
    [Header("VFX Settings")]
    public bool fixSortingLayers = true;
    public bool fixCanvasScale = true;
    public bool createUISortingLayer = true;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    void Start()
    {
        if (targetCanvas == null)
            targetCanvas = GetComponent<Canvas>();
            
        if (uiCamera == null)
            uiCamera = Camera.main;
            
        FixCanvasIssues();
    }
    
    /// <summary>
    /// Fixes common canvas issues that cause VFX visibility problems
    /// </summary>
    [ContextMenu("Fix Canvas Issues")]
    public void FixCanvasIssues()
    {
        if (targetCanvas == null)
        {
            Debug.LogError("CanvasVFXHelper: No target canvas assigned!");
            return;
        }
        
        Debug.Log("=== Fixing Canvas Issues ===");
        
        // Fix 1: Ensure proper render mode
        if (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (targetCanvas.worldCamera == null)
            {
                targetCanvas.worldCamera = uiCamera;
                Debug.Log("✅ Fixed: Assigned UI camera to Screen Space - Camera canvas");
            }
        }
        
        // Fix 2: Fix canvas scale issues
        if (fixCanvasScale)
        {
            if (targetCanvas.transform.localScale != Vector3.one)
            {
                targetCanvas.transform.localScale = Vector3.one;
                Debug.Log("✅ Fixed: Reset canvas scale to 1,1,1");
            }
        }
        
        // Fix 3: Create and assign UI sorting layer
        if (createUISortingLayer)
        {
            CreateUISortingLayer();
        }
        
        // Fix 4: Set proper sorting order
        if (fixSortingLayers)
        {
            targetCanvas.sortingOrder = 0;
            Debug.Log("✅ Fixed: Set canvas sorting order to 0");
        }
        
        Debug.Log("Canvas issues fixed!");
    }
    
    /// <summary>
    /// Creates a UI sorting layer if it doesn't exist
    /// </summary>
    private void CreateUISortingLayer()
    {
        // Check if UI sorting layer exists
        bool uiLayerExists = false;
        foreach (var layer in SortingLayer.layers)
        {
            if (layer.name == "UI")
            {
                uiLayerExists = true;
                break;
            }
        }
        
        if (!uiLayerExists)
        {
            // Note: We can't create sorting layers via script in runtime
            // This is just for information
            Debug.LogWarning("⚠️ UI sorting layer doesn't exist. Create it in Edit > Project Settings > Tags and Layers > Sorting Layers");
        }
        else
        {
            Debug.Log("✅ UI sorting layer exists");
        }
    }
    
    /// <summary>
    /// Tests VFX visibility at different positions
    /// </summary>
    [ContextMenu("Test VFX Visibility")]
    public void TestVFXVisibility()
    {
        if (VFXManager.Instance == null)
        {
            Debug.LogError("VFXManager not found! Cannot test VFX.");
            return;
        }
        
        Debug.Log("Testing VFX visibility...");
        
        // Test center
        Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        VFXManager.Instance.TestWrongVFXAtPosition(centerPos);
        
        // Test where the draggable part is
        if (targetCanvas != null)
        {
            Vector3 canvasPos = targetCanvas.transform.position;
            VFXManager.Instance.TestWrongVFXAtPosition(canvasPos);
        }
        
        Debug.Log("VFX visibility test completed. Check console and scene for results.");
    }
    
    /// <summary>
    /// Shows current canvas settings
    /// </summary>
    [ContextMenu("Show Canvas Settings")]
    public void ShowCanvasSettings()
    {
        if (targetCanvas == null) return;
        
        Debug.Log("=== Current Canvas Settings ===");
        Debug.Log($"Canvas: {targetCanvas.name}");
        Debug.Log($"Render Mode: {targetCanvas.renderMode}");
        Debug.Log($"Plane Distance: {targetCanvas.planeDistance}");
        Debug.Log($"Sorting Layer ID: {targetCanvas.sortingLayerID}");
        Debug.Log($"Sorting Order: {targetCanvas.sortingOrder}");
        Debug.Log($"Scale: {targetCanvas.transform.localScale}");
        Debug.Log($"World Camera: {targetCanvas.worldCamera?.name ?? "None"}");
    }
}
