using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// DropZone expects a specific partID. Provides feedback and notifies game manager on placement.
/// </summary>
public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string expectedPartID;
    public bool isRequired = true;
    public UnityEvent<string, DraggablePart, bool> OnPartPlaced; // partID, part, correct
    public string expectedCoinGroupID;
    
    [Header("Audio Feedback")]
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    [Header("Hint Override (Optional)")]
    [Tooltip("Override global hint settings for this specific drop zone")]
    public bool overrideHintSettings = false;
    [Tooltip("Enable hints for this drop zone (only if overrideHintSettings is true)")]
    public bool enableLocalHints = true;
    
    private Color originalColor;
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Image imageComponent;
    private bool isOccupied = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        imageComponent = GetComponent<Image>();
        
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else if (imageComponent != null)
            originalColor = imageComponent.color;
            
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Check if hints should be shown for this drop zone
    /// </summary>
    private bool ShouldShowHints()
    {
        if (overrideHintSettings)
        {
            return enableLocalHints;
        }
        return HintManager.AreHintsEnabled();
    }

    /// <summary>
    /// Check if color hints should be shown
    /// </summary>
    private bool ShouldShowColorHints()
    {
        if (overrideHintSettings)
        {
            return enableLocalHints;
        }
        return HintManager.AreColorHintsEnabled();
    }

    /// <summary>
    /// Check if scale hints should be shown
    /// </summary>
    private bool ShouldShowScaleHints()
    {
        if (overrideHintSettings)
        {
            return enableLocalHints;
        }
        return HintManager.AreScaleHintsEnabled();
    }

    /// <summary>
    /// Get the hint color (either from HintManager or default)
    /// </summary>
    private Color GetHintColor()
    {
        if (ShouldShowColorHints())
        {
            return HintManager.GetHintColor();
        }
        return originalColor; // No color change if hints disabled
    }

    /// <summary>
    /// Get the hint scale (either from HintManager or original scale)
    /// </summary>
    private Vector3 GetHintScale()
    {
        if (ShouldShowScaleHints())
        {
            return originalScale * HintManager.GetHintScaleMultiplier();
        }
        return originalScale; // No scale change if hints disabled
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isOccupied) return;
        
        // Only show hints if they're enabled
        if (!ShouldShowHints()) return;
        
        Color hintColor = GetHintColor();
        Vector3 hintScale = GetHintScale();
        
        if (spriteRenderer != null)
            spriteRenderer.color = hintColor;
        else if (imageComponent != null)
            imageComponent.color = hintColor;
            
        transform.localScale = hintScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isOccupied) return;
        
        // Reset to original appearance
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        else if (imageComponent != null)
            imageComponent.color = originalColor;
            
        transform.localScale = originalScale;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggable = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<DraggablePart>() : null;
        if (draggable == null) return;

        bool correct = !isOccupied && draggable.partID == expectedPartID && draggable.coinGroupID == expectedCoinGroupID;
        if (correct)
        {
            isOccupied = true;
            draggable.SnapTo(transform, true, expectedPartID, expectedCoinGroupID);
            
            // Show success color (green) regardless of hint settings
            if (spriteRenderer != null) spriteRenderer.color = Color.green;
            else if (imageComponent != null) imageComponent.color = Color.green;
            
            // Play correct audio
            if (correctSFX != null && MiniGameAudioManager.Instance != null) 
                MiniGameAudioManager.Instance.PlaySFX(correctSFX);
            else if (MiniGameAudioManager.Instance != null) 
                MiniGameAudioManager.Instance.PlayCorrectSFX();
            
            // Find and enable the sibling CorrectVisual
            Transform parent = transform.parent;
            if (parent != null)
            {
                foreach (Transform sibling in parent)
                {
                    if (sibling != transform && sibling.gameObject.name.Contains("CorrectVisual"))
                    {
                        sibling.gameObject.SetActive(true);
                        break;
                    }
                }
            }
            
            // Destroy this DropZone
            Destroy(gameObject);
            
            // Add score for correct match
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null)
                scoreManager.AddScore();
            else
                Debug.LogWarning("DropZone: No MinigameScoreManager found! Cannot add score.");
        }
        else
        {
            // Trigger wrong VFX from drop zone position before returning to tray
            draggable.PlayWrongVFX(transform.position);
            
            draggable.ReturnToTray();
            
            // Show error color (red) regardless of hint settings
            if (spriteRenderer != null) spriteRenderer.color = Color.red;
            else if (imageComponent != null) imageComponent.color = Color.red;
            
            Invoke(nameof(ResetColor), 0.3f);
            
            // Play incorrect audio
            if (wrongSFX != null && MiniGameAudioManager.Instance != null) 
                MiniGameAudioManager.Instance.PlaySFX(wrongSFX);
            else if (MiniGameAudioManager.Instance != null) 
                MiniGameAudioManager.Instance.PlayWrongSFX();
            
            // Deduct score for incorrect match
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null)
                scoreManager.SubtractScore();
            else
                Debug.LogWarning("DropZone: No MinigameScoreManager found! Cannot subtract score.");
        }
        OnPartPlaced?.Invoke(expectedPartID, draggable, correct);
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        else if (imageComponent != null)
            imageComponent.color = originalColor;
    }

    public void ResetZone()
    {
        isOccupied = false;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        else if (imageComponent != null)
            imageComponent.color = originalColor;
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Override hint settings for this specific drop zone
    /// </summary>
    public void SetHintOverride(bool enabled, bool showHints = true)
    {
        overrideHintSettings = enabled;
        enableLocalHints = showHints;
    }
    
    /// <summary>
    /// Reset to use global hint settings
    /// </summary>
    public void ResetToGlobalHints()
    {
        overrideHintSettings = false;
        enableLocalHints = true;
    }
} 