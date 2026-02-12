using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// CoinSlot validates if the correct coin is dropped onto it.
/// </summary>
public class CoinSlot : MonoBehaviour, IDropHandler
{
    public string expectedCoinType;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    public void OnDrop(PointerEventData eventData)
    {
        var coin = eventData.pointerDrag?.GetComponent<CoinTypeIdentifier>();
        if (coin != null && coin.coinType == expectedCoinType)
        {
            // Correct match
            coin.transform.position = transform.position;
            coin.transform.SetParent(transform);
            if (correctSFX != null) MiniGameAudioManager.Instance.PlaySFX(correctSFX);
            else Debug.LogWarning("Correct SFX not assigned in CoinSlot!");
            
            // Add score for correct match
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null)
            {
                scoreManager.AddItemCompleted();
            }
        }
        else
        {
            // Incorrect match
            if (wrongSFX != null) MiniGameAudioManager.Instance.PlaySFX(wrongSFX);
            else Debug.LogWarning("Wrong SFX not assigned in CoinSlot!");
            
            // Record mistake
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null)
            {
                scoreManager.RecordMistake();
            }
        }
    }
} 