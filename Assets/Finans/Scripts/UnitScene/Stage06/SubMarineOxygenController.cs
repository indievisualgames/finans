using UnityEngine;
using System.Reflection;

namespace Game.Categories.NumberKeys
{
    /// <summary>
    /// Controls the SubMarine_oxygen GameObject activation and behaviour based on oxygen/gauge progress.
    /// - Activation factor (0-1) is calculated from correct answers/points
    /// - At 0% correct: GameObject visible but inactive (no movement/animation)
    /// - Submarine stays inactive until 50% correct answers
    /// - Gradually scales movement speed/behaviour from 50% to 90% correct answers
    /// - At/Above 90% correct: Fully functional (100% activation)
    /// </summary>
    public class SubMarineOxygenController : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private ScreenBoundedFloatingObject floatingObject;
        [SerializeField] private Animator animator;
        
        [Header("Movement Scaling")]
        [Tooltip("Base movement speed multiplier. This will be scaled by activation factor (0-1).")]
        [SerializeField] private float baseMovementSpeed = 1f;
        
        [Tooltip("Base lerp speed multiplier. This will be scaled by activation factor (0-1).")]
        [SerializeField] private float baseLerpSpeed = 1f;
        
        [Tooltip("Base horizontal travel speed. This will be scaled by activation factor (0-1).")]
        [SerializeField] private float baseHorizontalTravelSpeed = 1f;
        
        [Tooltip("Base vertical travel speed. This will be scaled by activation factor (0-1).")]
        [SerializeField] private float baseVerticalTravelSpeed = 1f;
        
        // Original values from ScreenBoundedFloatingObject (stored on Start)
        private float originalMovementSpeed = 2f; // Default from ScreenBoundedFloatingObject
        private float originalLerpSpeed = 2f; // Default from ScreenBoundedFloatingObject
        private float originalHorizontalTravelSpeed = 300f; // Default from ScreenBoundedFloatingObject
        private float originalVerticalTravelSpeed = 280f; // Default from ScreenBoundedFloatingObject
        private bool originalValuesStored = false;
        
        // Current activation factor (0 = inactive, 1 = fully active)
        private float currentActivationFactor = 0f;
        
        private void Awake()
        {
            // Auto-find components if not assigned
            if (floatingObject == null)
            {
                floatingObject = GetComponent<ScreenBoundedFloatingObject>();
            }
            
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            
            // Store original values (if floatingObject exists)
            // Note: ScreenBoundedFloatingObject initializes in Awake, so values should be available
            if (floatingObject != null && !originalValuesStored)
            {
                StoreOriginalValues();
            }
            
            // Initially disable functionality (but keep GameObject visible)
            SetActivation(0f);
        }
        
        private void Start()
        {
            // Backup: Try to store original values if not already stored
            // This ensures values are captured even if Awake didn't complete
            if (floatingObject != null && !originalValuesStored)
            {
                StoreOriginalValues();
            }
            
            // Ensure initial state is set (inactive but visible)
            SetActivation(0f);
        }
        
        private void StoreOriginalValues()
        {
            if (floatingObject == null || originalValuesStored) return;
            
            var type = floatingObject.GetType();
            
            // Store original movementSpeed
            var movementSpeedField = type.GetField("movementSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (movementSpeedField != null)
            {
                originalMovementSpeed = (float)movementSpeedField.GetValue(floatingObject);
            }
            
            // Store original lerpSpeed
            var lerpSpeedField = type.GetField("lerpSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (lerpSpeedField != null)
            {
                originalLerpSpeed = (float)lerpSpeedField.GetValue(floatingObject);
            }
            
            // Store original horizontalTravelSpeed
            var horizontalSpeedField = type.GetField("horizontalTravelSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (horizontalSpeedField != null)
            {
                originalHorizontalTravelSpeed = (float)horizontalSpeedField.GetValue(floatingObject);
            }
            
            // Store original verticalTravelSpeed
            var verticalSpeedField = type.GetField("verticalTravelSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (verticalSpeedField != null)
            {
                originalVerticalTravelSpeed = (float)verticalSpeedField.GetValue(floatingObject);
            }
            
            originalValuesStored = true;
        }
        
        /// <summary>
        /// Sets the activation factor for the submarine (0 = inactive, 1 = fully functional).
        /// Called by NumberKeysController based on oxygen gauge progress.
        /// GameObject remains visible but functionality is disabled when factor is 0.
        /// </summary>
        public void SetActivation(float factor)
        {
            factor = Mathf.Clamp01(factor);
            currentActivationFactor = factor;
            
            // Ensure GameObject is always active (visible)
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            
            if (factor <= 0f)
            {
                // Disable functionality components when factor is 0 or below
                // GameObject stays visible but inactive
                
                // Disable ScreenBoundedFloatingObject to stop movement
                if (floatingObject != null)
                {
                    floatingObject.enabled = false;
                }
                
                // Disable animator if it exists
                if (animator != null)
                {
                    animator.enabled = false;
                }
            }
            else
            {
                // Enable functionality components when factor is above 0
                
                // Enable ScreenBoundedFloatingObject
                if (floatingObject != null)
                {
                    floatingObject.enabled = true;
                }
                
                // Enable animator if it exists
                if (animator != null)
                {
                    animator.enabled = true;
                    // Set animator speed based on factor (0 = stopped, 1 = full speed)
                    animator.speed = factor;
                }
                
                // Scale movement parameters based on activation factor
                UpdateMovementSpeeds(factor);
            }
        }
        
        private void UpdateMovementSpeeds(float factor)
        {
            if (floatingObject == null) return;
            
            // Ensure original values are stored
            if (!originalValuesStored)
            {
                StoreOriginalValues();
            }
            
            var type = floatingObject.GetType();
            
            // Calculate scaled speeds from original values (not from current values)
            // Apply base multiplier and activation factor
            float scaledMovementSpeed = originalMovementSpeed * baseMovementSpeed * factor;
            float scaledLerpSpeed = originalLerpSpeed * baseLerpSpeed * factor;
            float scaledHorizontalSpeed = originalHorizontalTravelSpeed * baseHorizontalTravelSpeed * factor;
            float scaledVerticalSpeed = originalVerticalTravelSpeed * baseVerticalTravelSpeed * factor;
            
            // Apply scaled values via reflection
            var movementSpeedField = type.GetField("movementSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (movementSpeedField != null)
            {
                movementSpeedField.SetValue(floatingObject, scaledMovementSpeed);
            }
            
            var lerpSpeedField = type.GetField("lerpSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (lerpSpeedField != null)
            {
                lerpSpeedField.SetValue(floatingObject, scaledLerpSpeed);
            }
            
            var horizontalSpeedField = type.GetField("horizontalTravelSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (horizontalSpeedField != null)
            {
                horizontalSpeedField.SetValue(floatingObject, scaledHorizontalSpeed);
            }
            
            var verticalSpeedField = type.GetField("verticalTravelSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (verticalSpeedField != null)
            {
                verticalSpeedField.SetValue(floatingObject, scaledVerticalSpeed);
            }
        }
        
        /// <summary>
        /// Gets the current activation factor (0-1).
        /// </summary>
        public float GetActivationFactor()
        {
            return currentActivationFactor;
        }
        
        /// <summary>
        /// Resets the submarine to inactive state.
        /// Called when category starts or resets.
        /// </summary>
        public void Reset()
        {
            SetActivation(0f);
        }
    }
}

