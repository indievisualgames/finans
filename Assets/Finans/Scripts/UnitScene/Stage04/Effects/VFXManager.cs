using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages VFX effects for Screen Space - Camera canvas mode
/// Handles proper positioning and visibility of particle effects
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;
    
    [Header("VFX Settings")]
    public Camera uiCamera;
    public Canvas targetCanvas;
    public float effectScale = 1f;
    public bool enableDebugLogs = true;
    
    [Header("Default Effects")]
    public ParticleSystem defaultCorrectEffect;
    public ParticleSystem defaultWrongEffect;
    public ParticleSystem defaultPickupEffect;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVFXManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeVFXManager()
    {
        // Find UI camera if not assigned
        if (uiCamera == null)
        {
            uiCamera = Camera.main;
            if (uiCamera == null)
            {
                var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
                foreach (var cam in cameras)
                {
                    if (cam.orthographic)
                    {
                        uiCamera = cam;
                        break;
                    }
                }
            }
        }
        
        // Find target canvas if not assigned
        if (targetCanvas == null)
        {
            targetCanvas = Object.FindFirstObjectByType<Canvas>();
        }
        
        if (enableDebugLogs)
            Debug.Log($"VFXManager initialized. UI Camera: {uiCamera?.name}, Canvas: {targetCanvas?.name}");
    }
    
    /// <summary>
    /// Converts screen position to world position for VFX
    /// </summary>
    public Vector3 ScreenToWorldPosition(Vector3 screenPosition)
    {
        if (uiCamera == null)
        {
            Debug.LogWarning("VFXManager: No UI camera found!");
            return screenPosition;
        }
        
        // Convert screen position to world position
        Vector3 worldPosition = uiCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, uiCamera.nearClipPlane + 1f));
        
        if (enableDebugLogs)
            Debug.Log($"Screen position {screenPosition} converted to world position {worldPosition}");
        
        return worldPosition;
    }
    
    /// <summary>
    /// Converts UI position to world position for better VFX placement
    /// </summary>
    public Vector3 UIToWorldPosition(Vector3 uiPosition, Canvas targetCanvas = null)
    {
        Canvas canvas = targetCanvas ?? this.targetCanvas;
        if (canvas == null || uiCamera == null)
        {
            Debug.LogWarning("VFXManager: Canvas or UI camera not found for UI to world conversion!");
            return uiPosition;
        }
        
        // For Screen Space - Camera mode, use the canvas's plane distance
        float planeDistance = canvas.planeDistance;
        Vector3 worldPosition = uiCamera.ScreenToWorldPoint(new Vector3(uiPosition.x, uiPosition.y, planeDistance));
        
        if (enableDebugLogs)
            Debug.Log($"UI position {uiPosition} converted to world position {worldPosition} using plane distance {planeDistance}");
        
        return worldPosition;
    }
    
    /// <summary>
    /// Plays a correct VFX effect at the specified position
    /// </summary>
    public void PlayCorrectVFX(Vector3 position, Transform parent = null, bool isWorldPosition = false)
    {
        Vector3 worldPosition = isWorldPosition ? position : ScreenToWorldPosition(position);
        
        if (defaultCorrectEffect != null)
        {
            PlayVFXEffect(defaultCorrectEffect, worldPosition, parent);
        }
        else
        {
            // Don't use hardcoded fallback - log warning instead
            Debug.LogWarning("VFXManager: No default correct effect assigned and no fallback available!");
        }
        
        if (enableDebugLogs)
            Debug.Log($"Correct VFX played at world position: {worldPosition}");
    }
    
    /// <summary>
    /// Plays a wrong VFX effect at the specified position
    /// </summary>
    public void PlayWrongVFX(Vector3 position, Transform parent = null, bool isWorldPosition = false)
    {
        Vector3 worldPosition = isWorldPosition ? position : ScreenToWorldPosition(position);
        
        if (defaultWrongEffect != null)
        {
            PlayVFXEffect(defaultWrongEffect, worldPosition, parent);
        }
        else
        {
            // Don't use hardcoded fallback - log warning instead
            Debug.LogWarning("VFXManager: No default wrong effect assigned and no fallback available!");
        }
        
        if (enableDebugLogs)
            Debug.Log($"Wrong VFX played at world position: {worldPosition}");
    }
    
    /// <summary>
    /// Plays a wrong VFX effect with enhanced visibility and positioning
    /// </summary>
    public void PlayWrongVFXEnhanced(Vector3 position, Transform parent = null, bool isWorldPosition = false)
    {
        Vector3 worldPosition = isWorldPosition ? position : ScreenToWorldPosition(position);
        
        // Priority 1: Use default wrong effect if assigned
        if (defaultWrongEffect != null)
        {
            // Create the effect with enhanced settings
            var effect = Instantiate(defaultWrongEffect, worldPosition, Quaternion.identity);
            
            // Fix the scale issue - the prefab has scale 10,10,10 which is too big!
            effect.transform.localScale = Vector3.one; // Reset to normal scale
            
            // Set parent
            if (parent != null)
            {
                effect.transform.SetParent(parent);
            }
            
            // Ensure maximum visibility
            var renderer = effect.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 9999; // Maximum sorting order
                renderer.sortingLayerName = "UI";
                renderer.enabled = true;
            }
            
            // Ensure the effect is active and visible
            effect.gameObject.SetActive(true);
            effect.Play();
            
            // Destroy after completion
            float destroyDelay = effect.main.duration + 2f; // Extra delay for wrong VFX
            Destroy(effect.gameObject, destroyDelay);
            
            if (enableDebugLogs)
                Debug.Log($"Enhanced Wrong VFX played at world position: {worldPosition}");
        }
        else
        {
            // Priority 2: Create a fixed wrong VFX with proper settings
            CreateFixedWrongVFX(worldPosition, parent);
            Debug.Log("Created fixed wrong VFX since no default effect was assigned");
        }
    }
    
    /// <summary>
    /// Plays a pickup VFX effect at the specified position
    /// </summary>
    public void PlayPickupVFX(Vector3 position, Transform parent = null, bool isWorldPosition = false)
    {
        Vector3 worldPosition = isWorldPosition ? position : ScreenToWorldPosition(position);
        
        if (defaultPickupEffect != null)
        {
            PlayVFXEffect(defaultPickupEffect, worldPosition, parent);
        }
        else
        {
            var effect = ParticleEffectManager.CreatePickupEffect();
            effect.transform.position = worldPosition;
            if (parent != null) effect.transform.SetParent(parent);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration + 1f);
        }
        
        if (enableDebugLogs)
            Debug.Log($"Pickup VFX played at world position: {worldPosition}");
    }
    
    /// <summary>
    /// Plays a custom VFX effect at the specified position
    /// </summary>
    public void PlayVFXEffect(ParticleSystem effectPrefab, Vector3 worldPosition, Transform parent = null)
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning("VFXManager: Effect prefab is null!");
            return;
        }
        
        // Create the effect
        var effect = Instantiate(effectPrefab, worldPosition, Quaternion.identity);
        
        // Scale the effect if needed
        if (effectScale != 1f)
        {
            effect.transform.localScale = Vector3.one * effectScale;
        }
        
        // Set parent
        if (parent != null)
        {
            effect.transform.SetParent(parent);
        }
        else
        {
            effect.transform.SetParent(null); // Root level
        }
        
        // Ensure proper sorting order and visibility
        var renderer = effect.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 1000; // High sorting order to ensure visibility
            renderer.sortingLayerName = "UI"; // Use UI sorting layer if available
            
            // Ensure the renderer is enabled
            renderer.enabled = true;
        }
        
        // Ensure the particle system is enabled
        effect.gameObject.SetActive(true);
        
        // Play the effect
        effect.Play();
        
        // Destroy after completion
        float destroyDelay = effect.main.duration + 1f;
        Destroy(effect.gameObject, destroyDelay);
        
        if (enableDebugLogs)
            Debug.Log($"Custom VFX effect played at world position: {worldPosition}, Destroy delay: {destroyDelay}s");
    }
    
    /// <summary>
    /// Creates a VFX effect that follows the UI element
    /// </summary>
    public void PlayVFXOnUIElement(RectTransform uiElement, ParticleSystem effectPrefab, bool followElement = false)
    {
        if (uiElement == null || effectPrefab == null) return;
        
        Vector3 worldPosition = ScreenToWorldPosition(uiElement.position);
        
        if (followElement)
        {
            // Create effect as child of UI element
            PlayVFXEffect(effectPrefab, worldPosition, uiElement);
        }
        else
        {
            // Create effect at world position
            PlayVFXEffect(effectPrefab, worldPosition, null);
        }
    }
    
    /// <summary>
    /// Debug method to test VFX positioning
    /// </summary>
    [ContextMenu("Test VFX Positioning")]
    public void TestVFXPositioning()
    {
        Vector3 testPosition = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        PlayCorrectVFX(testPosition);
        
        Debug.Log("VFX positioning test completed. Check the scene for the effect.");
    }
    
    /// <summary>
    /// Debug method to test wrong VFX specifically
    /// </summary>
    [ContextMenu("Test Wrong VFX")]
    public void TestWrongVFX()
    {
        Vector3 testPosition = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        PlayWrongVFXEnhanced(testPosition);
        
        Debug.Log("Wrong VFX test completed. Check the scene for the effect.");
    }
    
    /// <summary>
    /// Debug method to test wrong VFX at specific UI position
    /// </summary>
    public void TestWrongVFXAtPosition(Vector3 uiPosition)
    {
        Vector3 worldPosition = UIToWorldPosition(uiPosition);
        PlayWrongVFXEnhanced(worldPosition, null, true);
        
        Debug.Log($"Wrong VFX test at UI position {uiPosition} -> World position {worldPosition}");
    }
    
    /// <summary>
    /// Validates canvas and camera settings for VFX visibility
    /// </summary>
    [ContextMenu("Validate Canvas Settings")]
    public void ValidateCanvasSettings()
    {
        Debug.Log("=== Canvas & Camera Validation ===");
        
        if (targetCanvas == null)
        {
            Debug.LogError("❌ Target Canvas is NULL!");
            return;
        }
        
        Debug.Log($"✅ Canvas: {targetCanvas.name}");
        Debug.Log($"✅ Render Mode: {targetCanvas.renderMode}");
        Debug.Log($"✅ Plane Distance: {targetCanvas.planeDistance}");
        Debug.Log($"✅ Sorting Layer ID: {targetCanvas.sortingLayerID}");
        Debug.Log($"✅ Sorting Order: {targetCanvas.sortingOrder}");
        
        if (uiCamera == null)
        {
            Debug.LogError("❌ UI Camera is NULL!");
            return;
        }
        
        Debug.Log($"✅ UI Camera: {uiCamera.name}");
        Debug.Log($"✅ Camera Type: {(uiCamera.orthographic ? "Orthographic" : "Perspective")}");
        Debug.Log($"✅ Near Clip: {uiCamera.nearClipPlane}");
        Debug.Log($"✅ Far Clip: {uiCamera.farClipPlane}");
        
        // Check if canvas is in Screen Space - Camera mode
        if (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (targetCanvas.worldCamera == null)
            {
                Debug.LogError("❌ Screen Space - Camera canvas has no world camera assigned!");
            }
            else
            {
                Debug.Log($"✅ World Camera: {targetCanvas.worldCamera.name}");
            }
        }
        
        // Check sorting layers
        var sortingLayers = SortingLayer.layers;
        Debug.Log($"✅ Available Sorting Layers: {sortingLayers.Length}");
        foreach (var layer in sortingLayers)
        {
            Debug.Log($"   - {layer.name} (ID: {layer.id})");
        }
    }
    
    /// <summary>
    /// Tests VFX at multiple positions to identify visibility issues
    /// </summary>
    [ContextMenu("Test VFX at Multiple Positions")]
    public void TestVFXAtMultiplePositions()
    {
        Debug.Log("Testing VFX at multiple positions...");
        
        // Test center of screen
        Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        TestWrongVFXAtPosition(centerPos);
        
        // Test top-left
        Vector3 topLeftPos = new Vector3(100f, Screen.height - 100f, 0f);
        TestWrongVFXAtPosition(topLeftPos);
        
        // Test bottom-right
        Vector3 bottomRightPos = new Vector3(Screen.width - 100f, 100f, 0f);
        TestWrongVFXAtPosition(bottomRightPos);
        
        Debug.Log("Multiple position test completed. Check console for results.");
    }
    
    /// <summary>
    /// Debug method to show current settings
    /// </summary>
    [ContextMenu("Show VFX Settings")]
    public void ShowVFXSettings()
    {
        Debug.Log($"=== VFX Manager Settings ===");
        Debug.Log($"UI Camera: {uiCamera?.name ?? "Not assigned"}");
        Debug.Log($"Target Canvas: {targetCanvas?.name ?? "Not assigned"}");
        Debug.Log($"Effect Scale: {effectScale}");
        Debug.Log($"Debug Logs: {enableDebugLogs}");
        Debug.Log($"Default Correct Effect: {defaultCorrectEffect?.name ?? "Not assigned"}");
        Debug.Log($"Default Wrong Effect: {defaultWrongEffect?.name ?? "Not assigned"}");
        Debug.Log($"Default Pickup Effect: {defaultPickupEffect?.name ?? "Not assigned"}");
    }

    /// <summary>
    /// Creates a properly configured wrong VFX effect with correct scaling and visibility
    /// </summary>
    public ParticleSystem CreateFixedWrongVFX(Vector3 position, Transform parent = null)
    {
        // Create a new GameObject for the effect
        GameObject effectObj = new GameObject("FixedWrongVFX");
        effectObj.transform.position = position;
        
        // Add ParticleSystem component
        ParticleSystem particleSystem = effectObj.AddComponent<ParticleSystem>();
        
        // Configure Main module
        var main = particleSystem.main;
        main.duration = 2f;
        main.loop = false;
        main.startLifetime = 1.5f;
        main.startSpeed = 5f;
        main.startSize = 0.3f; // Smaller, more appropriate size
        main.startColor = new Color(1f, 0.2f, 0.2f); // Bright red
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.maxParticles = 100;
        
        // Configure Emission module
        var emission = particleSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 50));
        emission.SetBurst(1, new ParticleSystem.Burst(0.2f, 30));
        
        // Configure Shape module
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        shape.arc = 360f;
        
        // Configure Color over lifetime
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.red, 0.0f), 
                new GradientColorKey(Color.orange, 0.5f),
                new GradientColorKey(Color.yellow, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(1.0f, 0.8f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        // Configure Size over lifetime
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.2f);
        curve.AddKey(0.3f, 1.0f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        
        // Configure Velocity over lifetime
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(4f);
        
        // Configure Renderer with maximum visibility
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.sortingOrder = 9999; // Maximum sorting order
        renderer.sortingLayerName = "UI";
        renderer.enabled = true;
        
        // Set parent if specified
        if (parent != null)
        {
            effectObj.transform.SetParent(parent);
        }
        
        // Ensure the effect is active
        effectObj.SetActive(true);
        
        // Play the effect
        particleSystem.Play();
        
        // Destroy after completion
        float destroyDelay = main.duration + 1f;
        Destroy(effectObj, destroyDelay);
        
        if (enableDebugLogs)
            Debug.Log($"Fixed Wrong VFX created at position: {position}");
        
        return particleSystem;
    }
}
