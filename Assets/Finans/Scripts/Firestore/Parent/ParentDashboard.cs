using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using static IFirestoreEnums;
using Ricimi;


#if UNITY_EDITOR
using UnityEditor.Events;
#endif
[RequireComponent(typeof(InternetConnectivityCheck))]
public class ParentDashboard : Elevator
{
 
    [Header("UI References")]
    Dictionary<string, object> parentProfileData;
    [SerializeField] private Image parentPic;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private Text childCount;
    [SerializeField] private GameObject loading;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject screenContent;
    [SerializeField] private GameObject messageBoxPopupPrefab;
    [SerializeField] private GameObject childPinChangePopupPrefab;

    [Header("Private Fields")]
    private InternetConnectivityCheck internetConnectivityCheck;
     private IFirestoreOperator FirestoreClient;
    private GameObject popup;
    private bool autoRetryDone = false;
    private string context = "ParentDashboard";
    void Awake()
    {
        if (!PlayerInfo.IsAppAuthenticated)
        {
           Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }

    }

    async void Start()
    {
        FirestoreClient = new FirestoreDataOperationManager();
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        if (canvas == null)
        {
            var found = FindFirstObjectByType<Canvas>();
            if (found != null) { canvas = found; }
            else { Logger.LogError("Canvas not found in scene during start", context); }

        }
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
        }
        else
        {
            Logger.LogError("InternetConnectivityCheck component not found on ParentDashboard", context);
        }
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (await GetParentDashboardData())
            {
                Debug.Log($"Debug.Log Got Parent Dashboard data... ");
                FinalizeScreen(screenContent, loading);
            }
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            loading.SetActive(false);
        }
    }

    async Task<bool> GetParentDashboardData()
    {
        string existingCCount = "0";
        try
        {
            Logger.LogInfo($"Trying to get  Parent Dashboard data for player id {PlayerInfo.AuthenticatedID}...", context);
            parentProfileData = await FirestoreClient.GetFirestoreDataField(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSMapField.profile.ToString());
            if (parentProfileData.ContainsKey(ParentProfile.children.ToString()))
            {
                Dictionary<string, object> cCount = (Dictionary<string, object>)parentProfileData[ParentProfile.children.ToString()];
                existingCCount = cCount.Count.ToString();
            }

            childCount.text = existingCCount;
           Logger.LogInfo($"Parent Dashboard data fetched completed... Returning boolean true...", context);
            return true;

        }
        catch (System.Exception ex)
        {
           Logger.LogError($"Fetching Parent Dashboard data returned error {ex.Message}... ", context, ex);
            return false;
        }
    }




    async void RetryTheAction()
    {
        loading.SetActive(value: true);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (internetConnectivityCheck.ConnectionStatus) { internetConnectivityCheck.ConnectionStatus = false; }
            popup.GetComponent<Popup>().Close();
            if (autoRetryDone) { autoRetryDone = false; }

            if (await GetParentDashboardData())
            {
                LoadProfileAndFinalizeScreen(parentPic, displayName, screenContent, loading);

            }

        }

    }

    void Update()
    {
        // Intentionally left empty; connectivity retries are now driven by InternetConnectivityCheck.ConnectivityChanged
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected || autoRetryDone) return;
        autoRetryDone = true;
        Logger.LogWarning("Connectivity restored in ParentDashboard, retrying action", context);
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