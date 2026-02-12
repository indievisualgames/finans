using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

/// <summary>
/// Generic drag-and-drop handler for coin/note parts. Handles snapping, feedback, and tray return.
/// </summary>
public class DraggablePart : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string partID;
    public AudioClip pickupSFX;
    public AudioClip dropSFX;
    public AudioClip snapSFX;
    public ParticleSystem correctFX;
    public ParticleSystem wrongFX;
    public float dragSmoothness = 10f;
    public event Action<string, DraggablePart, PointerEventData> OnDropped;
    public AudioClip correctSFX;
    public AudioClip incorrectSFX;
    public string coinGroupID;

    private Vector3 startPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private bool isDragging = false;
    private Vector3 targetPosition;
    public Canvas parentCanvas;
    private Camera uiCamera;
    private RectTransform rectTransform;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Find UI camera for Screen Space - Camera mode
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            uiCamera = parentCanvas.worldCamera;
        }
        
        // Store initial position
        startPosition = transform.position;
        originalParent = transform.parent;
        
        // Validate VFX assignments
        ValidateVFXAssignments();
    }
    
    /// <summary>
    /// Validates VFX field assignments and logs warnings if missing
    /// </summary>
    private void ValidateVFXAssignments()
    {
        if (correctFX == null)
        {
            Debug.LogWarning($"DraggablePart '{gameObject.name}' has no Correct FX assigned. Will use fallback VFX.");
        }
        
        if (wrongFX == null)
        {
            Debug.LogWarning($"DraggablePart '{gameObject.name}' has no Wrong FX assigned. Will use fallback VFX.");
        }
        
        if (correctSFX == null)
        {
            Debug.LogWarning($"DraggablePart '{gameObject.name}' has no Correct SFX assigned.");
        }
        
        if (incorrectSFX == null)
        {
            Debug.LogWarning($"DraggablePart '{gameObject.name}' has no Incorrect SFX assigned.");
        }
    }
    
    /// <summary>
    /// Set custom correct VFX at runtime
    /// </summary>
    public void SetCorrectVFX(ParticleSystem newCorrectVFX)
    {
        correctFX = newCorrectVFX;
        Debug.Log($"Correct VFX updated for {gameObject.name}");
    }
    
    /// <summary>
    /// Set custom wrong VFX at runtime
    /// </summary>
    public void SetWrongVFX(ParticleSystem newWrongVFX)
    {
        wrongFX = newWrongVFX;
        Debug.Log($"Wrong VFX updated for {gameObject.name}");
    }
    
    /// <summary>
    /// Get current VFX assignment status
    /// </summary>
    public string GetVFXStatus()
    {
        return $"Correct FX: {(correctFX != null ? correctFX.name : "NOT ASSIGNED")}, " +
               $"Wrong FX: {(wrongFX != null ? wrongFX.name : "NOT ASSIGNED")}";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        originalParent = transform.parent;
        
        // Set parent to canvas root to avoid clipping issues
        if (parentCanvas != null)
        {
            transform.SetParent(parentCanvas.transform, true);
        }
        else
        {
            transform.SetParent(transform.root, true);
        }
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f; // Slight transparency while dragging
        isDragging = true;
        
        if (pickupSFX != null && MiniGameAudioManager.Instance != null) 
            MiniGameAudioManager.Instance.PlaySFX(pickupSFX);
            
        // Visual feedback (scale up) - use HintManager if available
        float scaleMultiplier = 1.1f; // Default scale
        if (HintManager.Instance != null && HintManager.AreScaleHintsEnabled())
        {
            scaleMultiplier = HintManager.GetHintScaleMultiplier();
        }
        transform.localScale = Vector3.one * scaleMultiplier;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // Handle both mouse and touch input
        Vector3 inputPosition;
        if (Input.touchCount > 0)
        {
            inputPosition = Input.GetTouch(0).position;
        }
        else
        {
            inputPosition = Input.mousePosition;
        }
        
        // Convert screen position to world position for Screen Space - Camera
        if (uiCamera != null)
        {
            Vector3 worldPosition = uiCamera.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, parentCanvas.planeDistance));
            targetPosition = worldPosition;
        }
        else
        {
            targetPosition = inputPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f; // Restore full opacity
        isDragging = false;
        transform.localScale = Vector3.one;
        
        if (dropSFX != null && MiniGameAudioManager.Instance != null) 
            MiniGameAudioManager.Instance.PlaySFX(dropSFX);
            
        OnDropped?.Invoke(partID, this, eventData);

        // Only allow drop if parent is a DropZone
        var dropZone = eventData.pointerEnter ? eventData.pointerEnter.GetComponent<DropZone>() : null;
        if (dropZone == null)
        {
            ReturnToTray();
        }
    }

    public void SnapTo(Transform target, bool correct, string expectedPartID = null, string expectedCoinGroupID = null)
    {
        transform.SetParent(target, true);
        transform.position = target.position;
        transform.localScale = Vector3.one;
        
        if (snapSFX != null && MiniGameAudioManager.Instance != null) 
            MiniGameAudioManager.Instance.PlaySFX(snapSFX);
            
        bool isCorrect = correct;
        if (expectedPartID != null && expectedCoinGroupID != null)
            isCorrect = (partID == expectedPartID && coinGroupID == expectedCoinGroupID);
        else if (expectedPartID != null)
            isCorrect = (partID == expectedPartID);
            
        if (isCorrect)
        {
            PlayCorrectVFX(target.position);
            // Play correct SFX if assigned
            if (correctSFX != null && MiniGameAudioManager.Instance != null)
                MiniGameAudioManager.Instance.PlaySFX(correctSFX);
        }
        else
        {
            PlayWrongVFX(target.position);
            // Play incorrect SFX if assigned
            if (incorrectSFX != null && MiniGameAudioManager.Instance != null)
                MiniGameAudioManager.Instance.PlaySFX(incorrectSFX);
        }
    }

    public void PlayCorrectVFX(Vector3? customPosition = null)
    {
        Vector3 effectPosition = customPosition ?? transform.position;
        bool isWorldPosition = customPosition.HasValue; // If custom position provided, it's world position
        
        // Priority 1: Use assigned correctFX from Inspector
        if (correctFX != null)
        {
            ParticleEffectManager.PlayVFXAtPosition(correctFX, effectPosition, transform.root);
            Debug.Log($"Custom Correct VFX triggered at position: {effectPosition}");
            return;
        }
        
        // Priority 2: Use VFXManager if available (non-hardcoded)
        if (VFXManager.Instance != null)
        {
            // Let VFXManager decide which VFX to use based on its configuration
            VFXManager.Instance.PlayCorrectVFX(effectPosition, transform.root, isWorldPosition);
            Debug.Log($"VFXManager Correct VFX triggered at position: {effectPosition}");
            return;
        }
        
        // Priority 3: Fallback to ParticleEffectManager (non-hardcoded)
        ParticleEffectManager.PlayCorrectVFXAtPosition(effectPosition, transform.root);
        Debug.Log($"Fallback Correct VFX triggered at position: {effectPosition}");
    }

    public void PlayWrongVFX(Vector3? customPosition = null)
    {
        Vector3 effectPosition = customPosition ?? transform.position;
        bool isWorldPosition = customPosition.HasValue; // If custom position provided, it's world position
        
        // Debug: Log the position and canvas information
        Debug.Log($"DraggablePart: Playing Wrong VFX at position: {effectPosition}, isWorldPosition: {isWorldPosition}");
        Debug.Log($"DraggablePart: Parent Canvas: {parentCanvas?.name}, Render Mode: {parentCanvas?.renderMode}");
        
        // Priority 1: Use assigned wrongFX from Inspector
        if (wrongFX != null)
        {
            Debug.Log($"DraggablePart: Using assigned wrongFX: {wrongFX.name}");
            ParticleEffectManager.PlayVFXAtPosition(wrongFX, effectPosition, transform.root);
            Debug.Log($"Custom Wrong VFX triggered at position: {effectPosition}");
            return;
        }
        
        // Priority 2: Use VFXManager if available (non-hardcoded)
        if (VFXManager.Instance != null)
        {
            Debug.Log($"DraggablePart: Using VFXManager enhanced wrong VFX");
            // Use enhanced wrong VFX for better visibility
            VFXManager.Instance.PlayWrongVFXEnhanced(effectPosition, transform.root, isWorldPosition);
            Debug.Log($"Enhanced Wrong VFX triggered at position: {effectPosition}");
            return;
        }
        
        // Priority 3: Fallback to ParticleEffectManager (non-hardcoded)
        Debug.Log($"DraggablePart: Using fallback ParticleEffectManager");
        ParticleEffectManager.PlayWrongVFXAtPosition(effectPosition, transform.root);
        Debug.Log($"Fallback Wrong VFX triggered at position: {effectPosition}");
    }

    public void ReturnToTray()
    {
        // Use TrayManager (new robust manager)
        var trayManager = TrayManager.Instance;
        if (trayManager != null)
        {
            trayManager.ReturnObjectToTray(gameObject);
            return;
        }
        
        // Fallback to previous logic
        transform.SetParent(originalParent, true);
        transform.position = startPosition;
        transform.localScale = Vector3.one;
        
        // Ensure visibility
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Enable all renderers and images
        var image = GetComponent<Image>();
        if (image != null) image.enabled = true;
        
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    void Update()
    {
        if (isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dragSmoothness);
        }
    }
} 