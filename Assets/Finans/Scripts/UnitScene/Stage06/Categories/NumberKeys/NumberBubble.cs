using UnityEngine;
using Game.Core;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace Game.Categories.NumberKeys
{
    public sealed class NumberBubble : MonoBehaviour
    {
        [Range(0, 9)] public int digit;
        [Header("Movement Settings")]
        [SerializeField] private float riseSpeed = 0.5f;
        [SerializeField] private Vector2 bobAmplitude = new Vector2(0.05f, 0.02f);
        [SerializeField] private float bobFrequency = 2f;
        [Header("Randomization")]
        [Tooltip("Enable randomization for movement values to make each bubble unique")]
        [SerializeField] private bool enableRandomization = true;
        [Tooltip("Variation range for rise speed (0.5 = ±50% variation)")]
        [SerializeField][Range(0f, 1f)] private float riseSpeedVariation = 0.3f;
        [Tooltip("Variation range for bob amplitude (0.5 = ±50% variation)")]
        [SerializeField][Range(0f, 1f)] private float bobAmplitudeVariation = 0.4f;
        [Tooltip("Variation range for bob frequency (0.5 = ±50% variation)")]
        [SerializeField][Range(0f, 1f)] private float bobFrequencyVariation = 0.3f;
        [Header("Screen Bounds")]
        [SerializeField] private float screenBoundaryMargin = 1f; // Margin beyond screen to consider "off-screen"
        [SerializeField] private bool checkScreenBounds = true;
        [Header("Highlighting")]
        [SerializeField] private Color highlightColor = new Color(1f, 0.9f, 0.2f, 1f); // Yellow highlight (kept for backward compatibility)
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private float highlightScale = 1.2f; // Scale up when highlighted
        [Header("Animated Highlight Colors")]
        [SerializeField] private Color highlightColor1 = new Color(1f, 0.9f, 0.2f, 1f); // Yellow
        [SerializeField] private Color highlightColor2 = new Color(1f, 0.4f, 0.2f, 1f); // Orange
        [SerializeField] private Color highlightColor3 = new Color(0.2f, 0.9f, 1f, 1f); // Cyan
        [SerializeField] private float animationSpeed = 2f; // Speed of color transition (cycles per second)
        [SerializeField] private bool useAnimatedHighlight = true; // Toggle animated highlighting

        [Header("Color Presets")]
        [Tooltip("Quick preset options for common color schemes")]
        [SerializeField] private ColorPreset colorPreset = ColorPreset.Custom;

        public enum ColorPreset
        {
            Custom,
            YellowOrangeCyan,
            RedPinkPurple,
            GreenBlueTeal,
            WarmColors,
            CoolColors,
            Rainbow
        }

        private Vector3 basePos;
        private Camera mainCamera;
        private bool isActiveTarget = false;
        private bool hasBeenMatched = false;
        private bool hasBeenMissed = false;
        private Image bubbleImage;
        private SpriteRenderer spriteRenderer;
        private Vector3 originalScale;
        private Color originalImageColor;
        private Coroutine highlightAnimationCoroutine;
        private bool pendingStartHighlightAnimation;
        private TMP_Text bubbleText;
        private Color originalTextColor;

        // Randomized movement values (set per instance)
        private float randomizedRiseSpeed;
        private Vector2 randomizedBobAmplitude;
        private float randomizedBobFrequency;

        // Callbacks
        public Action<NumberBubble> OnMissed;
        public Action<NumberBubble> OnMatched;

        // Target button reference (not used for highlighting, kept for compatibility)
        public UnityEngine.UI.Button targetButton;

        public float RiseSpeed => randomizedRiseSpeed;
        public bool IsActiveTarget => isActiveTarget;
        public bool HasBeenMatched => hasBeenMatched;
        public bool HasBeenMissed => hasBeenMissed;

        // Public properties for colors (read-only)
        public Color HighlightColor1 => highlightColor1;
        public Color HighlightColor2 => highlightColor2;
        public Color HighlightColor3 => highlightColor3;

        public void SetRiseSpeed(float speed)
        {
            riseSpeed = Mathf.Max(0f, speed);
            randomizedRiseSpeed = riseSpeed;
        }

        /// <summary>
        /// Set all three highlight colors at once
        /// </summary>
        public void SetHighlightColors(Color color1, Color color2, Color color3)
        {
            highlightColor1 = color1;
            highlightColor2 = color2;
            highlightColor3 = color3;

            // Restart animation if currently active
            if (isActiveTarget && useAnimatedHighlight && !hasBeenMatched && !hasBeenMissed)
                StartHighlightAnimationIfPossible();
        }

        /// <summary>
        /// Set individual highlight color by index (0, 1, or 2)
        /// </summary>
        public void SetHighlightColor(int index, Color color)
        {
            switch (index)
            {
                case 0:
                    highlightColor1 = color;
                    break;
                case 1:
                    highlightColor2 = color;
                    break;
                case 2:
                    highlightColor3 = color;
                    break;
                default:
                    Debug.LogWarning($"Invalid color index: {index}. Use 0, 1, or 2.");
                    return;
            }

            // Restart animation if currently active
            if (isActiveTarget && useAnimatedHighlight && !hasBeenMatched && !hasBeenMissed)
                StartHighlightAnimationIfPossible();
        }

        /// <summary>
        /// Apply a color preset
        /// </summary>
        public void ApplyColorPreset(ColorPreset preset)
        {
            colorPreset = preset;
            switch (preset)
            {
                case ColorPreset.YellowOrangeCyan:
                    SetHighlightColors(
                        new Color(1f, 0.9f, 0.2f, 1f),  // Yellow
                        new Color(1f, 0.4f, 0.2f, 1f),  // Orange
                        new Color(0.2f, 0.9f, 1f, 1f)   // Cyan
                    );
                    break;

                case ColorPreset.RedPinkPurple:
                    SetHighlightColors(
                        new Color(1f, 0.2f, 0.2f, 1f),  // Red
                        new Color(1f, 0.2f, 0.8f, 1f),  // Pink
                        new Color(0.6f, 0.2f, 1f, 1f)   // Purple
                    );
                    break;

                case ColorPreset.GreenBlueTeal:
                    SetHighlightColors(
                        new Color(0.2f, 1f, 0.4f, 1f),  // Green
                        new Color(0.2f, 0.6f, 1f, 1f),  // Blue
                        new Color(0.2f, 0.9f, 0.8f, 1f) // Teal
                    );
                    break;

                case ColorPreset.WarmColors:
                    SetHighlightColors(
                        new Color(1f, 0.9f, 0.2f, 1f),  // Yellow
                        new Color(1f, 0.5f, 0.2f, 1f),  // Orange
                        new Color(1f, 0.3f, 0.2f, 1f)   // Red-Orange
                    );
                    break;

                case ColorPreset.CoolColors:
                    SetHighlightColors(
                        new Color(0.2f, 0.9f, 1f, 1f),  // Cyan
                        new Color(0.2f, 0.6f, 1f, 1f),  // Blue
                        new Color(0.4f, 0.2f, 1f, 1f)   // Purple
                    );
                    break;

                case ColorPreset.Rainbow:
                    SetHighlightColors(
                        new Color(1f, 0.2f, 0.2f, 1f),  // Red
                        new Color(0.2f, 1f, 0.2f, 1f),  // Green
                        new Color(0.2f, 0.2f, 1f, 1f)   // Blue
                    );
                    break;

                case ColorPreset.Custom:
                    // Keep current colors
                    break;
            }
        }

        public void SetAsActiveTarget(bool active, UnityEngine.UI.Button button = null)
        {
            isActiveTarget = active;
            targetButton = button;

            // Stop any existing animation
            if (highlightAnimationCoroutine != null)
            {
                StopCoroutine(highlightAnimationCoroutine);
                highlightAnimationCoroutine = null;
            }
            pendingStartHighlightAnimation = false;

            UpdateHighlight();

            // Start animation if becoming active target
            if (active && useAnimatedHighlight && !hasBeenMatched && !hasBeenMissed)
            {
                StartHighlightAnimationIfPossible();
            }
        }

        private void OnEnable()
        {
            if (pendingStartHighlightAnimation)
            {
                StartHighlightAnimationIfPossible();
            }
        }

        private void OnDisable()
        {
            // Coroutines cannot run on inactive objects; ensure clean state.
            if (highlightAnimationCoroutine != null)
            {
                StopCoroutine(highlightAnimationCoroutine);
                highlightAnimationCoroutine = null;
            }
        }

        private void Awake()
        {
            basePos = transform.position;
            originalScale = transform.localScale;
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }

            // Randomize movement values for this instance
            RandomizeMovementValues();

            // Find bubble background image component
            bubbleImage = GetComponentInChildren<Image>();
            if (bubbleImage != null)
            {
                originalImageColor = bubbleImage.color;
            }
            else
            {
                // Try to find SpriteRenderer if it's a 3D object
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    originalImageColor = spriteRenderer.color;
                }
            }

            // Find text component
            bubbleText = GetComponentInChildren<TMP_Text>();
            if (bubbleText != null)
            {
                originalTextColor = bubbleText.color;
            }

            // Apply color preset if not custom
            if (colorPreset != ColorPreset.Custom)
            {
                ApplyColorPreset(colorPreset);
            }
        }

        private void StartHighlightAnimationIfPossible()
        {
            if (!isActiveTarget || !useAnimatedHighlight || hasBeenMatched || hasBeenMissed)
                return;

            // If this object (or any parent) is inactive, Unity will throw if we call StartCoroutine.
            // Defer until we're active in hierarchy again.
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                pendingStartHighlightAnimation = true;
                return;
            }

            pendingStartHighlightAnimation = false;

            if (highlightAnimationCoroutine != null)
            {
                StopCoroutine(highlightAnimationCoroutine);
                highlightAnimationCoroutine = null;
            }

            highlightAnimationCoroutine = StartCoroutine(AnimateHighlightColors());
        }

        private void Start()
        {
            // Update text to display the digit value
            UpdateDigitText();
        }

        /// <summary>
        /// Updates the text component to display the current digit value
        /// </summary>
        public void UpdateDigitText()
        {
            if (bubbleText != null)
            {
                bubbleText.text = digit.ToString();
            }
        }

        private void RandomizeMovementValues()
        {
            if (enableRandomization)
            {
                // Randomize rise speed
                float riseSpeedMin = riseSpeed * (1f - riseSpeedVariation);
                float riseSpeedMax = riseSpeed * (1f + riseSpeedVariation);
                randomizedRiseSpeed = UnityEngine.Random.Range(riseSpeedMin, riseSpeedMax);

                // Randomize bob amplitude
                float ampXMin = bobAmplitude.x * (1f - bobAmplitudeVariation);
                float ampXMax = bobAmplitude.x * (1f + bobAmplitudeVariation);
                float ampYMin = bobAmplitude.y * (1f - bobAmplitudeVariation);
                float ampYMax = bobAmplitude.y * (1f + bobAmplitudeVariation);
                randomizedBobAmplitude = new Vector2(
                    UnityEngine.Random.Range(ampXMin, ampXMax),
                    UnityEngine.Random.Range(ampYMin, ampYMax)
                );

                // Randomize bob frequency
                float freqMin = bobFrequency * (1f - bobFrequencyVariation);
                float freqMax = bobFrequency * (1f + bobFrequencyVariation);
                randomizedBobFrequency = UnityEngine.Random.Range(freqMin, freqMax);
            }
            else
            {
                // Use base values without randomization
                randomizedRiseSpeed = riseSpeed;
                randomizedBobAmplitude = bobAmplitude;
                randomizedBobFrequency = bobFrequency;
            }
        }

        // Called when values change in the Inspector
        private void OnValidate()
        {
            // Apply preset when changed in Inspector
            if (colorPreset != ColorPreset.Custom && Application.isPlaying)
            {
                ApplyColorPreset(colorPreset);
            }
        }

        private void UpdateHighlight()
        {
            if (isActiveTarget && !hasBeenMatched && !hasBeenMissed)
            {
                // If not using animated highlight, use static color
                if (!useAnimatedHighlight)
                {
                    SetColor(highlightColor);
                    UpdateTextColor(highlightColor);
                }
                // Otherwise, color will be set by animation coroutine

                // Scale up slightly
                transform.localScale = originalScale * highlightScale;
            }
            else
            {
                // Remove highlight
                SetColor(originalImageColor);
                RestoreTextColor();

                // Restore original scale
                transform.localScale = originalScale;
            }
        }

        private void SetColor(Color color)
        {
            if (bubbleImage != null)
            {
                bubbleImage.color = color;
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        private void UpdateTextColor(Color bubbleColor)
        {
            if (bubbleText != null && isActiveTarget)
            {
                // Invert the text color based on the bubble color
                Color invertedColor = new Color(1f - bubbleColor.r, 1f - bubbleColor.g, 1f - bubbleColor.b, bubbleText.color.a);
                bubbleText.color = invertedColor;
            }
        }

        private void RestoreTextColor()
        {
            if (bubbleText != null)
            {
                bubbleText.color = originalTextColor;
            }
        }

        private IEnumerator AnimateHighlightColors()
        {
            Color[] colors = { highlightColor1, highlightColor2, highlightColor3 };
            int currentColorIndex = 0;
            float cycleDuration = 1f / animationSpeed; // Time for one complete cycle through all 3 colors
            float transitionDuration = cycleDuration / 3f; // Time to transition between each color

            while (isActiveTarget && !hasBeenMatched && !hasBeenMissed)
            {
                int nextColorIndex = (currentColorIndex + 1) % colors.Length;
                Color startColor = colors[currentColorIndex];
                Color endColor = colors[nextColorIndex];

                float elapsed = 0f;
                while (elapsed < transitionDuration && isActiveTarget && !hasBeenMatched && !hasBeenMissed)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / transitionDuration;
                    // Use smooth step for smoother transitions
                    t = t * t * (3f - 2f * t);
                    Color currentColor = Color.Lerp(startColor, endColor, t);
                    SetColor(currentColor);
                    UpdateTextColor(currentColor);
                    yield return null;
                }

                // Ensure we end on the target color
                SetColor(endColor);
                UpdateTextColor(endColor);
                currentColorIndex = nextColorIndex;
            }
        }

        private void Update()
        {
            var t = Time.time * randomizedBobFrequency;
            var bob = new Vector3(Mathf.Sin(t) * randomizedBobAmplitude.x, Mathf.Cos(t) * randomizedBobAmplitude.y, 0f);
            transform.position += Vector3.up * randomizedRiseSpeed * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z) + bob * Time.deltaTime;

            // Check if bubble has left screen bounds (only for active target bubbles)
            if (checkScreenBounds && isActiveTarget && !hasBeenMatched && !hasBeenMissed)
            {
                CheckScreenBounds();
            }
        }

        private void CheckScreenBounds()
        {
            if (mainCamera == null) return;

            // Convert world position to viewport coordinates
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

            // Check if bubble is outside viewport (with margin)
            bool isOffScreen = viewportPos.y > 1f + screenBoundaryMargin ||
                              viewportPos.y < -screenBoundaryMargin ||
                              viewportPos.x > 1f + screenBoundaryMargin ||
                              viewportPos.x < -screenBoundaryMargin ||
                              viewportPos.z < mainCamera.nearClipPlane;

            if (isOffScreen)
            {
                OnMiss();
            }
        }

        public void OnMiss()
        {
            if (hasBeenMatched || hasBeenMissed) return;

            hasBeenMissed = true;
            isActiveTarget = false;
            UpdateHighlight(); // Remove highlight before destroying

            // Trigger miss callback
            OnMissed?.Invoke(this);

            // Play miss VFX (subtle fade-out effect)
            if (VFXService.Instance != null)
            {
                // Use wrong burst for miss visual feedback
                VFXService.Instance.SpawnWrongBurst(transform.position);
            }

            // Destroy the bubble
            Destroy(gameObject);
        }

        public void OnMatch()
        {
            if (hasBeenMatched || hasBeenMissed) return;

            hasBeenMatched = true;
            isActiveTarget = false;
            UpdateHighlight(); // Remove highlight before popping

            // Trigger match callback
            OnMatched?.Invoke(this);

            // Pop the bubble (existing behavior)
            Pop();
        }

        public void Pop()
        {
            VFXService.Instance?.SpawnCorrectBurst(transform.position);
            Destroy(gameObject);
        }
    }
}


