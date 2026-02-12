using UnityEngine;
using UnityEngine.EventSystems;

public class PiggyBankDropZone : MonoBehaviour, IDropHandler
{
    public PiggyBankMathManager mathManager;
    
    [Header("Audio Feedback")]
    public AudioClip wrongDropSFX;

    public void OnDrop(PointerEventData eventData)
    {
        var coin = eventData.pointerDrag?.GetComponent<Coin>();
        if (coin != null)
        {
            // Check if this drop would exceed the target amount
            float currentTotal = mathManager.GetCurrentTotal();
            float targetAmount = mathManager.GetTargetAmount();
            float coinValue = coin.value;
            
            // If adding this coin would exceed the target, it's a wrong drop
            if (currentTotal + coinValue > targetAmount)
            {
                // Wrong drop - use math manager's wrong drop handling
                mathManager.OnWrongDrop();
                
                // Destroy the coin
                Destroy(coin.gameObject);
                    
                Debug.Log($"Wrong drop: Adding {coinValue} to {currentTotal} would exceed target {targetAmount}");
            }
            else
            {
                // Valid drop - proceed with normal logic
                mathManager.OnCoinSelected(coin.value);
                Destroy(coin.gameObject);
            }
        }
        else
        {
            // Dropped something that's not a coin - use math manager's wrong drop handling
            mathManager.OnWrongDrop();
                
            Debug.Log("Wrong drop: Dropped something that's not a coin");
        }
    }
}