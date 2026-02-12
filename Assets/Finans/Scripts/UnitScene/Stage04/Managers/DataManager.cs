using UnityEngine;
using System.IO;
using System;

/// <summary>
/// DataManager handles saving and loading player progress, scores, and settings.
/// </summary>
public class MiniGameDataManager : MonoBehaviour
{
    public static MiniGameDataManager Instance;
    private string savePath;

    [Serializable]
    public class PlayerData
    {
        public int highScore;
        public int lastLevel;
        // Add more fields as needed
    }

    public PlayerData playerData = new PlayerData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Application.persistentDataPath + "/playerdata.json";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(savePath, json);
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
        }
    }
} 