using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example script demonstrating how to use PanelController with button events
/// </summary>
public class PanelControllerExample : MonoBehaviour
{
    [Header("Panel Controller Reference")]
    [Tooltip("Drag and drop the PanelController component here")]
    public PanelController panelController;
    
    [Header("Button References")]
    [Tooltip("Button to show panel 0")]
    public Button showPanel0Button;
    
    [Tooltip("Button to hide panel 0")]
    public Button hidePanel0Button;
    
    [Tooltip("Button to toggle panel 0")]
    public Button togglePanel0Button;
    
    [Tooltip("Button to show panel 1")]
    public Button showPanel1Button;
    
    [Tooltip("Button to hide panel 1")]
    public Button hidePanel1Button;
    
    [Tooltip("Button to toggle panel 1")]
    public Button togglePanel1Button;
    
    [Header("Batch Operation Buttons")]
    [Tooltip("Button to show all panels")]
    public Button showAllButton;
    
    [Tooltip("Button to hide all panels")]
    public Button hideAllButton;
    
    [Tooltip("Button to reset all panels")]
    public Button resetAllButton;
    
    [Header("Sequential Operations")]
    [Tooltip("Button to show panels in sequence")]
    public Button showSequenceButton;
    
    [Tooltip("Button to hide panels in sequence")]
    public Button hideSequenceButton;
    
    [Header("Settings")]
    [Tooltip("Delay between sequential operations")]
    [Range(0.1f, 2f)]
    public float sequenceDelay = 0.5f;

    void Start()
    {
        SetupButtonListeners();
    }
    
    /// <summary>
    /// Setup button click listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        // Individual panel controls
        if (showPanel0Button != null)
            showPanel0Button.onClick.AddListener(() => ShowPanel(0));
            
        if (hidePanel0Button != null)
            hidePanel0Button.onClick.AddListener(() => HidePanel(0));
            
        if (togglePanel0Button != null)
            togglePanel0Button.onClick.AddListener(() => TogglePanel(0));
            
        if (showPanel1Button != null)
            showPanel1Button.onClick.AddListener(() => ShowPanel(1));
            
        if (hidePanel1Button != null)
            hidePanel1Button.onClick.AddListener(() => HidePanel(1));
            
        if (togglePanel1Button != null)
            togglePanel1Button.onClick.AddListener(() => TogglePanel(1));
        
        // Batch operations
        if (showAllButton != null)
            showAllButton.onClick.AddListener(ShowAllPanels);
            
        if (hideAllButton != null)
            hideAllButton.onClick.AddListener(HideAllPanels);
            
        if (resetAllButton != null)
            resetAllButton.onClick.AddListener(ResetAllPanels);
        
        // Sequential operations
        if (showSequenceButton != null)
            showSequenceButton.onClick.AddListener(ShowPanelsInSequence);
            
        if (hideSequenceButton != null)
            hideSequenceButton.onClick.AddListener(HidePanelsInSequence);
    }
    
    /// <summary>
    /// Show a specific panel by index
    /// </summary>
    public void ShowPanel(int index)
    {
        if (panelController != null)
        {
            panelController.ShowPanel(index);
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Hide a specific panel by index
    /// </summary>
    public void HidePanel(int index)
    {
        if (panelController != null)
        {
            panelController.HidePanel(index);
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Toggle a specific panel by index
    /// </summary>
    public void TogglePanel(int index)
    {
        if (panelController != null)
        {
            panelController.TogglePanel(index);
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Show all panels
    /// </summary>
    public void ShowAllPanels()
    {
        if (panelController != null)
        {
            panelController.ShowAllPanels();
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Hide all panels
    /// </summary>
    public void HideAllPanels()
    {
        if (panelController != null)
        {
            panelController.HideAllPanels();
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Reset all panels
    /// </summary>
    public void ResetAllPanels()
    {
        if (panelController != null)
        {
            panelController.ResetAllPanels();
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Show panels in sequence with delays
    /// </summary>
    public void ShowPanelsInSequence()
    {
        if (panelController != null)
        {
            StartCoroutine(ShowPanelsInSequenceCoroutine());
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Hide panels in sequence with delays
    /// </summary>
    public void HidePanelsInSequence()
    {
        if (panelController != null)
        {
            StartCoroutine(HidePanelsInSequenceCoroutine());
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Coroutine to show panels in sequence
    /// </summary>
    private System.Collections.IEnumerator ShowPanelsInSequenceCoroutine()
    {
        int panelCount = panelController.GetPanelCount();
        
        for (int i = 0; i < panelCount; i++)
        {
            panelController.ShowPanel(i);
            yield return new WaitForSeconds(sequenceDelay);
        }
    }
    
    /// <summary>
    /// Coroutine to hide panels in sequence
    /// </summary>
    private System.Collections.IEnumerator HidePanelsInSequenceCoroutine()
    {
        int panelCount = panelController.GetPanelCount();
        
        for (int i = 0; i < panelCount; i++)
        {
            panelController.HidePanel(i);
            yield return new WaitForSeconds(sequenceDelay);
        }
    }
    
    /// <summary>
    /// Show a specific panel by GameObject reference
    /// </summary>
    public void ShowPanel(GameObject panel)
    {
        if (panelController != null)
        {
            panelController.ShowPanel(panel);
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Hide a specific panel by GameObject reference
    /// </summary>
    public void HidePanel(GameObject panel)
    {
        if (panelController != null)
        {
            panelController.HidePanel(panel);
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Toggle a specific panel by GameObject reference
    /// </summary>
    public void TogglePanel(GameObject panel)
    {
        if (panelController != null)
        {
            panelController.TogglePanel(panel);
        }
        else
        {
            Debug.LogError("PanelControllerExample: PanelController reference is missing!");
        }
    }
    
    /// <summary>
    /// Check if a panel is visible
    /// </summary>
    public bool IsPanelVisible(int index)
    {
        if (panelController != null)
        {
            return panelController.IsPanelVisible(index);
        }
        return false;
    }
    
    /// <summary>
    /// Check if a panel is visible by GameObject reference
    /// </summary>
    public bool IsPanelVisible(GameObject panel)
    {
        if (panelController != null)
        {
            return panelController.IsPanelVisible(panel);
        }
        return false;
    }
    
    /// <summary>
    /// Set panel fade duration
    /// </summary>
    public void SetPanelFadeDuration(int index, float duration)
    {
        if (panelController != null)
        {
            panelController.SetPanelFadeDuration(index, duration);
        }
    }
    
    /// <summary>
    /// Set panel visibility delay
    /// </summary>
    public void SetPanelVisibilityDelay(int index, float delay)
    {
        if (panelController != null)
        {
            panelController.SetPanelVisibilityDelay(index, delay);
        }
    }
    
    /// <summary>
    /// Enable/disable fade for a panel
    /// </summary>
    public void SetPanelFadeEnabled(int index, bool enabled)
    {
        if (panelController != null)
        {
            panelController.SetPanelFadeEnabled(index, enabled);
        }
    }
} 