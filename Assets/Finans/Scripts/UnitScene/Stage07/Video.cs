
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ricimi;
using System;
using static IFirestoreEnums;
using System.Collections.Generic;
using Google.MiniJSON;
using System.Text.RegularExpressions;
//using YoutubePlayer.Components;
using System.Linq;
[RequireComponent(typeof(InternetConnectivityCheck))]
[RequireComponent(typeof(Trivia_Video))]

public class Video : Elevator
{
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private GameObject loading;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject screenContent;
    [SerializeField] private Image childPic;
    [SerializeField] private GameObject messageBoxPopupPrefab;

    [Header("Private Fields")]
    private GameObject popup;
    private InternetConnectivityCheck internetConnectivityCheck;
    private DateTime currentDT;
    private const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
    private static Regex regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
    private static string[] validAuthorities = { "youtube.com", "www.youtube.com", "youtu.be", "www.youtu.be" };
    private string context = "Video";

    [Header("Public Fields")]
    public IFirestoreOperator FirestoreClient;
    public Dictionary<string, object> trivias;
    public string unitLevel = "";
    public string buttonName = "";
    public int level = 1;
    // InvidiousVideoPlayer invidiousVideoplayer;
    void Awake()
    {
        if (PlayerInfo.IsAppAuthenticated)
        {
            FirestoreClient = new FirestoreDataOperationManager();
            foreach (KeyValuePair<string, string> button in PlayerInfo.UnitButtonInfo)
            {
                unitLevel = button.Key;
                buttonName = button.Value;
            }
        }
        else
        {
            Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }

    }

    async void Start()
    {
        GameObject content = GameObject.FindWithTag("Level");
        if (content) { content.GetComponent<UnloadSceneAdditiveAsync>().toggleContent.SetActive(false); }
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
        }
        else
        {
            Debug.LogError("[Video] InternetConnectivityCheck component not found on Video");
        }
        currentDT = ServerDateTime.GetFastestNISTDate();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();


        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await LoadVideoData();

        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            loading.SetActive(false);

        }
    }
    async Task LoadVideoData()
    {
        try
        {
            //Disable below await FirestoreClient.GetFirestoreDocument as this is already called on child authentication in LocalAutheentication.cs file
            if (!Params.ChildDataloaded)
            {
                FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);
                Debug.Log($"Child data reloading done.....");
            }
            /*  invidiousVideoplayer = youtubePlayer.GetComponent<InvidiousVideoPlayer>();
              Debug.Log($"Found {invidiousVideoplayer.VideoId}");
              invidiousVideoplayer.enabled = true;*/
            Dictionary<string, object> unitStageFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_data.ToString());
            Dictionary<string, object> currentUnitData = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];
            Debug.Log($"Current unit for {buttonName} is {$"unit{unitLevel}"}");
            if (currentUnitData.ContainsKey(FSMapField.video.ToString()))
            {
                Dictionary<string, object> videoData = (Dictionary<string, object>)currentUnitData[FSMapField.video.ToString()];

                await PrepareVideoData(videoData);
            }
            else
            {
                Debug.Log($"Video mapdata inside unit stage data for {$"unit{unitLevel}"} not found..... ");
                await CreateVideoData();

            }
            LoadCProfileAndFinalizeScreen(childPic, displayName, screenContent, loading);


        }
        catch (Exception ex)
        {
            Debug.Log($"Error fetching unit video FS data:: {ex.Message}");
        }
    }
    async void RetryTheAction()
    {
        loading.SetActive(value: true);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (internetConnectivityCheck.ConnectionStatus) { internetConnectivityCheck.ConnectionStatus = false; }
            if (popup.GetComponent<Popup>() != null) { popup.GetComponent<Popup>().Close(); }
            await LoadVideoData();

        }
        loading.SetActive(false);
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Debug.Log("Connectivity restored in Video screen, retrying action");
        RetryTheAction();
    }

    void OnDestroy()
    {
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
        }
    }

    async Task CreateVideoData()
    {
        Dictionary<string, object> data = new Dictionary<string, object>(){
            {FSMapField.progress_data.ToString(), new Dictionary<string, object>(){
                    {ProgressData.current_stage_name.ToString(), buttonName}
                }},
             {FSMapField.unit_stage_btn_status.ToString(), new Dictionary<string, object>(){
                    {$"unit{unitLevel}", new Dictionary<string, object>(){
                    {UnitStageButtonStatus.video.ToString(), true}}}}},
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                {$"unit{unitLevel}", new Dictionary<string, object>(){
                    {FSMapField.video.ToString(), new Dictionary<string, object>(){
                        {Videos.level.ToString(), 1},
                            {Videos.levels.ToString(), new Dictionary<string, object>(){
                                {$"{unitLevel}",  new Dictionary<string, object>(){
                                    {Videos.id.ToString(), "5w85_aCzf0o"},
                                    {Videos.date.ToString(), currentDT.ToShortDateString()},
                                    {Videos.played.ToString(), false}
                                }}
                            }}
                        }}
                    }}
                }}
            };
        Debug.Log($"Creating video screen data {Json.Serialize(data)}");
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                data);
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogError($"No connection found while creating initial video data", context);
        }
    }

    async Task PrepareVideoData(Dictionary<string, object> _videodata)
    {
        level = Convert.ToInt32(_videodata[Videos.level.ToString()]);
        string formattedLevel = level < 10 ? $"0{level}" : $"{level}";
        Dictionary<string, object> levelsData = (Dictionary<string, object>)_videodata[Videos.levels.ToString()];
        Dictionary<string, object> levelGameData = (Dictionary<string, object>)levelsData[formattedLevel];
        bool played = Convert.ToBoolean(levelGameData[Videos.played.ToString()]);
        DateTime.TryParse((string)levelGameData[Videos.date.ToString()], out DateTime videoDate);
        bool valid = DateTime.Compare(currentDT.Date, videoDate.Date) > 0;
        if (valid && played) //new day
        {
            level++;
            string newFormattedLevel = level < 10 ? $"0{level}" : $"{level}";
            Debug.Log($"New video available...");
            Dictionary<string, object> data = new Dictionary<string, object>(){
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                 {$"unit{unitLevel}", new Dictionary<string, object>(){
                       {FSMapField.quizzes.ToString(), new Dictionary<string, object>(){
                        {buttonName, new Dictionary<string, object>(){
                            {Convert.ToString(level), false}
                            }}
                        }}
                    }},
                {$"unit{unitLevel}", new Dictionary<string, object>(){
                    {FSMapField.video.ToString(), new Dictionary<string, object>(){
                        {Videos.level.ToString(), level},
                          {Videos.levels.ToString(), new Dictionary<string, object>(){
                                {$"{newFormattedLevel}",  new Dictionary<string, object>(){
                                    {Videos.id.ToString(), "some youtube id"},
                                    {Videos.played.ToString(), false},
                                }}
                            }}
                        }}
                    }}
                }}
           };
            Debug.Log("Updating videos data to FS...............");
            if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
            {
                await FirestoreClient.FirestoreDataSave(
                    FSCollection.parent.ToString(),
                    PlayerInfo.AuthenticatedID,
                    FSCollection.children.ToString(),
                    PlayerInfo.AuthenticatedChildID,
                    data);
            }
            else
            {
                popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
                Logger.LogError($"No connection found while updating video data", context);
                return;
            }
            /*invidiousVideoplayer.VideoId = "some youtube id";
             invidiousVideoplayer.enabled = true;*/
            CreateVideoScreen("some youtube id");
            // setPlayButton.SetActive(true);
        }
        else //same day
        {
            int currentLevel = Convert.ToInt32(_videodata[Videos.level.ToString()]);
            string currentFormattedLevel = currentLevel < 10 ? $"0{currentLevel}" : $"{currentLevel}";
            //  bool currentVideoPlayed = Convert.ToBoolean(((Dictionary<string, object>)_videodata[currentFormattedLevel])[Videos.played.ToString()]);
            string videoID = (string)((Dictionary<string, object>)((Dictionary<string, object>)_videodata[Videos.levels.ToString()])[currentFormattedLevel])[Videos.id.ToString()];
            /*  if (!currentVideoPlayed)
              {

              }*/
            CreateVideoScreen(videoID);
        }


        Debug.Log($"Video data found, its loaded ready for creating the screen content");
    }

    void CreateVideoScreen(string _id)
    {
        /*invidiousVideoplayer.VideoId = videoID;
            invidiousVideoplayer.enabled = true;*/
    }

    async Task OnVideoPlayComplete()
    {
        Dictionary<string, object> data = new Dictionary<string, object>(){
                    {$"unit{unitLevel}", new Dictionary<string, object>(){
                            {FSMapField.video.ToString(), new Dictionary<string, object>(){
                                {Videos.level.ToString(), level},
                                {Videos.levels.ToString(), new Dictionary<string, object>(){
                                        {$"currentFormattedLevel",  new Dictionary<string, object>(){
                                            {Videos.date.ToString(), currentDT.ToShortDateString()},
                                            {Videos.played.ToString(), true},
                                        }}
                                    }}
                                }}
                            }}
                        };
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                data);
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogError($"No connection found while marking video play complete", context);
        }
    }
    public string ExtractVideoIdFromUri(Uri uri)
    {
        try
        {
            string authority = new UriBuilder(uri).Uri.Authority.ToLower();

            //check if the url is a youtube url
            if (validAuthorities.Contains(authority))
            {
                //and extract the id
                var regRes = regexExtractId.Match(uri.ToString());
                if (regRes.Success)
                {
                    return regRes.Groups[1].Value;
                }
            }
        }
        catch { }


        return null;
    }
}
