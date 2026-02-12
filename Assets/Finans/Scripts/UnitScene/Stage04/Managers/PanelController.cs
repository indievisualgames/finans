using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// PanelController provides array-based control for panels/game objects with fade effects,
/// visibility management, and delay functionality. Supports inspector-based drag and drop.
/// </summary>
public class PanelController : MonoBehaviour
{
    [System.Serializable]
    public class PanelElement
    {
        [Header("Panel Reference")]
        [Tooltip("Drag and drop the panel/game object to control")]
        public GameObject panel;
        
        [Header("Fade Settings")]
        [Tooltip("Enable fade in/out effects")]
        public bool enableFade = true;
        
        [Tooltip("Fade duration in seconds")]
        [Range(0.1f, 5f)]
        public float fadeDuration = 1f;
        
        [Tooltip("Fade curve type")]
        public FadeCurve fadeCurve = FadeCurve.EaseInOut;
        
        [Header("Visibility Settings")]
        [Tooltip("Initial visibility state")]
        public bool startVisible = false;
        
        [Tooltip("Delay before showing/hiding (seconds)")]
        [Range(0f, 5f)]
        public float visibilityDelay = 0f;
        
        [Header("Auto Control")]
        [Tooltip("Automatically show on start")]
        public bool autoShowOnStart = false;
        
        [Tooltip("Automatically hide after delay")]
        public bool autoHideAfterDelay = false;
        
        [Tooltip("Auto hide delay (seconds)")]
        [Range(1f, 30f)]
        public float autoHideDelay = 5f;
        
        [Header("Events")]
        [Tooltip("Event triggered when panel becomes visible")]
        public UnityEngine.Events.UnityEvent onPanelShow;
        
        [Tooltip("Event triggered when panel becomes hidden")]
        public UnityEngine.Events.UnityEvent onPanelHide;
        
        [Tooltip("Event triggered when fade completes")]
        public UnityEngine.Events.UnityEvent onFadeComplete;
        
        // Internal state
        [HideInInspector] public bool isVisible = false;
        [HideInInspector] public bool isFading = false;
        [HideInInspector] public CanvasGroup canvasGroup;
    }
    
    public enum FadeCurve
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce
    }
    
    [Header("Panel Elements")]
    [Tooltip("Array of panel elements to control")]
    public PanelElement[] panelElements = new PanelElement[0];
    
    [Header("Global Settings")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = true;
    
    [Tooltip("Default fade duration for all panels")]
    [Range(0.1f, 5f)]
    public float defaultFadeDuration = 1f;
    
    [Tooltip("Default visibility delay for all panels")]
    [Range(0f, 5f)]
    public float defaultVisibilityDelay = 0f;
    
    [Header("Batch Operations")]
    [Tooltip("Show all panels")]
    public bool showAllPanels = false;
    
    [Tooltip("Hide all panels")]
    public bool hideAllPanels = false;
    
    [Tooltip("Reset all panels to initial state")]
    public bool resetAllPanels = false;
    
    // Private variables
    private Dictionary<GameObject, PanelElement> panelLookup = new Dictionary<GameObject, PanelElement>();
    private Coroutine[] fadeCoroutines;
    
    void Awake()
    {
        InitializePanelElements();
    }
    
    void Start()
    {
        SetupInitialStates();
        StartAutoShowPanels();
    }
    
    void Update()
    {
        // Handle batch operations from inspector
        if (showAllPanels)
        {
            ShowAllPanels();
            showAllPanels = false;
        }
        
        if (hideAllPanels)
        {
            HideAllPanels();
            hideAllPanels = false;
        }
        
        if (resetAllPanels)
        {
            ResetAllPanels();
            resetAllPanels = false;
        }
    }
    
    /// <summary>
    /// Initialize panel elements and setup CanvasGroup components
    /// </summary>
    private void InitializePanelElements()
    {
        fadeCoroutines = new Coroutine[panelElements.Length];
        
        for (int i = 0; i < panelElements.Length; i++)
        {
            var element = panelElements[i];
            
            if (element.panel == null)
            {
                if (enableDebugLogs)
                    Debug.LogWarning($"PanelController: Panel element {i} has no panel assigned!");
                continue;
            }
            
            // Setup CanvasGroup for fade effects
            if (element.enableFade)
            {
                element.canvasGroup = element.panel.GetComponent<CanvasGroup>();
                if (element.canvasGroup == null)
                {
                    element.canvasGroup = element.panel.AddComponent<CanvasGroup>();
                }
            }
            
            // Add to lookup dictionary
            panelLookup[element.panel] = element;
            
            if (enableDebugLogs)
                Debug.Log($"PanelController: Initialized panel '{element.panel.name}'");
        }
    }
    
    /// <summary>
    /// Setup initial visibility states
    /// </summary>
    private void SetupInitialStates()
    {
        foreach (var element in panelElements)
        {
            if (element.panel == null) continue;
            
            element.isVisible = element.startVisible;
            element.panel.SetActive(element.startVisible);
            
            if (element.enableFade && element.canvasGroup != null)
            {
                element.canvasGroup.alpha = element.startVisible ? 1f : 0f;
            }
        }
    }
    
    /// <summary>
    /// Start auto-show panels that are configured for it
    /// </summary>
    private void StartAutoShowPanels()
    {
        foreach (var element in panelElements)
        {
            if (element.panel == null) continue;
            
            if (element.autoShowOnStart)
            {
                StartCoroutine(ShowPanelWithDelay(element, element.visibilityDelay));
            }
        }
    }
    
    /// <summary>
    /// Show a specific panel by index
    /// </summary>
    public void ShowPanel(int index)
    {
        if (index < 0 || index >= panelElements.Length)
        {
            if (enableDebugLogs)
                Debug.LogError($"PanelController: Invalid panel index {index}");
            return;
        }
        
        ShowPanel(panelElements[index]);
    }
    
    /// <summary>
    /// Show a specific panel by GameObject reference
    /// </summary>
    public void ShowPanel(GameObject panel)
    {
        if (panelLookup.TryGetValue(panel, out PanelElement element))
        {
            ShowPanel(element);
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogError($"PanelController: Panel '{panel.name}' not found in panel elements!");
        }
    }
    
    /// <summary>
    /// Show a panel element
    /// </summary>
    private void ShowPanel(PanelElement element)
    {
        if (element.panel == null) return;
        
        if (element.visibilityDelay > 0f)
        {
            StartCoroutine(ShowPanelWithDelay(element, element.visibilityDelay));
        }
        else
        {
            ShowPanelImmediate(element);
        }
    }
    
    /// <summary>
    /// Show panel with delay
    /// </summary>
    private IEnumerator ShowPanelWithDelay(PanelElement element, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowPanelImmediate(element);
    }
    
    /// <summary>
    /// Show panel immediately
    /// </summary>
    private void ShowPanelImmediate(PanelElement element)
    {
        if (element.panel == null) return;
        
        // Stop any existing fade coroutine
        StopFadeCoroutine(element);
        
        if (element.enableFade && element.canvasGroup != null)
        {
            // Start fade in
            element.panel.SetActive(true);
            element.isFading = true;
            fadeCoroutines[System.Array.IndexOf(panelElements, element)] = 
                StartCoroutine(FadePanel(element, 0f, 1f, element.fadeDuration));
        }
        else
        {
            // Instant show
            element.panel.SetActive(true);
            element.isVisible = true;
            element.onPanelShow?.Invoke();
            
            if (enableDebugLogs)
                Debug.Log($"PanelController: Showed panel '{element.panel.name}'");
        }
        
        // Setup auto-hide if configured
        if (element.autoHideAfterDelay)
        {
            StartCoroutine(AutoHidePanel(element, element.autoHideDelay));
        }
    }
    
    /// <summary>
    /// Hide a specific panel by index
    /// </summary>
    public void HidePanel(int index)
    {
        if (index < 0 || index >= panelElements.Length)
        {
            if (enableDebugLogs)
                Debug.LogError($"PanelController: Invalid panel index {index}");
            return;
        }
        
        HidePanel(panelElements[index]);
    }
    
    /// <summary>
    /// Hide a specific panel by GameObject reference
    /// </summary>
    public void HidePanel(GameObject panel)
    {
        if (panelLookup.TryGetValue(panel, out PanelElement element))
        {
            HidePanel(element);
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogError($"PanelController: Panel '{panel.name}' not found in panel elements!");
        }
    }
    
    /// <summary>
    /// Hide a panel element
    /// </summary>
    private void HidePanel(PanelElement element)
    {
        if (element.panel == null) return;
        
        if (element.visibilityDelay > 0f)
        {
            StartCoroutine(HidePanelWithDelay(element, element.visibilityDelay));
        }
        else
        {
            HidePanelImmediate(element);
        }
    }
    
    /// <summary>
    /// Hide panel with delay
    /// </summary>
    private IEnumerator HidePanelWithDelay(PanelElement element, float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePanelImmediate(element);
    }
    
    /// <summary>
    /// Hide panel immediately
    /// </summary>
    private void HidePanelImmediate(PanelElement element)
    {
        if (element.panel == null) return;
        
        // Stop any existing fade coroutine
        StopFadeCoroutine(element);
        
        if (element.enableFade && element.canvasGroup != null)
        {
            // Start fade out
            element.isFading = true;
            fadeCoroutines[System.Array.IndexOf(panelElements, element)] = 
                StartCoroutine(FadePanel(element, 1f, 0f, element.fadeDuration));
        }
        else
        {
            // Instant hide
            element.panel.SetActive(false);
            element.isVisible = false;
            element.onPanelHide?.Invoke();
            
            if (enableDebugLogs)
                Debug.Log($"PanelController: Hid panel '{element.panel.name}'");
        }
    }
    
    /// <summary>
    /// Toggle panel visibility
    /// </summary>
    public void TogglePanel(int index)
    {
        if (index < 0 || index >= panelElements.Length)
        {
            if (enableDebugLogs)
                Debug.LogError($"PanelController: Invalid panel index {index}");
            return;
        }
        
        var element = panelElements[index];
        if (element.isVisible)
        {
            HidePanel(element);
        }
        else
        {
            ShowPanel(element);
        }
    }
    
    /// <summary>
    /// Toggle panel visibility by GameObject reference
    /// </summary>
    public void TogglePanel(GameObject panel)
    {
        if (panelLookup.TryGetValue(panel, out PanelElement element))
        {
            if (element.isVisible)
            {
                HidePanel(element);
            }
            else
            {
                ShowPanel(element);
            }
        }
    }
    
    /// <summary>
    /// Show all panels
    /// </summary>
    public void ShowAllPanels()
    {
        foreach (var element in panelElements)
        {
            ShowPanel(element);
        }
        
        if (enableDebugLogs)
            Debug.Log("PanelController: Showed all panels");
    }
    
    /// <summary>
    /// Hide all panels
    /// </summary>
    public void HideAllPanels()
    {
        foreach (var element in panelElements)
        {
            HidePanel(element);
        }
        
        if (enableDebugLogs)
            Debug.Log("PanelController: Hid all panels");
    }
    
    /// <summary>
    /// Reset all panels to initial state
    /// </summary>
    public void ResetAllPanels()
    {
        foreach (var element in panelElements)
        {
            ResetPanel(element);
        }
        
        if (enableDebugLogs)
            Debug.Log("PanelController: Reset all panels");
    }
    
    /// <summary>
    /// Reset a panel to its initial state
    /// </summary>
    public void ResetPanel(int index)
    {
        if (index < 0 || index >= panelElements.Length)
        {
            if (enableDebugLogs)
                Debug.LogError($"PanelController: Invalid panel index {index}");
            return;
        }
        
        ResetPanel(panelElements[index]);
    }
    
    /// <summary>
    /// Reset a panel to its initial state
    /// </summary>
    private void ResetPanel(PanelElement element)
    {
        if (element.panel == null) return;
        
        // Stop any existing fade coroutine
        StopFadeCoroutine(element);
        
        // Reset to initial state
        element.isVisible = element.startVisible;
        element.isFading = false;
        element.panel.SetActive(element.startVisible);
        
        if (element.enableFade && element.canvasGroup != null)
        {
            element.canvasGroup.alpha = element.startVisible ? 1f : 0f;
        }
    }
    
    /// <summary>
    /// Fade panel with specified curve
    /// </summary>
    private IEnumerator FadePanel(PanelElement element, float startAlpha, float endAlpha, float duration)
    {
        if (element.canvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        element.canvasGroup.alpha = startAlpha;
        
        // Ensure panel is active for fade
        element.panel.SetActive(true);
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;
            
            // Apply fade curve
            float curveValue = GetFadeCurveValue(normalizedTime, element.fadeCurve);
            element.canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            
            yield return null;
        }
        
        // Ensure final alpha value
        element.canvasGroup.alpha = endAlpha;
        
        // Update visibility state
        element.isVisible = endAlpha > 0f;
        element.isFading = false;
        
        // Deactivate panel if faded out
        if (endAlpha <= 0f)
        {
            element.panel.SetActive(false);
            element.onPanelHide?.Invoke();
        }
        else
        {
            element.onPanelShow?.Invoke();
        }
        
        element.onFadeComplete?.Invoke();
        
        if (enableDebugLogs)
            Debug.Log($"PanelController: Fade completed for '{element.panel.name}' (Alpha: {endAlpha})");
    }
    
    /// <summary>
    /// Get fade curve value based on curve type
    /// </summary>
    private float GetFadeCurveValue(float normalizedTime, FadeCurve curve)
    {
        switch (curve)
        {
            case FadeCurve.Linear:
                return normalizedTime;
            case FadeCurve.EaseIn:
                return Mathf.Pow(normalizedTime, 2f);
            case FadeCurve.EaseOut:
                return 1f - Mathf.Pow(1f - normalizedTime, 2f);
            case FadeCurve.EaseInOut:
                return normalizedTime < 0.5f ? 
                    2f * Mathf.Pow(normalizedTime, 2f) : 
                    1f - 2f * Mathf.Pow(1f - normalizedTime, 2f);
            case FadeCurve.Bounce:
                return 1f - Mathf.Pow(1f - normalizedTime, 2f) * Mathf.Sin(normalizedTime * Mathf.PI * 3f);
            default:
                return normalizedTime;
        }
    }
    
    /// <summary>
    /// Auto-hide panel after delay
    /// </summary>
    private IEnumerator AutoHidePanel(PanelElement element, float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePanel(element);
    }
    
    /// <summary>
    /// Stop fade coroutine for a panel
    /// </summary>
    private void StopFadeCoroutine(PanelElement element)
    {
        int index = System.Array.IndexOf(panelElements, element);
        if (index >= 0 && fadeCoroutines[index] != null)
        {
            StopCoroutine(fadeCoroutines[index]);
            fadeCoroutines[index] = null;
        }
        element.isFading = false;
    }
    
    /// <summary>
    /// Get panel visibility state
    /// </summary>
    public bool IsPanelVisible(int index)
    {
        if (index < 0 || index >= panelElements.Length) return false;
        return panelElements[index].isVisible;
    }
    
    /// <summary>
    /// Get panel visibility state by GameObject reference
    /// </summary>
    public bool IsPanelVisible(GameObject panel)
    {
        if (panelLookup.TryGetValue(panel, out PanelElement element))
        {
            return element.isVisible;
        }
        return false;
    }
    
    /// <summary>
    /// Check if panel is currently fading
    /// </summary>
    public bool IsPanelFading(int index)
    {
        if (index < 0 || index >= panelElements.Length) return false;
        return panelElements[index].isFading;
    }
    
    /// <summary>
    /// Set panel fade duration
    /// </summary>
    public void SetPanelFadeDuration(int index, float duration)
    {
        if (index < 0 || index >= panelElements.Length) return;
        panelElements[index].fadeDuration = duration;
    }
    
    /// <summary>
    /// Set panel visibility delay
    /// </summary>
    public void SetPanelVisibilityDelay(int index, float delay)
    {
        if (index < 0 || index >= panelElements.Length) return;
        panelElements[index].visibilityDelay = delay;
    }
    
    /// <summary>
    /// Enable/disable fade for a panel
    /// </summary>
    public void SetPanelFadeEnabled(int index, bool enabled)
    {
        if (index < 0 || index >= panelElements.Length) return;
        panelElements[index].enableFade = enabled;
    }
    
    /// <summary>
    /// Get panel element by index
    /// </summary>
    public PanelElement GetPanelElement(int index)
    {
        if (index < 0 || index >= panelElements.Length) return null;
        return panelElements[index];
    }
    
    /// <summary>
    /// Get panel element by GameObject reference
    /// </summary>
    public PanelElement GetPanelElement(GameObject panel)
    {
        panelLookup.TryGetValue(panel, out PanelElement element);
        return element;
    }
    
    /// <summary>
    /// Get total panel count
    /// </summary>
    public int GetPanelCount()
    {
        return panelElements.Length;
    }
} 