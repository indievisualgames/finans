using System.Collections.Generic;
using UnityEngine;

namespace Game.Categories.NumberKeys
{
    public sealed class BubbleSpawner : MonoBehaviour
    {
        [Header("Prefab & Area")]
        [Tooltip("Prefab to spawn for each number bubble.")]
        [SerializeField] private NumberBubble bubblePrefab;
        [Tooltip("World-space rectangle where bubbles can spawn (x,y = min; width,height).")]
        [SerializeField] private Rect spawnArea = new Rect(-4f, -2f, 8f, 4f);

        [Header("Counts")]
        [Tooltip("Base number of distractor bubbles per round (in addition to the target).")]
        [SerializeField] private int baseDistractors = 2;

        [Header("Placement")]
        [Tooltip("Minimum distance between spawned bubbles.")]
        [SerializeField] private float minSeparation = 1.0f; // minimum distance between spawned bubbles
        [Tooltip("Number of attempts per bubble to find a free position before falling back.")]
        [SerializeField] private int placementAttempts = 32;   // attempts per bubble to find a free spot

        // Public properties for runtime tweaking (fields remain serialized for Inspector)
        public NumberBubble BubblePrefab { get => bubblePrefab; set => bubblePrefab = value; }
        public Rect SpawnArea { get => spawnArea; set => spawnArea = value; }
        public int BaseDistractors { get => baseDistractors; set => baseDistractors = Mathf.Max(0, value); }
        public float MinSeparation { get => minSeparation; set => minSeparation = Mathf.Max(0f, value); }
        public int PlacementAttempts { get => placementAttempts; set => placementAttempts = Mathf.Max(1, value); }

        private readonly List<NumberBubble> active = new List<NumberBubble>();

        public void Clear()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (active[i] != null) Destroy(active[i].gameObject);
            }
            active.Clear();
        }

        public void SpawnRound(int targetDigit, int roundIndex, float bubbleSpeed = -1f, NumberKeysController controller = null)
        {
            if (bubblePrefab == null)
            {
                Debug.LogError("BubbleSpawner: bubblePrefab is not assigned! Cannot spawn bubbles.", this);
                return;
            }
            
            Clear();
            int distractors = baseDistractors + Mathf.Min(roundIndex, 5);
            
            Debug.Log($"BubbleSpawner: Spawning round {roundIndex}, target digit: {targetDigit}, distractors: {distractors}");

            // Ensure at least one target bubble
            var usedPositions = new List<Vector3>();
            NumberBubble targetBubble = SpawnBubble(targetDigit, usedPositions, bubbleSpeed);
            
            if (targetBubble == null)
            {
                Debug.LogError("BubbleSpawner: Failed to spawn target bubble!", this);
                return;
            }
            
            // Register target bubble with controller if provided
            if (controller != null && targetBubble != null)
            {
                controller.RegisterActiveTargetBubble(targetBubble);
            }

            var used = new HashSet<int> { targetDigit };
            for (int i = 0; i < distractors; i++)
            {
                int d;
                do { d = Random.Range(0, 10); } while (!used.Add(d) && used.Count < 10);
                SpawnBubble(d, usedPositions, bubbleSpeed);
            }
            
            Debug.Log($"BubbleSpawner: Spawned {active.Count} bubbles total");
        }

        public void PopAllWithDigit(int digit)
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var b = active[i];
                if (b == null) { active.RemoveAt(i); continue; }
                if (b.digit == digit)
                {
                    b.Pop();
                    active.RemoveAt(i);
                }
            }
        }

        private NumberBubble SpawnBubble(int digit, List<Vector3> used, float bubbleSpeed = -1f)
        {
            if (bubblePrefab == null)
            {
                Debug.LogError("BubbleSpawner: bubblePrefab is null, cannot spawn bubble!", this);
                return null;
            }
            
            Vector3 pos;
            if (!TryGetSpawnPosition(out pos, used))
            {
                // Fallback to a random position if no free slot found
                pos = new Vector3(
                    Random.Range(spawnArea.xMin, spawnArea.xMax),
                    Random.Range(spawnArea.yMin, spawnArea.yMax),
                    0f);
            }
            
            var bubble = Instantiate(bubblePrefab, pos, Quaternion.identity, transform);
            if (bubble == null)
            {
                Debug.LogError($"BubbleSpawner: Failed to instantiate bubble for digit {digit} at position {pos}", this);
                return null;
            }
            
            // Ensure the bubble is active
            bubble.gameObject.SetActive(true);
            
            bubble.digit = digit;
            bubble.UpdateDigitText(); // Update text to display the digit
            if (bubbleSpeed >= 0f)
            {
                bubble.SetRiseSpeed(bubbleSpeed);
            }
            active.Add(bubble);
            used?.Add(pos);
            
            Debug.Log($"BubbleSpawner: Spawned bubble with digit {digit} at position {pos}, active: {bubble.gameObject.activeInHierarchy}, enabled: {bubble.enabled}");
            return bubble;
        }

        private bool TryGetSpawnPosition(out Vector3 position, List<Vector3> used)
        {
            // Sample a few candidates and ensure they are at least minSeparation from others
            for (int attempt = 0; attempt < Mathf.Max(1, placementAttempts); attempt++)
            {
                var candidate = new Vector3(
                    Random.Range(spawnArea.xMin, spawnArea.xMax),
                    Random.Range(spawnArea.yMin, spawnArea.yMax),
                    0f);

                bool ok = true;
                if (used != null)
                {
                    float minDistSqr = minSeparation * minSeparation;
                    for (int i = 0; i < used.Count; i++)
                    {
                        if ((used[i] - candidate).sqrMagnitude < minDistSqr)
                        {
                            ok = false;
                            break;
                        }
                    }
                }
                if (ok)
                {
                    position = candidate;
                    return true;
                }
            }
            position = default;
            return false;
        }
    }
}


