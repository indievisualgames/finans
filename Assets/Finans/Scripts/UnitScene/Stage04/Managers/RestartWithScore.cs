using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// RestartWithScore - Accumulates the player's current/final score into a persistent total
/// and restarts the game session. If a specific game manager is assigned, it will call its
/// restart method; otherwise it reloads the active scene.
/// </summary>
[AddComponentMenu("MiniGames/Managers/Restart With Score")]
[DisallowMultipleComponent]
public class RestartWithScore : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Score manager used to read current and final scores")] public MinigameScoreManager scoreManager;
    [Tooltip("Optional coin game manager to restart the session (if present)")] public MasterCoinGameManager coinGameManager;

    [Header("Persistence")] 
    [Tooltip("PlayerPrefs key used to persist the accumulated score across restarts")]
    public string cumulativeScoreKey = "MG_TotalScore";

    [Header("Restart Behavior")] 
    [Tooltip("If true and no specific game manager is assigned, reload the active scene to restart")]
    public bool reloadSceneIfNoManager = true;

    void Awake()
    {
        // Best-effort auto-wiring when not assigned in inspector
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<MinigameScoreManager>();
        }
        if (coinGameManager == null)
        {
            coinGameManager = FindFirstObjectByType<MasterCoinGameManager>();
        }
    }

    /// <summary>
    /// Adds the current session's score to the cumulative total and restarts the game.
    /// Intended to be hooked to a UI button.
    /// </summary>
    public void RestartAndAccumulate()
    {
        int scoreToAdd = 0;
        if (scoreManager != null)
        {
            // If the game is completed, prefer finalScore; otherwise use currentScore
            scoreToAdd = scoreManager.IsGameCompleted() ? scoreManager.finalScore : scoreManager.GetCurrentScore();
        }

        // Accumulate into PlayerPrefs
        int currentTotal = PlayerPrefs.GetInt(cumulativeScoreKey, 0);
        int newTotal = Mathf.Max(0, currentTotal + Mathf.Max(0, scoreToAdd));
        PlayerPrefs.SetInt(cumulativeScoreKey, newTotal);
        PlayerPrefs.Save();
        Debug.Log($"RestartWithScore: Added {scoreToAdd} to cumulative total. New total = {newTotal}");

        // Restart via coin game manager if available; otherwise reload scene
        if (coinGameManager != null)
        {
            coinGameManager.RestartSession();
            return;
        }
        
        if (reloadSceneIfNoManager)
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }

    /// <summary>
    /// Returns the current cumulative score from PlayerPrefs.
    /// </summary>
    public int GetCumulativeScore()
    {
        return PlayerPrefs.GetInt(cumulativeScoreKey, 0);
    }
}


