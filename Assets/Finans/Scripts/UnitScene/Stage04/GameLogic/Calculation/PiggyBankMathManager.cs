using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PiggyBankMathManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI targetAmountText;
    public TextMeshProUGUI currentTotalText;
    public TextMeshProUGUI feedbackText;

    [Header("Coin/Bill Prefabs")]
    public List<GameObject> coinPrefabs; // Assign prefabs in Inspector

    [Header("Spawn Area")]
    public Transform coinSpawnArea;

    [Header("Audio Feedback")]
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    private float targetAmount;
    private float currentTotal;

    private List<GameObject> spawnedCoins = new List<GameObject>();

    void Start()
    {
        StartNewRound();
    }

    void StartNewRound()
    {
        targetAmount = GenerateRandomAmount();
        currentTotal = 0f;
        UpdateUI();
        feedbackText.text = "";

        ClearCoins();
        SpawnCoins();
    }

    float GenerateRandomAmount()
    {
        // Example: $0.25 to $10.00, rounded to nearest $0.05
        float amount = Random.Range(5, 200) * 0.05f;
        return Mathf.Round(amount * 100f) / 100f;
    }

    void SpawnCoins()
    {
        // Example: spawn 5-8 random coins/bills
        for (int i = 0; i < 6; i++)
        {
            int idx = Random.Range(0, coinPrefabs.Count);
            GameObject coin = Instantiate(coinPrefabs[idx], coinSpawnArea);
            // Assume each prefab has a Coin script with value and click event
            Coin coinScript = coin.GetComponent<Coin>();
            coinScript.OnCoinSelected = OnCoinSelected;
            spawnedCoins.Add(coin);
        }
    }

    void ClearCoins()
    {
        foreach (var coin in spawnedCoins)
            Destroy(coin);
        spawnedCoins.Clear();
    }

    public void OnCoinSelected(float value)
    {
        currentTotal += value;
        UpdateUI();

        if (Mathf.Approximately(currentTotal, targetAmount))
        {
            feedbackText.text = "Correct!";
            // Play correct audio
            if (correctSFX != null) MiniGameAudioManager.Instance.PlaySFX(correctSFX);
            else MiniGameAudioManager.Instance.PlayCorrectSFX();
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null) scoreManager.AddScore(10);
            Invoke(nameof(StartNewRound), 1.0f);
        }
        else if (currentTotal > targetAmount)
        {
            feedbackText.text = "Too much! Try again.";
            // Play incorrect audio
            if (wrongSFX != null) MiniGameAudioManager.Instance.PlaySFX(wrongSFX);
            else MiniGameAudioManager.Instance.PlayWrongSFX();
            var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
            if (scoreManager != null) scoreManager.SubtractScore(5);
            // Clear feedback after 2 seconds
            Invoke(nameof(ClearFeedback), 2.0f);
        }
        else
        {
            // Valid drop but not yet correct - provide positive feedback
            feedbackText.text = $"Added ${value:F2}. Keep going!";
            // Clear feedback after 1.5 seconds for positive feedback
            Invoke(nameof(ClearFeedback), 1.5f);
        }
    }
    
    // Method to handle wrong drops from drop zone
    public void OnWrongDrop()
    {
        feedbackText.text = "Wrong drop! Try again.";
        // Play incorrect audio
        if (wrongSFX != null) MiniGameAudioManager.Instance.PlaySFX(wrongSFX);
        else MiniGameAudioManager.Instance.PlayWrongSFX();
                    var scoreManager = MiniGameServices.MinigameScoreService.GetClosest(transform);
                    if (scoreManager != null) scoreManager.SubtractScore(1);
        
        // Clear feedback after 2 seconds
        Invoke(nameof(ClearFeedback), 2.0f);
    }
    
    // Method to clear feedback text
    private void ClearFeedback()
    {
        feedbackText.text = "";
    }

    void UpdateUI()
    {
        targetAmountText.text = $"Target: ${targetAmount:F2}";
        currentTotalText.text = $"Total: ${currentTotal:F2}";
    }
    
    // Public getters for drop zone validation
    public float GetCurrentTotal()
    {
        return currentTotal;
    }
    
    public float GetTargetAmount()
    {
        return targetAmount;
    }
}