using System.Collections.Generic;
using UnityEngine;
using static IFirestoreEnums;
using System.Threading.Tasks;
using Ricimi;
using System.Collections;


[RequireComponent(typeof(InternetConnectivityCheck))]
public class GameLevels : Elevator
{
    [Header("UI References")]
    [SerializeField] private GameObject loading;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject messageBoxPopupPrefab;
    [SerializeField] GameObject screenContent;


    [Header("Private Fields")]
    private IFirestoreOperator FirestoreClient;
    private GameObject popup;
    private InternetConnectivityCheck internetConnectivityCheck;
    private string context = "GameLevels";

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
        internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;

        if (canvas == null)
        {
            var found = FindFirstObjectByType<Canvas>();
            if (found != null) { canvas = found; }
            Logger.LogError("Canvas not found in scene during start", context);

        }

        if (!Params.ChildDataloaded)
        {
            if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
            {
                await ReloadChildData();
                Logger.LogInfo($"Child data reloaded from Firestore", context); return;

            }
            else
            {
                retryAction += async () => await ReloadChildData();
                popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
                loading.SetActive(false);
                Logger.LogError($"No connectivity found", context); return;
            }
        }

        await LoadUnitStatusData();

    }
    private async Task ReloadChildData()
    {
        FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);
        Logger.LogInfo($"Re Loading data from FS", context);
        await LoadUnitStatusData();
    }

    async Task LoadUnitStatusData()
    {
        FirestoreClient.LoadPointsAndScore(FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.points_score.ToString()));
        unitStatusFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_btn_status.ToString());
        Logger.LogInfo("Loading points score and unit button status from loaded data", context);
        StartCoroutine(FinishLoading());
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogInfo("Connectivity restored in GameLevels, retrying action", context);
        RetryTheAction(popup);
    }

    IEnumerator FinishLoading()
    {
        yield return new WaitForSeconds(0.10f);
        screenContent.SetActive(true);
        loading.SetActive(false);
    }
    public void OnGoToDashboardClick()
    {
        Transition.LoadLevel(SceneName.ChildDashboard.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }

    void OnDestroy()
    {
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
        }
    }

}
