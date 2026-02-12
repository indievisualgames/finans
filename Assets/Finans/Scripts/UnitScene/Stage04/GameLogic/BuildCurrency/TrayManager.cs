using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TrayManager : MonoBehaviour
{
    public static TrayManager Instance;
    public Transform trayPanelParent; // Assign the Tray Panel parent in Inspector
    public GameObject[] allTrayPartPrefabs; // Assign all possible tray part prefabs in Inspector
    [Header("Decoy Settings")]
    [SerializeField, Range(0, 10)]
    public int minDecoys = 2;
    [SerializeField, Range(0, 10)]
    public int maxDecoys = 4;
    // Note: Inspector values override script defaults. If you change these defaults in code, reset the component in the Inspector to update.

    void Awake() { Instance = this; }

    private void EnableAllChildrenAndComponents(GameObject go)
    {
        go.SetActive(true);

        // Enable all components that have an enabled property
        foreach (var comp in go.GetComponents<Component>())
        {
            var type = comp.GetType();
            var enabledProp = type.GetProperty("enabled");
            if (enabledProp != null && enabledProp.PropertyType == typeof(bool))
            {
                enabledProp.SetValue(comp, true, null);
            }
        }

        // Ensure CanvasGroup is properly configured
        var canvasGroup = go.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        // Ensure Image components are enabled
        var image = go.GetComponent<Image>();
        if (image != null)
        {
            image.enabled = true;
        }

        // Ensure SpriteRenderer components are enabled
        var spriteRenderer = go.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Recursively enable all children
        foreach (Transform child in go.transform)
        {
            EnableAllChildrenAndComponents(child.gameObject);
        }
    }

    // Call this method with the required part prefabs for the current coin
    public void SetupTray(GameObject[] requiredParts)
    {
        Debug.Log($"SetupTray called. allTrayPartPrefabs count: {allTrayPartPrefabs?.Length ?? 0}");
        Debug.Log($"SetupTray requiredParts count: {requiredParts?.Length ?? 0}");
        Debug.Log($"Decoy sliders -> min: {minDecoys}, max: {maxDecoys}");

        if (trayPanelParent == null)
        {
            Debug.LogError("TrayManager: trayPanelParent is null! Cannot setup tray.");
            return;
        }

        // Get the current prefix from MasterCoinGameManager
        string currentPrefix = MasterCoinGameManager.Instance != null ? MasterCoinGameManager.Instance.GetCurrentPrefix() : string.Empty;
        // Get all session prefixes from MasterCoinGameManager
        var usedPrefixes = MasterCoinGameManager.Instance != null ? MasterCoinGameManager.Instance.GetAllSessionPrefixes() : new System.Collections.Generic.List<string>();

        // Collect all tray children once
        var allChildren = new List<Transform>();
        foreach (Transform child in trayPanelParent)
        {
            allChildren.Add(child);
        }

        // Identify required parts (current coin parts) by prefix
        var requiredChildren = new HashSet<GameObject>();
        foreach (var child in allChildren)
        {
            if (child.gameObject.name.StartsWith(currentPrefix))
            {
                requiredChildren.Add(child.gameObject);
            }
        }

        // Build decoy candidate list from tray children, excluding required and any used session prefixes
        var candidateChildren = new List<Transform>();
        foreach (var child in allChildren)
        {
            var name = child.gameObject.name;
            if (requiredChildren.Contains(child.gameObject))
                continue;
            bool isAllowed = true;
            foreach (var prefix in usedPrefixes)
            {
                if (!string.IsNullOrEmpty(prefix) && name.StartsWith(prefix))
                {
                    isAllowed = false;
                    break;
                }
            }
            if (isAllowed)
                candidateChildren.Add(child);
        }

        // Fallback: if not enough candidates, relax filter to any non-required child
        if (candidateChildren.Count < minDecoys)
        {
            candidateChildren.Clear();
            foreach (var child in allChildren)
            {
                if (!child.gameObject.name.StartsWith(currentPrefix))
                {
                    candidateChildren.Add(child);
                }
            }
            Debug.Log($"Relaxed decoy candidate filter. Candidate count now: {candidateChildren.Count}");
        }

        int clampedMin = Mathf.Clamp(minDecoys, 0, candidateChildren.Count);
        int clampedMax = Mathf.Clamp(maxDecoys, clampedMin, candidateChildren.Count);

        // Choose a decoy count within [min, max]
        int decoyCount = UnityEngine.Random.Range(clampedMin, clampedMax + 1);
        Debug.Log($"Decoy selection -> candidates: {candidateChildren.Count}, chosen count: {decoyCount}");

        // Randomly pick decoy children
        var chosenDecoyObjects = new HashSet<GameObject>();
        for (int i = 0; i < decoyCount && candidateChildren.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, candidateChildren.Count);
            chosenDecoyObjects.Add(candidateChildren[idx].gameObject);
            candidateChildren.RemoveAt(idx);
        }

        // Enable/disable trayPanelParent children accordingly
        foreach (Transform child in trayPanelParent)
        {
            if (requiredChildren.Contains(child.gameObject) || chosenDecoyObjects.Contains(child.gameObject))
            {
                EnableAllChildrenAndComponents(child.gameObject);

                // Ensure proper positioning and scale
                child.localPosition = Vector3.zero;
                child.localScale = Vector3.one;

                // Ensure proper sorting order for UI elements
                var canvas = child.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 1;
                }

                //       Debug.Log($"Tray part enabled: {child.gameObject.name}, isDecoy={(chosenDecoyObjects.Contains(child.gameObject) ? "Yes" : "No")}");
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    // Method to return an object to the tray with proper positioning
    public void ReturnObjectToTray(GameObject obj)
    {
        if (trayPanelParent == null || obj == null) return;

        obj.transform.SetParent(trayPanelParent, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;

        // Ensure visibility
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        // Enable renderers
        var image = obj.GetComponent<Image>();
        if (image != null) image.enabled = true;

        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        obj.SetActive(true);
    }
}