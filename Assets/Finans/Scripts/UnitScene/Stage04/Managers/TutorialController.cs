using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// TutorialController integrates with the existing HowToTutorialTemplate
/// and provides easy access to tutorial functionality
/// </summary>
public class TutorialController : MonoBehaviour
{
    [Header("Tutorial System")]
    public HowToTutorialTemplate tutorialTemplate;
    public TutAudioManager audioManager; // Optional: will be auto-located if null
    public bool showTutorialOnStart = true;
    public bool enableDebugMode = true;

    [Header("Default Tutorial Content")]
    public string[] defaultTutorialSteps = {
        "Welcome to the Mini Game Tutorial!",
        "This is step 2 of the tutorial",
        "Here's step 3 with more information",
        "Final step - you're ready to begin!"
    };
    public AudioClip[] defaultStepAudioClips; // Optional: align by index with defaultTutorialSteps

    [Header("Tutorial Triggers")]
    public bool showTutorialOnFirstPlay = true;
    public bool showTutorialOnLevelStart = false;
    public bool showTutorialOnGameStart = false;

    [Header("Tutorial Settings")]
    public bool allowSkip = true;
    public bool showProgress = true;
    public bool autoAdvance = false;
    public float autoAdvanceDelay = 3f;

    // Private variables
    private bool hasShownTutorial = false;
    private bool isInitialized = false;

    // Public properties
    public bool IsTutorialActive => tutorialTemplate != null && tutorialTemplate.IsTutorialActive();

    void Start()
    {
        InitializeTutorialSystem();

        if (showTutorialOnStart)
        {
            Invoke("ShowTutorialOnStart", 1f);
        }
    }

    /// <summary>
    /// Initialize the tutorial system
    /// </summary>
    private void InitializeTutorialSystem()
    {
        if (isInitialized) return;

        // Try to find existing tutorial template
        if (tutorialTemplate == null)
        {
            tutorialTemplate = Object.FindFirstObjectByType<HowToTutorialTemplate>();
        }

        // Try to find audio manager if not set
        if (audioManager == null)
        {
            audioManager = TutAudioManager.Instance != null ? TutAudioManager.Instance : Object.FindFirstObjectByType<TutAudioManager>();
        }

        // Configure tutorial settings if found
        if (tutorialTemplate != null)
        {
            tutorialTemplate.allowSkip = allowSkip;
            tutorialTemplate.showProgress = showProgress;
            tutorialTemplate.autoAdvance = autoAdvance;
            tutorialTemplate.autoAdvanceDelay = autoAdvanceDelay;
            if (audioManager != null && tutorialTemplate.audioManager == null)
            {
                tutorialTemplate.audioManager = audioManager;
            }

            // Set up event listeners
            SetupTutorialEvents();

            isInitialized = true;

            if (enableDebugMode)
                Debug.Log("TutorialController: Tutorial system initialized successfully");
        }
        else
        {
            Debug.LogError("TutorialController: HowToTutorialTemplate not found in scene!");
        }
    }

    /// <summary>
    /// Set up tutorial event listeners
    /// </summary>
    private void SetupTutorialEvents()
    {
        if (tutorialTemplate == null) return;

        tutorialTemplate.OnTutorialStart.AddListener(() =>
        {
            if (enableDebugMode) Debug.Log("Tutorial started!");
        });

        tutorialTemplate.OnTutorialComplete.AddListener(() =>
        {
            if (enableDebugMode) Debug.Log("Tutorial completed!");
            hasShownTutorial = true;
        });

        tutorialTemplate.OnTutorialSkip.AddListener(() =>
        {
            //            if (enableDebugMode) Debug.Log("Tutorial skipped!");
            hasShownTutorial = true;
        });

        tutorialTemplate.OnStepChanged.AddListener((stepIndex) =>
        {
            //            if (enableDebugMode) Debug.Log($"Step changed to: {stepIndex + 1}");
        });
    }

    /// <summary>
    /// Show tutorial when scene starts
    /// </summary>
    private void ShowTutorialOnStart()
    {
        if (tutorialTemplate != null && ShouldShowTutorial())
        {
            ShowDefaultTutorial();
        }
    }

    /// <summary>
    /// Show the default tutorial
    /// </summary>
    public void ShowDefaultTutorial()
    {
        if (tutorialTemplate == null)
        {
            Debug.LogWarning("TutorialController: Tutorial template not found!");
            return;
        }

        // Ensure the TutorialManager GameObject is active before showing tutorial
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("TutorialController: TutorialManager GameObject is inactive. Activating it to show tutorial.");
            gameObject.SetActive(true);
        }

        // If audio clips are provided and lengths match, build advanced steps
        if (defaultStepAudioClips != null && defaultStepAudioClips.Length == defaultTutorialSteps.Length)
        {
            var steps = new HowToTutorialTemplate.TutorialStep[defaultTutorialSteps.Length];
            for (int i = 0; i < steps.Length; i++)
            {
                steps[i] = new HowToTutorialTemplate.TutorialStep
                {
                    title = $"Step {i + 1}",
                    description = defaultTutorialSteps[i],
                    showPreviousButton = i > 0,
                    showNextButton = i < defaultTutorialSteps.Length - 1,
                    showSkipButton = allowSkip,
                    stepAudio = defaultStepAudioClips[i]
                };
            }
            tutorialTemplate.ShowTutorial(steps);
        }
        else
        {
            tutorialTemplate.ShowTutorial(defaultTutorialSteps);
        }
    }

    /// <summary>
    /// Show custom tutorial steps
    /// </summary>
    public void ShowTutorial(string[] steps)
    {
        if (tutorialTemplate == null)
        {
            Debug.LogWarning("TutorialController: Tutorial template not found!");
            return;
        }

        // Ensure the TutorialManager GameObject is active before showing tutorial
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("TutorialController: TutorialManager GameObject is inactive. Activating it to show tutorial.");
            gameObject.SetActive(true);
        }

        tutorialTemplate.ShowTutorial(steps);
    }

    /// <summary>
    /// Show custom tutorial with advanced options
    /// </summary>
    public void ShowAdvancedTutorial(HowToTutorialTemplate.TutorialStep[] steps)
    {
        if (tutorialTemplate == null)
        {
            Debug.LogWarning("TutorialController: Tutorial template not found!");
            return;
        }

        // Ensure the TutorialManager GameObject is active before showing tutorial
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("TutorialController: TutorialManager GameObject is inactive. Activating it to show tutorial.");
            gameObject.SetActive(true);
        }

        tutorialTemplate.ShowTutorial(steps);
    }

    /// <summary>
    /// Close the current tutorial
    /// </summary>
    public void CloseTutorial()
    {
        if (tutorialTemplate != null)
        {
            tutorialTemplate.CloseTutorial();
        }
    }

    /// <summary>
    /// Check if tutorial should be shown
    /// </summary>
    public bool ShouldShowTutorial()
    {
        if (!showTutorialOnFirstPlay) return false;
        if (hasShownTutorial) return false;
        return true;
    }

    /// <summary>
    /// Force show tutorial (for testing)
    /// </summary>
    [ContextMenu("Show Tutorial")]
    public void ForceShowTutorial()
    {
        ShowDefaultTutorial();
    }

    /// <summary>
    /// Reset tutorial state
    /// </summary>
    [ContextMenu("Reset Tutorial State")]
    public void ResetTutorialState()
    {
        hasShownTutorial = false;
        if (enableDebugMode) Debug.Log("TutorialController: Tutorial state reset");
    }

    /// <summary>
    /// Test tutorial from console
    /// </summary>
    [ContextMenu("Test Tutorial")]
    public void TestTutorial()
    {
        ShowDefaultTutorial();
    }

    /// <summary>
    /// Test custom tutorial from console
    /// </summary>
    [ContextMenu("Test Custom Tutorial")]
    public void TestCustomTutorial()
    {
        ShowTutorial(new string[] {
            "Custom Tutorial Step 1",
            "Custom Tutorial Step 2",
            "Custom Tutorial Step 3"
        });
    }
}
