using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Star Rating System - Converts game performance into visual star ratings
/// Integrates with MinigameScoreManager to provide star-based feedback
/// </summary>
[AddComponentMenu("MiniGames/Star Rating System")]
[DisallowMultipleComponent]
public class StarRatingSystem : MonoBehaviour
{
    [Header("Star UI Components")]
    [Tooltip("Array of star GameObjects (should be 3-5 stars)")]
    public GameObject[] stars = new GameObject[3];
    

    [Header("Animation Settings")]
    [Tooltip("Delay between each star animation")]
    public float starAnimationDelay = 0.3f;
    
    [Tooltip("Star scale animation duration")]
    public float starScaleDuration = 0.5f;
    
    [Tooltip("Star scale multiplier for animation")]
    public float starScaleMultiplier = 1.3f;
    
    [Tooltip("Enable particle effects for stars")]
    public bool enableStarParticles = true;
    
    [Tooltip("Particle system for star effects")]
    public ParticleSystem starParticleEffect;

    [Header("Score Thresholds")]
    [Tooltip("Score thresholds for star ratings (0-3 stars)")]
    public int[] scoreThresholds = { 0, 200, 400, 600 };
    
    [Tooltip("Accuracy thresholds for bonus stars")]
    public float[] accuracyThresholds = { 0.5f, 0.7f, 0.9f };
    
    [Tooltip("Time efficiency thresholds for bonus stars")]
    public float[] timeEfficiencyThresholds = { 0.3f, 0.6f, 0.8f };

    [Header("Integration")]
    [Tooltip("Reference to MinigameScoreManager")]
    public MinigameScoreManager scoreManager;
    
    [Tooltip("Auto-find score manager if not assigned")]
    public bool autoFindScoreManager = true;

    [Header("Events")]
    [Tooltip("Called when star rating is calculated")]
    public UnityEngine.Events.UnityEvent<int> OnStarRatingCalculated;
    
    [Tooltip("Called when all stars are animated")]
    public UnityEngine.Events.UnityEvent OnStarAnimationComplete;

    // Private variables
    private int currentStarRating = 0;
    private bool isAnimating = false;
    private Coroutine starAnimationCoroutine;

    void Start()
    {
        InitializeStarSystem();
    }

    /// <summary>
    /// Initialize the star system
    /// </summary>
    private void InitializeStarSystem()
    {
        // Auto-find score manager if needed
        if (autoFindScoreManager && scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<MinigameScoreManager>();
        }

        // Initialize stars as empty
        ResetStars();
        
        Debug.Log("StarRatingSystem: Initialized with " + stars.Length + " stars");
    }


    /// <summary>
    /// Calculate and display star rating based on game performance
    /// </summary>
    public void CalculateAndDisplayStars()
    {
        if (scoreManager == null)
        {
            Debug.LogWarning("StarRatingSystem: No score manager assigned!");
            return;
        }

        int finalScore = scoreManager.GetCurrentScore();
        float accuracy = scoreManager.GetAccuracy();
        float timeEfficiency = scoreManager.GetTimeSaved();
        int mistakes = scoreManager.GetMistakesMade();

        currentStarRating = CalculateStarRating(finalScore, accuracy, timeEfficiency, mistakes);
        
        Debug.Log($"StarRatingSystem: Calculated {currentStarRating} stars (Score: {finalScore}, Accuracy: {accuracy:P0}, Time Efficiency: {timeEfficiency:P0})");
        
        // Start star animation
        if (starAnimationCoroutine != null)
        {
            StopCoroutine(starAnimationCoroutine);
        }
        starAnimationCoroutine = StartCoroutine(AnimateStars(currentStarRating));
        
        OnStarRatingCalculated?.Invoke(currentStarRating);
    }

    /// <summary>
    /// Calculate star rating based on performance metrics
    /// </summary>
    private int CalculateStarRating(int score, float accuracy, float timeEfficiency, int mistakes)
    {
        int starCount = 0;
        
        // Base stars from score
        for (int i = 0; i < scoreThresholds.Length - 1; i++)
        {
            if (score >= scoreThresholds[i + 1])
            {
                starCount = i + 1;
            }
        }
        
        // Bonus star for high accuracy
        if (accuracy >= accuracyThresholds[2]) // 90%+ accuracy
        {
            starCount = Mathf.Min(starCount + 1, this.stars.Length);
        }
        
        // Bonus star for excellent time efficiency
        if (timeEfficiency >= timeEfficiencyThresholds[2]) // 80%+ time saved
        {
            starCount = Mathf.Min(starCount + 1, this.stars.Length);
        }
        
        // Penalty for too many mistakes
        if (mistakes > 3)
        {
            starCount = Mathf.Max(starCount - 1, 0);
        }
        
        return Mathf.Clamp(starCount, 0, this.stars.Length);
    }

    /// <summary>
    /// Animate stars appearing one by one
    /// </summary>
    private IEnumerator AnimateStars(int starCount)
    {
        isAnimating = true;
        
        // Reset all stars first
        ResetStars();
        
        // Animate each star
        for (int i = 0; i < starCount && i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                yield return new WaitForSeconds(starAnimationDelay);
                yield return StartCoroutine(AnimateSingleStar(i));
            }
        }
        
        isAnimating = false;
        OnStarAnimationComplete?.Invoke();
        
        Debug.Log($"StarRatingSystem: Animation complete - {starCount} stars displayed");
    }

    /// <summary>
    /// Animate a single star
    /// </summary>
    private IEnumerator AnimateSingleStar(int starIndex)
    {
        if (starIndex >= stars.Length || stars[starIndex] == null) yield break;
        
        GameObject star = stars[starIndex];
        Vector3 originalScale = star.transform.localScale;
        
        // Show the star
        star.SetActive(true);
        
        // Play particle effect if enabled
        if (enableStarParticles && starParticleEffect != null)
        {
            PlayStarParticleEffect(star.transform.position);
        }
        
        // Scale animation
        float elapsedTime = 0f;
        while (elapsedTime < starScaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / starScaleDuration;
            
            // Ease out animation
            float scale = Mathf.Lerp(0f, starScaleMultiplier, 1f - Mathf.Pow(1f - normalizedTime, 3f));
            star.transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        // Return to normal scale
        elapsedTime = 0f;
        while (elapsedTime < starScaleDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / (starScaleDuration * 0.5f);
            
            float scale = Mathf.Lerp(starScaleMultiplier, 1f, normalizedTime);
            star.transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        star.transform.localScale = originalScale;
        
        // Star is now active
    }


    /// <summary>
    /// Play particle effect at star position
    /// </summary>
    private void PlayStarParticleEffect(Vector3 position)
    {
        if (starParticleEffect != null)
        {
            starParticleEffect.transform.position = position;
            starParticleEffect.Play();
        }
    }

    /// <summary>
    /// Reset all stars to empty state
    /// </summary>
    public void ResetStars()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(false);
            }
        }
        currentStarRating = 0;
    }

    /// <summary>
    /// Show stars immediately without animation
    /// </summary>
    public void ShowStarsImmediate(int starCount)
    {
        if (starAnimationCoroutine != null)
        {
            StopCoroutine(starAnimationCoroutine);
        }
        
        ResetStars();
        currentStarRating = Mathf.Clamp(starCount, 0, stars.Length);
        
        for (int i = 0; i < currentStarRating; i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(true);
            }
        }
        
        OnStarRatingCalculated?.Invoke(currentStarRating);
    }

    /// <summary>
    /// Hide all stars
    /// </summary>
    public void HideStars()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Get current star rating
    /// </summary>
    public int GetCurrentStarRating()
    {
        return currentStarRating;
    }

    /// <summary>
    /// Check if stars are currently animating
    /// </summary>
    public bool IsAnimating()
    {
        return isAnimating;
    }

    /// <summary>
    /// Set custom score thresholds
    /// </summary>
    public void SetScoreThresholds(int[] thresholds)
    {
        scoreThresholds = thresholds;
    }

    /// <summary>
    /// Set custom accuracy thresholds
    /// </summary>
    public void SetAccuracyThresholds(float[] thresholds)
    {
        accuracyThresholds = thresholds;
    }

    /// <summary>
    /// Set custom time efficiency thresholds
    /// </summary>
    public void SetTimeEfficiencyThresholds(float[] thresholds)
    {
        timeEfficiencyThresholds = thresholds;
    }
}
