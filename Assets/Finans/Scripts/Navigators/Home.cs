
using Facebook.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static IFirestoreEnums;

public class Home : MonoBehaviour
{
    [SerializeField] private Canvas m_canvas;
    [SerializeField] private GameObject messageBoxPopupPrefab;
    public void OnHomeButtonClick()
    {
        SceneManager.LoadSceneAsync(SceneName.HomeScene.ToString());
    }
    public void OnQuit()
    {

        GameObject ChildDetailsGO = Instantiate(messageBoxPopupPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        m_canvas.GetComponent<CanvasScaler>().scaleFactor = 1.1f;
        ChildDetailsGO.transform.SetParent(m_canvas.transform, false);
        ChildDetailsGO.GetComponent<MessageBox>().Headline = "Confirm Logout";
        ChildDetailsGO.GetComponent<MessageBox>().Message = "Are you sure you want to logout?";
        ChildDetailsGO.GetComponent<MessageBox>().actionButton.GetComponentInChildren<TMP_Text>().text = "Yes";
        ChildDetailsGO.GetComponent<MessageBox>().secondaryButton.GetComponentInChildren<TMP_Text>().text = "No";
        ChildDetailsGO.GetComponent<MessageBox>().actionButton.GetComponent<Button>().onClick.AddListener(ConfirmedLogout);
        ChildDetailsGO.GetComponent<MessageBox>().actionButton.SetActive(true);
        ChildDetailsGO.GetComponent<MessageBox>().secondaryButton.SetActive(true);

        ChildDetailsGO.GetComponent<MessageBox>().secondaryButton.GetComponent<Button>().onClick.AddListener(() => OnClose(ChildDetailsGO));
        ChildDetailsGO.SetActive(true);


    }

    public void OnClose(GameObject ChildDetailsGO)
    {
        ChildDetailsGO.GetComponent<MessageBox>().ResetCanvasScale();
    }
    private void ConfirmedLogout()
    {
        try
        {
            PlayerInfo.AuthenticatedID = "";
            PlayerInfo.AuthenticatedChildID = "";
            PlayerInfo.IsAppAuthenticated = false;
            PlayerInfo.ProfileImageSprite = null;
            Params.ChildDataloaded = false;
            PlayerInfo.UnitButtonInfo.Clear();
            Logger.LogInfo($"Debug Log: Signing out {Params.__auth.CurrentUser}", "Home");
            if (PlayerInfo.CurrentLoginAttempt == "Facebook")
            {
                FB.LogOut();
            }
            PlayerInfo.CurrentLoginAttempt = string.Empty;
            Params.__auth.SignOut();
        }
        catch
        {
            // best-effort sign out; ignore
        }
        SceneManager.LoadSceneAsync(SceneName.HomeScene.ToString());
    }
}
