using UnityEngine;
using System.Collections.Generic;

public class AdvancedDragAndDropManager : MonoBehaviour
{
    [Header("Panels")]
    public Transform dropZonesPanel;
    public Transform draggablePartsPanel;

    [Header("Prefabs")]
    public GameObject dropZonePrefab;
    public GameObject draggablePartPrefab;

    [Header("Coin/Part Data")]
    public List<string> coinTypes = new List<string> { "Penny", "Dime", "Nickel" };
    public int partsPerCoin = 3;

    private List<GameObject> spawnedDropZones = new List<GameObject>();
    private List<GameObject> spawnedDraggables = new List<GameObject>();

    void Start()
    {
        SpawnAll();
    }

    public void SpawnAll()
    {
        ClearAll();

        foreach (var coinType in coinTypes)
        {
            for (int i = 0; i < partsPerCoin; i++)
            {
                string partID = $"{coinType}_Part_{i}";

                // Spawn DropZone
                GameObject dz = Instantiate(dropZonePrefab, dropZonesPanel);
                var dzScript = dz.GetComponent<DropZone>();
                if (dzScript == null)
                {
                    Debug.LogError("DropZone prefab does not have a DropZone component attached! Skipping this drop zone.");
                    continue;
                }
                dzScript.expectedPartID = partID;
                dzScript.expectedCoinGroupID = coinType;
                spawnedDropZones.Add(dz);

                // Spawn DraggablePart
                GameObject dp = Instantiate(draggablePartPrefab, draggablePartsPanel);
                var dpScript = dp.GetComponent<DraggablePart>();
                if (dpScript == null)
                {
                    Debug.LogError("DraggablePart prefab does not have a DraggablePart component attached! Skipping this draggable part.");
                    Destroy(dp);
                    continue;
                }
                dpScript.partID = partID;
                dpScript.coinGroupID = coinType;
                spawnedDraggables.Add(dp);

                // Optionally randomize position within panel (for tray)
                var rect = dp.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition += new Vector2(Random.Range(-100, 100), Random.Range(-50, 50));
                }
            }
        }
    }

    public void ClearAll()
    {
        foreach (var go in spawnedDropZones)
            if (go) Destroy(go);
        foreach (var go in spawnedDraggables)
            if (go) Destroy(go);
        spawnedDropZones.Clear();
        spawnedDraggables.Clear();
    }

    public void ResetGame()
    {
        SpawnAll();
    }
} 