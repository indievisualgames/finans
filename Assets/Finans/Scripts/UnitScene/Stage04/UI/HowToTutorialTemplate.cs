using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enhanced HowToTutorialTemplate displays a sequence of tutorial steps for any mini-game.
/// Features dynamic panel control, customizable buttons, progress tracking, and smooth transitions.
/// </summary>
public class HowToTutorialTemplate : MonoBehaviour
{
    [Header("Audio")]
    public TutAudioManager audioManager; // Central tutorial audio manager
    public Button repeatAudioButton; // Button to repeat current step audio
    public bool showRepeatDuringPlayback = false; // If false, show repeat only after audio finished
    public GameObject audioPlayingIndicator; // Optional UI indicator when audio is playing
    public GameObject audioPausedIndicator; // Optional UI indicator when audio is paused

    [Header("Tutorial Panels")]
    public GameObject tutorialPanel; // Main tutorial container
    public GameObject[] stepPanels; // Individual step panels (optional)
    public GameObject progressPanel; // Progress indicator panel

    [Header("Text Elements")]
    public TextMeshProUGUI tutorialTitle; // Tutorial title text
    public TextMeshProUGUI tutorialText; // Main tutorial step text
    public TextMeshProUGUI stepCounterText; // Shows "Step X of Y"
    public TextMeshProUGUI progressText; // Progress percentage or text

    [Header("Navigation Buttons")]
    public Button previousButton; // Go to previous step
    public Button nextButton; // Go to next step
    public Button skipButton; // Skip entire tutorial
    public Button closeButton; // Close tutorial
    public Button restartButton; // Restart tutorial from beginning

    [Header("Button Text")]
    public TextMeshProUGUI previousButtonText;
    public TextMeshProUGUI nextButtonText;
    public TextMeshProUGUI skipButtonText;
    public TextMeshProUGUI closeButtonText;
    public TextMeshProUGUI restartButtonText;

    [Header("Progress Bar")]
    public Slider progressBar; // Visual progress indicator
    public Image progressFill; // Progress bar fill image

    [Header("Animation & Effects")]
    public Animator tutorialAnimator; // Tutorial panel animator
    public float stepTransitionDelay = 0.3f; // Delay between step transitions
    public bool enableStepTransitions = true; // Enable smooth step transitions

    [Header("Tutorial Settings")]
    public bool allowSkip = true; // Whether tutorial can be skipped
    public bool showProgress = true; // Show progress indicator
    public bool loopTutorial = false; // Loop tutorial when finished
    public bool autoAdvance = false; // Auto-advance to next step
    public float autoAdvanceDelay = 3f; // Delay before auto-advancing

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnTutorialStart;
    public UnityEngine.Events.UnityEvent OnTutorialComplete;
    public UnityEngine.Events.UnityEvent OnTutorialSkip;
    public UnityEngine.Events.UnityEvent<int> OnStepChanged; // Passes current step index

    // Private variables
    private TutorialStep[] tutorialSteps;
    private int currentStepIndex = 0;
    private bool isTutorialActive = false;
    private Coroutine autoAdvanceCoroutine;
    private Coroutine transitionCoroutine;

    // Tutorial step data structure
    [System.Serializable]
    public class TutorialStep
    {
        public string title; // Step title
        public string description; // Step description
        public Sprite stepImage; // Optional step image
        public GameObject highlightObject; // Object to highlight during this step
        public bool showPreviousButton = true; // Show previous button for this step
        public bool showNextButton = true; // Show next button for this step
        public bool showSkipButton = true; // Show skip button for this step
        public string customNextButtonText; // Custom text for next button
        public string customPreviousButtonText; // Custom text for previous button
        public AudioClip stepAudio; // Optional audio for this step
    }

    void Start()
    {
        InitializeButtons();
        SetupEventListeners();
        HideAllStepPanels();

        // Attempt to find audio manager if not assigned
        if (audioManager == null)
            audioManager = TutAudioManager.Instance != null ? TutAudioManager.Instance : Object.FindFirstObjectByType<TutAudioManager>();

        WireAudioEvents();
        UpdateAudioUIState(isPlaying: false, isPaused: false, clipEnded: true);
    }

    /// <summary>
    /// Initialize button listeners
    /// </summary>
    private void InitializeButtons()
    {
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousStep);
        if (nextButton != null)
            nextButton.onClick.AddListener(NextStep);
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipTutorial);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTutorial);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartTutorial);

        if (repeatAudioButton != null)
            repeatAudioButton.onClick.AddListener(RepeatCurrentAudio);
    }

    /// <summary>
    /// Setup event listeners for dynamic control
    /// </summary>
    private void SetupEventListeners()
    {
        // You can add custom event listeners here
        OnTutorialStart?.AddListener(() => Debug.Log("Tutorial started"));
        OnTutorialComplete?.AddListener(() => Debug.Log("Tutorial completed"));
        OnTutorialSkip?.AddListener(() => Debug.Log("Tutorial skipped"));
        OnStepChanged?.AddListener((stepIndex) => Debug.Log($"Step changed to: {stepIndex}"));
    }

    /// <summary>
    /// Wire audio manager event callbacks to update UI state
    /// </summary>
    private void WireAudioEvents()
    {
        if (audioManager == null) return;

        // Clear previous listeners to avoid duplicate subscriptions
        audioManager.OnClipStarted.RemoveListener(OnAudioStarted);
        audioManager.OnClipPaused.RemoveListener(OnAudioPaused);
        audioManager.OnClipResumed.RemoveListener(OnAudioResumed);
        audioManager.OnClipStopped.RemoveListener(OnAudioStopped);
        audioManager.OnClipEnded.RemoveListener(OnAudioEnded);

        audioManager.OnClipStarted.AddListener(OnAudioStarted);
        audioManager.OnClipPaused.AddListener(OnAudioPaused);
        audioManager.OnClipResumed.AddListener(OnAudioResumed);
        audioManager.OnClipStopped.AddListener(OnAudioStopped);
        audioManager.OnClipEnded.AddListener(OnAudioEnded);
    }

    /// <summary>
    /// Show tutorial with custom steps
    /// </summary>
    public void ShowTutorial(TutorialStep[] steps)
    {
        if (steps == null || steps.Length == 0)
        {
            Debug.LogWarning("No tutorial steps provided!");
            return;
        }

        tutorialSteps = steps;
        currentStepIndex = 0;
        isTutorialActive = true;

        // Show main tutorial panel
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        // Trigger tutorial start event
        OnTutorialStart?.Invoke();

        // Show first step
        ShowCurrentStep();

        // Start auto-advance if enabled
        if (autoAdvance)
            StartAutoAdvance();
    }

    /// <summary>
    /// Show tutorial with simple string array (backward compatibility)
    /// </summary>
    public void ShowTutorial(string[] stepDescriptions)
    {
        TutorialStep[] steps = new TutorialStep[stepDescriptions.Length];

        for (int i = 0; i < stepDescriptions.Length; i++)
        {
            steps[i] = new TutorialStep
            {
                title = $"Step {i + 1}",
                description = stepDescriptions[i],
                showPreviousButton = i > 0,
                showNextButton = i < stepDescriptions.Length - 1,
                showSkipButton = allowSkip
            };
        }

        ShowTutorial(steps);
    }

    /// <summary>
    /// Show the current tutorial step
    /// </summary>
    private void ShowCurrentStep()
    {
        if (tutorialSteps == null || currentStepIndex >= tutorialSteps.Length)
            return;

        TutorialStep currentStep = tutorialSteps[currentStepIndex];

        // Update step content
        if (tutorialTitle != null)
            tutorialTitle.text = currentStep.title;

        if (tutorialText != null)
            tutorialText.text = currentStep.description;

        // Update step counter
        if (stepCounterText != null)
            stepCounterText.text = $"Step {currentStepIndex + 1} of {tutorialSteps.Length}";

        // Update progress
        UpdateProgress();

        // Show/hide step panels if available
        ShowStepPanel(currentStepIndex);

        // Update button states
        UpdateButtonStates();

        // Update button text
        UpdateButtonText();

        // Highlight object if specified
        HighlightObject(currentStep.highlightObject);

        // Play step audio via central audio manager
        HandleStepAudio(currentStep.stepAudio);

        // Trigger step change event
        OnStepChanged?.Invoke(currentStepIndex);

        // Start transition animation
        if (enableStepTransitions)
            StartStepTransition();
    }

    /// <summary>
    /// Show specific step panel
    /// </summary>
    private void ShowStepPanel(int stepIndex)
    {
        if (stepPanels == null || stepIndex >= stepPanels.Length)
            return;

        // Hide all step panels first
        HideAllStepPanels();

        // Show current step panel
        if (stepPanels[stepIndex] != null)
            stepPanels[stepIndex].SetActive(true);
    }

    /// <summary>
    /// Hide all step panels
    /// </summary>
    private void HideAllStepPanels()
    {
        if (stepPanels == null) return;

        foreach (GameObject panel in stepPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    /// <summary>
    /// Update button visibility and interactability
    /// </summary>
    private void UpdateButtonStates()
    {
        TutorialStep currentStep = tutorialSteps[currentStepIndex];

        // Previous button
        if (previousButton != null)
        {
            previousButton.gameObject.SetActive(currentStep.showPreviousButton && currentStepIndex > 0);
            previousButton.interactable = currentStepIndex > 0;
        }

        // Next button
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(currentStep.showNextButton && currentStepIndex < tutorialSteps.Length - 1);
            nextButton.interactable = currentStepIndex < tutorialSteps.Length - 1;
        }

        // Skip button
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(currentStep.showSkipButton && allowSkip);
            skipButton.interactable = allowSkip;
        }

        // Close button (always visible when tutorial is active)
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(true);
        }

        // Restart button (show when tutorial is complete)
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(currentStepIndex >= tutorialSteps.Length - 1);
        }
    }

    /// <summary>
    /// Update button text based on current step
    /// </summary>
    private void UpdateButtonText()
    {
        TutorialStep currentStep = tutorialSteps[currentStepIndex];

        // Next button text
        if (nextButtonText != null)
        {
            if (!string.IsNullOrEmpty(currentStep.customNextButtonText))
                nextButtonText.text = currentStep.customNextButtonText;
            else if (currentStepIndex >= tutorialSteps.Length - 1)
                nextButtonText.text = "Finish";
            else
                nextButtonText.text = "Next";
        }

        // Previous button text
        if (previousButtonText != null)
        {
            if (!string.IsNullOrEmpty(currentStep.customPreviousButtonText))
                previousButtonText.text = currentStep.customPreviousButtonText;
            else
                previousButtonText.text = "Previous";
        }

        // Skip button text
        if (skipButtonText != null)
            skipButtonText.text = "Skip Tutorial";

        // Close button text
        if (closeButtonText != null)
            closeButtonText.text = "Close";

        // Restart button text
        if (restartButtonText != null)
            restartButtonText.text = "Restart Tutorial";
    }

    /// <summary>
    /// Update progress indicator
    /// </summary>
    private void UpdateProgress()
    {
        if (!showProgress) return;

        float progress = (float)(currentStepIndex + 1) / tutorialSteps.Length;

        // Update progress bar
        if (progressBar != null)
            progressBar.value = progress;

        // Update progress text
        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";

        // Update progress fill color
        if (progressFill != null)
        {
            Color progressColor = Color.Lerp(Color.red, Color.green, progress);
            progressFill.color = progressColor;
        }
    }

    /// <summary>
    /// Highlight specific object during tutorial step
    /// </summary>
    private void HighlightObject(GameObject objectToHighlight)
    {
        // You can implement custom highlighting logic here
        // For example, add outline, change color, or show pointer
        if (objectToHighlight != null)
        {
            Debug.Log($"Highlighting object: {objectToHighlight.name}");
        }
    }

    /// <summary>
    /// Play audio for current step
    /// </summary>
    private void HandleStepAudio(AudioClip audioClip)
    {
        if (audioManager == null)
            audioManager = TutAudioManager.Instance != null ? TutAudioManager.Instance : Object.FindFirstObjectByType<TutAudioManager>();

        if (audioManager == null)
        {
            // Fallback: no audio manager available
            UpdateAudioUIState(isPlaying: false, isPaused: false, clipEnded: true);
            return;
        }

        // Stop any currently playing audio to prevent overlap, then play new clip
        audioManager.Stop();
        if (audioClip != null)
        {
            audioManager.Play(audioClip, loop: false);
        }
        else
        {
            UpdateAudioUIState(isPlaying: false, isPaused: false, clipEnded: true);
        }
    }

    private void RepeatCurrentAudio()
    {
        if (audioManager == null) return;
        // Restart current clip from beginning
        audioManager.Repeat();
    }

    private void OnAudioStarted()
    {
        UpdateAudioUIState(isPlaying: true, isPaused: false, clipEnded: false);
    }

    private void OnAudioPaused()
    {
        UpdateAudioUIState(isPlaying: false, isPaused: true, clipEnded: false);
    }

    private void OnAudioResumed()
    {
        UpdateAudioUIState(isPlaying: true, isPaused: false, clipEnded: false);
    }

    private void OnAudioStopped()
    {
        UpdateAudioUIState(isPlaying: false, isPaused: false, clipEnded: true);
    }

    private void OnAudioEnded()
    {
        UpdateAudioUIState(isPlaying: false, isPaused: false, clipEnded: true);
    }

    private void UpdateAudioUIState(bool isPlaying, bool isPaused, bool clipEnded)
    {
        if (audioPlayingIndicator != null)
            audioPlayingIndicator.SetActive(isPlaying);
        if (audioPausedIndicator != null)
            audioPausedIndicator.SetActive(isPaused);

        if (repeatAudioButton != null)
        {
            bool showRepeat = showRepeatDuringPlayback ? (isPlaying || isPaused || clipEnded) : clipEnded;
            repeatAudioButton.gameObject.SetActive(showRepeat);
        }
    }

    /// <summary>
    /// Start step transition animation
    /// </summary>
    private void StartStepTransition()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutineSafe(StepTransition());
    }

    /// <summary>
    /// Step transition coroutine
    /// </summary>
    private IEnumerator StepTransition()
    {
        // You can add fade in/out effects here
        yield return new WaitForSeconds(stepTransitionDelay);

        transitionCoroutine = null;
    }

    /// <summary>
    /// Go to next step
    /// </summary>
    public void NextStep()
    {
        if (!isTutorialActive || currentStepIndex >= tutorialSteps.Length - 1)
        {
            CompleteTutorial();
            return;
        }

        // Stop current step audio before moving on
        if (audioManager != null) audioManager.Stop();

        currentStepIndex++;
        ShowCurrentStep();

        // Restart auto-advance if enabled
        if (autoAdvance)
            StartAutoAdvance();
    }

    /// <summary>
    /// Go to previous step
    /// </summary>
    public void PreviousStep()
    {
        if (!isTutorialActive || currentStepIndex <= 0)
            return;

        // Stop current step audio before moving back
        if (audioManager != null) audioManager.Stop();

        currentStepIndex--;
        ShowCurrentStep();

        // Restart auto-advance if enabled
        if (autoAdvance)
            StartAutoAdvance();
    }

    /// <summary>
    /// Skip entire tutorial
    /// </summary>
    public void SkipTutorial()
    {
        if (!allowSkip) return;

        OnTutorialSkip?.Invoke();
        CloseTutorial();
    }

    /// <summary>
    /// Restart tutorial from beginning
    /// </summary>
    public void RestartTutorial()
    {
        currentStepIndex = 0;
        ShowCurrentStep();

        // Restart auto-advance if enabled
        if (autoAdvance)
            StartAutoAdvance();
    }

    /// <summary>
    /// Complete tutorial
    /// </summary>
    private void CompleteTutorial()
    {
        OnTutorialComplete?.Invoke();

        if (loopTutorial)
        {
            RestartTutorial();
        }
        else
        {
            CloseTutorial();
        }
    }

    /// <summary>
    /// Close tutorial
    /// </summary>
    public void CloseTutorial()
    {
        isTutorialActive = false;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        // Ensure any audio is stopped
        if (audioManager != null) audioManager.Stop();

        // Stop auto-advance
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutineSafe(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        // Stop transition
        if (transitionCoroutine != null)
        {
            StopCoroutineSafe(transitionCoroutine);
            transitionCoroutine = null;
        }

        // Hide all step panels
        HideAllStepPanels();
    }

    /// <summary>
    /// Start auto-advance functionality
    /// </summary>
    private void StartAutoAdvance()
    {
        if (!autoAdvance) return;

        if (autoAdvanceCoroutine != null)
            StopCoroutineSafe(autoAdvanceCoroutine);

        autoAdvanceCoroutine = StartCoroutineSafe(AutoAdvanceCoroutine());
    }

    /// <summary>
    /// Auto-advance coroutine
    /// </summary>
    private IEnumerator AutoAdvanceCoroutine()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);

        if (isTutorialActive && currentStepIndex < tutorialSteps.Length - 1)
        {
            NextStep();
        }

        autoAdvanceCoroutine = null;
    }

    /// <summary>
    /// Go to specific step by index
    /// </summary>
    public void GoToStep(int stepIndex)
    {
        if (!isTutorialActive || stepIndex < 0 || stepIndex >= tutorialSteps.Length)
            return;

        currentStepIndex = stepIndex;
        ShowCurrentStep();
    }

    /// <summary>
    /// Get current step index
    /// </summary>
    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }

    /// <summary>
    /// Get total number of steps
    /// </summary>
    public int GetTotalSteps()
    {
        return tutorialSteps?.Length ?? 0;
    }

    /// <summary>
    /// Check if tutorial is currently active
    /// </summary>
    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }

    /// <summary>
    /// Enable or disable skip functionality
    /// </summary>
    public void SetSkipEnabled(bool enabled)
    {
        allowSkip = enabled;
        if (isTutorialActive)
            UpdateButtonStates();
    }

    /// <summary>
    /// Set auto-advance delay
    /// </summary>
    public void SetAutoAdvanceDelay(float delay)
    {
        autoAdvanceDelay = delay;
    }

    /// <summary>
    /// Enable or disable auto-advance
    /// </summary>
    public void SetAutoAdvance(bool enabled)
    {
        autoAdvance = enabled;
        if (enabled && isTutorialActive)
            StartAutoAdvance();
        else if (!enabled && autoAdvanceCoroutine != null)
            StopCoroutineSafe(autoAdvanceCoroutine);
    }

    /// <summary>
    /// Safely start a coroutine, handling inactive GameObject scenarios
    /// </summary>
    private Coroutine StartCoroutineSafe(IEnumerator coroutine)
    {
        // Check if this GameObject is active and enabled
        if (gameObject.activeInHierarchy && enabled)
        {
            try
            {
                return StartCoroutine(coroutine);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"HowToTutorialTemplate: Failed to start coroutine - {e.Message}");
                return null;
            }
        }
        else
        {
            Debug.LogWarning("HowToTutorialTemplate: Cannot start coroutine - GameObject is inactive or component is disabled");

            // Execute the coroutine logic immediately without delay if it's a simple transition
            if (coroutine is System.Collections.IEnumerator)
            {
                // For simple transitions, we can execute immediately
                if (coroutine.GetType().Name.Contains("StepTransition"))
                {
                    // Skip the transition delay and continue
                    return null;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Safely stop a coroutine, handling null references
    /// </summary>
    private void StopCoroutineSafe(Coroutine coroutine)
    {
        if (coroutine != null && gameObject.activeInHierarchy && enabled)
        {
            try
            {
                StopCoroutine(coroutine);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"HowToTutorialTemplate: Failed to stop coroutine - {e.Message}");
            }
        }
    }
}