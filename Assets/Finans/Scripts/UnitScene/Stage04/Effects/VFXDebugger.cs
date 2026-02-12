using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Debug tool for testing VFX effects
/// </summary>
public class VFXDebugger : MonoBehaviour
{
    [Header("Test Settings")]
    public bool enableDebugMode = true;
    public KeyCode testCorrectVFXKey = KeyCode.C;
    public KeyCode testWrongVFXKey = KeyCode.W;
    public KeyCode testPickupVFXKey = KeyCode.P;
    
    [Header("Test Position")]
    public Vector3 testPosition = Vector3.zero;
    public bool useMousePosition = true;
    
    void Update()
    {
        if (!enableDebugMode) return;
        
        // Test correct VFX
        if (Input.GetKeyDown(testCorrectVFXKey))
        {
            TestCorrectVFX();
        }
        
        // Test wrong VFX
        if (Input.GetKeyDown(testWrongVFXKey))
        {
            TestWrongVFX();
        }
        
        // Test pickup VFX
        if (Input.GetKeyDown(testPickupVFXKey))
        {
            TestPickupVFX();
        }
    }
    
    void TestCorrectVFX()
    {
        Vector3 position = useMousePosition ? Input.mousePosition : testPosition;
        
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayCorrectVFX(position, null, !useMousePosition);
            Debug.Log($"Test: Correct VFX triggered at {position}");
        }
        else
        {
            ParticleEffectManager.PlayCorrectVFXAtPosition(position, null);
            Debug.Log($"Test: Correct VFX triggered at {position} (direct)");
        }
    }
    
    void TestWrongVFX()
    {
        Vector3 position = useMousePosition ? Input.mousePosition : testPosition;
        
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayWrongVFX(position, null, !useMousePosition);
            Debug.Log($"Test: Wrong VFX triggered at {position}");
        }
        else
        {
            ParticleEffectManager.PlayWrongVFXAtPosition(position, null);
            Debug.Log($"Test: Wrong VFX triggered at {position} (direct)");
        }
    }
    
    void TestPickupVFX()
    {
        Vector3 position = useMousePosition ? Input.mousePosition : testPosition;
        
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayPickupVFX(position, null, !useMousePosition);
            Debug.Log($"Test: Pickup VFX triggered at {position}");
        }
        else
        {
            var effect = ParticleEffectManager.CreatePickupEffect();
            effect.transform.position = position;
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration + 1f);
            Debug.Log($"Test: Pickup VFX triggered at {position} (direct)");
        }
    }
    
    [ContextMenu("Test Correct VFX")]
    public void TestCorrectVFXContext()
    {
        TestCorrectVFX();
    }
    
    [ContextMenu("Test Wrong VFX")]
    public void TestWrongVFXContext()
    {
        TestWrongVFX();
    }
    
    [ContextMenu("Test Pickup VFX")]
    public void TestPickupVFXContext()
    {
        TestPickupVFX();
    }
    
    [ContextMenu("Test All VFX")]
    public void TestAllVFX()
    {
        TestCorrectVFX();
        TestWrongVFX();
        TestPickupVFX();
    }
    
    void OnGUI()
    {
        if (!enableDebugMode) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("VFX Debugger", GUI.skin.box);
        GUILayout.Label($"Press {testCorrectVFXKey} to test Correct VFX");
        GUILayout.Label($"Press {testWrongVFXKey} to test Wrong VFX");
        GUILayout.Label($"Press {testPickupVFXKey} to test Pickup VFX");
        GUILayout.Label($"VFXManager: {(VFXManager.Instance != null ? "Found" : "Missing")}");
        GUILayout.EndArea();
    }
}
