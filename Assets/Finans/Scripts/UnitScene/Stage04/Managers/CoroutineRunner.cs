using UnityEngine;

/// <summary>
/// Global coroutine host for cases where the original MonoBehaviour may be inactive.
/// </summary>
[AddComponentMenu("MiniGames/Utility/Coroutine Runner")]
[DisallowMultipleComponent]
public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner instance;
    public static CoroutineRunner Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("CoroutineRunner");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<CoroutineRunner>();
            }
            return instance;
        }
    }
}


