using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper utility to ensure proper Canvas setup for Screen Space - Camera mode
/// </summary>
public class CanvasSetupHelper : MonoBehaviour
{
    [Header("Canvas Settings")]
    public Canvas targetCanvas;
    public Camera uiCamera;
    public float planeDistance = 100f;
    
    [Header("UI Camera Settings")]
    public bool createUICamera = true;
    public LayerMask uiLayerMask = 1 << 5; // UI layer
    
    void Awake()
    {
        SetupCanvas();
    }
    
    void SetupCanvas()
    {
        if (targetCanvas == null)
        {
            targetCanvas = GetComponent<Canvas>();
        }
        
        if (targetCanvas == null)
        {
            Debug.LogError("CanvasSetupHelper: No Canvas found!");
            return;
        }
        
        // Set up Screen Space - Camera mode
        targetCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        
        // Set up UI Camera
        if (uiCamera == null && createUICamera)
        {
            CreateUICamera();
        }
        
        if (uiCamera != null)
        {
            targetCanvas.worldCamera = uiCamera;
            targetCanvas.planeDistance = planeDistance;
        }
        
        // Ensure proper sorting
        targetCanvas.sortingOrder = 0;
        
        // Set up Graphic Raycaster
        var graphicRaycaster = targetCanvas.GetComponent<GraphicRaycaster>();
        if (graphicRaycaster == null)
        {
            graphicRaycaster = targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        
        // Configure Graphic Raycaster for Screen Space - Camera
        graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
        graphicRaycaster.ignoreReversedGraphics = true;
        
        Debug.Log($"CanvasSetupHelper: Canvas '{targetCanvas.name}' configured for Screen Space - Camera mode");
    }
    
    void CreateUICamera()
    {
        GameObject cameraObj = new GameObject("UI Camera");
        cameraObj.transform.SetParent(transform);
        
        uiCamera = cameraObj.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.cullingMask = uiLayerMask;
        uiCamera.orthographic = true;
        uiCamera.orthographicSize = 5f;
        uiCamera.depth = 1;
        uiCamera.nearClipPlane = 0.1f;
        uiCamera.farClipPlane = 1000f;
        
        // Position camera
        cameraObj.transform.position = new Vector3(0, 0, -10);
        cameraObj.transform.rotation = Quaternion.identity;
        
        Debug.Log("CanvasSetupHelper: Created UI Camera");
    }
    
    [ContextMenu("Refresh Canvas Setup")]
    public void RefreshCanvasSetup()
    {
        SetupCanvas();
    }
    
    [ContextMenu("Fix UI Elements Visibility")]
    public void FixUIElementsVisibility()
    {
        if (targetCanvas == null) return;
        
        // Fix all UI elements in the canvas
        var uiElements = targetCanvas.GetComponentsInChildren<Graphic>(true);
        foreach (var element in uiElements)
        {
            // Ensure proper CanvasGroup settings
            var canvasGroup = element.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            
            // Ensure Image components are enabled
            var image = element as Image;
            if (image != null)
            {
                image.enabled = true;
            }
            
            // Ensure proper RectTransform settings
            var rectTransform = element.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
        }
        
        Debug.Log($"CanvasSetupHelper: Fixed visibility for {uiElements.Length} UI elements");
    }
}
