using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles drag-and-drop logic for note objects, supporting mouse and touch input.
/// Snaps to valid slot and triggers validation.
/// </summary>
public class NoteDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private bool isDragging = false;
    private Vector3 targetPosition;
    public float dragSmoothness = 10f;

    [Header("Feedback")]
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    public ParticleSystem correctFX;
    public ParticleSystem wrongFX;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        targetPosition = Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        isDragging = false;
        var noteType = GetComponent<NoteTypeIdentifier>();
        var slot = eventData.pointerEnter ? eventData.pointerEnter.GetComponent<NoteSlot>() : null;
        
        if (slot != null && noteType != null && noteType.noteType == slot.expectedNoteType)
        {
            // Correct match
            transform.SetParent(slot.transform, true);
            transform.position = slot.transform.position;
            
            // Play feedback
            if (correctSFX != null) MiniGameAudioManager.Instance.PlaySFX(correctSFX);
            else Debug.LogWarning("Correct SFX not assigned!");
            if (correctFX != null)
            {
                var effect = Instantiate(correctFX, transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }
            
            // Show success message
            // MessageManager.Instance.ShowMessage($"Correct! ${noteType.noteType} note matched!", MessageType.Success);
            
            // Add score for correct match
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null)
            {
                scoreManager.AddItemCompleted();
            }
            Destroy(gameObject);
        }
        else if (slot != null && noteType != null)
        {
            // Dropped on wrong slot - this is the only case where we should penalize
            if (wrongSFX != null) MiniGameAudioManager.Instance.PlaySFX(wrongSFX);
            else Debug.LogWarning("Wrong SFX not assigned!");
            if (wrongFX != null)
            {
                var effect = Instantiate(wrongFX, transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }
            
            // Show error message
            // MessageManager.Instance.ShowMessage("Wrong note! Try again.", MessageType.Error);
            
            // Record mistake
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null)
            {
                scoreManager.RecordMistake();
            }
            
            // Return to original position
            transform.position = startPosition;
            transform.SetParent(originalParent, true);
        }
        else
        {
            // Dropped outside of any slot - just return to original position without penalty
            transform.position = startPosition;
            transform.SetParent(originalParent, true);
        }
    }

    void Update()
    {
        if (isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dragSmoothness);
        }
    }
} 