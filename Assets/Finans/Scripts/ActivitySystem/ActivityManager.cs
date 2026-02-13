using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Finans.ActivitySystem
{
    /// <summary>
    /// Loads ALL *.json files directly from StreamingAssets/ActivityData/Activities/
    /// No manifest needed — each JSON is self-contained.
    /// </summary>
    public class ActivityManager : MonoBehaviour
    {
        public static ActivityManager Instance { get; private set; }

        // ── Loaded Data ──
        [Header("Loaded Activities (Runtime)")]
        [SerializeField] private List<ActivityContent> allActivities = new List<ActivityContent>();
        [SerializeField] private ActivityContent currentActivity;

        // ── Debug ──
        [Header("Debug Info")]
        [SerializeField] private int totalLoaded = 0;
        [SerializeField] private bool isLoaded = false;

        // ── Constants ──
        private const string ACTIVITIES_FOLDER = "ActivityData/Activities";

        // ── Events ──
        public event Action OnAllActivitiesLoaded;
        public event Action<string> OnLoadError;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Debug.Log("[ActivityManager] Starting direct folder load...");
            StartCoroutine(LoadAllFromFolderCoroutine());
        }

        /// <summary>
        /// Scans the Activities folder and loads every *.json file
        /// </summary>
        private IEnumerator LoadAllFromFolderCoroutine()
        {
            string folderPath = Path.Combine(Application.streamingAssetsPath, ACTIVITIES_FOLDER);
            Debug.Log($"[ActivityManager] Scanning folder: {folderPath}");

            #if UNITY_ANDROID && !UNITY_EDITOR
            // Android: use a known file list (or embedded index)
            // Since Directory.GetFiles doesn't work on Android APK,
            // we scan with a numbered pattern approach
            yield return StartCoroutine(LoadAllAndroid(folderPath));
            #else
            // Desktop / iOS: direct folder scan
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError($"[ActivityManager] Folder NOT found: {folderPath}");
                OnLoadError?.Invoke("Activities folder not found");
                yield break;
            }

            string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");
            Debug.Log($"[ActivityManager] Found {jsonFiles.Length} JSON files in the directory.");

            if (jsonFiles.Length == 0)
            {
                Debug.LogWarning($"[ActivityManager] No .json files found in {folderPath}. Check your folder structure.");
            }

            foreach (string filePath in jsonFiles)
            {
                string jsonText = File.ReadAllText(filePath);
                string fileName = Path.GetFileName(filePath);

                try
                {
                    ActivityContent content = JsonUtility.FromJson<ActivityContent>(jsonText);
                    if (content == null)
                    {
                        Debug.LogError($"[ActivityManager] Failed to deserialize {fileName}. Result was null.");
                        continue;
                    }

                    allActivities.Add(content);
                    Debug.Log($"[ActivityManager] Successfully loaded activity: {content.activityName} (ID: {content.activityId})");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ActivityManager] Exception parsing {fileName}: {e.Message}");
                }

                yield return null; // Spread across frames
            }
            #endif

            // Sort by day number
            allActivities = allActivities.OrderBy(a => a.dayNumber).ToList();

            // Set first as current
            if (allActivities.Count > 0)
            {
                currentActivity = allActivities[0];
            }

            totalLoaded = allActivities.Count;
            isLoaded = true;

            Debug.Log($"[ActivityManager] Load sequence complete. Total activities in memory: {totalLoaded}");
            OnAllActivitiesLoaded?.Invoke();
        }

        #if UNITY_ANDROID && !UNITY_EDITOR
        private IEnumerator LoadAllAndroid(string folderPath)
        {
            // Android can't scan folders — use known filenames
            string[] knownFiles = {
                "01_flashcard.json", "02_minigames.json", "03_vocabulary.json",
                "04_storybook.json", "05_funfact.json", "06_calculator.json",
                "07_megaquiz.json"
            };

            foreach (string fileName in knownFiles)
            {
                string filePath = Path.Combine(folderPath, fileName);
                UnityEngine.Networking.UnityWebRequest www =
                    UnityEngine.Networking.UnityWebRequest.Get(filePath);
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    try
                    {
                        ActivityContent content =
                            JsonUtility.FromJson<ActivityContent>(www.downloadHandler.text);
                        allActivities.Add(content);
                        Debug.Log($"[ActivityManager] Loaded: {content.activityName} from {fileName}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ActivityManager] Failed to parse {fileName}: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[ActivityManager] File not found (skipped): {fileName}");
                }
            }
        }
        #endif

        // ── Public API ──

        public List<ActivityContent> GetAllActivities() => allActivities;
        public int Count => allActivities.Count;
        public bool IsLoaded => isLoaded;

        public ActivityContent GetActivity(string activityId)
        {
            return allActivities.Find(a => a.activityId == activityId);
        }

        public ActivityContent GetCurrentActivity() => currentActivity;

        public void SetCurrentActivity(string activityId)
        {
            var found = GetActivity(activityId);
            if (found != null)
            {
                currentActivity = found;
                Debug.Log($"[ActivityManager] Current → {currentActivity.activityName}");
            }
        }
    }
}
