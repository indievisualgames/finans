using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static IFirestoreEnums;
using UnityEngine.Events;
using Ricimi;


#if UNITY_EDITOR
using UnityEditor.Events;
#endif
[RequireComponent(typeof(InternetConnectivityCheck))]

public class AddChildDashboard : Elevator
{
    [Header("UI References")]
    [SerializeField] private GameObject loading; 
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject screenContent;
    [SerializeField] private GameObject childDetailsContainer;
    [SerializeField] private GameObject childDetailsInRowPrefab;
    [SerializeField] private Image parentPic;
    [SerializeField ] private TMP_Text displayName;
   [SerializeField] private GameObject messageBoxPopupPrefab;
  
    [Header("Private Fields")]
    private IFirestoreOperator FirestoreClient;
    private GameObject popup;
    private List<string> childDashboardData = new List<string>(); 
    private InternetConnectivityCheck internetConnectivityCheck;
    private string context = "AddChildDashboard";


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
            Logger.LogError("InternetConnectivityCheck component not found on AddChildDashboard", context);
        }

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (await FetchChildDashboardData(false))
            {
                LoadProfileAndFinalizeScreen(parentPic, displayName, screenContent, loading);
            }
        }
        else
        {
            //:------------------No Internet Popup Dialog
            Logger.LogError($"No connectivity found", context);
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
            internetConnectivityCheck.CheckNow(true);
        }

    }

    public async Task<bool> FetchChildDashboardData(bool clear)
    {
        childDashboardData.Clear();
        if (clear)
        {
            foreach (Transform child in childDetailsContainer.transform)
            {
               Logger.LogInfo($"Child found: {child.gameObject.name}", context);
                Destroy(child.gameObject);
            }
        }
        try
        {
            int srno = 1;
            childDashboardData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString());
            foreach (var childkey in childDashboardData)
            {
                Logger.LogInfo($"Childs id found are :- {childkey}", context);
                Dictionary<string, object> childData = await FirestoreClient.GetFirestoreDataField(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), childkey, FSMapField.profile.ToString());
                GameObject ChildDetailsGO = Instantiate(childDetailsInRowPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
                ChildDetailsInRow ChildDetailsInRowValues = ChildDetailsGO.GetComponent<ChildDetailsInRow>();
                ChildDetailsInRowValues.ChildID = childkey;
                ChildDetailsGO.transform.SetParent(childDetailsContainer.transform, false);

                foreach (var key in childData)
                {
                    Logger.LogInfo($"keys found are {key.Key} and values are  {key.Value}", context);
                    ChildDetailsInRowValues.SrNo = srno.ToString();
                    if (key.Key == ChildProfile.dob.ToString()) { ChildDetailsInRowValues.DOB = (string)key.Value; }
                    if (key.Key == ChildProfile.age.ToString()) { ChildDetailsInRowValues.Age = (string)key.Value; }
                    if (key.Key == ChildProfile.firstname.ToString()) { ChildDetailsInRowValues.FirstName = (string)key.Value; }
                    if (key.Key == ChildProfile.lastname.ToString()) { ChildDetailsInRowValues.LastName = (string)key.Value; }
                    if (key.Key == ChildProfile.gender.ToString()) { ChildDetailsInRowValues.Gender = (string)key.Value; }
                    if (key.Key == ChildProfile.avatar.ToString()) { ChildDetailsInRowValues.AvatarName = (string)key.Value; }
                    if (key.Key == ChildProfile.grade.ToString()) { ChildDetailsInRowValues.Grade = (string)key.Value; }
                    if (key.Key == ChildProfile.plan.ToString()) { ChildDetailsInRowValues.Plan = (string)key.Value; }
                    if (key.Key == ChildProfile.screentime.ToString()) { ChildDetailsInRowValues.ScreenTime = (string)key.Value; }
                    if (key.Key == ChildProfile.pinhint.ToString()) { ChildDetailsInRowValues.PinHint = (string)key.Value; }
                    if (key.Key == ChildProfile.pin.ToString()) { ChildDetailsInRowValues.Pin = (string)key.Value; }
                }
                srno++;
            }
        }

        catch (System.Exception ex)
        {
            Logger.LogError("Error occurred fetching data ", context, ex);
            return false; 
        }
        return true;
    }
    async void RetryTheAction()
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (internetConnectivityCheck.ConnectionStatus) { internetConnectivityCheck.ConnectionStatus = false; }
            if (await FetchChildDashboardData(false))
            {
                popup.GetComponent<Popup>().Close();
                LoadProfileAndFinalizeScreen(parentPic, displayName, screenContent, loading);
            }
        }
    }

    void Update()
    {
        // Intentionally left empty; connectivity retries are now event-driven via InternetConnectivityCheck.ConnectivityChanged
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogWarning("Connectivity restored in AddChildDashboard, retrying action", context);
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