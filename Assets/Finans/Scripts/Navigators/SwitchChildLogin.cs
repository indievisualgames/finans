
using UnityEngine;
using UnityEngine.SceneManagement;
using static IFirestoreEnums;

public class SwitchChildLogin : MonoBehaviour
{
    public void OnChildSwitchButtonClick()
    {
        SceneManager.LoadSceneAsync(SceneName.ChildAuthentication.ToString(), LoadSceneMode.Additive);
    }
}
