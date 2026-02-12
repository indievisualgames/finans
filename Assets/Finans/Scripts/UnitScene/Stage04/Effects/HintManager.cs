using UnityEngine;

/// <summary>
/// Global manager for controlling hint settings across all drop zones
/// </summary>
public class HintManager : MonoBehaviour
{
    public static HintManager Instance;
    
    [Header("Hint Settings")]
    [Tooltip("Enable/disable yellow color hints when dragging objects over drop zones")]
    public bool enableHints = true;
    
    [Tooltip("Color to show when dragging over drop zones (hint color)")]
    public Color hintColor = Color.yellow;
    
    [Tooltip("Scale multiplier when dragging over drop zones")]
    public float hintScaleMultiplier = 1.1f;
    
    [Header("Advanced Settings")]
    [Tooltip("Enable/disable scale hints")]
    public bool enableScaleHints = true;
    
    [Tooltip("Enable/disable color hints")]
    public bool enableColorHints = true;
    
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
    /// Check if hints are enabled globally
    /// </summary>
    public static bool AreHintsEnabled()
    {
        return Instance != null && Instance.enableHints;
    }
    
    /// <summary>
    /// Check if color hints are enabled
    /// </summary>
    public static bool AreColorHintsEnabled()
    {
        return Instance != null && Instance.enableHints && Instance.enableColorHints;
    }
    
    /// <summary>
    /// Check if scale hints are enabled
    /// </summary>
    public static bool AreScaleHintsEnabled()
    {
        return Instance != null && Instance.enableHints && Instance.enableScaleHints;
    }
    
    /// <summary>
    /// Get the hint color
    /// </summary>
    public static Color GetHintColor()
    {
        return Instance != null ? Instance.hintColor : Color.yellow;
    }
    
    /// <summary>
    /// Get the hint scale multiplier
    /// </summary>
    public static float GetHintScaleMultiplier()
    {
        return Instance != null ? Instance.hintScaleMultiplier : 1.1f;
    }
    
    /// <summary>
    /// Enable hints globally
    /// </summary>
    public void EnableHints()
    {
        enableHints = true;
        Debug.Log("Hints enabled globally");
    }
    
    /// <summary>
    /// Disable hints globally
    /// </summary>
    public void DisableHints()
    {
        enableHints = false;
        Debug.Log("Hints disabled globally");
    }
    
    /// <summary>
    /// Toggle hints on/off
    /// </summary>
    public void ToggleHints()
    {
        enableHints = !enableHints;
        Debug.Log($"Hints {(enableHints ? "enabled" : "disabled")} globally");
    }
    
    [ContextMenu("Enable Hints")]
    public void EnableHintsContext()
    {
        EnableHints();
    }
    
    [ContextMenu("Disable Hints")]
    public void DisableHintsContext()
    {
        DisableHints();
    }
    
    [ContextMenu("Toggle Hints")]
    public void ToggleHintsContext()
    {
        ToggleHints();
    }
    
    [ContextMenu("Show Hint Settings")]
    public void ShowHintSettings()
    {
        Debug.Log($"=== Hint Manager Settings ===");
        Debug.Log($"Hints Enabled: {enableHints}");
        Debug.Log($"Color Hints: {enableColorHints}");
        Debug.Log($"Scale Hints: {enableScaleHints}");
        Debug.Log($"Hint Color: {hintColor}");
        Debug.Log($"Hint Scale Multiplier: {hintScaleMultiplier}");
    }
}
