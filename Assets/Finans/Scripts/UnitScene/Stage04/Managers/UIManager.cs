using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// UIManager manages UI transitions and fade-in sequences for UI panels.
/// </summary>
public class MiniGameUIManager : MonoBehaviour
{
    public static MiniGameUIManager Instance;

    [Header("Fade-In Sequence Settings")]
    [Tooltip("Drag and drop UI panels here in the order you want them to fade in")]
    public List<GameObject> uiPanelsToFade = new List<GameObject>();

    [Tooltip("Delay between each panel fade-in (in seconds)")]
    public float fadeDelay = 0.5f;

    [Tooltip("Duration of each fade-in animation (in seconds)")]
    public float fadeDuration = 1.0f;

    [Tooltip("Automatically start fade sequence on Start")]
    public bool autoStartFade = true;

    [Header("Tap to Start Settings")]
    [Tooltip("Drag and drop your custom Tap to Start button here")]
    public Button tapToStartButton;

    [Tooltip("Drag and drop your custom Tap to Start panel here")]
    public GameObject tapToStartPanel;

    [Tooltip("Show tap to start after fade sequence")]
    public bool showTapToStartAfterFade = true;

    [Tooltip("Tap to start button animation duration")]
    public float tapToStartAnimationDuration = 0.5f;

    [Tooltip("Pulse animation for tap to start button")]
    public bool enablePulseAnimation = true;

    [Tooltip("Pulse animation speed")]
    public float pulseSpeed = 1.0f;

    [Header("Game Start Events")]
    [Tooltip("Event triggered when game should start")]
    public UnityEvent onGameStartRequested;

    private bool fadeSequenceCompleted = false;
    private bool gameStarted = false;
    private CanvasGroup tapToStartCanvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("UIManager: Awake called. Instance set.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Set up tap to start panel
        SetupTapToStartPanel();

        if (autoStartFade)
        {
            StartFadeSequence();
        }
    }

    /// <summary>
    /// Set up the tap to start panel and button
    /// </summary>
    private void SetupTapToStartPanel()
    {
        // Set up button listener if assigned
        if (tapToStartButton != null)
        {
            tapToStartButton.onClick.RemoveAllListeners();
            tapToStartButton.onClick.AddListener(OnTapToStartClicked);
        }

        // Initially hide the tap to start panel if assigned
        if (tapToStartPanel != null)
        {
            tapToStartPanel.SetActive(false);
        }

        // Get or create CanvasGroup for animations
        tapToStartCanvasGroup = tapToStartPanel?.GetComponent<CanvasGroup>();
        if (tapToStartCanvasGroup == null && tapToStartPanel != null)
        {
            tapToStartCanvasGroup = tapToStartPanel.AddComponent<CanvasGroup>();
        }

        // Ensure CanvasGroup alpha is set to 0 initially
        if (tapToStartCanvasGroup != null)
        {
            tapToStartCanvasGroup.alpha = 0f;
        }

        //        Debug.Log($"UIManager: Tap to Start panel setup complete. Panel: {tapToStartPanel != null}, Button: {tapToStartButton != null}, CanvasGroup: {tapToStartCanvasGroup != null}");
    }

    /// <summary>
    /// Starts the fade-in sequence for all UI panels in the specified order
    /// </summary>
    public void StartFadeSequence()
    {
        StartCoroutineSafely(FadeInSequence());
    }

    /// <summary>
    /// Fade in sequence for UI panels
    /// </summary>
    private IEnumerator FadeInSequence()
    {
        //       Debug.Log("UIManager: Starting fade-in sequence");

        // Ensure all panels start inactive and with alpha 0
        foreach (GameObject panel in uiPanelsToFade)
        {
            if (panel != null)
            {
                panel.SetActive(false);
                CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = panel.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
            }
        }

        // Fade in each panel sequentially
        foreach (GameObject panel in uiPanelsToFade)
        {
            if (panel != null)
            {
                panel.SetActive(true);
                StartCoroutineSafely(FadeInPanel(panel));
                yield return new WaitForSeconds(fadeDelay);
            }
        }

        // Wait for the last panel to finish fading in
        yield return new WaitForSeconds(fadeDuration);
        Debug.Log("UIManager: Fade sequence completed");
        fadeSequenceCompleted = true;

        // After fade sequence, trigger tutorial instead of directly showing tap to start
        if (!gameStarted)
        {
            TriggerTutorialStart();
        }
    }

    /// <summary>
    /// Trigger tutorial start after fade sequence
    /// </summary>
    private void TriggerTutorialStart()
    {
        Debug.Log("UIManager: Triggering tutorial start after fade sequence");

        // Show tap to start directly since GameStartController has been removed
        if (showTapToStartAfterFade && tapToStartPanel != null)
        {
            ShowTapToStart();
        }
    }

    /// <summary>
    /// Show the tap to start button with animation
    /// </summary>
    public void ShowTapToStart()
    {
        if (tapToStartPanel != null && !gameStarted)
        {
            tapToStartPanel.SetActive(true);
            StartCoroutineSafely(FadeInTapToStart());

            if (enablePulseAnimation && tapToStartButton != null)
            {
                StartCoroutineSafely(PulseAnimation());
            }

            Debug.Log("UIManager: Tap to Start button shown");
        }
    }

    /// <summary>
    /// Fade in the tap to start button
    /// </summary>
    private IEnumerator FadeInTapToStart()
    {
        if (tapToStartCanvasGroup == null) yield break;

        float elapsedTime = 0f;
        float startAlpha = 0f;
        float targetAlpha = 1f;

        while (elapsedTime < tapToStartAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / tapToStartAnimationDuration;

            tapToStartCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);

            yield return null;
        }

        tapToStartCanvasGroup.alpha = targetAlpha;
    }

    /// <summary>
    /// Pulse animation for the tap to start button
    /// </summary>
    private IEnumerator PulseAnimation()
    {
        if (tapToStartButton == null) yield break;

        Vector3 originalScale = tapToStartButton.transform.localScale;

        while (tapToStartPanel.activeInHierarchy && !gameStarted)
        {
            // Scale up
            float scaleUpTime = 0f;
            while (scaleUpTime < 1f / pulseSpeed)
            {
                scaleUpTime += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 1.1f, scaleUpTime * pulseSpeed);
                tapToStartButton.transform.localScale = originalScale * scale;
                yield return null;
            }

            // Scale down
            float scaleDownTime = 0f;
            while (scaleDownTime < 1f / pulseSpeed)
            {
                scaleDownTime += Time.deltaTime;
                float scale = Mathf.Lerp(1.1f, 1f, scaleDownTime * pulseSpeed);
                tapToStartButton.transform.localScale = originalScale * scale;
                yield return null;
            }
        }

        // Reset scale when animation stops
        tapToStartButton.transform.localScale = originalScale;
    }

    /// <summary>
    /// Handle tap to start button click
    /// </summary>
    private void OnTapToStartClicked()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            Debug.Log("UIManager: Tap to Start clicked - Game starting!");

            // Hide tap to start panel
            StartCoroutineSafely(FadeOutTapToStart());

            // Trigger game start event
            onGameStartRequested?.Invoke();
        }
    }

    /// <summary>
    /// Fade out the tap to start button
    /// </summary>
    private IEnumerator FadeOutTapToStart()
    {
        if (tapToStartCanvasGroup == null) yield break;

        float elapsedTime = 0f;
        float startAlpha = tapToStartCanvasGroup.alpha;
        float targetAlpha = 0f;

        while (elapsedTime < tapToStartAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / tapToStartAnimationDuration;

            tapToStartCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);

            yield return null;
        }

        tapToStartCanvasGroup.alpha = targetAlpha;
        tapToStartPanel.SetActive(false);
    }

    /// <summary>
    /// Coroutine that fades in a single panel
    /// </summary>
    private IEnumerator FadeInPanel(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            yield break;
        }

        float elapsedTime = 0f;
        float startAlpha = 0f;
        float targetAlpha = 1f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeDuration;

            // Smooth fade-in using lerp
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);

            yield return null;
        }

        // Ensure final alpha is exactly 1
        canvasGroup.alpha = targetAlpha;
    }

    /// <summary>
    /// Manually add a panel to the fade sequence
    /// </summary>
    public void AddPanelToFadeSequence(GameObject panel)
    {
        if (panel != null && !uiPanelsToFade.Contains(panel))
        {
            uiPanelsToFade.Add(panel);
            Debug.Log($"UIManager: Added panel to fade sequence: {panel.name}");
        }
    }

    /// <summary>
    /// Remove a panel from the fade sequence
    /// </summary>
    public void RemovePanelFromFadeSequence(GameObject panel)
    {
        if (uiPanelsToFade.Contains(panel))
        {
            uiPanelsToFade.Remove(panel);
            Debug.Log($"UIManager: Removed panel from fade sequence: {panel.name}");
        }
    }

    /// <summary>
    /// Clear all panels from the fade sequence
    /// </summary>
    public void ClearFadeSequence()
    {
        uiPanelsToFade.Clear();
        Debug.Log("UIManager: Cleared fade sequence");
    }

    /// <summary>
    /// Set fade delay between panels
    /// </summary>
    public void SetFadeDelay(float delay)
    {
        fadeDelay = delay;
    }

    /// <summary>
    /// Set fade duration for individual panels
    /// </summary>
    public void SetFadeDuration(float duration)
    {
        fadeDuration = duration;
    }

    /// <summary>
    /// Immediately hide all panels in the fade sequence
    /// </summary>
    public void HideAllPanels()
    {
        foreach (GameObject panel in uiPanelsToFade)
        {
            if (panel != null)
            {
                panel.SetActive(false);
                CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                }
            }
        }
        Debug.Log("UIManager: All panels hidden");
    }

    /// <summary>
    /// Immediately show all panels in the fade sequence (no fade effect)
    /// </summary>
    public void ShowAllPanels()
    {
        foreach (GameObject panel in uiPanelsToFade)
        {
            if (panel != null)
            {
                panel.SetActive(true);
                CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }
        }
        Debug.Log("UIManager: All panels shown");
    }

    /// <summary>
    /// Check if fade sequence is completed
    /// </summary>
    public bool IsFadeSequenceCompleted()
    {
        return fadeSequenceCompleted;
    }

    /// <summary>
    /// Check if game has started
    /// </summary>
    public bool IsGameStarted()
    {
        return gameStarted;
    }

    /// <summary>
    /// Reset the game start state (for restarting)
    /// </summary>
    public void ResetGameStartState()
    {
        gameStarted = false;
        fadeSequenceCompleted = false;
    }

    private void StartCoroutineSafely(IEnumerator routine)
    {
        if (routine == null) return;
        if (isActiveAndEnabled)
        {
            StartCoroutine(routine);
            return;
        }
        GetFallbackRunner().StartCoroutine(routine);
    }

    private static MonoBehaviour GetFallbackRunner()
    {
        if (fallbackRunner != null) return fallbackRunner;
        var go = GameObject.Find("_UIManagerCoroutineRunner");
        if (go == null)
        {
            go = new GameObject("_UIManagerCoroutineRunner");
            GameObject.DontDestroyOnLoad(go);
        }
        fallbackRunner = go.GetComponent<CoroutineProxy>();
        if (fallbackRunner == null) fallbackRunner = go.AddComponent<CoroutineProxy>();
        return fallbackRunner;
    }

    private static CoroutineProxy fallbackRunner;

    private class CoroutineProxy : MonoBehaviour { }
}

