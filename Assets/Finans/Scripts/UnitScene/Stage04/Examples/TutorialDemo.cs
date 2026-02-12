using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple demo script to test the tutorial system
/// Attach this to any GameObject in the tutorial.unity scene
/// </summary>
public class TutorialDemo : MonoBehaviour
{
    [Header("Demo Settings")]
    public bool showTutorialOnStart = true;
    public KeyCode testKey = KeyCode.T;
    public KeyCode customKey = KeyCode.Y;
    public KeyCode closeKey = KeyCode.U;
    
    [Header("Custom Tutorial Content")]
    public string[] customTutorialSteps = {
        "This is a custom tutorial!",
        "Step 2: Learn something new",
        "Step 3: Almost there!",
        "Final step: You're ready!"
    };
    
    private TutorialController tutorialController;
    
    void Start()
    {
        // Find the tutorial controller
        tutorialController = Object.FindFirstObjectByType<TutorialController>();
        
        if (tutorialController == null)
        {
            Debug.LogWarning("TutorialDemo: TutorialController not found! Make sure it's added to the TutorialManager GameObject.");
        }
        
        if (showTutorialOnStart)
        {
            Invoke("ShowTutorialOnStart", 1f);
        }
    }
    
    void Update()
    {
        // Press T to show default tutorial
        if (Input.GetKeyDown(testKey))
        {
            ShowDefaultTutorial();
        }
        
        // Press Y to show custom tutorial
        if (Input.GetKeyDown(customKey))
        {
            ShowCustomTutorial();
        }
        
        // Press U to close tutorial
        if (Input.GetKeyDown(closeKey))
        {
            CloseTutorial();
        }
    }
    
    /// <summary>
    /// Show tutorial when scene starts
    /// </summary>
    private void ShowTutorialOnStart()
    {
        if (tutorialController != null && tutorialController.ShouldShowTutorial())
        {
            tutorialController.ShowDefaultTutorial();
        }
    }
    
    /// <summary>
    /// Show the default tutorial
    /// </summary>
    public void ShowDefaultTutorial()
    {
        if (tutorialController != null)
        {
            tutorialController.ShowDefaultTutorial();
        }
        else
        {
            Debug.LogWarning("TutorialDemo: TutorialController not found!");
        }
    }
    
    /// <summary>
    /// Show custom tutorial steps
    /// </summary>
    public void ShowCustomTutorial()
    {
        if (tutorialController != null)
        {
            tutorialController.ShowTutorial(customTutorialSteps);
        }
        else
        {
            Debug.LogWarning("TutorialDemo: TutorialController not found!");
        }
    }
    
    /// <summary>
    /// Close the current tutorial
    /// </summary>
    public void CloseTutorial()
    {
        if (tutorialController != null)
        {
            tutorialController.CloseTutorial();
        }
    }
    
    /// <summary>
    /// Test tutorial from console
    /// </summary>
    [ContextMenu("Test Default Tutorial")]
    public void TestDefaultTutorial()
    {
        ShowDefaultTutorial();
    }
    
    /// <summary>
    /// Test custom tutorial from console
    /// </summary>
    [ContextMenu("Test Custom Tutorial")]
    public void TestCustomTutorial()
    {
        ShowCustomTutorial();
    }
    
    /// <summary>
    /// Close tutorial from console
    /// </summary>
    [ContextMenu("Close Tutorial")]
    public void CloseTutorialFromConsole()
    {
        CloseTutorial();
    }
}
