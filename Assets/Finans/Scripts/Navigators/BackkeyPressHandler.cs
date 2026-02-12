using UnityEngine;
using UnityEngine.SceneManagement;
using static IFirestoreEnums;

// Quits the finans app when the user hits escape
[RequireComponent(typeof(CurrentSceneName))]

public class BackkeyPressHandler : MonoBehaviour
{

    void Start()
    {

    }
    /*  void Update()
      {
          if (Application.platform == RuntimePlatform.Android && Input.GetKey(KeyCode.Escape))
          {
              OnBackButtonClick();
          }


      }*/
    public void OnBackButtonClick()
    {
        string childScene = GetComponent<CurrentSceneName>().thisSceneName;

        GameObject _level;
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "Level":
                Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                _level = GameObject.FindWithTag("Level");
                _level.GetComponent<UnloadSceneAdditiveAsync>().toggleContent.SetActive(true); SceneManager.UnloadSceneAsync(childScene);
                Debug.Log($"Scene {childScene} unloaded... ");
                break;
            case "ChildDashboard":
                Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                _level = GameObject.FindWithTag("Level");
                Debug.Log($"Found {_level.name} gameobject ");
                _level.GetComponent<UnloadSceneAdditiveAsync>().toggleContent.SetActive(true);
                Debug.Log($"{_level.name} gameobject activating  {_level.GetComponent<UnloadSceneAdditiveAsync>().toggleContent.name}");
                SceneManager.UnloadSceneAsync(childScene);
                Debug.Log($"Scene {childScene} unloaded... ");
                break;
            /* case "MainGame":
                 Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                 SceneManager.LoadScene(SceneName.Level.ToString());
                 break;*/
            /* case "Lesson":
                 Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                 SceneManager.LoadScene(SceneName.Level.ToString());
                 break;*/
            case "Agreement":
                Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                SceneManager.LoadScene(SceneName.HomeScene.ToString());
                break;
            case "ParentForm":
                SceneManager.LoadScene(SceneName.LanguageRegion.ToString());
                break;
            case "AddChildDashboard":
                Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                SceneManager.LoadScene(SceneName.ParentAccount.ToString());
                break;
            case "LanguageRegion":
                SceneManager.LoadScene(SceneName.Agreement.ToString());
                break;
            /* case "MiniGames":
                 Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                 SceneManager.UnloadSceneAsync(SceneName.MiniGames.ToString());
                 break;*/
            /*  case "ChildDashboardStats":
                 Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                 SceneManager.LoadScene(SceneName.ParentAccount.ToString());
                 break; */
            case "ParentDashboard":
                Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                SceneManager.LoadScene(SceneName.ParentAccount.ToString());
                break;
            case "ChildProgressDashboard":
                Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                SceneManager.LoadScene(SceneName.ParentAccount.ToString());
                break;
            default:
                Debug.Log($"Back key pressed: Active scene name is {sceneName}");
                SceneManager.LoadScene(SceneName.Level.ToString());
                break;

        }
    }
}