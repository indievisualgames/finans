using Ricimi;
using UnityEngine;
using UnityEngine.SceneManagement;
using static IFirestoreEnums;

public class HUDNavigation : MonoBehaviour
{

    public void OnHomeButtonClick()
    {
        SceneManager.LoadSceneAsync(SceneName.HomeScene.ToString(), LoadSceneMode.Additive);
    }

    public void OnSwitchButtonClick()
    {
        SceneManager.LoadSceneAsync(SceneName.ChildAuthentication.ToString(), LoadSceneMode.Additive);
    }

    public void OnDashboardButtonClick()
    {
        SceneManager.LoadSceneAsync(SceneName.ChildDashboard.ToString(), LoadSceneMode.Additive);
    }


    public void GotoDashboardButtonClick()
    {
        //SceneManager.LoadSceneAsync(SceneName.ChildDashboard.ToString(), LoadSceneMode.Single);
        Transition.LoadLevel(SceneName.ChildDashboard.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }
    public void GotoMaps()
    {
        //SceneManager.LoadSceneAsync(SceneName.ChildDashboard.ToString(), LoadSceneMode.Single);
        Transition.LoadLevel(SceneName.Level.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }


}
