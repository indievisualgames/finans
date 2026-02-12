using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToChildDashboard : MonoBehaviour
{
    ChildDashboard _scriptable;
    string _thisSceneName;
    bool keypressed = false;

    void Start()
    {
        GameObject scriptableGO = GameObject.Find("DashboardScriptable");
        _thisSceneName = GetComponent<CurrentSceneName>().thisSceneName;
        if (scriptableGO != null)
        {
            _scriptable = scriptableGO.GetComponent<ChildDashboard>();
            _scriptable.disableOnAsyncLoad.SetActive(false);
            Debug.Log($"I found the file by name {_scriptable.name}.");
        }


    }
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape) && !keypressed)
            {
                keypressed = true;
                SceneManager.UnloadSceneAsync(_thisSceneName);
                if (_scriptable)
                {
                    _scriptable.disableOnAsyncLoad.SetActive(true);
                    keypressed = false;
                }

            }

        }


    }


}
/*  void Update()
 {
     if (Application.platform == RuntimePlatform.Android)
     {
         if (Input.GetKey(KeyCode.Escape))
         {
             if (!waitTimeStarted)
             {
                 waitTimeStarted = true;
                 StartCoroutine(SetDisableOnAsyncActive());

             }
         }

     }


 }

 IEnumerator SetDisableOnAsyncActive()
 {
     SceneManager.UnloadSceneAsync(_thisSceneName);
     yield return new WaitForSeconds(0.5f);
     if (_scriptable)
     {
         _scriptable.disableOnAsyncLoad.SetActive(true);
     }
 }

}*/
