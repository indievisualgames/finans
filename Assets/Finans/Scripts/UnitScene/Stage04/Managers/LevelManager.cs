using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// LevelManager generates levels and manages difficulty for all mini-games.
/// </summary>
public class MiniGameLevelManager : MonoBehaviour
{
    /// <summary>
    /// LevelData holds information about a generated level.
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        public int itemsToMatch;
        public string gameType;
    }

    public static MiniGameLevelManager Instance;

    public int currentLevel = 1;
    public int maxLevel = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Generates level data based on the current level and game type.
    /// </summary>
    public LevelData GenerateLevel(string gameType)
    {
        // Example: For drag-and-drop, increase number of coins/notes to match
        int itemsToMatch = Mathf.Min(3 + currentLevel - 1, 10); // Level 1: 3, Level 2: 4, etc.
        return new LevelData { itemsToMatch = itemsToMatch, gameType = gameType };
    }

    public void NextLevel()
    {
        if (currentLevel < maxLevel)
            currentLevel++;
    }

    public void ResetLevel()
    {
        currentLevel = 1;
    }

    // Removed GetCoinDefinitionForLevel and USCurrencyManager references as the new system uses CoinLevelDefinition and DailyLevelManager.
} 