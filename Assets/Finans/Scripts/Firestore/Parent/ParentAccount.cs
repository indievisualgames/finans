using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using static IFirestoreEnums;
using Ricimi;
using System.Collections;
using UnityEngine.Networking;




#if UNITY_EDITOR
using UnityEditor.Events;
#endif
[RequireComponent(typeof(InternetConnectivityCheck))]
public class ParentAccount : Elevator
{
  
    [SerializeField] Image parentPic;
    [SerializeField] TMP_Text parentName;
    [SerializeField] TMP_Text displayName;
    [SerializeField] TMP_Text country;
    [SerializeField] TMP_Text currency;
    [SerializeField] TMP_Text language;
    public GameObject loading;
    public GameObject screenContent;
    GameObject popup;
    Canvas m_canvas;
    InternetConnectivityCheck internetConnectivityCheck;
    public GameObject messageBoxPopupPrefab;
    private string context = "ParentAccount";
    void Awake()
    {
        if (!PlayerInfo.IsAppAuthenticated)
        {
            Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }

    }

    async void Start()
    {
        Debug.Log($"Debug.Log::::::::::Async start initiated in Parent Account");
        var foundCanvasGO = GameObject.Find("Canvas");
        if (foundCanvasGO != null) { m_canvas = foundCanvasGO.GetComponent<Canvas>(); }
        if (m_canvas == null)
        {
            var fallback = FindFirstObjectByType<Canvas>();
            if (fallback != null) { m_canvas = fallback; }
            else { Logger.LogError("Canvas not found in scene during start", context); }
        }
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
        }
        else
        {
            Logger.LogError("InternetConnectivityCheck component not found on ParentAccount", context);
        }

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (GetParentData())
            {
                Debug.Log("Fetched parent data....Finalizing screen..........");
                if (PlayerInfo.ProfileImageSprite != null)
                {
                    OperationFinished(screenContent, loading);
                }
                else
                {
                    StartCoroutine(LoadParentProfilePic());
                }
            }
        }
        else
        {
            popup = ShowNoConnectivityPopup(m_canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            loading.SetActive(false);
            Logger.LogError($"No connectivity found", context);
        }
    }

    bool GetParentData()
    {
        try
        {
            Logger.LogInfo($"Trying to get Parent Data for player id {PlayerInfo.AuthenticatedID}", context);

            foreach (var key in FirestoreDatabase.ParentData)
            {
                if (key.Key == ParentProfile.parentname.ToString()) { parentName.text = (string)key.Value; }
                if (key.Key == ParentProfile.country.ToString()) { country.text = (string)key.Value; }
                if (key.Key == ParentProfile.currency.ToString()) { currency.text = (string)key.Value; }
                if (key.Key == ParentProfile.language.ToString()) { language.text = (string)key.Value; }


            }

            Logger.LogInfo($"Debug.Log Data fetched completed... Returning boolean true", context);
            return true;

        }
        catch (System.Exception ex)
        {
            Logger.LogError($"GetParentData returned error", context,ex);
            return false;
        }
    }

    public async void ChildAccountButton_Click()
    {
        if (await HaveConnectivity(m_canvas, messageBoxPopupPrefab, loading))
        {
            Transition.LoadLevel(SceneName.AddChildDashboard.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
    }

    public async void ChildDashboardButton_Click()
    {
        if (await HaveConnectivity(m_canvas, messageBoxPopupPrefab, loading))
        {
            Transition.LoadLevel(SceneName.ChildProgressDashboard.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
    }
    public async void ParentDashboardButton_Click()
    {
        if (await HaveConnectivity(m_canvas, messageBoxPopupPrefab, loading))
        {
            Transition.LoadLevel(SceneName.ParentDashboard.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
    }


    async void RetryTheAction()
    {
        loading.SetActive(value: true);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (internetConnectivityCheck.ConnectionStatus) { internetConnectivityCheck.ConnectionStatus = false; }
            if (popup != null && popup.GetComponent<Popup>() != null)
            {
                popup.GetComponent<Popup>().Close();
            }

            if (GetParentData())
            {
                Logger.LogInfo($"Loaded parent data", context);
                StartCoroutine(LoadParentProfilePic());

            }
        }

    }

    void Update()
    {
        // Intentionally left empty; connectivity retries are now event-driven via InternetConnectivityCheck.ConnectivityChanged
    }

    GameObject logoutPopup;
    public void ConfirmLogOut()
    {
        logoutPopup = ShowMessagePopup(m_canvas, messageBoxPopupPrefab, "Logout", "Do you want to logout", "Yes", "Nope", 2, null);
        logoutPopup.GetComponent<MessageBox>().actionButton.GetComponent<Button>().onClick.AddListener(LogOut);

    }
    public void LogOut()
    {
        Logger.LogInfo("Sucessfully logout...........", context);
        logoutPopup.GetComponent<Popup>().Close();
        Logger.LogInfo($"Trying to sign out {Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser}",context);
        PlayerInfo.AuthenticatedID = string.Empty;
        PlayerInfo.IsAppAuthenticated = false;
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
        Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        Logger.LogInfo($"Clearing PlayerInfo.AuthenticatedID", context);
    }
    IEnumerator LoadParentProfilePic()
    {
        string url = $"{Application.streamingAssetsPath}/transitionBG.png";
        if (Firebase.Auth.FirebaseAuth.DefaultInstance != null)
        {
            if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                url = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.PhotoUrl.ToString();
            }
        }
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite spr = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2());
            PlayerInfo.ProfileImageSprite = spr;//initial loading from parent accout
        }
        LoadProfileAndFinalizeScreen(parentPic, displayName, screenContent, loading);
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogWarning("Connectivity restored in ParentAccount, retrying action", context);
        RetryTheAction();
    }

    void OnDestroy()
    {
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
        }
    }
}