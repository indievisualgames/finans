using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace Finans.ActivitySystem
{
    /// <summary>
    /// Listens for OnAllActivitiesLoaded, then instantiates cards
    /// and calls BindFullContent with the ActivityContent directly.
    /// </summary>
    public class ActivityHomeController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform activityCardsContainer;
        [SerializeField] private GameObject activityCardPrefab;

        private List<ActivityCardBinder> instantiatedCards = new List<ActivityCardBinder>();
        private bool isSubscribed = false;

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            // Fallback in case ActivityManager wasn't ready during OnEnable
            if (!isSubscribed)
            {
                TrySubscribe();
            }

            // Diagnostic check for assigned references
            if (activityCardsContainer == null)
                Debug.LogError($"[ActivityHomeController] activityCardsContainer is NOT assigned on {gameObject.name}!", this);
            
            if (activityCardPrefab == null)
                Debug.LogError($"[ActivityHomeController] activityCardPrefab is NOT assigned on {gameObject.name}!", this);
        }

        private void TrySubscribe()
        {
            if (ActivityManager.Instance != null && !isSubscribed)
            {
                ActivityManager.Instance.OnAllActivitiesLoaded += OnAllActivitiesLoaded;
                isSubscribed = true;
                Debug.Log("[ActivityHomeController] Successfully subscribed to ActivityManager.");

                // If already loaded (e.g., scene re-entry), trigger immediately
                if (ActivityManager.Instance.IsLoaded)
                {
                    OnAllActivitiesLoaded();
                }
            }
        }

        private void OnDisable()
        {
            if (ActivityManager.Instance != null && isSubscribed)
            {
                ActivityManager.Instance.OnAllActivitiesLoaded -= OnAllActivitiesLoaded;
                isSubscribed = false;
            }

            foreach (var card in instantiatedCards)
                if (card != null) card.OnCardTapped -= OnCardTapped;
        }

        private void OnAllActivitiesLoaded()
        {
            if (ActivityManager.Instance == null) return;

            List<ActivityContent> activities = ActivityManager.Instance.GetAllActivities();
            Debug.Log($"[ActivityHomeController] OnAllActivitiesLoaded triggered. Creating {activities.Count} cards...");

            if (activityCardsContainer == null || activityCardPrefab == null)
            {
                Debug.LogError("[ActivityHomeController] Cannot create cards: Prefab or Container is NULL.");
                return;
            }

            ClearCards();

            foreach (var content in activities)
            {
                GameObject cardObj = Instantiate(activityCardPrefab, activityCardsContainer);
                ActivityCardBinder binder = cardObj.GetComponent<ActivityCardBinder>();

                if (binder == null)
                {
                    Debug.LogWarning($"[ActivityHomeController] Prefab {activityCardPrefab.name} is missing ActivityCardBinder. Adding one.");
                    binder = cardObj.AddComponent<ActivityCardBinder>();
                }

                binder.BindFullContent(content);
                binder.OnCardTapped += OnCardTapped;
                instantiatedCards.Add(binder);
            }
            
            Debug.Log($"[ActivityHomeController] Successfully instantiated {instantiatedCards.Count} cards.");
            
            // Proactively fix ScrollRect settings in case they were set incorrectly in the editor
            FixScrollRectSettings();

            // Force layout rebuild so ScrollRect knows the new content size
            StartCoroutine(RefreshLayoutRoutine());
        }

        private void FixScrollRectSettings()
        {
            ScrollRect sr = activityCardsContainer?.GetComponentInParent<ScrollRect>();
            if (sr == null) sr = GetComponentInParent<ScrollRect>();

            if (sr != null)
            {
                sr.horizontal = false; // Vertical list only
                sr.vertical = true;
                if (sr.scrollSensitivity < 10) sr.scrollSensitivity = 25f; // Faster scroll
                sr.movementType = ScrollRect.MovementType.Elastic;
                Debug.Log($"[ActivityHomeController] Optimized ScrollRect: Sensitivity={sr.scrollSensitivity}, Horizontal=False");
            }
        }

        private IEnumerator RefreshLayoutRoutine()
        {
            // Wait TWO frames to ensure all nested layouts (Prefab -> Content) ripple up
            yield return null; 
            yield return new WaitForEndOfFrame();
            
            if (activityCardsContainer != null)
            {
                RectTransform rect = activityCardsContainer.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // Rebuild the container and the viewport
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                    
                    ScrollRect sr = rect.GetComponentInParent<ScrollRect>();
                    if (sr != null)
                    {
                        if (sr.viewport != null) LayoutRebuilder.ForceRebuildLayoutImmediate(sr.viewport);
                        
                        // Debug current heights to diagnose why it might not be scrolling
                        Debug.Log($"[ActivityHomeController] SCROLL DIAGNOSTIC:");
                        Debug.Log($" - Content Height: {rect.rect.height}");
                        Debug.Log($" - Viewport Height: {sr.viewport.rect.height}");
                        Debug.Log($" - Is Scrollable: {rect.rect.height > sr.viewport.rect.height}");
                        
                        // If it's still not scrolling but should be, it might be a Raycast issue
                        if (rect.GetComponent<Image>() == null)
                        {
                            var img = rect.gameObject.AddComponent<Image>();
                            img.color = new Color(0, 0, 0, 0); // Transparent but raycast target
                            Debug.Log("[ActivityHomeController] Added transparent Image to Content to ensure it catches drag events.");
                        }
                    }
                    
                    Debug.Log("[ActivityHomeController] Layout rebuild forced for container and viewport.");
                }
            }
        }

        private void OnCardTapped(string activityId)
        {
            Debug.Log($"[ActivityHomeController] Tapped: {activityId}");
            ActivityManager.Instance?.SetCurrentActivity(activityId);
        }

        private void ClearCards()
        {
            foreach (var card in instantiatedCards)
                if (card != null) card.OnCardTapped -= OnCardTapped;

            instantiatedCards.Clear();

            if (activityCardsContainer != null)
            {
                foreach (Transform child in activityCardsContainer)
                    Destroy(child.gameObject);
            }
        }
    }
}
