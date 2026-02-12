using System;
using System.Collections;
using System.Collections.Generic;
using Game.Core;
using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.Categories.OperatorMatch
{
    public enum OperatorSign
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    /// <summary>
    /// Basic operator matching gameplay:
    /// - Show a single operator sign (+, -, ×, ÷) in a simple popup
    /// - Player must press the matching operator key on the calculator
    /// - Correct → reward + next round, Wrong → feedback, stay on same round
    /// </summary>
    public sealed class OperatorMatchLevelController : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private int totalRounds = 10;
        [SerializeField] private float nextRoundDelaySeconds = 0.2f;
        [Tooltip("Extra pause after correct feedback finishes (hand animation + any special toggles) before starting next round.")]
        [SerializeField] private float postCorrectPauseSeconds = 1f;
        [SerializeField] private string progressCategoryKey = "operators";

        [Header("UI (Optional)")]
        [Tooltip("Displays total rounds. Example: \"Total : 10\"")]
        public TMP_Text totalRoundsText;

        [Tooltip("Displays remaining rounds. Example: \"Remaining : 7\"")]
        public TMP_Text remainingRoundsText;

        [Tooltip("Legacy combined field (optional). If set, shows \"Round X/Total  |  Remaining Y\".")]
        public TMP_Text roundsCounterText;

        [Header("Allowed Operators")]
        [SerializeField] private bool allowAdd = true;
        [SerializeField] private bool allowSubtract = true;
        [SerializeField] private bool allowMultiply = true;
        [SerializeField] private bool allowDivide = true;

        [Header("Feedback (Optional)")]
        [SerializeField] private GuideCharacter guide;
        [Tooltip("If set, wrong feedback will shake this transform; otherwise it shakes this GameObject.")]
        [SerializeField] private Transform shakeTarget;
        [Tooltip("World-space anchor used to spawn VFX for correct/complete. Recommended when popup is a UI Canvas.")]
        [SerializeField] private Transform vfxAnchor;

        [Header("Correct Feedback: Hand (Optional)")]
        [SerializeField] private bool showHandBaseOnCorrect = true;
        [SerializeField] private GameObject handBase;
        [Tooltip("Animator state to play on correct (uses layer 0). If missing, plays default state.")]
        [SerializeField] private string handBaseStateName = "Arm";
        [Tooltip("Seconds to pause after hand animation completes before disabling Hand_base.")]
        [SerializeField] private float handBasePostAnimationPauseSeconds = 2f;
        [Tooltip("Optional UI panel to hide while the hand animation is playing.")]
        [SerializeField] private GameObject msgPanel;
        [Tooltip("Only for '+' round: after correct + and after hand animation, swap Liquid_half → Liquid_fill and hold before next round.")]
        [SerializeField] private float addCorrectLiquidHoldSeconds = 1f;
        [Tooltip("Only for '−' round: after correct − and after hand animation, swap Liquid_fill → Liquid_half and hold before next round.")]
        [SerializeField] private float subtractCorrectLiquidHoldSeconds = 1f;

        [Header("Calculator (Optional)")]
        [Tooltip("Calculator UI controller to clear display at each round start.")]
        [SerializeField] private CalculatorController calculator;
        [SerializeField] private GameObject scriptableObjectContainer;
        [Header("Reactor / Liquid (Optional)")]
        [Tooltip("Enabled only for + and − operators (and disabled for × and ÷).")]
        [SerializeField] private GameObject reactor;
        [Tooltip("Enabled for + operator (disabled for others by default).")]
        [SerializeField] private GameObject liquidHalf;
        [Tooltip("Enabled for − operator (disabled for others by default).")]
        [SerializeField] private GameObject liquidFill;
        [Tooltip("Optional particle effect (usually a child under Liquid_fill). Disabled for '-' correct transition.")]
        [SerializeField] private GameObject waterFlowing;

        [Header("Divide Feedback (Optional)")]
        [Tooltip("Shown only for the ÷ operator. On correct ÷, we toggle StuckedBlob → SlicedBlob after the hand animation.")]
        [SerializeField] private GameObject blobMachine;
        [SerializeField] private GameObject stuckedBlob;
        [SerializeField] private GameObject slicedBlob;
        [Tooltip("Optional UI overlay for ÷ rounds. Will be enabled only for divide.")]
        [SerializeField] private GameObject pizzaSplitter;
        [Tooltip("Game objects to disable when Pizza_Splitter is enabled.")]
        [SerializeField] private List<GameObject> objectsToDisableWhenSplitterActive = new List<GameObject>();
        [Tooltip("Unsplit pizza object under Pizza_Splitter. Disabled on correct divide answer.")]
        [SerializeField] private GameObject unsplitPizza;
        [Tooltip("Parent object containing split pizza variants (Split_two, Split_three, Split_four).")]
        [SerializeField] private GameObject splitPizzas;
        [Tooltip("Split pizza for divide by 2. Child of Split_pizzas.")]
        [SerializeField] private GameObject splitTwo;
        [Tooltip("Split pizza for divide by 3. Child of Split_pizzas.")]
        [SerializeField] private GameObject splitThree;
        [Tooltip("Split pizza for divide by 4. Child of Split_pizzas.")]
        [SerializeField] private GameObject splitFour;

        [Header("Multiply Feedback (Optional)")]
        [Tooltip("Shown only for the × operator.")]
        [SerializeField] private GameObject jellyMultiplier;
        [Tooltip("Multiply state: disable this on correct ×.")]
        [SerializeField] private GameObject lonelyJelly;
        [Tooltip("Multiply state: enable this on correct ×.")]
        [SerializeField] private GameObject cloneJelly;
        [Tooltip("Pizza Cloner root object. Children pizza_1, pizza_2, pizza_3, pizza_4 are enabled based on multiply amount.")]
        [SerializeField] private GameObject pizzaCloner;
        [Tooltip("Cloner pizza children (pizza_1, pizza_2, pizza_3, pizza_4). If empty, auto-resolved from Pizza_Cloner.")]
        [SerializeField] private List<GameObject> clonerPizzas = new List<GameObject>();

        [Header("Stacker Pizza Visuals (1-4)")]
        [Tooltip("Optional root for Pizza Stacker UI (expects children: Full, Half).")]
        [SerializeField] private Transform pizzaStackerRoot;
        [Tooltip("Optional child under Pizza_Stacker for full pizzas (+).")]
        [SerializeField] private Transform pizzaStackerFullRoot;
        [Tooltip("Optional child under Pizza_Stacker for half pizzas (−).")]
        [SerializeField] private Transform pizzaStackerHalfRoot;
        [Tooltip("Pizzas shown for '+' answers (size 1-4). If empty, auto-fills from Pizza_Stacker/Half/1_pizza..4_pizza.")]
        [SerializeField] private List<GameObject> pizzaFullPizzas = new List<GameObject>();
        [Tooltip("Pizzas shown for '−' answers (size 1-4). If empty, auto-fills from Pizza_Stacker/Full/1_pizza..4_pizza.")]
        [SerializeField] private List<GameObject> pizzaHalfPizzas = new List<GameObject>();
        [Tooltip("Seconds to hold the stacker display after a correct +/− answer before the next round.")]
        [SerializeField] private float stackerAnswerHoldSeconds = 2f;

        [Header("Operator Audio (Optional)")]
        [SerializeField] private OperatorMatchOperatorAudioPlayer operatorAudio;

        [Header("Recipe Mode (Operator + Number)")]
        [Tooltip("Enable Jelly Recipe tickets: player presses operator then the shown amount.")]
        [SerializeField] private bool useRecipeTickets = true;
        [SerializeField] private OperatorMatchRecipeTicketView recipeTicketView;
        [Tooltip("Possible amounts for tickets (ignored if <= 0).")]
        [SerializeField] private List<int> recipeAmounts = new List<int> { 2, 3, 4 };
        [Tooltip("Reveal the number automatically after a short delay.")]
        [SerializeField] private bool autoRevealDigit = true;

        [Header("Recipe Machine Hooks (Optional)")]
        [Tooltip("Animator for cloner (× and +). Triggered with clonerSpinTrigger.")]
        [SerializeField] private Animator clonerAnimator;
        [SerializeField] private string clonerSpinTrigger = "Spin";
        [Tooltip("Animator for squeezer (−). Triggered with squeezerTrigger.")]
        [SerializeField] private Animator squeezerAnimator;
        [SerializeField] private string squeezerTrigger = "Squeeze";
        [Tooltip("Animator for splitter (÷). Triggered with splitterTrigger.")]
        [SerializeField] private Animator splitterAnimator;
        [SerializeField] private string splitterTrigger = "Split";

        [Header("Recipe Spawned Jelly Visuals (Optional)")]
        [Tooltip("Prefab used to show blobs/ghosts when amount > 0. If null, falls back to toggling slots.")]
        [SerializeField] private GameObject jellyBlobPrefab;
        [Tooltip("Parent/anchor for spawned blobs near the reactor top.")]
        [SerializeField] private Transform jellySpawnRoot;
        [SerializeField] private Vector2 jellySpawnScatter = new Vector2(0.35f, 0.25f);
        [Tooltip("Existing slots for + (ghost outlines). If populated, we toggle instead of instantiating prefabs.")]
        [SerializeField] private List<GameObject> addGhostSlots = new List<GameObject>();
        [Tooltip("Existing slots for ÷ reaction (split blobs).")]
        [SerializeField] private List<GameObject> divideGhostSlots = new List<GameObject>();
        [Tooltip("Existing slots for − squeeze removal.")]
        [SerializeField] private List<GameObject> subtractSlots = new List<GameObject>();
        [Tooltip("Existing slots for × clones.")]
        [SerializeField] private List<GameObject> multiplySlots = new List<GameObject>();

        [Header("Ghost Slot Animation (Optional)")]
        [SerializeField] private OperatorMatchGhostSlotAnimator ghostSlotAnimator;

        [Header("Tool Messages & Audio (Optional)")]
        [Tooltip("TMP text on Msg_Panel/Operator_message.")]
        [SerializeField] private TMP_Text operatorMessageText;
        [Tooltip("Msg_Panel image (optional). If null will be auto-resolved.")]
        [SerializeField] private UnityEngine.UI.Image msgPanelImage;
        [Tooltip("AudioSource used for tool feedback clips.")]
        [SerializeField] private AudioSource toolAudioSource;
        [Tooltip("Delay before continuing after showing the tool message.")]
        [SerializeField] private float toolMessageDelaySeconds = 2f;
        [SerializeField] private string stackerAddMessage = "Stack it up!";
        [SerializeField] private string stackerSubtractMessage = "Stack it down!";
        [Tooltip("Optional clip just for the + stacker prompt. Falls back to stackerClip.")]
        [SerializeField] private AudioClip stackerAddClip;
        [SerializeField] private AudioClip stackerClip;
        [SerializeField] private string clonerMessage = "Cloning...";
        [SerializeField] private AudioClip clonerClip;
        [SerializeField] private string splitterMessage = "Splitting!";
        [SerializeField] private AudioClip splitterClip;

        [Header("Popup (Optional - auto-created if missing)")]
        [SerializeField] private OperatorMatchPopupView popupView;
        [SerializeField] private Transform popupRoot;
        [SerializeField] private SpriteRenderer popupBackground;
        [SerializeField] private TMP_Text popupText;

        private CalculatorFundamentals calculatorFundamentals;
        [Header("Completion (Optional)")]
        [Tooltip("Prefab (or scene object) to show when all rounds are completed.")]
        public GameObject endGamePrefab;
        [Tooltip("Optional parent/anchor for the end-game prefab. If empty, uses current Gameplay_* root (if detected).")]
        public Transform endGameSpawnParent;
        [Tooltip("If true, instantiate the prefab on completion. If false, just SetActive(true) on the referenced object.")]
        public bool instantiateEndGamePrefab = true;

        [Header("Gameplay Root Scoping")]
        [Tooltip("If set, all auto-resolve searches are scoped under this root (recommended when multiple Gameplay_* groups exist).")]
        [SerializeField] private Transform gameplayRootOverride;
        [Tooltip("If true and no override is provided, the controller will auto-detect the nearest parent named Gameplay_1 / Gameplay_2 / Gameplay_3.")]
        [SerializeField] private bool autoDetectGameplayRoot = true;

        private ICalculatorInputSource inputSource;
        private readonly List<OperatorSign> allowed = new();
        private OperatorSign target;
        private int roundIndex;
        private int correctCount;
        private int score;
        private bool locked;
        private Coroutine handRoutine;
        private bool msgPanelWasActive;
        private OperatorRecipeTicket currentRecipe;
        private bool expectingOperator;
        private bool expectingNumber;
        private Coroutine digitRevealRoutine;
        private readonly List<GameObject> spawnedBlobs = new();
        private bool toolFeedbackStartedThisRound;
        private bool isLevelActive;
        private GameObject endGameInstance;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Only run cleanup if not playing and in a valid state
            if (Application.isPlaying) return;

            try
            {
                // Ensure all lists are initialized first
                if (objectsToDisableWhenSplitterActive == null)
                    objectsToDisableWhenSplitterActive = new List<GameObject>();
                if (clonerPizzas == null)
                    clonerPizzas = new List<GameObject>();
                if (pizzaFullPizzas == null)
                    pizzaFullPizzas = new List<GameObject>();
                if (pizzaHalfPizzas == null)
                    pizzaHalfPizzas = new List<GameObject>();
                if (recipeAmounts == null)
                    recipeAmounts = new List<int> { 2, 3, 4 };
                if (addGhostSlots == null)
                    addGhostSlots = new List<GameObject>();
                if (divideGhostSlots == null)
                    divideGhostSlots = new List<GameObject>();
                if (subtractSlots == null)
                    subtractSlots = new List<GameObject>();
                if (multiplySlots == null)
                    multiplySlots = new List<GameObject>();

                // Clean up null references from all lists to prevent Inspector errors
                CleanupNullReferencesInList(objectsToDisableWhenSplitterActive);
                CleanupNullReferencesInList(clonerPizzas);
                CleanupNullReferencesInList(pizzaFullPizzas);
                CleanupNullReferencesInList(pizzaHalfPizzas);
                CleanupNullReferencesInList(addGhostSlots);
                CleanupNullReferencesInList(divideGhostSlots);
                CleanupNullReferencesInList(subtractSlots);
                CleanupNullReferencesInList(multiplySlots);
            }
            catch (System.Exception)
            {
                // Silently catch any serialization errors during OnValidate
                // This prevents Inspector crashes when SerializedObject is in an invalid state
            }
        }
        void Start()
        {
            calculatorFundamentals = scriptableObjectContainer.GetComponent<CalculatorFundamentals>();
        }
        private void CleanupNullReferencesInList(List<GameObject> list)
        {
            if (list == null) return;

            try
            {
                // Remove null or destroyed references
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == null)
                    {
                        list.RemoveAt(i);
                    }
                }
            }
            catch (System.Exception)
            {
                // Silently catch errors during cleanup
            }
        }

        private void Reset()
        {
            // Called when component is first added or reset in Inspector
            // Ensure all lists are initialized
            if (objectsToDisableWhenSplitterActive == null)
                objectsToDisableWhenSplitterActive = new List<GameObject>();
            if (clonerPizzas == null)
                clonerPizzas = new List<GameObject>();
            if (pizzaFullPizzas == null)
                pizzaFullPizzas = new List<GameObject>();
            if (pizzaHalfPizzas == null)
                pizzaHalfPizzas = new List<GameObject>();
            if (recipeAmounts == null)
                recipeAmounts = new List<int> { 2, 3, 4 };
            if (addGhostSlots == null)
                addGhostSlots = new List<GameObject>();
            if (divideGhostSlots == null)
                divideGhostSlots = new List<GameObject>();
            if (subtractSlots == null)
                subtractSlots = new List<GameObject>();
            if (multiplySlots == null)
                multiplySlots = new List<GameObject>();
        }
#endif

        private void Awake()
        {
            // Ensure all serialized lists are initialized to prevent Unity Inspector null reference errors
            if (objectsToDisableWhenSplitterActive == null)
                objectsToDisableWhenSplitterActive = new List<GameObject>();
            if (clonerPizzas == null)
                clonerPizzas = new List<GameObject>();
            if (pizzaFullPizzas == null)
                pizzaFullPizzas = new List<GameObject>();
            if (pizzaHalfPizzas == null)
                pizzaHalfPizzas = new List<GameObject>();
            if (recipeAmounts == null)
                recipeAmounts = new List<int> { 2, 3, 4 };
            if (addGhostSlots == null)
                addGhostSlots = new List<GameObject>();
            if (divideGhostSlots == null)
                divideGhostSlots = new List<GameObject>();
            if (subtractSlots == null)
                subtractSlots = new List<GameObject>();
            if (multiplySlots == null)
                multiplySlots = new List<GameObject>();

            // Auto-resolve key scene references (Gameplay_2 visuals) in case inspector wiring is missing/wrong.
            // This is important because multiple OperatorMatchSystem instances can exist in a scene and
            // GameFlowController/category resolution may pick a different one.
            ResolveCoreSceneReferences();
        }

        private Transform GetGameplayRoot()
        {
            if (gameplayRootOverride != null) return gameplayRootOverride;

            if (autoDetectGameplayRoot)
            {
                var p = transform;
                while (p != null)
                {
                    var n = p.name;
                    if (n == "Gameplay_1" || n == "Gameplay_2" || n == "Gameplay_3")
                    {
                        return p;
                    }
                    p = p.parent;
                }
            }

            // Backward-compat fallback (older scenes): prefer Gameplay_2 if present.
            var go2 = GameObject.Find("Gameplay_2");
            if (go2 != null) return go2.transform;

            var go3 = GameObject.Find("Gameplay_3");
            if (go3 != null) return go3.transform;

            var go1 = GameObject.Find("Gameplay_1");
            if (go1 != null) return go1.transform;

            return null;
        }

        private void ResolveCoreSceneReferences()
        {
            // Scope all searches under the correct Gameplay root (important when multiple Gameplay_* groups exist).
            Transform gameplayRoot = GetGameplayRoot();

            // Core operator visuals
            reactor = ResolveByName(gameplayRoot, reactor, "Reactor", "Rector");
            blobMachine = ResolveByName(gameplayRoot, blobMachine, "Blob_machine");
            jellyMultiplier = ResolveByName(gameplayRoot, jellyMultiplier, "Jelly_Multiplier", "Jelly_multiplier");
            handBase = ResolveByName(gameplayRoot, handBase, "Hand_base");
            pizzaStackerRoot = ResolveByName(gameplayRoot, pizzaStackerRoot != null ? pizzaStackerRoot.gameObject : null, "Pizza_Stacker")?.transform ?? pizzaStackerRoot;
            pizzaCloner = ResolveByName(gameplayRoot, pizzaCloner, "Pizza_Cloner");
            pizzaSplitter = ResolveByName(gameplayRoot, pizzaSplitter, "Pizza_Splitter");

            // Gameplay_2: divide visuals live under Blob_machine (not Pizza_Splitter).
            if (blobMachine != null)
            {
                var blobTf = blobMachine.transform;
                stuckedBlob = ResolveByName(blobTf, stuckedBlob, "StuckedBlob");
                slicedBlob = ResolveByName(blobTf, slicedBlob, "SlicedBlob");
            }

            // Gameplay_2: multiply visuals live under Jelly_multiplier (not Pizza_Cloner).
            if (jellyMultiplier != null)
            {
                var jellyTf = jellyMultiplier.transform;
                lonelyJelly = ResolveByName(jellyTf, lonelyJelly, "Lonely_jelly");
                cloneJelly = ResolveByName(jellyTf, cloneJelly, "Clone_jelly");
            }

            // Reactor internals
            if (reactor != null)
            {
                var reactorTf = reactor.transform;
                liquidHalf = ResolveByName(reactorTf, liquidHalf, "Liquid_half");
                liquidFill = ResolveByName(reactorTf, liquidFill, "Liquid_fill");

                // WaterFlowing is usually a child under Liquid_fill.
                if (waterFlowing == null && liquidFill != null)
                {
                    var wf = liquidFill.transform.Find("WaterFlowing");
                    if (wf != null) waterFlowing = wf.gameObject;
                }
            }
        }

        private static GameObject ResolveByName(Transform root, GameObject current, params string[] desiredNames)
        {
            if (current != null && desiredNames != null)
            {
                for (int i = 0; i < desiredNames.Length; i++)
                {
                    if (current.name == desiredNames[i]) return current;
                }
            }

            // Try under root first (if provided)
            if (root != null)
            {
                if (desiredNames != null)
                {
                    for (int i = 0; i < desiredNames.Length; i++)
                    {
                        var desiredName = desiredNames[i];
                        if (root.name == desiredName) return root.gameObject;
                        var found = FindChildRecursive(root, desiredName);
                        if (found != null) return found.gameObject;
                    }
                }
            }

            // Do NOT scene-wide search by default. With multiple Gameplay_* groups, global find can
            // return the wrong object's visuals and cause cross-gameplay toggling.
            return current;
        }

        public void Initialize(ICalculatorInputSource input)
        {
            inputSource = input;
            if (inputSource != null)
            {
                inputSource.OnKey += HandleKey;
            }
        }

        public void StartLevel()
        {
            isLevelActive = true;
            ResolveCoreSceneReferences();
            ConfigureModeForGameplay();
            BuildAllowedList();
            EnsurePopupExists();
            EnsureGuideExists();
            ClearStackerPizzas();

            roundIndex = 0;
            correctCount = 0;
            score = 0;
            locked = false;
            toolFeedbackStartedThisRound = false;
            UpdateRoundCounter();

            ClearSpawnedBlobs();
            if (useRecipeTickets)
            {
                NextRecipeRound();
            }
            else
            {
                NextRound();
            }
        }

        private void ConfigureModeForGameplay()
        {
            var root = GetGameplayRoot();
            if (root == null) return;

            // Gameplay-wise expectations (as per scene setup):
            // - Gameplay_2 => operator-only (Reactor/Blob_machine/Jelly_multiplier/Hand_base)
            // - Gameplay_3 => recipe tickets (RecipeTicketView + Pizza tools)
            if (root.name == "Gameplay_2")
            {
                useRecipeTickets = false;
            }
            else if (root.name == "Gameplay_3")
            {
                useRecipeTickets = true;
            }

            // If recipe mode is enabled but no view exists in this gameplay, fall back to operator-only.
            if (useRecipeTickets && recipeTicketView == null)
            {
                recipeTicketView = root.GetComponentInChildren<OperatorMatchRecipeTicketView>(true);
            }

            if (useRecipeTickets && recipeTicketView == null)
            {
                Debug.LogWarning($"{nameof(OperatorMatchLevelController)}: Recipe mode enabled but no {nameof(OperatorMatchRecipeTicketView)} found under '{root.name}'. Falling back to operator-only mode.");
                useRecipeTickets = false;
            }
        }

        private void OnDestroy()
        {
            isLevelActive = false;
            if (inputSource != null)
            {
                inputSource.OnKey -= HandleKey;
            }
        }

        private void OnDisable()
        {
            // Stop processing input when disabled
            isLevelActive = false;
        }

        private void BuildAllowedList()
        {
            allowed.Clear();
            if (allowAdd) allowed.Add(OperatorSign.Add);
            if (allowSubtract) allowed.Add(OperatorSign.Subtract);
            if (allowMultiply) allowed.Add(OperatorSign.Multiply);
            if (allowDivide) allowed.Add(OperatorSign.Divide);

            // Safety: never allow empty list.
            if (allowed.Count == 0)
            {
                allowed.Add(OperatorSign.Add);
            }
        }

        private void EnsureGuideExists()
        {
            if (guide != null) return;

            // Prefer a guide under the same Gameplay root.
            var root = GetGameplayRoot();
            if (root != null)
            {
                guide = root.GetComponentInChildren<GuideCharacter>(true);
                if (guide != null) return;
            }

            guide = FindFirstObjectByType<GuideCharacter>();
        }

        private void EnsurePopupExists()
        {
            if (popupView == null)
            {
                popupView = FindFirstObjectByType<OperatorMatchPopupView>();
                if (popupView != null)
                {
                    popupRoot = popupView.transform;
                }
            }

            // If we have a popup view (typically UI), do not auto-create any world-space popup.
            if (popupView != null) return;

            if (popupRoot != null && popupBackground != null && popupText != null) return;

            if (popupRoot == null)
            {
                var go = new GameObject("OperatorPopup");
                popupRoot = go.transform;

                var cam = Camera.main;
                if (cam != null)
                {
                    popupRoot.position = cam.transform.position + new Vector3(0f, 2.0f, 10f);
                    popupRoot.position = new Vector3(popupRoot.position.x, popupRoot.position.y, 0f);
                }
                else
                {
                    popupRoot.position = new Vector3(0f, 2f, 0f);
                }
            }

            if (popupBackground == null)
            {
                var bgGo = new GameObject("BG");
                bgGo.transform.SetParent(popupRoot, false);
                popupBackground = bgGo.AddComponent<SpriteRenderer>();
                popupBackground.sprite = GetPlaceholderSprite();
                popupBackground.color = new Color(0f, 0f, 0f, 0.65f);
                popupBackground.sortingOrder = 200;
                bgGo.transform.localScale = new Vector3(2.5f, 1.4f, 1f);
            }

            if (popupText == null)
            {
                var txtGo = new GameObject("Text");
                txtGo.transform.SetParent(popupRoot, false);
                txtGo.transform.localPosition = new Vector3(0f, 0f, 0f);

                var tmp = txtGo.AddComponent<TextMeshPro>();
                tmp.fontSize = 8;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.sortingOrder = 201;
                popupText = tmp;
            }
        }

        private static Sprite placeholderSprite;
        private static Sprite GetPlaceholderSprite()
        {
            if (placeholderSprite != null) return placeholderSprite;
            var tex = Texture2D.whiteTexture;
            placeholderSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            placeholderSprite.name = "OperatorMatch_Placeholder";
            return placeholderSprite;
        }

        private void NextRound()
        {
            // Reset all operator objects to ensure clean state
            ResetAllOperatorObjects();

            if (useRecipeTickets)
            {
                NextRecipeRound();
                return;
            }

            // Increment roundIndex first, then check if we've completed all rounds
            roundIndex++;

            if (roundIndex > totalRounds)
            {
                Complete();
                return;
            }

            // Ensure locked is false so input can be processed
            locked = false;

            // Ensure we have allowed operators
            if (allowed == null || allowed.Count == 0)
            {
                BuildAllowedList();
            }

            target = allowed[UnityEngine.Random.Range(0, allowed.Count)];
            toolFeedbackStartedThisRound = false;
            UpdateRoundCounter();

            ResetCalculatorDisplay();
            ShowTarget(target);
        }

        private void ShowTarget(OperatorSign sign)
        {
            // Ensure popup exists before showing
            EnsurePopupExists();

            if (popupRoot != null)
            {
                popupRoot.gameObject.SetActive(true);
            }

            string symbol = ToDisplaySymbol(sign);
            if (popupView != null)
            {
                popupView.gameObject.SetActive(true);
                popupView.Show(sign);
            }
            else if (popupText != null)
            {
                popupText.text = symbol;
            }

            ApplyReactorForOperator(sign);
            ApplyBlobMachineForOperator(sign);
            ApplyJellyMultiplierForOperator(sign);
            operatorAudio?.PlayFor(sign);
            guide?.ShowPrompt($"Press {symbol}");

            // Show tool message/audio early for non-recipe flow.
            StartCoroutine(PlayToolFeedback(sign, waitForAudio: true, skipIfAlreadyShown: true));
        }

        private void HandleKey(string key)
        {
            // Only process input if level is active, component is enabled, and GameObject is active
            if (!isLevelActive || !enabled || !gameObject.activeInHierarchy || locked || string.IsNullOrEmpty(key)) return;

            if (useRecipeTickets)
            {
                HandleRecipeKey(key);
                return;
            }

            if (!TryParseOperator(key, out var pressed))
            {
                Wrong("Press an operator (+, −, ×, ÷).");
                return;
            }

            if (pressed == target)
            {
                // Update OperatorPopup under Hand_base to show the correct operator
                UpdateOperatorPopupUnderHand(target);
                StartCoroutine(CorrectThenAdvance());
                return;
            }

            Wrong($"Try again! Press {ToDisplaySymbol(target)}");
        }

        private IEnumerator CorrectThenAdvance()
        {
            locked = true;
            correctCount++;
            score++;

            AudioService.Instance?.PlayCorrectImmediate();
            AudioService.Instance?.PlayCorrect();

            var vfxPos = vfxAnchor != null
                ? vfxAnchor.position
                : (shakeTarget != null ? shakeTarget.position : transform.position);
            VFXService.Instance?.SpawnCorrectBurst(vfxPos);

            guide?.ShowSuccess("Correct!");

            // Wait for the correct feedback animation (e.g., Hand_base) to finish before advancing.
            // PlayCorrectFeedbackAnimation() already includes: animation plays fully, then 2 second pause
            yield return PlayCorrectFeedbackAnimation();

            // Special case: on '+' round after correct, show Liquid_fill for a moment before advancing.
            if (target == OperatorSign.Add)
            {
                yield return PlayAddCorrectLiquidTransition(addCorrectLiquidHoldSeconds);
            }
            else if (target == OperatorSign.Subtract)
            {
                yield return PlaySubtractCorrectLiquidTransition(subtractCorrectLiquidHoldSeconds);
            }
            else if (target == OperatorSign.Divide)
            {
                // After hand animation finishes: toggle StuckedBlob -> SlicedBlob.
                ToggleDivideBlobToSliced();

                // Extra pause specifically for divide before advancing.
                if (postCorrectPauseSeconds > 0f)
                {
                    yield return new WaitForSeconds(postCorrectPauseSeconds);
                }
            }
            else if (target == OperatorSign.Multiply)
            {
                ToggleMultiplyJellyToClone();

                // Extra pause specifically for multiply before advancing.
                if (postCorrectPauseSeconds > 0f)
                {
                    yield return new WaitForSeconds(postCorrectPauseSeconds);
                }
            }

            // Animation and pause are already handled in PlayCorrectFeedbackAnimation()
            // Now load next round if there are more rounds
            locked = false;

            // Small delay to ensure all animations/transitions are complete
            yield return null;

            // Ensure level is active before loading next round
            if (!isLevelActive)
            {
                isLevelActive = true;
            }

            // Always try to load next round
            NextRound();
        }

        private void ResetAllOperatorObjects()
        {
            // In case scene references were changed/duplicated at runtime, re-resolve before toggling.
            ResolveCoreSceneReferences();

            // Disable all main operator objects
            if (reactor != null) reactor.SetActive(false);
            if (blobMachine != null) blobMachine.SetActive(false);
            if (jellyMultiplier != null) jellyMultiplier.SetActive(false);
            if (handBase != null) handBase.SetActive(false);

            // Also hard-reset optional overlays so visuals never leak between rounds.
            if (pizzaSplitter != null) pizzaSplitter.SetActive(false);
            if (pizzaCloner != null) pizzaCloner.SetActive(false);

            // Disable reactor-related objects
            if (liquidHalf != null) liquidHalf.SetActive(false);
            if (liquidFill != null) liquidFill.SetActive(false);
            if (waterFlowing != null) waterFlowing.SetActive(false);

            // Disable blob machine related objects
            if (pizzaSplitter != null) pizzaSplitter.SetActive(false);
            if (stuckedBlob != null) stuckedBlob.SetActive(false);
            if (slicedBlob != null) slicedBlob.SetActive(false);

            // Disable multiply related objects
            if (lonelyJelly != null) lonelyJelly.SetActive(false);
            if (cloneJelly != null) cloneJelly.SetActive(false);
            if (pizzaCloner != null) pizzaCloner.SetActive(false);

            // Re-enable objects that should be disabled when splitter is active
            if (objectsToDisableWhenSplitterActive != null)
            {
                foreach (var obj in objectsToDisableWhenSplitterActive)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                }
            }

            // Ensure popupRoot is enabled for non-recipe mode (it will be set correctly in ShowTarget)
            // Don't disable popupRoot here as it's managed by ShowTarget/ShowRecipeTicket
        }

        private void ApplyReactorForOperator(OperatorSign sign)
        {
            bool isAddOrSubtract = sign == OperatorSign.Add || sign == OperatorSign.Subtract;
            if (reactor != null) reactor.SetActive(isAddOrSubtract);

            // Default liquid state per operator:
            // +  => half ON, fill OFF
            // −  => fill ON, half OFF (and ensure waterFlowing is ON if present)
            // ×/÷ => both OFF (and waterFlowing OFF)
            if (liquidHalf != null) liquidHalf.SetActive(sign == OperatorSign.Add);
            if (liquidFill != null) liquidFill.SetActive(sign == OperatorSign.Subtract);

            ResolveWaterFlowingIfNeeded();
            if (waterFlowing != null)
            {
                waterFlowing.SetActive(sign == OperatorSign.Subtract);
            }
        }

        private void ApplyBlobMachineForOperator(OperatorSign sign)
        {
            bool isDivide = sign == OperatorSign.Divide;

            if (pizzaSplitter == null && isDivide)
            {
                // Scoped resolution (avoid grabbing a splitter from another Gameplay_* group).
                var root = GetGameplayRoot();
                pizzaSplitter = ResolveByName(root, pizzaSplitter, "Pizza_Splitter");
            }

            if (pizzaSplitter != null)
            {
                pizzaSplitter.SetActive(isDivide);
                // Disable/enable objects based on Pizza_Splitter state
                ToggleObjectsForSplitter(isDivide);
            }

            if (blobMachine == null && stuckedBlob == null && slicedBlob == null) return;

            if (blobMachine != null) blobMachine.SetActive(isDivide);

            if (!isDivide) return;

            // Reset state for each divide round.
            if (stuckedBlob != null) stuckedBlob.SetActive(true);
            if (slicedBlob != null) slicedBlob.SetActive(false);

            // Reset split pizza state for new divide round
            ResetSplitPizzaState();
        }

        private void ToggleObjectsForSplitter(bool splitterActive)
        {
            if (objectsToDisableWhenSplitterActive == null) return;

            foreach (var obj in objectsToDisableWhenSplitterActive)
            {
                if (obj != null)
                {
                    obj.SetActive(!splitterActive);
                }
            }
        }

        private void ShowSplitPizzas(int divideAmount)
        {
            // Resolve references if not set
            ResolveSplitPizzaReferences();

            // Disable Unsplit_Pizza on correct divide answer
            if (unsplitPizza != null)
            {
                unsplitPizza.SetActive(false);
            }

            // Enable the appropriate split variant based on divide amount
            // Clamp amount to valid range (2, 3, or 4)
            divideAmount = Mathf.Clamp(divideAmount, 2, 4);

            // Disable all split variants first
            if (splitTwo != null) splitTwo.SetActive(false);
            if (splitThree != null) splitThree.SetActive(false);
            if (splitFour != null) splitFour.SetActive(false);

            // Enable the matching split variant
            switch (divideAmount)
            {
                case 2:
                    if (splitTwo != null) splitTwo.SetActive(true);
                    break;
                case 3:
                    if (splitThree != null) splitThree.SetActive(true);
                    break;
                case 4:
                    if (splitFour != null) splitFour.SetActive(true);
                    break;
            }
        }

        private void ResolveSplitPizzaReferences()
        {
            // Auto-resolve Pizza_Splitter if not set (scoped to this Gameplay root)
            if (pizzaSplitter == null)
            {
                var root = GetGameplayRoot();
                pizzaSplitter = ResolveByName(root, pizzaSplitter, "Pizza_Splitter");
            }

            if (pizzaSplitter == null) return;

            // Resolve Unsplit_Pizza
            if (unsplitPizza == null)
            {
                var unsplitTf = pizzaSplitter.transform.Find("Unsplit_Pizza");
                if (unsplitTf != null)
                {
                    unsplitPizza = unsplitTf.gameObject;
                }
                else
                {
                    unsplitTf = FindChildRecursive(pizzaSplitter.transform, "Unsplit_Pizza");
                    if (unsplitTf != null)
                    {
                        unsplitPizza = unsplitTf.gameObject;
                    }
                }
            }

            // Resolve Split_pizzas parent
            if (splitPizzas == null)
            {
                var splitPizzasTf = pizzaSplitter.transform.Find("Split_pizzas");
                if (splitPizzasTf != null)
                {
                    splitPizzas = splitPizzasTf.gameObject;
                }
                else
                {
                    splitPizzasTf = FindChildRecursive(pizzaSplitter.transform, "Split_pizzas");
                    if (splitPizzasTf != null)
                    {
                        splitPizzas = splitPizzasTf.gameObject;
                    }
                }
            }

            // Resolve split variants if Split_pizzas is found
            if (splitPizzas != null)
            {
                if (splitTwo == null)
                {
                    var splitTwoTf = splitPizzas.transform.Find("Split_two");
                    if (splitTwoTf != null)
                    {
                        splitTwo = splitTwoTf.gameObject;
                    }
                    else
                    {
                        splitTwoTf = FindChildRecursive(splitPizzas.transform, "Split_two");
                        if (splitTwoTf != null)
                        {
                            splitTwo = splitTwoTf.gameObject;
                        }
                    }
                }

                if (splitThree == null)
                {
                    var splitThreeTf = splitPizzas.transform.Find("Split_three");
                    if (splitThreeTf != null)
                    {
                        splitThree = splitThreeTf.gameObject;
                    }
                    else
                    {
                        splitThreeTf = FindChildRecursive(splitPizzas.transform, "Split_three");
                        if (splitThreeTf != null)
                        {
                            splitThree = splitThreeTf.gameObject;
                        }
                    }
                }

                if (splitFour == null)
                {
                    var splitFourTf = splitPizzas.transform.Find("Split_four");
                    if (splitFourTf != null)
                    {
                        splitFour = splitFourTf.gameObject;
                    }
                    else
                    {
                        splitFourTf = FindChildRecursive(splitPizzas.transform, "Split_four");
                        if (splitFourTf != null)
                        {
                            splitFour = splitFourTf.gameObject;
                        }
                    }
                }
            }
        }

        private void ResetSplitPizzaState()
        {
            // Resolve references if not set
            ResolveSplitPizzaReferences();

            // Enable Unsplit_Pizza (reset state for new divide round)
            if (unsplitPizza != null)
            {
                unsplitPizza.SetActive(true);
            }

            // Disable all split variants
            if (splitTwo != null) splitTwo.SetActive(false);
            if (splitThree != null) splitThree.SetActive(false);
            if (splitFour != null) splitFour.SetActive(false);
        }

        private void ShowClonerPizzas(int multiplyAmount)
        {
            // Resolve references if not set
            ResolveClonerPizzaReferences();

            // Clamp amount to valid range (1-4)
            multiplyAmount = Mathf.Clamp(multiplyAmount, 1, 4);

            // Enable Pizza_Cloner root
            if (pizzaCloner != null)
            {
                pizzaCloner.SetActive(true);
            }

            // Enable first N pizzas (pizza_1, pizza_2, ... pizza_N)
            if (clonerPizzas != null && clonerPizzas.Count > 0)
            {
                for (int i = 0; i < clonerPizzas.Count; i++)
                {
                    var pizza = clonerPizzas[i];
                    if (pizza != null)
                    {
                        // Enable pizzas 0 through (multiplyAmount-1), which corresponds to pizza_1 through pizza_N
                        pizza.SetActive(i < multiplyAmount);
                    }
                }
            }
        }

        private void ResetClonerPizzas()
        {
            // Resolve references if not set
            ResolveClonerPizzaReferences();

            // Disable all cloner pizzas
            if (clonerPizzas != null && clonerPizzas.Count > 0)
            {
                for (int i = 0; i < clonerPizzas.Count; i++)
                {
                    var pizza = clonerPizzas[i];
                    if (pizza != null)
                    {
                        pizza.SetActive(false);
                    }
                }
            }
        }

        private void ResolveClonerPizzaReferences()
        {
            // Auto-resolve Pizza_Cloner if not set (scoped to this Gameplay root)
            if (pizzaCloner == null)
            {
                var root = GetGameplayRoot();
                pizzaCloner = ResolveByName(root, pizzaCloner, "Pizza_Cloner");
            }

            if (pizzaCloner == null) return;

            // Resolve cloner pizza children if list is empty
            if (clonerPizzas.Count == 0)
            {
                for (int i = 1; i <= 4; i++)
                {
                    var childTf = pizzaCloner.transform.Find($"pizza_{i}");
                    if (childTf != null)
                    {
                        clonerPizzas.Add(childTf.gameObject);
                    }
                    else
                    {
                        // Try recursive search as fallback
                        childTf = FindChildRecursive(pizzaCloner.transform, $"pizza_{i}");
                        if (childTf != null)
                        {
                            clonerPizzas.Add(childTf.gameObject);
                        }
                    }
                }
            }
        }

        private void ToggleDivideBlobToSliced()
        {
            if (stuckedBlob != null) stuckedBlob.SetActive(false);
            if (slicedBlob != null) slicedBlob.SetActive(true);
        }

        private void ApplyJellyMultiplierForOperator(OperatorSign sign)
        {
            if (jellyMultiplier == null) return;
            jellyMultiplier.SetActive(sign == OperatorSign.Multiply);

            if (sign != OperatorSign.Multiply) return;

            // Reset multiply state each time the multiply operator is shown.
            if (lonelyJelly != null) lonelyJelly.SetActive(true);
            if (cloneJelly != null) cloneJelly.SetActive(false);

            // Reset cloner pizzas for new multiply round
            ResetClonerPizzas();
        }

        private void ToggleMultiplyJellyToClone()
        {
            if (lonelyJelly != null) lonelyJelly.SetActive(false);
            if (cloneJelly != null) cloneJelly.SetActive(true);
        }

        private void ResolveWaterFlowingIfNeeded()
        {
            if (waterFlowing != null) return;
            if (liquidFill == null) return;

            var tf = liquidFill.transform.Find("WaterFlowing");
            if (tf != null) waterFlowing = tf.gameObject;
        }

        private IEnumerator PlayAddCorrectLiquidTransition(float holdSeconds)
        {
            if (holdSeconds < 0f) holdSeconds = 0f;

            // Liquid_half -> Liquid_fill, then hold.
            if (liquidHalf != null) liquidHalf.SetActive(false);
            if (liquidFill != null) liquidFill.SetActive(true);

            ResolveWaterFlowingIfNeeded();
            if (waterFlowing != null) waterFlowing.SetActive(true);

            if (holdSeconds > 0f)
            {
                yield return new WaitForSeconds(holdSeconds);
            }
        }

        private IEnumerator PlaySubtractCorrectLiquidTransition(float holdSeconds)
        {
            if (holdSeconds < 0f) holdSeconds = 0f;

            // Liquid_fill -> Liquid_half, disable WaterFlowing, then hold.
            if (liquidFill != null) liquidFill.SetActive(false);
            if (liquidHalf != null) liquidHalf.SetActive(true);

            ResolveWaterFlowingIfNeeded();
            if (waterFlowing != null) waterFlowing.SetActive(false);

            if (holdSeconds > 0f)
            {
                yield return new WaitForSeconds(holdSeconds);
            }
        }

        private IEnumerator PlayCorrectFeedbackAnimation()
        {
            // Always enable Hand_base on correct answer, regardless of showHandBaseOnCorrect flag
            if (handBase != null)
            {
                handBase.SetActive(true);
            }

            // Only play the animation routine if showHandBaseOnCorrect is true
            if (showHandBaseOnCorrect && handBase != null)
            {
                // Ensure we don't have a previous hand routine still running.
                if (handRoutine != null)
                {
                    StopCoroutine(handRoutine);
                    handRoutine = null;
                }

                // Run inline so CorrectThenAdvance can wait on completion.
                yield return PlayHandBaseRoutine();
            }
            else if (handBase != null)
            {
                // If animation is disabled, just wait 2 seconds then disable Hand_base
                yield return new WaitForSeconds(handBasePostAnimationPauseSeconds);
                handBase.SetActive(false);
            }
        }

        private void TriggerHandBase()
        {
            if (!showHandBaseOnCorrect || handBase == null) return;

            if (handRoutine != null)
            {
                StopCoroutine(handRoutine);
                handRoutine = null;
            }
            handRoutine = StartCoroutine(PlayHandBaseRoutine());
        }

        private IEnumerator PlayHandBaseRoutine()
        {
            handBase.SetActive(true);

            if (msgPanel != null)
            {
                msgPanelWasActive = msgPanel.activeSelf;
                msgPanel.SetActive(false);
            }

            var animator = handBase.GetComponent<Animator>();
            if (animator == null)
            {
                // If no animator, still wait 2 seconds before disabling Hand_base
                yield return new WaitForSeconds(handBasePostAnimationPauseSeconds);
                if (handBase != null) handBase.SetActive(false);
                if (msgPanel != null) msgPanel.SetActive(msgPanelWasActive);
                handRoutine = null;
                yield break;
            }

            animator.enabled = true;

            // Reset animator to ensure animation plays from the beginning
            animator.Rebind();
            animator.Update(0f);

            const int layer = 0;
            if (!string.IsNullOrWhiteSpace(handBaseStateName) &&
                animator.HasState(layer, Animator.StringToHash(handBaseStateName)))
            {
                animator.Play(handBaseStateName, layer, 0f);
            }
            else
            {
                animator.Play(0, layer, 0f);
            }

            // Ensure Animator processes the Play() immediately.
            animator.Update(0f);

            // Wait a frame so state info is reliable.
            yield return null;

            // Wait for animation to play fully (at least one complete cycle)
            // Check normalized time to ensure we've completed at least one cycle
            var info = animator.GetCurrentAnimatorStateInfo(layer);
            var effectiveSpeed = Mathf.Max(0.0001f, Mathf.Abs(info.speed * info.speedMultiplier));
            var duration = info.length / effectiveSpeed;

            // Wait for animation to complete at least one full cycle
            var elapsed = 0f;
            while (elapsed < duration && handBase != null && handBase.activeInHierarchy)
            {
                elapsed += Time.deltaTime;

                // Also check if we've completed at least one normalized cycle
                info = animator.GetCurrentAnimatorStateInfo(layer);
                if (info.normalizedTime >= 1f)
                {
                    // Animation has completed at least one cycle
                    break;
                }

                yield return null;
            }

            // Stop the animator to prevent further looping
            if (animator != null && animator.enabled)
            {
                animator.enabled = false;
            }

            // Pause for 2 seconds after animation completes before disabling Hand_base
            if (handBase != null && handBase.activeInHierarchy)
            {
                yield return new WaitForSeconds(handBasePostAnimationPauseSeconds);
            }

            if (handBase != null) handBase.SetActive(false);
            if (msgPanel != null) msgPanel.SetActive(msgPanelWasActive);
            handRoutine = null;
        }

        private void Wrong(string message)
        {
            AudioService.Instance?.PlayWrongImmediate();
            AudioService.Instance?.PlayWrong();

            var t = shakeTarget != null ? shakeTarget : transform;
            VFXService.Instance?.ShakeOnWrong(t);
            VFXService.Instance?.SpawnWrongBurst(t.position);

            guide?.ShowError(message);
        }

        private void Complete()
        {
            isLevelActive = false;
            locked = true;

            guide?.ShowSuccess("You did it!");
            AudioService.Instance?.PlayComplete();

            var vfxPos = vfxAnchor != null
                ? vfxAnchor.position
                : (shakeTarget != null ? shakeTarget.position : transform.position);
            VFXService.Instance?.SpawnCorrectBurst(vfxPos);
            guide?.PlayCompletionBubbleEffect();

            ProgressService.SetBestScore(progressCategoryKey, score);
            var last = ProgressService.GetLastUnlocked();
            if (last < 1) ProgressService.SetLastUnlocked(1);

            // Ensure UI shows completion state.
            UpdateRoundCounter(forceComplete: true);

            // Requirement: when all rounds finish, hide Msg_Panel so it doesn't stay visible.
            DisableMsgPanelOnComplete();

            // Optional: show an end-game prefab/overlay.
            ShowEndGameOnComplete();
        }

        private void ShowEndGameOnComplete()
        {
            if (endGamePrefab == null) return;

            // Pick parent: explicit override -> gameplay root -> none.
            Transform parent = endGameSpawnParent != null ? endGameSpawnParent : GetGameplayRoot();

            if (!instantiateEndGamePrefab)
            {
                endGamePrefab.SetActive(true);
                return;
            }

            // Instantiate only once.
            if (endGameInstance != null) return;

            endGameInstance = parent != null
                ? Instantiate(endGamePrefab, parent, worldPositionStays: false)
                : Instantiate(endGamePrefab);

            endGameInstance.SetActive(true);
            Logger.LogInfo(" Level 2 completed..", "OperatorMatch");

            calculatorFundamentals.MarkGamePlayed();
        }

        private void DisableMsgPanelOnComplete()
        {
            // Prefer existing reference (inspector-wired).
            if (msgPanel == null)
            {
                // Try to find under the active Gameplay root first to avoid cross-gameplay collisions.
                var root = GetGameplayRoot();
                if (root != null)
                {
                    var tf = root.Find("Msg_Panel");
                    if (tf != null) msgPanel = tf.gameObject;
                    else
                    {
                        // Fallback recursive search
                        tf = FindChildRecursive(root, "Msg_Panel");
                        if (tf != null) msgPanel = tf.gameObject;
                    }
                }

                // Final fallback: scene-wide.
                if (msgPanel == null)
                {
                    msgPanel = GameObject.Find("Msg_Panel");
                }
            }

            if (msgPanel != null)
            {
                msgPanel.SetActive(false);
            }
        }

        private void UpdateRoundCounter(bool forceComplete = false)
        {
            int shownRound = forceComplete ? totalRounds : Mathf.Clamp(roundIndex, 0, totalRounds);
            int remaining = forceComplete ? 0 : Mathf.Max(0, totalRounds - roundIndex);

            if (totalRoundsText != null)
            {
                totalRoundsText.text = $"Total : {totalRounds}";
            }

            if (remainingRoundsText != null)
            {
                remainingRoundsText.text = $"Remaining : {remaining}";
            }

            if (roundsCounterText != null)
            {
                roundsCounterText.text = $"Round {shownRound}/{totalRounds}  |  Remaining {remaining}";
            }
        }

        #region Recipe Mode (Operator + Number)

        private void NextRecipeRound()
        {
            // Reset all operator objects to ensure clean state
            ResetAllOperatorObjects();

            // Increment roundIndex first, then check if we've completed all rounds
            roundIndex++;

            if (roundIndex > totalRounds)
            {
                Complete();
                return;
            }

            if (recipeAmounts == null || recipeAmounts.Count == 0)
            {
                recipeAmounts = new List<int> { 2, 3, 4 };
            }

            var amountChoices = recipeAmounts.FindAll(a => a > 0);
            if (amountChoices.Count == 0)
            {
                amountChoices.Add(1);
            }

            // Ensure we have allowed operators
            if (allowed == null || allowed.Count == 0)
            {
                BuildAllowedList();
            }

            var sign = allowed[UnityEngine.Random.Range(0, allowed.Count)];
            var amount = amountChoices[UnityEngine.Random.Range(0, amountChoices.Count)];

            UpdateRoundCounter();

            ClearStackerPizzas();
            currentRecipe = new OperatorRecipeTicket(sign, amount);
            expectingOperator = true;
            expectingNumber = false;
            locked = false;
            toolFeedbackStartedThisRound = false;

            ResetCalculatorDisplay();
            ShowRecipeTicket(currentRecipe);
        }

        private void ShowRecipeTicket(OperatorRecipeTicket recipe)
        {
            if (popupRoot != null) popupRoot.gameObject.SetActive(false);

            ApplyReactorForOperator(recipe.Sign);
            ApplyBlobMachineForOperator(recipe.Sign);
            ApplyJellyMultiplierForOperator(recipe.Sign);

            // Prepare stacker when question is shown
            if (IsStackerSign(recipe.Sign))
            {
                PrepareStackerForQuestion(recipe.Sign);
            }

            recipeTicketView?.ShowTicket(recipe, revealDigitImmediately: !autoRevealDigit);
            if (autoRevealDigit && recipeTicketView != null)
            {
                if (digitRevealRoutine != null)
                {
                    StopCoroutine(digitRevealRoutine);
                }
                digitRevealRoutine = StartCoroutine(recipeTicketView.RevealDigit(recipe.Amount));
            }

            operatorAudio?.PlayFor(recipe.Sign);
            guide?.ShowPrompt($"Use {ToDisplaySymbol(recipe.Sign)}, then {recipe.Amount}");

            // Show tool message/audio early for recipe flow.
            StartCoroutine(PlayToolFeedback(recipe.Sign, waitForAudio: true, skipIfAlreadyShown: true));
        }

        private void HandleRecipeKey(string key)
        {
            if (expectingOperator)
            {
                if (!TryParseOperator(key, out var pressed))
                {
                    Wrong("Press the operator on the ticket first.");
                    return;
                }

                if (pressed != currentRecipe.Sign)
                {
                    Wrong($"Ticket wants {ToDisplaySymbol(currentRecipe.Sign)}.");
                    return;
                }

                // Enable Hand_base immediately when operator matches
                if (handBase != null)
                {
                    handBase.SetActive(true);
                }

                // Update OperatorPopup under Hand_base to show the correct operator
                UpdateOperatorPopupUnderHand(currentRecipe.Sign);

                // Play VFX and sound on correct operator match
                AudioService.Instance?.PlayCorrectImmediate();
                AudioService.Instance?.PlayCorrect();

                var vfxPos = vfxAnchor != null
                    ? vfxAnchor.position
                    : (shakeTarget != null ? shakeTarget.position : transform.position);
                VFXService.Instance?.SpawnCorrectBurst(vfxPos);

                guide?.ShowSuccess("Good!");

                expectingOperator = false;
                expectingNumber = true;
                recipeTicketView?.ShowOperatorConfirmed();
                guide?.ShowPrompt($"Now press {currentRecipe.Amount}");
                return;
            }

            if (expectingNumber)
            {
                if (!TryParseDigit(key, out var digit))
                {
                    Wrong($"Press the number {currentRecipe.Amount}.");
                    return;
                }

                if (digit != currentRecipe.Amount)
                {
                    Wrong($"Ticket asks for {currentRecipe.Amount}.");
                    return;
                }

                expectingNumber = false;
                StartCoroutine(HandleRecipeSuccess());
                return;
            }
        }

        private IEnumerator HandleRecipeSuccess()
        {
            locked = true;
            correctCount++;
            score++;

            AudioService.Instance?.PlayCorrectImmediate();
            AudioService.Instance?.PlayCorrect();

            var vfxPos = vfxAnchor != null
                ? vfxAnchor.position
                : (shakeTarget != null ? shakeTarget.position : transform.position);
            VFXService.Instance?.SpawnCorrectBurst(vfxPos);

            guide?.ShowSuccess("Correct!");

            // Play hand animation for recipe mode
            // PlayCorrectFeedbackAnimation() already includes: animation plays fully, then 2 second pause
            yield return PlayCorrectFeedbackAnimation();

            yield return PlayRecipeReaction(currentRecipe);

            // Extra pause specifically for divide/multiply in recipe mode before burning ticket / next round.
            if ((currentRecipe.Sign == OperatorSign.Divide || currentRecipe.Sign == OperatorSign.Multiply) &&
                postCorrectPauseSeconds > 0f)
            {
                yield return new WaitForSeconds(postCorrectPauseSeconds);
            }

            if (IsStackerSign(currentRecipe.Sign))
            {
                ShowStackerPizzas(currentRecipe.Sign, currentRecipe.Amount);
            }

            if (recipeTicketView != null)
            {
                yield return recipeTicketView.BurnAndHide();
            }

            // Animation and pause are already handled in PlayCorrectFeedbackAnimation()
            // Now load next round if there are more rounds
            ClearStackerPizzas();
            ClearSpawnedBlobs();
            locked = false;

            // Small delay to ensure all animations/transitions are complete
            yield return null;

            // Ensure level is active before loading next round
            if (!isLevelActive)
            {
                isLevelActive = true;
            }

            // Always try to load next round
            NextRecipeRound();
        }

        private IEnumerator PlayRecipeReaction(OperatorRecipeTicket recipe)
        {
            switch (recipe.Sign)
            {
                case OperatorSign.Add:
                    yield return PlayAddReaction(recipe.Amount);
                    break;
                case OperatorSign.Subtract:
                    yield return PlaySubtractReaction(recipe.Amount);
                    break;
                case OperatorSign.Multiply:
                    yield return PlayMultiplyReaction(recipe.Amount);
                    break;
                case OperatorSign.Divide:
                    yield return PlayDivideReaction(recipe.Amount);
                    break;
                default:
                    yield return null;
                    break;
            }
        }

        private IEnumerator PlayAddReaction(int amount)
        {
            yield return SpawnOrToggle(amount, addGhostSlots);
            yield return PlayAnimatorIfPresent(clonerAnimator, clonerSpinTrigger);
            AnimateGhostSlots(addGhostSlots);

            // Toggle Reactor objects on correct Add answer: liquidHalf OFF, liquidFill ON
            if (liquidHalf != null) liquidHalf.SetActive(false);
            if (liquidFill != null) liquidFill.SetActive(true);
            ResolveWaterFlowingIfNeeded();
            if (waterFlowing != null) waterFlowing.SetActive(true);
        }

        private IEnumerator PlaySubtractReaction(int amount)
        {
            yield return RemoveOrToggle(amount, subtractSlots);
            yield return PlayAnimatorIfPresent(squeezerAnimator, squeezerTrigger);
            ResolveWaterFlowingIfNeeded();
            if (waterFlowing != null) waterFlowing.SetActive(false);
            AnimateGhostSlots(subtractSlots);

            // Toggle Reactor objects on correct Subtract answer: liquidFill OFF, liquidHalf ON
            if (liquidFill != null) liquidFill.SetActive(false);
            if (liquidHalf != null) liquidHalf.SetActive(true);
        }

        private IEnumerator PlayMultiplyReaction(int amount)
        {
            yield return SpawnOrToggle(amount, multiplySlots);
            yield return PlayAnimatorIfPresent(clonerAnimator, clonerSpinTrigger);
            ToggleMultiplyJellyToClone();
            AnimateGhostSlots(multiplySlots);

            // Handle Pizza_Cloner special logic
            ShowClonerPizzas(amount);
        }

        private IEnumerator PlayDivideReaction(int amount)
        {
            yield return SpawnOrToggle(amount, divideGhostSlots);
            yield return PlayAnimatorIfPresent(splitterAnimator, splitterTrigger);
            ToggleDivideBlobToSliced();
            AnimateGhostSlots(divideGhostSlots);

            // Handle Pizza_Splitter special logic
            ShowSplitPizzas(amount);
        }

        private void PrepareStackerForQuestion(OperatorSign sign)
        {
            if (!IsStackerSign(sign)) return;

            ResolvePizzaStacker();

            // Ensure Pizza_Stacker root is active
            if (pizzaStackerRoot != null)
            {
                pizzaStackerRoot.gameObject.SetActive(true);
            }

            if (sign == OperatorSign.Add)
            {
                // For Add: Disable Half game object and all 4 children (all pizzas invisible)
                if (pizzaStackerHalfRoot != null)
                {
                    pizzaStackerHalfRoot.gameObject.SetActive(false);
                }
                // Also disable all individual pizzas
                SetActiveCount(pizzaHalfPizzas, 0);
                // Clear Full pizzas
                SetActiveCount(pizzaFullPizzas, 0);
            }
            else // Subtract
            {
                // For Subtract: Enable Full game object and all 4 children (all pizzas visible)
                if (pizzaStackerFullRoot != null)
                {
                    pizzaStackerFullRoot.gameObject.SetActive(true);
                }
                // Enable all 4 pizzas under Full
                if (pizzaFullPizzas != null && pizzaFullPizzas.Count > 0)
                {
                    for (int i = 0; i < pizzaFullPizzas.Count; i++)
                    {
                        var pizza = pizzaFullPizzas[i];
                        if (pizza != null)
                        {
                            pizza.SetActive(true);
                        }
                    }
                }
                // Clear Half pizzas
                SetActiveCount(pizzaHalfPizzas, 0);
            }
        }

        private void ShowStackerPizzas(OperatorSign sign, int amount)
        {
            if (!IsStackerSign(sign)) return;

            ResolvePizzaStacker();

            amount = Mathf.Clamp(amount, 1, 4);

            // Ensure Pizza_Stacker root is active
            if (pizzaStackerRoot != null)
            {
                pizzaStackerRoot.gameObject.SetActive(true);
            }

            // + N: Enable first N pizzas under Pizza_Stacker/Half after correct answer
            if (sign == OperatorSign.Add)
            {
                // Enable Half game object first
                if (pizzaStackerHalfRoot != null)
                {
                    pizzaStackerHalfRoot.gameObject.SetActive(true);
                }

                // Enable first N pizzas under Half (1_pizza, 2_pizza, ... N_pizza)
                if (pizzaHalfPizzas != null && pizzaHalfPizzas.Count > 0)
                {
                    for (int i = 0; i < pizzaHalfPizzas.Count; i++)
                    {
                        var pizza = pizzaHalfPizzas[i];
                        if (pizza != null)
                        {
                            // Enable pizzas 0 through (amount-1), which corresponds to 1_pizza through N_pizza
                            pizza.SetActive(i < amount);
                        }
                    }
                }
                // Clear Full pizzas
                SetActiveCount(pizzaFullPizzas, 0);
            }
            else // Subtract: - N: Disable first N pizzas under Pizza_Stacker/Full after correct answer
            {
                // Full should already be enabled with all pizzas from PrepareStackerForQuestion
                // Now disable first N pizzas (1_pizza, 2_pizza, ... N_pizza)
                if (pizzaFullPizzas != null && pizzaFullPizzas.Count > 0)
                {
                    for (int i = 0; i < amount && i < pizzaFullPizzas.Count; i++)
                    {
                        var pizza = pizzaFullPizzas[i];
                        if (pizza != null)
                        {
                            pizza.SetActive(false);
                        }
                    }
                }
                // Clear Half pizzas
                SetActiveCount(pizzaHalfPizzas, 0);
            }
        }

        private void ClearStackerPizzas()
        {
            ResolvePizzaStacker();
            SetActiveCount(pizzaHalfPizzas, 0);
            SetActiveCount(pizzaFullPizzas, 0);
        }

        private void SetActiveCount(List<GameObject> list, int count)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var go = list[i];
                if (go != null)
                {
                    go.SetActive(i < count);
                }
            }
        }

        private void SetAllActive(List<GameObject> list, bool active)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var go = list[i];
                if (go != null)
                {
                    go.SetActive(active);
                }
            }
        }

        private void DisableCount(List<GameObject> list, int count)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count && i < count; i++)
            {
                var go = list[i];
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
        }

        private void ResolvePizzaStacker()
        {
            // If already wired in inspector, keep those.
            if (pizzaStackerRoot == null)
            {
                var root = GetGameplayRoot();
                var go = ResolveByName(root, null, "Pizza_Stacker");
                if (go != null) pizzaStackerRoot = go.transform;
            }

            if (pizzaStackerRoot != null)
            {
                if (pizzaStackerFullRoot == null)
                {
                    pizzaStackerFullRoot = pizzaStackerRoot.Find("Full");
                }
                if (pizzaStackerHalfRoot == null)
                {
                    pizzaStackerHalfRoot = pizzaStackerRoot.Find("Half");
                }
            }

            // Populate Half list (for + operator)
            if (pizzaHalfPizzas.Count == 0 && pizzaStackerHalfRoot != null)
            {
                pizzaHalfPizzas.Clear();
                for (int i = 1; i <= 4; i++)
                {
                    var child = pizzaStackerHalfRoot.Find($"{i}_pizza");
                    if (child != null)
                    {
                        pizzaHalfPizzas.Add(child.gameObject);
                    }
                    else
                    {
                        // Try recursive search as fallback
                        child = FindChildRecursive(pizzaStackerHalfRoot, $"{i}_pizza");
                        if (child != null)
                        {
                            pizzaHalfPizzas.Add(child.gameObject);
                        }
                    }
                }
            }

            // Populate Full list (for - operator)
            if (pizzaFullPizzas.Count == 0 && pizzaStackerFullRoot != null)
            {
                pizzaFullPizzas.Clear();
                for (int i = 1; i <= 4; i++)
                {
                    var child = pizzaStackerFullRoot.Find($"{i}_pizza");
                    if (child != null)
                    {
                        pizzaFullPizzas.Add(child.gameObject);
                    }
                    else
                    {
                        // Try recursive search as fallback
                        child = FindChildRecursive(pizzaStackerFullRoot, $"{i}_pizza");
                        if (child != null)
                        {
                            pizzaFullPizzas.Add(child.gameObject);
                        }
                    }
                }
            }
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent == null) return null;

            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }
                var found = FindChildRecursive(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static bool IsStackerSign(OperatorSign sign)
        {
            return sign == OperatorSign.Add || sign == OperatorSign.Subtract;
        }

        private IEnumerator PlayAnimatorIfPresent(Animator animator, string triggerName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(triggerName))
            {
                yield break;
            }

            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);
            yield return null;
        }

        private IEnumerator SpawnOrToggle(int amount, List<GameObject> slots)
        {
            amount = Mathf.Max(1, amount);

            if (slots != null && slots.Count > 0)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i];
                    if (slot != null) slot.SetActive(i < amount);
                }
                yield break;
            }

            if (jellySpawnRoot != null && jellyBlobPrefab != null)
            {
                ClearSpawnedBlobs();
                for (int i = 0; i < amount; i++)
                {
                    var pos = jellySpawnRoot.position + new Vector3(
                        UnityEngine.Random.Range(-jellySpawnScatter.x, jellySpawnScatter.x),
                        UnityEngine.Random.Range(0f, jellySpawnScatter.y),
                        0f);
                    var blob = Instantiate(jellyBlobPrefab, pos, Quaternion.identity, jellySpawnRoot);
                    spawnedBlobs.Add(blob);
                }
            }

            yield return null;
        }

        private IEnumerator RemoveOrToggle(int amount, List<GameObject> slots)
        {
            amount = Mathf.Max(1, amount);

            if (slots != null && slots.Count > 0)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i];
                    if (slot != null) slot.SetActive(i >= amount);
                }
                yield break;
            }

            if (spawnedBlobs.Count > 0)
            {
                for (int i = 0; i < amount && spawnedBlobs.Count > 0; i++)
                {
                    var blob = spawnedBlobs[spawnedBlobs.Count - 1];
                    spawnedBlobs.RemoveAt(spawnedBlobs.Count - 1);
                    if (blob != null)
                    {
                        Destroy(blob);
                    }
                }
            }

            yield return null;
        }

        private void ClearSpawnedBlobs()
        {
            for (int i = 0; i < spawnedBlobs.Count; i++)
            {
                var blob = spawnedBlobs[i];
                if (blob != null)
                {
                    Destroy(blob);
                }
            }
            spawnedBlobs.Clear();
        }

        private void AnimateGhostSlots(IEnumerable<GameObject> slots)
        {
            if (ghostSlotAnimator == null) return;
            ghostSlotAnimator.ResetPostClip();
            ghostSlotAnimator.AnimateSlots(slots);
        }

        private IEnumerator PlayToolFeedback(OperatorSign sign, bool waitForAudio = true, bool skipIfAlreadyShown = false)
        {
            if (skipIfAlreadyShown && toolFeedbackStartedThisRound) yield break;
            toolFeedbackStartedThisRound = true;

            // Map operators to tools: + and − => Stacker, × => Cloner, ÷ => Splitter.
            string message = null;
            AudioClip clip = null;

            switch (sign)
            {
                case OperatorSign.Add:
                    message = stackerAddMessage;
                    clip = stackerAddClip != null ? stackerAddClip : stackerClip;
                    break;
                case OperatorSign.Subtract:
                    message = stackerSubtractMessage;
                    clip = stackerClip;
                    break;
                case OperatorSign.Multiply:
                    message = clonerMessage;
                    clip = clonerClip;
                    break;
                case OperatorSign.Divide:
                    message = splitterMessage;
                    clip = splitterClip;
                    break;
            }

            ShowOperatorMessage(message);

            if (waitForAudio && toolMessageDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(toolMessageDelaySeconds);
            }

            if (toolAudioSource != null && clip != null)
            {
                toolAudioSource.Stop();
                toolAudioSource.clip = clip;
                toolAudioSource.Play();
                if (waitForAudio)
                {
                    yield return new WaitForSeconds(clip.length);
                }
            }
        }

        private void ShowOperatorMessage(string message)
        {
            EnsureOperatorMessageRef();

            if (operatorMessageText == null || string.IsNullOrEmpty(message)) return;

            EnsureMsgPanelVisible();
            operatorMessageText.gameObject.SetActive(true);
            operatorMessageText.enabled = true;

            operatorMessageText.text = message;
        }

        private void EnsureOperatorMessageRef()
        {
            if (operatorMessageText != null) return;

            var panel = msgPanel != null ? msgPanel : GameObject.Find("Msg_Panel");
            if (panel == null) return;

            var tf = panel.transform.Find("Operator_message");
            if (tf != null)
            {
                operatorMessageText = tf.GetComponent<TMP_Text>();
            }
        }

        private void EnsureMsgPanelVisible()
        {
            if (msgPanel == null)
            {
                msgPanel = GameObject.Find("Msg_Panel");
            }

            if (msgPanel != null && !msgPanel.activeSelf)
            {
                msgPanel.SetActive(true);
            }

            if (msgPanelImage == null && msgPanel != null)
            {
                msgPanelImage = msgPanel.GetComponent<UnityEngine.UI.Image>();
            }

            if (msgPanelImage != null)
            {
                msgPanelImage.enabled = true;
            }
        }

        private static bool TryParseDigit(string key, out int digit)
        {
            return int.TryParse(key, out digit);
        }

        #endregion

        private void ResetCalculatorDisplay()
        {
            if (calculator == null)
            {
                calculator = FindFirstObjectByType<CalculatorController>();
            }

            if (calculator != null && calculator.Model != null)
            {
                calculator.Model.ResetAll();
            }
        }

        private static string ToDisplaySymbol(OperatorSign sign)
        {
            return sign switch
            {
                OperatorSign.Add => "+",
                OperatorSign.Subtract => "−",
                OperatorSign.Multiply => "×",
                OperatorSign.Divide => "÷",
                _ => "?"
            };
        }

        private static bool TryParseOperator(string key, out OperatorSign op)
        {
            op = OperatorSign.Add;
            switch (key)
            {
                case "+":
                case "＋":
                    op = OperatorSign.Add;
                    return true;
                case "-":
                case "−":
                    op = OperatorSign.Subtract;
                    return true;
                case "*":
                case "×":
                case "x":
                case "X":
                    op = OperatorSign.Multiply;
                    return true;
                case "/":
                case "÷":
                    op = OperatorSign.Divide;
                    return true;
                default:
                    return false;
            }
        }

        private void UpdateOperatorPopupUnderHand(OperatorSign sign)
        {
            if (handBase == null) return;

            // Find OperatorPopup under Hand_base/Hand_rotor/Hand_hook/OperatorPopup/
            Transform handRotor = handBase.transform.Find("Hand_rotor");
            if (handRotor == null)
            {
                // Try recursive search
                handRotor = FindChildRecursive(handBase.transform, "Hand_rotor");
            }

            if (handRotor == null) return;

            Transform handHook = handRotor.Find("Hand_hook");
            if (handHook == null)
            {
                handHook = FindChildRecursive(handRotor, "Hand_hook");
            }

            if (handHook == null) return;

            Transform operatorPopup = handHook.Find("OperatorPopup");
            if (operatorPopup == null)
            {
                operatorPopup = FindChildRecursive(handHook, "OperatorPopup");
            }

            if (operatorPopup == null) return;

            // Get the child name for the operator
            string operatorChildName = sign switch
            {
                OperatorSign.Add => "Plus",
                OperatorSign.Subtract => "Minus",
                OperatorSign.Multiply => "Multiply",
                OperatorSign.Divide => "Divide",
                _ => null
            };

            if (string.IsNullOrEmpty(operatorChildName)) return;

            // Disable all operator children first
            Transform plusChild = operatorPopup.Find("Plus");
            Transform minusChild = operatorPopup.Find("Minus");
            Transform multiplyChild = operatorPopup.Find("Multiply");
            Transform divideChild = operatorPopup.Find("Divide");

            // Also try recursive search for children
            if (plusChild == null) plusChild = FindChildRecursive(operatorPopup, "Plus");
            if (minusChild == null) minusChild = FindChildRecursive(operatorPopup, "Minus");
            if (multiplyChild == null) multiplyChild = FindChildRecursive(operatorPopup, "Multiply");
            if (divideChild == null) divideChild = FindChildRecursive(operatorPopup, "Divide");

            // Disable all
            if (plusChild != null) plusChild.gameObject.SetActive(false);
            if (minusChild != null) minusChild.gameObject.SetActive(false);
            if (multiplyChild != null) multiplyChild.gameObject.SetActive(false);
            if (divideChild != null) divideChild.gameObject.SetActive(false);

            // Enable the correct one
            Transform targetChild = operatorPopup.Find(operatorChildName);
            if (targetChild == null)
            {
                targetChild = FindChildRecursive(operatorPopup, operatorChildName);
            }

            if (targetChild != null)
            {
                targetChild.gameObject.SetActive(true);
            }
        }
    }
}


