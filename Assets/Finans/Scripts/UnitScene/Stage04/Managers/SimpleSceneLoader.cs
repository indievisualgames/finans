using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SimpleSceneLoader - Minimal helpers to load scenes by name, index, or next scene.
/// </summary>
[AddComponentMenu("MiniGames/Managers/Simple Scene Loader")]
[DisallowMultipleComponent]
public class SimpleSceneLoader : MonoBehaviour
{
    [Tooltip("Optional: name of scene to load when calling LoadConfiguredScene")]
    public string sceneName;

    [Tooltip("Optional: build index of scene to load when calling LoadConfiguredSceneByIndex")]
    public int sceneIndex = -1;

    public void LoadSceneByName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            SceneManager.LoadScene(name);
        }
    }

    public void LoadSceneByIndex(int index)
    {
        if (index >= 0)
        {
            SceneManager.LoadScene(index);
        }
    }

    public void LoadNextScene()
    {
        var current = SceneManager.GetActiveScene();
        int nextIndex = current.buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            // Wrap to first scene
            SceneManager.LoadScene(0);
        }
    }

    public void LoadConfiguredScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            LoadSceneByName(sceneName);
        }
    }

    public void LoadConfiguredSceneByIndex()
    {
        if (sceneIndex >= 0)
        {
            LoadSceneByIndex(sceneIndex);
        }
    }
}


