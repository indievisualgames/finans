using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using static IFirestoreEnums;
using Ricimi;
using System;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif
[RequireComponent(typeof(InternetConnectivityCheck))]
public class ChildDashboard : ChildDashboardStats
{
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject screenContent;
    [SerializeField] private Image childPic;
    [SerializeField] public GameObject disableOnAsyncLoad;
    [SerializeField] private GameObject messageBoxPopupPrefab;

    [Header("Private Fields")]
    private GameObject popup;
    private InternetConnectivityCheck internetConnectivityCheck;
    private string context = "ChildDashboard";

    [Header("Public Fields")]
    public Dictionary<string, object> unitStatusFSData = new Dictionary<string, object>();

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
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
        }
        else
        {
            Logger.LogError("InternetConnectivityCheck component not found on ChildDashboard", context);
        }

        if (canvas == null)
        {
            var found = FindFirstObjectByType<Canvas>();
            if (found != null) { canvas = found; }
            else { Logger.LogError("Canvas not found in scene during start", context); }
        }
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await LoadChildFSData();
        }
        else
        {
            MessageBox msgBox = messageBoxPopupPrefab.GetComponent<MessageBox>();
            msgBox.Headline = Message.MBNoInternetHeadline;
            msgBox.Message = Message.MBNoInternetMessage;
            msgBox.ActionText = Message.MBActionButtonText;
#if UNITY_EDITOR
            UnityEventTools.AddPersistentListener(msgBox.actionButton.GetComponent<Button>().onClick, new UnityAction(RetryTheAction));
#elif UNITY_ANDROID
            msgBox.actionButton.GetComponent<Button>().onClick.AddListener(RetryTheAction);
#endif
            popup = Inference.OpenPopup(canvas, messageBoxPopupPrefab);
            internetConnectivityCheck.CheckNow(true); loading.SetActive(false);
        }
    }


    async void RetryTheAction()
    {
        loading.SetActive(value: true);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (internetConnectivityCheck.ConnectionStatus) { internetConnectivityCheck.ConnectionStatus = false; }
            if (popup.GetComponent<Popup>() != null) { popup.GetComponent<Popup>().Close(); }
            await LoadChildFSData();
        }
        loading.SetActive(false);
    }

    void Update()
    {
        // Intentionally left empty; connectivity retries are now event-driven via InternetConnectivityCheck.ConnectivityChanged
    }


    async Task LoadChildFSData()
    {
        try
        {
            if (!Params.ChildDataloaded)
            {
                FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);
            }

            Dictionary<string, object> points_score_data = (Dictionary<string, object>)FirestoreDatabase.ChildData[FSMapField.points_score.ToString()];
            Dictionary<string, object> progress_data = (Dictionary<string, object>)FirestoreDatabase.ChildData[FSMapField.progress_data.ToString()];
            unitStatusFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_btn_status.ToString());
            LoadAllDataAndFinish(progress_data, childPic, displayName, screenContent, loading);

        }
        catch (Exception ex)
        {
            Logger.LogError($"Unable to fetch player data", context, ex);
        }
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogWarning("Connectivity restored in ChildDashboard, retrying action", context);
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
