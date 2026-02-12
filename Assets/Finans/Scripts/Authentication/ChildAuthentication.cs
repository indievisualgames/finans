using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static IFirestoreEnums;
using UnityEngine.Events;
using Ricimi;
using UnityEngine.UI;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor.Events;
#endif
[RequireComponent(typeof(InternetConnectivityCheck))]
public class ChildAuthentication : Elevator
{

    [Header("UI References")]
    [SerializeField] private TMP_Text childPinHint;
    [SerializeField] private TMP_InputField childPin;
    [SerializeField] private TMP_Dropdown childIds;
    [SerializeField] private TMP_Text childErrorText;
    [SerializeField] private TMP_Text screenLabel;
    [SerializeField] private GameObject loading;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject messageBoxPopupPrefab;
    List<Item> childIdsLists = new List<Item>();


    [Header("Private Fields")]
    private IFirestoreOperator FirestoreClient;
    private Dictionary<string, object> parentData = new Dictionary<string, object>();
    private bool childIsLoggedIn = false;
    private int validChildForLogin = 0;
    private string context = "ChildAuthentication";
    private GameObject popup;
    private InternetConnectivityCheck internetConnectivityCheck;
    private bool autoRetryDone = false;

    public class Item
    {
        public string ID = "";
        public string NAME = "";

    }
    void Awake()
    {
        if (!PlayerInfo.IsAppAuthenticated)
        {
            Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
        if (PlayerInfo.AuthenticatedChildID == string.Empty)
        {
            screenLabel.text = Message.ChildAuthenticationLabel;
        }
        else
        {
            screenLabel.text = Message.SwitchChildAuthenticationLabel;
        }

        if (childPin != null)
        {
            childPin.characterLimit = Params.pinCharLimit;
        }

    }
    async void Start()
    {
        try
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
                Logger.LogError("InternetConnectivityCheck component not found on ChildAuthentication", context);
            }

            if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
            {
                await CheckAndLoadChildAccounts();
            }
            else
            {
                popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
                internetConnectivityCheck.CheckNow(true);
            }
            loading.SetActive(false);
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Something went wrong during start", context, ex);
        }
    }

    async Task CheckAndLoadChildAccounts()
    {
        parentData = await FirestoreClient.GetFirestoreDataField(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSMapField.profile.ToString());



        if (parentData != null && parentData.ContainsKey(ParentProfile.children.ToString()))
        {

            foreach (var item in parentData[ParentProfile.children.ToString()] as Dictionary<string, object>)
            {
                if (PlayerInfo.AuthenticatedChildID != (string)item.Key)
                {
                    childIdsLists.Add(new Item() { NAME = (string)item.Value, ID = (string)item.Key });
                }
                else if (PlayerInfo.AuthenticatedChildID == (string)item.Key)
                { childIsLoggedIn = true; }
            }
            if (childIds != null)
            {
                for (int i = 0; i < childIdsLists.Count; i++)
                {
                    childIds.AddOptions(new List<string>() { childIdsLists[i].NAME });
                    validChildForLogin++;
                }
            }
            if (!childIdsLists.Any())
            {

                NoChildAccountFound();
            }
        }
        else
        {
            NoChildAccountFound();

        }

    }
    public void OnNextForLogin()
    {
        if (!childIdsLists.Any())
        { return; }


        if (childPin == null || childPin.text == string.Empty)
        {
            if (childErrorText != null) childErrorText.text = Message.EmptyPin;
            return;
        }


        if (loading != null) loading.SetActive(true);
        OnLoginAsync();
    }
    private async void OnLoginAsync()
    {
        // Connectivity guard to avoid runtime errors when offline
        if (!await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (canvas == null)
            {
                var found = FindFirstObjectByType<Canvas>();
                if (found != null) { canvas = found; }
            }
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            if (loading != null) loading.SetActive(false);
            return;
        }
        if (childIds == null || childIds.value < 0 || childIds.value >= childIdsLists.Count)
        {
            if (loading != null) loading.SetActive(false);
            if (childErrorText != null) childErrorText.text = Message.ChildAccountNotFound;
            return;
        }
        Dictionary<string, object> childData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), childIdsLists[childIds.value].ID);
        if (childData == null || !childData.ContainsKey(FSMapField.profile.ToString()))
        {
            if (loading != null) loading.SetActive(false);
            if (childErrorText != null) childErrorText.text = Message.ChildAccountNotFound;
            return;
        }
        Dictionary<string, object> profileData = (Dictionary<string, object>)childData[FSMapField.profile.ToString()];

        var storedPin = profileData.ContainsKey(ChildProfile.pin.ToString()) ? profileData[ChildProfile.pin.ToString()] as string : null;
        if (childPin != null && childPin.text == storedPin)
        {
            PlayerInfo.AuthenticatedChildID = childIdsLists[childIds.value].ID;
            /***This child data is populated once when switch child account authentication is sucessfull and is available througout the session*********/
            FirestoreDatabase.ChildData = childData;
            /***************************************************************************/
            if (profileData.ContainsKey(ChildProfile.avatar.ToString()))
            {
                PlayerInfo.AvatarURL = profileData[ChildProfile.avatar.ToString()] as string;
            }
            if (profileData.ContainsKey(ChildProfile.firstname.ToString()))
            {
                PlayerInfo.ChildName = profileData[ChildProfile.firstname.ToString()] as string;
            }
            Logger.LogInfo("Child PIN correct; proceeding to Level", "ChildAuth");

            Transition.LoadLevel(SceneName.Level.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);


            if (loading != null) loading.SetActive(false);

        }
        else
        {
            if (loading != null) loading.SetActive(false);
            if (childErrorText != null) childErrorText.text = Message.IncorrectPinError;
            var pinHintObj = profileData.ContainsKey(ChildProfile.pinhint.ToString()) ? profileData[ChildProfile.pinhint.ToString()] : null;
            if (childPinHint != null) childPinHint.text = $"Pin hint: {pinHintObj as string ?? "No hint available"}";
            Logger.LogWarning(Message.IncorrectPinError, "ChildAuth");
        }

        Logger.LogDebug("Showing the pin (debug placeholder)", "ChildAuth");

    }
    void NoChildAccountFound()
    {
        if (childPin != null) childPin.readOnly = true;
        if (childIds != null) childIds.interactable = false;
        if (childErrorText != null) childErrorText.text = (childIsLoggedIn && validChildForLogin == 0) ? Message.ChildSwitchAccountNotFound : Message.ChildAccountNotFound;
    }

    public void OnPasswordEnter()
    {
        if (childErrorText != null && childErrorText.text != string.Empty)
        {
            childErrorText.text = "";
            if (childPinHint != null) childPinHint.text = "";
        }
    }
    public void OnClose()
    {
        Debug.Log($"Unloading ChildAuthentication loaded as SceneAsync");
        SceneManager.UnloadSceneAsync(SceneName.ChildAuthentication.ToString());
    }
    async void RetryTheAction()
    {
        if (loading != null) loading.SetActive(value: true);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (internetConnectivityCheck != null && internetConnectivityCheck.ConnectionStatus) { internetConnectivityCheck.ConnectionStatus = false; }
            if (popup != null && popup.GetComponent<Popup>() != null) popup.GetComponent<Popup>().Close();
            await CheckAndLoadChildAccounts();
            if (autoRetryDone) { autoRetryDone = false; }

        }
        if (loading != null) loading.SetActive(false);
    }

    void Update()
    {
        // Intentionally left empty; connectivity retries are now driven by InternetConnectivityCheck.ConnectivityChanged
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected || autoRetryDone) return;
        autoRetryDone = true;
        Debug.Log("Internet connectivity restored (event), retrying ChildAuthentication");
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
