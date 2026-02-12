using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// NoteSlot validates if the correct note is dropped onto it and handles its visual arrangement.
/// </summary>
public class NoteSlot : MonoBehaviour, IDropHandler
{
    public string expectedNoteType;
    
    [Header("Visual Settings")]
    public Color defaultColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color highlightColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    public Color correctColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
    public Color wrongColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
    
    [Header("Layout Settings")]
    public Vector2 slotSize = new Vector2(100f, 100f);
    public float spacing = 20f;
    public int maxSlotsPerRow = 4;
    
    private Image slotImage;
    private RectTransform rectTransform;
    private bool isOccupied = false;

    private void Awake()
    {
        // Get or add required components
        slotImage = GetComponent<Image>();
        if (slotImage == null)
        {
            slotImage = gameObject.AddComponent<Image>();
        }
        
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        // Set default visual properties
        slotImage.color = defaultColor;
        rectTransform.sizeDelta = slotSize;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // The feedback and validation logic is now handled in NoteDragHandler
    }

    public void SetHighlighted(bool highlighted)
    {
        if (slotImage != null)
        {
            slotImage.color = highlighted ? highlightColor : defaultColor;
        }
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
        if (slotImage != null)
        {
            slotImage.color = occupied ? correctColor : defaultColor;
        }
    }

    public void SetWrongAttempt()
    {
        if (slotImage != null)
        {
            slotImage.color = wrongColor;
            Invoke(nameof(ResetColor), 0.5f);
        }
    }

    private void ResetColor()
    {
        if (slotImage != null)
        {
            slotImage.color = defaultColor;
        }
    }

    public static void ArrangeSlots(Transform parent, NoteSlot[] slots)
    {
        if (slots == null || slots.Length == 0) return;

        float currentX = 0;
        float currentY = 0;
        int slotsInCurrentRow = 0;

        foreach (NoteSlot slot in slots)
        {
            if (slot == null) continue;

            RectTransform rectTransform = slot.GetComponent<RectTransform>();
            if (rectTransform == null) continue;

            // Calculate position
            rectTransform.anchoredPosition = new Vector2(currentX, currentY);

            // Update position for next slot
            currentX += slot.slotSize.x + slot.spacing;
            slotsInCurrentRow++;

            // Move to next row if needed
            if (slotsInCurrentRow >= slot.maxSlotsPerRow)
            {
                currentX = 0;
                currentY -= slot.slotSize.y + slot.spacing;
                slotsInCurrentRow = 0;
            }
        }
    }
} 