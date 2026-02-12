using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// SlotValidator is a generic validator for drag-and-drop slots, supporting both coins and notes.
/// Set the expectedType and it will validate any draggable with a matching identifier.
/// </summary>
public class SlotValidator : MonoBehaviour, IDropHandler
{
    public string expectedType; // e.g., "Penny", "$1", etc.
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    public void OnDrop(PointerEventData eventData)
    {
        // Try to get a coin or note identifier
        var coin = eventData.pointerDrag?.GetComponent<CoinTypeIdentifier>();
        var note = eventData.pointerDrag?.GetComponent<NoteTypeIdentifier>();
        bool isMatch = false;

        if (coin != null && coin.coinType == expectedType)
        {
            isMatch = true;
        }
        else if (note != null && note.noteType == expectedType)
        {
            isMatch = true;
        }

        if (isMatch)
        {
            Destroy(eventData.pointerDrag);
            if (correctSFX != null) MiniGameAudioManager.Instance.PlaySFX(correctSFX);
            else Debug.LogWarning("Correct SFX not assigned in SlotValidator!");
            
            // Add score for correct match
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null)
            {
                scoreManager.AddItemCompleted();
            }
        }
        else
        {
            if (wrongSFX != null) MiniGameAudioManager.Instance.PlaySFX(wrongSFX);
            else Debug.LogWarning("Wrong SFX not assigned in SlotValidator!");
            
            // Record mistake
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null)
            {
                scoreManager.RecordMistake();
            }
        }
    }
} 