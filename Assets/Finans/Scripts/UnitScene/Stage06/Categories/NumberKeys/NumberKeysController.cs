using UnityEngine;
using Game.Core;
using Game.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using TMPro;

namespace Game.Categories.NumberKeys
{
    public sealed class NumberKeysController : MonoBehaviour, ICategoryController
    {
        // Public Events (C# events for code-based subscriptions)
        public event Action OnCategoryStarted;
        public event Action<int, int> OnRoundStarted; // targetDigit, roundIndex
        public event Action<int> OnCorrectAnswer; // correct digit
        public event Action<int, int> OnWrongAnswer; // pressed digit, target digit
        public event Action<int, int> OnRoundCompleted; // roundIndex, correctCount
        public event Action<int, int> OnCategoryCompleted; // finalScore, totalRounds
        public event Action<int, int> OnProgressUpdated; // correctCount, totalRounds
        public event Action OnBubbleMissed; // bubble missed event

        // Unity Events (for Inspector-based event binding)
        [Header("Events")]
        [SerializeField] private UnityEvent onCategoryStarted;
        [SerializeField] private UnityEvent<int, int> onRoundStarted; // targetDigit, roundIndex
        [SerializeField] private UnityEvent<int> onCorrectAnswer; // correct digit
        [SerializeField] private UnityEvent<int, int> onWrongAnswer; // pressed digit, target digit
        [SerializeField] private UnityEvent<int, int> onRoundCompleted; // roundIndex, correctCount
        [SerializeField] private UnityEvent<int, int> onCategoryCompleted; // finalScore, totalRounds
        [SerializeField] private UnityEvent<int, int> onProgressUpdated; // correctCount, totalRounds
        [SerializeField] private UnityEvent onBubbleMissed;
        [SerializeField] private BubbleSpawner spawner;
        [SerializeField] private GuideCharacter guide;
        [SerializeField] private Transform shakeTarget; // e.g., Canvas root
                                                        //[SerializeField] private int totalRounds = 10;
        public int totalRounds = 10;
        [SerializeField] private GameObject endScreen; // assign an inactive scene instance

        [Header("Scoring")]
        [SerializeField] private int pointsPerMatch = 1;
        [SerializeField] private int pointsPerMiss = -1;
        [SerializeField] private bool allowNegativeScore = true;

        [Header("Spawner Settings")]
        [Tooltip("Speed at which bubbles rise (units per second). Higher values = faster bubbles.")]
        [SerializeField] private float bubbleRiseSpeed = 0.5f;

        [Header("HUD")]
        [SerializeField] private RectTransform[] oxygenNeedles;
        [SerializeField] private float oxygenNeedleMinRotation = 0f;
        [SerializeField] private float oxygenNeedleMaxRotation = -245f;
        [SerializeField] private SubMarineOxygenController submarineOxygenController;
        [Header("Scriptable")]
        [SerializeField] private GameObject scriptableObjectContainer;

        private ICalculatorInputSource inputSource;
        private int roundIndex;
        private int targetDigit;
        private int correctCount;
        private int currentScore = 0;
        private NumberBubble activeTargetBubble;
        private bool isCategoryActive;
        private CalculatorFundamentals calculatorFundamentals;
        // Public Properties (read-only access to current state)
        public int CurrentRound => roundIndex;
        public int TargetDigit => targetDigit;
        public int CorrectCount => correctCount;
        public int TotalRounds => totalRounds;
        public int CurrentScore => currentScore;
        public float Progress => totalRounds > 0 ? (float)correctCount / totalRounds : 0f;
        public bool IsCategoryComplete => roundIndex >= totalRounds;
        public NumberBubble ActiveTargetBubble => activeTargetBubble;

        // Public setters for runtime control
        public float BubbleRiseSpeed
        {
            get => bubbleRiseSpeed;
            set => bubbleRiseSpeed = Mathf.Max(0f, value);
        }

        private void Awake()
        {
            // Auto-find SubMarineOxygenController if not assigned
            if (submarineOxygenController == null)
            {
                submarineOxygenController = FindObjectOfType<SubMarineOxygenController>();
            }
        }
        void Start()
        {
            calculatorFundamentals = scriptableObjectContainer.GetComponent<CalculatorFundamentals>();
            Logger.LogInfo(" NumberKeysController started..found calculatorFundamentals", "NumberKeysController");
        }
        public void Initialize(ICalculatorInputSource input)
        {
            inputSource = input;
            inputSource.OnKey += HandleKey;
        }

        public void StartCategory()
        {
            isCategoryActive = true;
            roundIndex = 0;
            correctCount = 0;
            currentScore = 0;
            activeTargetBubble = null;

            // Reset submarine to inactive state
            if (submarineOxygenController != null)
            {
                submarineOxygenController.Reset();
            }

            UpdateOxygenNeedle();

            // Trigger category started events
            OnCategoryStarted?.Invoke();
            onCategoryStarted?.Invoke();

            NextRound();
        }

        private void OnDestroy()
        {
            isCategoryActive = false;
            if (inputSource != null)
            {
                inputSource.OnKey -= HandleKey;
            }

            // Clean up active bubble references
            if (activeTargetBubble != null)
            {
                activeTargetBubble.OnMissed -= HandleBubbleMissed;
                activeTargetBubble.OnMatched -= HandleBubbleMatched;
            }
        }

        private void OnDisable()
        {
            // Stop processing input when disabled
            isCategoryActive = false;
        }

        private void NextRound()
        {
            if (roundIndex >= totalRounds)
            {
                OnCategoryComplete();
                return;
            }

            targetDigit = UnityEngine.Random.Range(0, 10);

            if (spawner == null)
            {
                Debug.LogError("NumberKeysController: spawner is not assigned! Cannot spawn bubbles.", this);
            }
            else
            {
                spawner.SpawnRound(targetDigit, roundIndex, bubbleRiseSpeed, this);
            }

            guide?.ShowPrompt($"Press {targetDigit}");

            // Trigger round started events
            OnRoundStarted?.Invoke(targetDigit, roundIndex);
            onRoundStarted?.Invoke(targetDigit, roundIndex);

            roundIndex++;
        }

        public void RegisterActiveTargetBubble(NumberBubble bubble)
        {
            // Clear previous active bubble if any
            if (activeTargetBubble != null && activeTargetBubble != bubble)
            {
                activeTargetBubble.SetAsActiveTarget(false);
            }

            activeTargetBubble = bubble;
            if (bubble != null)
            {
                bubble.SetAsActiveTarget(true, null); // No button reference needed
                bubble.OnMissed += HandleBubbleMissed;
                bubble.OnMatched += HandleBubbleMatched;
            }
        }

        private void HandleBubbleMissed(NumberBubble bubble)
        {
            if (bubble != activeTargetBubble) return;

            activeTargetBubble = null;

            // Decrement score
            currentScore = allowNegativeScore ? currentScore + pointsPerMiss : Mathf.Max(0, currentScore + pointsPerMiss);
            correctCount = Mathf.Max(0, correctCount - 1);

            // Play miss sound - immediate first, then delayed
            AudioService.Instance?.PlayWrongImmediate();
            AudioService.Instance?.PlayWrong();

            // Show miss feedback
            guide?.ShowError($"Missed! Look for {targetDigit}.");

            // Trigger miss event
            OnBubbleMissed?.Invoke();
            onBubbleMissed?.Invoke();

            UpdateOxygenNeedle();

            // Continue to next round
            NextRound();
        }

        private void HandleBubbleMatched(NumberBubble bubble)
        {
            if (bubble != activeTargetBubble) return;

            activeTargetBubble = null;
        }

        private void HandleKey(string key)
        {
            // Only process input if category is active, component is enabled, and GameObject is active
            if (!isCategoryActive || !enabled || !gameObject.activeInHierarchy || string.IsNullOrEmpty(key)) return;
            if (!int.TryParse(key, out var pressed))
            {
                // Play wrong sound - immediate first, then delayed
                AudioService.Instance?.PlayWrongImmediate();
                AudioService.Instance?.PlayWrong();
                VFXService.Instance?.ShakeOnWrong(shakeTarget != null ? shakeTarget : transform);
                guide?.ShowError("Try a number!");
                return;
            }

            // Check if there's an active target bubble and if the pressed number matches
            if (activeTargetBubble != null && !activeTargetBubble.HasBeenMatched && !activeTargetBubble.HasBeenMissed)
            {
                if (pressed == targetDigit && pressed == activeTargetBubble.digit)
                {
                    // Match! Trigger bubble's match handler
                    activeTargetBubble.OnMatch();

                    // Award points
                    currentScore += pointsPerMatch;
                    correctCount = Mathf.Min(correctCount + 1, totalRounds);

                    // Play correct sound - immediate first (when VFX triggers), then delayed
                    AudioService.Instance?.PlayCorrectImmediate();
                    AudioService.Instance?.PlayCorrect();

                    // VFX is handled by the bubble's Pop() method - no need to spawn here

                    guide?.ShowSuccess("Great job!");
                    UpdateOxygenNeedle();

                    // Trigger correct answer events
                    OnCorrectAnswer?.Invoke(targetDigit);
                    onCorrectAnswer?.Invoke(targetDigit);

                    // Trigger progress updated events
                    OnProgressUpdated?.Invoke(correctCount, totalRounds);
                    onProgressUpdated?.Invoke(correctCount, totalRounds);

                    // Trigger round completed events
                    OnRoundCompleted?.Invoke(roundIndex - 1, correctCount);
                    onRoundCompleted?.Invoke(roundIndex - 1, correctCount);

                    // Continue to next round (bubble's OnMatch will handle cleanup)
                    NextRound();
                    return;
                }
                else if (pressed == targetDigit)
                {
                    // Correct number but bubble might have been missed already
                    // Still treat as correct if bubble exists
                    spawner?.PopAllWithDigit(targetDigit);
                    // Play correct sound - immediate first (when VFX triggers), then delayed
                    AudioService.Instance?.PlayCorrectImmediate();
                    AudioService.Instance?.PlayCorrect();

                    // VFX is handled by the bubble's Pop() method - no need to spawn here

                    guide?.ShowSuccess("Great job!");
                    currentScore += pointsPerMatch;
                    correctCount = Mathf.Min(correctCount + 1, totalRounds);
                    UpdateOxygenNeedle();

                    OnCorrectAnswer?.Invoke(targetDigit);
                    onCorrectAnswer?.Invoke(targetDigit);
                    OnProgressUpdated?.Invoke(correctCount, totalRounds);
                    onProgressUpdated?.Invoke(correctCount, totalRounds);
                    OnRoundCompleted?.Invoke(roundIndex - 1, correctCount);
                    onRoundCompleted?.Invoke(roundIndex - 1, correctCount);

                    NextRound();
                    return;
                }
            }
            else if (pressed == targetDigit)
            {
                // No active bubble but correct number pressed (fallback to old behavior)
                spawner?.PopAllWithDigit(targetDigit);
                // Play correct sound - immediate first (when VFX triggers), then delayed
                AudioService.Instance?.PlayCorrectImmediate();
                AudioService.Instance?.PlayCorrect();

                // VFX is handled by the bubble's Pop() method - no need to spawn here

                guide?.ShowSuccess("Great job!");
                currentScore += pointsPerMatch;
                correctCount = Mathf.Min(correctCount + 1, totalRounds);
                UpdateOxygenNeedle();

                OnCorrectAnswer?.Invoke(targetDigit);
                onCorrectAnswer?.Invoke(targetDigit);
                OnProgressUpdated?.Invoke(correctCount, totalRounds);
                onProgressUpdated?.Invoke(correctCount, totalRounds);
                OnRoundCompleted?.Invoke(roundIndex - 1, correctCount);
                onRoundCompleted?.Invoke(roundIndex - 1, correctCount);

                NextRound();
                return;
            }

            // Wrong number pressed
            // Play wrong sound - immediate first (when VFX triggers), then delayed
            AudioService.Instance?.PlayWrongImmediate();
            AudioService.Instance?.PlayWrong();
            var wrongAnchor = shakeTarget != null ? shakeTarget : transform;
            VFXService.Instance?.ShakeOnWrong(wrongAnchor);
            VFXService.Instance?.SpawnWrongBurst(wrongAnchor.position);
            guide?.ShowError($"Not {pressed}. Look for {targetDigit}.");

            // Trigger wrong answer events
            OnWrongAnswer?.Invoke(pressed, targetDigit);
            onWrongAnswer?.Invoke(pressed, targetDigit);
        }

        private void OnCategoryComplete()
        {
            isCategoryActive = false;
            correctCount = totalRounds;
            UpdateOxygenNeedle();
            guide?.ShowSuccess("You did it!");
            // Audio & VFX for completion
            AudioService.Instance?.PlayComplete();
            var vfxPos = transform.position;
            VFXService.Instance?.SpawnCorrectBurst(vfxPos);
            // Also trigger UI-based effect on the speech bubble if assigned in scene
            guide?.PlayCompletionBubbleEffect();
            if (endScreen != null)
            {
                // If a scene instance is assigned, just enable it; if a prefab asset is assigned, instantiate it
                if (endScreen.scene.IsValid())
                {
                    endScreen.SetActive(true);
                    Logger.LogInfo(" Level 1 completed on endScreen..", "NumberKeysController");
                    // calculatorFundamentals.MarkGamePlayed();
                }
                else
                {
                    var parent = shakeTarget != null ? shakeTarget : transform;
                    var instance = Instantiate(endScreen, parent, false);
                    instance.SetActive(true);
                    Logger.LogInfo(" Level 1 completed on instance..", "NumberKeysController");
                    calculatorFundamentals.MarkGamePlayed();
                }
            }
            ProgressService.SetBestScore("numberkeys", totalRounds);
            var last = ProgressService.GetLastUnlocked();
            if (last < 1) ProgressService.SetLastUnlocked(1); // unlock next category index

            // Trigger category completed events
            OnCategoryCompleted?.Invoke(correctCount, totalRounds);
            //onCategoryCompleted?.Invoke(correctCount, totalRounds);
        }

        private void UpdateOxygenNeedle()
        {
            if (oxygenNeedles == null || oxygenNeedles.Length == 0) return;
            int maxScore = Mathf.Max(1, totalRounds);
            // Treat 90% correct as the "max" of the gauge.
            // Example: 90% => needle at max rotation, 100% still max rotation (clamped).
            const float gaugeMaxAtProgress = 0.90f;
            float progress = Mathf.Clamp01((float)correctCount / maxScore);
            float t = Mathf.Clamp01(progress / gaugeMaxAtProgress);
            float rotation = Mathf.Lerp(oxygenNeedleMinRotation, oxygenNeedleMaxRotation, t);

            foreach (var needle in oxygenNeedles)
            {
                if (needle != null)
                {
                    needle.localRotation = Quaternion.Euler(0f, 0f, rotation);
                }
            }

            // Update submarine activation based on correct answers
            // Submarine stays inactive until 50% correct, then ramps to full by 90% correct.
            if (submarineOxygenController != null)
            {
                const float submarineStartProgress = 0.50f;
                const float submarineFullProgress = 0.90f;

                // 0 until 50%, then 0..1 between 50%..90%, clamped above 90%.
                float activationFactor = progress < submarineStartProgress
                    ? 0f
                    : Mathf.InverseLerp(submarineStartProgress, submarineFullProgress, progress);
                submarineOxygenController.SetActivation(activationFactor);
            }
        }
    }
}


