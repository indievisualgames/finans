using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using UnityEngine.Networking;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;

using Ricimi;
using static IFirestoreEnums;
using TMPro;





#if UNITY_EDITOR
using UnityEditor.Events;
#endif
[RequireComponent(typeof(InternetConnectivityCheck))]


public class StoryBook : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject timerObject;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject screenContent;
    [SerializeField] private Image childPic;
    [SerializeField] private GameObject gamePlayScreen;
    [SerializeField] private GameObject playTriviaQuizButton;
    [SerializeField] private GameObject collectTriviaPassButton;
    [SerializeField] private GameObject gameQuizComplete;
    [SerializeField] private GameObject quizReadyToPlay;
    [SerializeField] private GameObject passesNotCollected;
    // [SerializeField] private Trivia_Vocabs triviaScript;
    [SerializeField] private GameObject messageBoxPopupPrefab;


    [Header("Private Fields")]
    private InternetConnectivityCheck internetConnectivityCheck;
    private IFirestoreOperator FirestoreClient;
    private Trivia_Vocabs triviaScript;
    private GameObject popup;
    private DateTime currentDT;
    Dictionary<string, object> currentUnitData;

    [Header("Public Fields")]
    public string unitLevel = "";
    public string buttonName = "";
    public int level = 1;




    FirebaseStorage storage;
    StorageReference storageRef;
    string pageDataPath;

    public class PageText
    {
        public string Title;
        public string SubTitle;
        public string Description;
    }
    /*TMP_Text pageOne;
    TMP_Text pageTwo;
    TMP_Text pageThree;
    TMP_Text pageFour;
    TMP_Text pageFive;*/
    [SerializeField]
    TMP_Text[] pages;



















    PageText pageText = new PageText();
    // Start is called before the first frame update
    void Awake()
    {
        FirestoreClient = new FirestoreDataOperationManager();
        ////  storage = FirebaseStorage.DefaultInstance;
        ////  storageRef = storage.GetReferenceFromUrl("gs://eparent-finans.appspot.com/");

        foreach (KeyValuePair<string, string> button in PlayerInfo.UnitButtonInfo)
        {
            unitLevel = button.Key;
            buttonName = button.Value;
        }
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        pageDataPath = $"{Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}";

        /*  Debug.Log($"Strorage ref path is childdashboard/unit{unitLevel}/{buttonName}/json/{unitLevel}.json");
         StorageReference collectablesRef = storageRef.Child($"childdashboard/unit{unitLevel}/{buttonName}/json/{unitLevel}.json");
          _ = collectablesRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
                 {
                     if (!task.IsFaulted && !task.IsCanceled)
                     {
                         StartCoroutine(LoadCollectablesJSON(Convert.ToString(task.Result)));
                         Debug.Log($"Debug.Log {Convert.ToString(task.Result)}");
                     }
                     else
                     {
                         Debug.Log($"Debug.Log::::::Exception occured {task.Exception}");
                     }
                 });
                 */
        Debug.Log($"unitLevel is unit{unitLevel} buttonName is {buttonName}");
    }
    async void Start()
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            SceneLoad();
        }
        else
        {
            CreateAndShowMessageBox();
        }
    }
    void CreateAndShowMessageBox()
    {
        Debug.Log($"Debug.Log:------------------Debug log: No connection found ");
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
    async void SceneLoad()
    {
        Dictionary<string, object> data = await FirestoreClient.GetFirestoreDataField(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID, FSMapField.unit_stage_data.ToString());
        foreach (var keyItem in data)
        {
            if (keyItem.Key == $"unit{unitLevel}")
            {
                //Debug.Log($"Debug.Log:-SceneLoadBase:------------------{keyItem.Key} ");
                foreach (var map in data[keyItem.Key] as Dictionary<string, object>)
                {
                    // if (map.Key == buttonName)
                    if (map.Key == "storybook")
                    {

                        Debug.Log($"Debug Log:-SceneLoad:------------------{map.Key} ");
                        foreach (var item in data[map.Key] as Dictionary<string, object>)
                        { Debug.Log($"Debug Log:-SceneLoad1:------------------{map.Key} "); }


                    }
                }
            }
        }
        for (int i = 1; i < 14; i++)
        {
            StartCoroutine(LoadPageTextJSON(i));
        }
        /*
         screenContent.SetActive(true);
         loadingActivity.SetActive(false);
         */
    }

    // Update is intentionally left empty; connectivity retries are event-driven via InternetConnectivityCheck.ConnectivityChanged
    void Update()
    {
    }

    async void RetryTheAction()
    {
        loading.SetActive(value: true);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {

            if (internetConnectivityCheck.ConnectionStatus) { internetConnectivityCheck.ConnectionStatus = false; }
            if (popup.GetComponent<Popup>() != null) { popup.GetComponent<Popup>().Close(); }
            SceneLoad();


        }

    }

    private void OnEnable()
    {
        internetConnectivityCheck = internetConnectivityCheck != null ? internetConnectivityCheck : GetComponent<InternetConnectivityCheck>();
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
        }
        else
        {
            Debug.LogError("InternetConnectivityCheck component not found on StoryBook");
        }
    }

    private void OnDisable()
    {
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
        }
    }

    private void OnConnectivityRestored(bool isConnected)
    {

        Debug.Log("Connectivity restored in StoryBook, retrying action");
        RetryTheAction();
    }
    IEnumerator LoadPageTextJSON(int _pageNo)
    {
        string JSONUrl = $"{pageDataPath}/json/{_pageNo}.json";
        UnityWebRequest request = UnityWebRequest.Get(JSONUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        Debug.Log($"Loading json for storybook from path {JSONUrl}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            pageText = JsonUtility.FromJson<PageText>(request.downloadHandler.text);
            pages[_pageNo].text = pageText.Title;
            // Debug.Log($"Loaded JSON {JsonUtility.FromJson<Collectables>(request.downloadHandler.text)}");
            Debug.Log($"Loaded JSON {pageText.SubTitle}");
        }
        else
        {
            Debug.Log(request.error);
        }
        StartCoroutine(LoadPageImage(_pageNo));
    }

    IEnumerator LoadPageImage(int _pageNo)
    {
        string url = $"{pageDataPath}/images/{_pageNo}.png";
        UnityWebRequest uwr = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        uwr.downloadHandler = texDl;
        Debug.Log($"Loading images for storybook page from path {url}");
        yield return uwr.SendWebRequest();
        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Texture2D t = texDl.texture;
            Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                Vector2.zero, 1f);

            pages[_pageNo].transform.parent.parent.GetComponent<Image>().sprite = s;
            Debug.Log($"Image is attached to {pages[_pageNo].transform.parent.parent.name}");

        }
        IsLoadingComplete(_pageNo);
    }
    void IsLoadingComplete(int __pageNo)
    {
        //  Debug.Log($"__pageNo is {__pageNo} and pages length is {pages.Length}");
        if (pages.Length == __pageNo + 1)
        {
            screenContent.SetActive(true);
            loading.SetActive(false);
        }
    }

}
