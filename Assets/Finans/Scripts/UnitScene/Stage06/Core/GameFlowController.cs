using System.Linq;
using UnityEngine;

namespace Game.Core
{
    public sealed class GameFlowController : MonoBehaviour
    {
        [Header("Calculator Input")]
        [Tooltip("Direct reference to CalculatorInputRouter (if router is separate). Leave empty to search in Calculator Prefab.")]
        [SerializeField] private CalculatorInputRouter calculatorRouter;
        [Tooltip("Calculator GameObject/Prefab (if router is a child of calculator). Will search for router in children.")]
        [SerializeField] private GameObject calculatorPrefab;
        
        [Header("Category")]
        [SerializeField] private MonoBehaviour categoryBehaviour; // implements ICategoryController

        private ICategoryController category;
        private CalculatorInputRouter router;
        private bool gameStarted = false;

        private void Awake()
        {
            category = categoryBehaviour as ICategoryController ?? ResolveCategory();
            if (category == null)
            {
                Debug.LogError("Category behaviour must implement ICategoryController. Assign a component that implements ICategoryController.");
                enabled = false;
                return;
            }
            
            // Cache the resolved behaviour back to the serialized field for clarity in the inspector
            if (categoryBehaviour == null)
            {
                categoryBehaviour = category as MonoBehaviour;
            }
            
            // Find router: prefer direct reference, then search in calculator prefab, then try to find in scene
            router = calculatorRouter;
            if (router == null && calculatorPrefab != null)
            {
                router = calculatorPrefab.GetComponentInChildren<CalculatorInputRouter>(true);
            }
            if (router == null)
            {
                router = FindObjectOfType<CalculatorInputRouter>();
            }
            
            if (router == null)
            {
                Debug.LogError("CalculatorInputRouter not found. Assign either 'Calculator Router' directly or 'Calculator Prefab' (with router as child).");
                enabled = false;
                return;
            }
            
            category.Initialize(router);
        }

        private void Start()
        {
            // Wait for tap to start button instead of auto-starting
            // Try to subscribe to UI manager's game start event
            // Use a coroutine to check for UI manager in case it initializes after this script
            StartCoroutine(WaitForUIManagerAndSubscribe());
        }

        private System.Collections.IEnumerator WaitForUIManagerAndSubscribe()
        {
            // Wait up to 2 seconds for UI manager to be available
            float timeout = 2f;
            float elapsed = 0f;
            
            while (MiniGameUIManager.Instance == null && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (MiniGameUIManager.Instance != null)
            {
                MiniGameUIManager.Instance.onGameStartRequested.AddListener(StartGame);
                Debug.Log("GameFlowController: Subscribed to tap to start button event.");
            }
            else
            {
                // Fallback: if no UI manager found, start after a short delay
                Debug.LogWarning("GameFlowController: MiniGameUIManager not found. Starting game after 1 second delay.");
                Invoke(nameof(StartGame), 1f);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from event to prevent memory leaks
            if (MiniGameUIManager.Instance != null)
            {
                MiniGameUIManager.Instance.onGameStartRequested.RemoveListener(StartGame);
            }
        }

        private void StartGame()
        {
            if (enabled && !gameStarted)
            {
                gameStarted = true;
                category.StartCategory();
            }
        }

        private ICategoryController ResolveCategory()
        {
            // Try current GameObject first, then children (including inactive), then search scene
            var found = GetComponent<ICategoryController>();
            if (found != null) return found;

            found = GetComponentInChildren<ICategoryController>(true);
            if (found != null) return found;

            return FindObjectsOfType<MonoBehaviour>(true).OfType<ICategoryController>().FirstOrDefault();
        }
    }
}


