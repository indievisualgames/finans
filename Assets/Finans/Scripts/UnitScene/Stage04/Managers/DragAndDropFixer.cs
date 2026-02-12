using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Comprehensive fix for drag and drop visibility and VFX issues
/// Run this script to fix all common problems
/// </summary>
public class DragAndDropFixer : MonoBehaviour
{
    [Header("Fix Options")]
    public bool fixDraggableObjects = true;
    public bool fixVFXEffects = true;
    public bool fixCanvasSetup = true;
    public bool fixTrayManager = true;
    public bool fixAudioManager = true;
    public bool fixVFXManager = true;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    void Start()
    {
        if (fixDraggableObjects) FixDraggableObjects();
        if (fixVFXEffects) FixVFXEffects();
        if (fixCanvasSetup) FixCanvasSetup();
        if (fixTrayManager) FixTrayManager();
        if (fixAudioManager) FixAudioManager();
        if (fixVFXManager) FixVFXManager();
    }
    
    [ContextMenu("Fix All Issues")]
    public void FixAllIssues()
    {
        FixDraggableObjects();
        FixVFXEffects();
        FixCanvasSetup();
        FixTrayManager();
        FixAudioManager();
        FixVFXManager();
        
        if (showDebugLogs)
            Debug.Log("DragAndDropFixer: All issues have been addressed!");
    }
    
    void FixDraggableObjects()
    {
        var draggableParts = Object.FindObjectsByType<DraggablePart>(FindObjectsSortMode.None);
        foreach (var part in draggableParts)
        {
            // Ensure CanvasGroup exists and is properly configured
            var canvasGroup = part.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = part.gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            
            // Ensure proper RectTransform
            var rectTransform = part.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
            
            // Ensure renderers are enabled
            var image = part.GetComponent<Image>();
            if (image != null) image.enabled = true;
            
            var spriteRenderer = part.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            
            if (showDebugLogs)
                Debug.Log($"Fixed draggable object: {part.name}");
        }
        
        if (showDebugLogs)
            Debug.Log($"Fixed {draggableParts.Length} draggable objects");
    }
    
    void FixVFXEffects()
    {
        var dropZones = Object.FindObjectsByType<DropZone>(FindObjectsSortMode.None);
        foreach (var dropZone in dropZones)
        {
            // Ensure proper component references
            var spriteRenderer = dropZone.GetComponent<SpriteRenderer>();
            var image = dropZone.GetComponent<Image>();
            
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            
            if (image != null)
            {
                image.enabled = true;
            }
            
            if (showDebugLogs)
                Debug.Log($"Fixed drop zone: {dropZone.name}");
        }
        
        if (showDebugLogs)
            Debug.Log($"Fixed {dropZones.Length} drop zones");
    }
    
    void FixCanvasSetup()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            // Ensure proper render mode
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // Ensure UI camera is assigned
                if (canvas.worldCamera == null)
                {
                    var mainCamera = Camera.main;
                    if (mainCamera != null)
                    {
                        canvas.worldCamera = mainCamera;
                    }
                    else
                    {
                        // Create a UI camera if none exists
                        CreateUICamera(canvas);
                    }
                }
                
                // Ensure proper plane distance
                if (canvas.planeDistance <= 0)
                {
                    canvas.planeDistance = 100f;
                }
            }
            
            // Ensure GraphicRaycaster exists
            var graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            if (showDebugLogs)
                Debug.Log($"Fixed canvas: {canvas.name}");
        }
        
        if (showDebugLogs)
            Debug.Log($"Fixed {canvases.Length} canvases");
    }
    
    void CreateUICamera(Canvas canvas)
    {
        GameObject cameraObj = new GameObject("UI Camera");
        cameraObj.transform.SetParent(canvas.transform);
        
        var uiCamera = cameraObj.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.cullingMask = 1 << 5; // UI layer
        uiCamera.orthographic = true;
        uiCamera.orthographicSize = 5f;
        uiCamera.depth = 1;
        uiCamera.nearClipPlane = 0.1f;
        uiCamera.farClipPlane = 1000f;
        
        cameraObj.transform.position = new Vector3(0, 0, -10);
        cameraObj.transform.rotation = Quaternion.identity;
        
        canvas.worldCamera = uiCamera;
        
        if (showDebugLogs)
            Debug.Log("Created UI Camera for canvas");
    }
    
    void FixTrayManager()
    {
        var trayManager = TrayManager.Instance;
        if (trayManager != null)
        {
            // Ensure tray panel parent is assigned
            if (trayManager.trayPanelParent == null)
            {
                Debug.LogWarning("TrayManager: trayPanelParent is not assigned!");
            }
            
            if (showDebugLogs)
                Debug.Log("TrayManager instance found and verified");
        }
        else
        {
            Debug.LogWarning("TrayManager instance not found!");
        }
    }
    
    void FixAudioManager()
    {
        var audioManager = MiniGameAudioManager.Instance;
        if (audioManager != null)
        {
            // Ensure audio sources are properly configured
            if (audioManager.sfxSource != null)
            {
                audioManager.sfxSource.volume = audioManager.sfxVolume;
            }
            
            if (audioManager.musicSource != null)
            {
                audioManager.musicSource.volume = audioManager.musicVolume;
            }
            
            if (showDebugLogs)
                Debug.Log("AudioManager instance found and configured");
        }
        else
        {
            Debug.LogWarning("MiniGameAudioManager instance not found!");
        }
    }
    
    void FixVFXManager()
    {
        var vfxManager = VFXManager.Instance;
        if (vfxManager != null)
        {
            // Ensure VFXManager is properly configured
            if (vfxManager.uiCamera == null)
            {
                vfxManager.uiCamera = Camera.main;
                if (vfxManager.uiCamera == null)
                {
                    var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
                    foreach (var cam in cameras)
                    {
                        if (cam.orthographic)
                        {
                            vfxManager.uiCamera = cam;
                            break;
                        }
                    }
                }
            }
            
            if (vfxManager.targetCanvas == null)
            {
                vfxManager.targetCanvas = Object.FindFirstObjectByType<Canvas>();
            }
            
            if (showDebugLogs)
                Debug.Log("VFXManager instance found and configured");
        }
        else
        {
            Debug.LogWarning("VFXManager instance not found! Consider adding VFXManager to your scene.");
        }
    }
    
    [ContextMenu("Check All Components")]
    public void CheckAllComponents()
    {
        var draggableParts = Object.FindObjectsByType<DraggablePart>(FindObjectsSortMode.None);
        var dropZones = Object.FindObjectsByType<DropZone>(FindObjectsSortMode.None);
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        var trayManager = TrayManager.Instance;
        var audioManager = MiniGameAudioManager.Instance;
        var vfxManager = VFXManager.Instance;
        
        Debug.Log($"=== Component Check Results ===");
        Debug.Log($"DraggableParts: {draggableParts.Length}");
        Debug.Log($"DropZones: {dropZones.Length}");
        Debug.Log($"Canvases: {canvases.Length}");
        Debug.Log($"TrayManager: {(trayManager != null ? "Found" : "Missing")}");
        Debug.Log($"AudioManager: {(audioManager != null ? "Found" : "Missing")}");
        Debug.Log($"VFXManager: {(vfxManager != null ? "Found" : "Missing")}");
        
        // Check for common issues
        foreach (var canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
            {
                Debug.LogWarning($"Canvas '{canvas.name}' is in Screen Space - Camera mode but has no camera assigned!");
            }
        }
        
        foreach (var part in draggableParts)
        {
            var canvasGroup = part.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"DraggablePart '{part.name}' has no CanvasGroup component!");
            }
        }
        
        // Check VFX components
        foreach (var part in draggableParts)
        {
            if (part.correctFX == null && part.wrongFX == null)
            {
                Debug.LogWarning($"DraggablePart '{part.name}' has no VFX effects assigned!");
            }
        }
    }
}
