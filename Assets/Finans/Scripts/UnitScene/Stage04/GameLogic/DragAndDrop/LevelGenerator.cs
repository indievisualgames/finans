using UnityEngine;
using UnityEngine.UI; // Needed for UI
using System.Collections.Generic;

/// <summary>
/// LevelGenerator randomly spawns coins, notes, and slots in the UI.
/// Attach this script to an empty GameObject in your scene.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    public GameObject[] coinPrefabs; // Drag your coin prefabs here in Unity
    public GameObject[] notePrefabs; // Drag your note prefabs here in Unity
    public GameObject[] coinSlotPrefabs;   // Array of different slot prefabs for coins
    public GameObject[] noteSlotPrefabs;   // Array of different slot prefabs for notes

    [Header("UI Panels")]
    public Transform coinSlotsPanel;    // Panel for coin slots
    public Transform noteSlotsPanel;    // Panel for note slots
    public Transform coinObjectsPanel;  // Panel for draggable coins
    public Transform noteObjectsPanel;  // Panel for draggable notes

    [Header("Spawn Settings")]
    public int numberOfCoins = 5;
    public int numberOfNotes = 3;
    public int numberOfSlots = 8;
    public Vector2 scaleRange = new Vector2(0.7f, 1.3f); // Min and max scale

    private List<GameObject> spawnedCoins = new List<GameObject>();
    private List<GameObject> spawnedNotes = new List<GameObject>();
    private Dictionary<string, GameObject> typeToCoinSlotPrefab = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> typeToNoteSlotPrefab = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> typeToCoinPrefab = new Dictionary<string, GameObject>();

    void Start()
    {
        // Validate spawn settings
        ValidateSpawnSettings();
        
        InitializeDictionaries();
        ClearExistingObjects();
        SpawnRandomObjects(coinPrefabs, numberOfCoins);
        SpawnRandomObjects(notePrefabs, numberOfNotes);
        SpawnSlots();
    }

    private void ValidateSpawnSettings()
    {
        // Ensure we have at least one slot
        if (numberOfSlots < 1)
        {
            Debug.LogWarning("Number of slots must be at least 1. Setting to 1.");
            numberOfSlots = 1;
        }

        // Ensure we have at least one coin or note
        if (numberOfCoins < 1 && numberOfNotes < 1)
        {
            Debug.LogWarning("Must have at least one coin or note. Setting coins to 1.");
            numberOfCoins = 1;
            numberOfNotes = 0;
        }

        // Ensure total objects don't exceed number of slots
        int totalObjects = numberOfCoins + numberOfNotes;
        if (totalObjects > numberOfSlots)
        {
            Debug.LogWarning($"Total objects ({totalObjects}) exceeds number of slots ({numberOfSlots}). Adjusting objects to match slots.");
            // Distribute slots proportionally between coins and notes
            if (numberOfCoins > 0 && numberOfNotes > 0)
            {
                float coinRatio = (float)numberOfCoins / totalObjects;
                numberOfCoins = Mathf.RoundToInt(numberOfSlots * coinRatio);
                numberOfNotes = numberOfSlots - numberOfCoins;
            }
            else if (numberOfCoins > 0)
            {
                numberOfCoins = numberOfSlots;
                numberOfNotes = 0;
            }
            else
            {
                numberOfNotes = numberOfSlots;
                numberOfCoins = 0;
            }
        }
    }

    private void InitializeDictionaries()
    {
        // Initialize coin slot prefab dictionary
        typeToCoinSlotPrefab.Clear();
        foreach (var slotPrefab in coinSlotPrefabs)
        {
            var coinSlot = slotPrefab.GetComponent<CoinSlot>();
            if (coinSlot != null)
            {
                typeToCoinSlotPrefab[coinSlot.expectedCoinType] = slotPrefab;
            }
        }

        // Initialize note slot prefab dictionary
        typeToNoteSlotPrefab.Clear();
        foreach (var slotPrefab in noteSlotPrefabs)
        {
            var noteSlot = slotPrefab.GetComponent<NoteSlot>();
            if (noteSlot != null)
            {
                typeToNoteSlotPrefab[noteSlot.expectedNoteType] = slotPrefab;
            }
        }

        // Initialize coin prefab dictionary
        typeToCoinPrefab.Clear();
        foreach (var coinPrefab in coinPrefabs)
        {
            var coinType = coinPrefab.GetComponent<CoinTypeIdentifier>();
            if (coinType != null)
            {
                typeToCoinPrefab[coinType.coinType] = coinPrefab;
            }
        }
    }

    void ClearExistingObjects()
    {
        if (coinObjectsPanel != null)
        {
            foreach (Transform child in coinObjectsPanel)
            {
                Destroy(child.gameObject);
            }
        }

        if (noteObjectsPanel != null)
        {
            foreach (Transform child in noteObjectsPanel)
            {
                Destroy(child.gameObject);
            }
        }

        spawnedCoins.Clear();
        spawnedNotes.Clear();

        if (coinSlotsPanel != null)
        {
            foreach (Transform child in coinSlotsPanel)
            {
                Destroy(child.gameObject);
            }
        }

        if (noteSlotsPanel != null)
        {
            foreach (Transform child in noteSlotsPanel)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private GameObject GetMatchingCoinSlotPrefab(string type)
    {
        if (typeToCoinSlotPrefab.TryGetValue(type, out GameObject prefab))
        {
            return prefab;
        }
        Debug.LogWarning($"No matching coin slot prefab found for type: {type}. Using default coin slot.");
        return coinSlotPrefabs.Length > 0 ? coinSlotPrefabs[0] : null;
    }

    private GameObject GetMatchingNoteSlotPrefab(string type)
    {
        if (typeToNoteSlotPrefab.TryGetValue(type, out GameObject prefab))
        {
            return prefab;
        }
        Debug.LogWarning($"No matching note slot prefab found for type: {type}. Using default note slot.");
        return noteSlotPrefabs.Length > 0 ? noteSlotPrefabs[0] : null;
    }

    void SpawnSlots()
    {
        if (coinSlotsPanel == null || noteSlotsPanel == null) return;

        // Create slots for coins
        for (int i = 0; i < spawnedCoins.Count; i++)
        {
            var coinType = spawnedCoins[i].GetComponent<CoinTypeIdentifier>();
            if (coinType != null)
            {
                GameObject slotPrefab = GetMatchingCoinSlotPrefab(coinType.coinType);
                if (slotPrefab != null)
                {
                    GameObject slot = Instantiate(slotPrefab, coinSlotsPanel);
                    slot.name = "Slot_Coin_" + coinType.coinType;
                    
                    var coinSlot = slot.GetComponent<CoinSlot>();
                    if (coinSlot == null)
                    {
                        coinSlot = slot.AddComponent<CoinSlot>();
                    }
                    coinSlot.expectedCoinType = coinType.coinType;
                    
                    slot.transform.localScale = Vector3.one;
                }
            }
        }

        // Create slots for notes
        for (int i = 0; i < spawnedNotes.Count; i++)
        {
            var noteType = spawnedNotes[i].GetComponent<NoteTypeIdentifier>();
            if (noteType != null)
            {
                GameObject slotPrefab = GetMatchingNoteSlotPrefab(noteType.noteType);
                if (slotPrefab != null)
                {
                    GameObject slot = Instantiate(slotPrefab, noteSlotsPanel);
                    slot.name = "Slot_Note_" + noteType.noteType;
                    
                    var noteSlot = slot.GetComponent<NoteSlot>();
                    if (noteSlot == null)
                    {
                        noteSlot = slot.AddComponent<NoteSlot>();
                    }
                    noteSlot.expectedNoteType = noteType.noteType;
                    
                    slot.transform.localScale = Vector3.one;
                }
            }
        }
    }

    void SpawnRandomObjects(GameObject[] prefabs, int count)
    {
        if (prefabs.Length == 0) return;

        // For coins, ensure we have a good distribution of types
        if (prefabs == coinPrefabs)
        {
            if (coinObjectsPanel == null) return;

            // First, spawn one of each coin type
            List<string> availableTypes = new List<string>(typeToCoinPrefab.Keys);
            int typesToSpawn = Mathf.Min(availableTypes.Count, count);
            
            for (int i = 0; i < typesToSpawn; i++)
            {
                string type = availableTypes[i];
                if (typeToCoinPrefab.TryGetValue(type, out GameObject prefab))
                {
                    GameObject obj = Instantiate(prefab, coinObjectsPanel);
                    float scale = Random.Range(scaleRange.x, scaleRange.y);
                    obj.transform.localScale = new Vector3(scale, scale, 1);
                    spawnedCoins.Add(obj);
                }
            }

            // Then, if we need more coins, spawn them randomly
            int remainingCoins = count - typesToSpawn;
            if (remainingCoins > 0)
            {
                for (int i = 0; i < remainingCoins; i++)
                {
                    string selectedType = availableTypes[Random.Range(0, availableTypes.Count)];
                    if (typeToCoinPrefab.TryGetValue(selectedType, out GameObject prefab))
                    {
                        GameObject obj = Instantiate(prefab, coinObjectsPanel);
                        float scale = Random.Range(scaleRange.x, scaleRange.y);
                        obj.transform.localScale = new Vector3(scale, scale, 1);
                        spawnedCoins.Add(obj);
                    }
                }
            }
        }
        else
        {
            if (noteObjectsPanel == null) return;

            // For notes, use random selection
            for (int i = 0; i < count; i++)
            {
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject obj = Instantiate(prefab, noteObjectsPanel);
                float scale = Random.Range(scaleRange.x, scaleRange.y);
                obj.transform.localScale = new Vector3(scale, scale, 1);
                spawnedNotes.Add(obj);
            }
        }
    }
} 