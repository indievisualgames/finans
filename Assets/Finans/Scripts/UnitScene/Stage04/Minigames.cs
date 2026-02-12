
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ricimi;
using System;
using static IFirestoreEnums;
using System.Collections.Generic;
using Google.MiniJSON;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(InternetConnectivityCheck))]
public class Minigames : Elevator
{
    [Header("UI References")]
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject timerObject;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject screenContent;
    [SerializeField] private Image childPic;
    [SerializeField] private GameObject gamePlayScreen;
    [SerializeField] private Trivia_Minigames triviaScript;
    [SerializeField] private GameObject playTriviaQuizButton;
    [SerializeField] private GameObject collectTriviaPassButton;
    [SerializeField] private GameObject gameQuizComplete;
    [SerializeField] private GameObject quizReadyToPlay;
    [SerializeField] private GameObject passesNotCollected;
    [SerializeField] private GameObject starContainer;
    [SerializeField] private GameObject coinTemplate;
    [SerializeField] private GameObject hintManager;
    [SerializeField] private GameObject messageBoxPopupPrefab;

    [Header("Private Fields")]
    private InternetConnectivityCheck internetConnectivityCheck;
    private IFirestoreOperator FirestoreClient;
    private ScreenTimer screenTimer;
    private GameObject popup;
    private DateTime currentDT;
    Dictionary<string, object> currentUnitData;
    private Dictionary<string, object> quizzes;
    private bool triviaPass;
    private bool played;
    private bool finalizeScreen = false;

    private readonly string context = "Minigames";

    [Header("Public Fields")]
    public string unitLevel = "";
    public string buttonName = "";
    public int level = 1;
    void Awake()
    {
        if (!PlayerInfo.IsAppAuthenticated)
        {
            Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
            return;
        }

        foreach (KeyValuePair<string, string> button in PlayerInfo.UnitButtonInfo)
        {
            unitLevel = button.Key;
            buttonName = button.Value;
        }

    }

    async void Start()
    {
        GameObject content = GameObject.FindWithTag(Tags.Level.ToString());
        if (content) { content.GetComponent<UnloadSceneAdditiveAsync>().toggleContent.SetActive(false); }

        FirestoreClient = new FirestoreDataOperationManager();
        InitializeConnectivityAndCanvas();
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            currentDT = ServerDateTime.GetFastestNISTDate();
            if (!Params.ChildDataloaded)
            {
                await ReloadChildData();
                Logger.LogInfo($"Child data reloaded from Firestore", context);
                return;

            }
        }
        else
        {
            retryAction += async () => await ReloadChildData();
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
            loading.SetActive(false);
            Logger.LogError($"No connectivity found", context);
            return;
        }
        await LoadMinigameData();
    }

    private void InitializeConnectivityAndCanvas()
    {
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;

        var found = FindFirstObjectByType<Canvas>();
        if (found != null) { canvas = found; }
        else { Logger.LogError("Canvas not found in scene during start", context); }

    }
    private async Task ReloadChildData()
    {
        FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);
        Logger.LogInfo($"Re Loading data from FS", context);
        await LoadMinigameData();
    }
    private bool TryResolveCurrentUnitData()
    {
        Dictionary<string, object> unitStageFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_data.ToString());
        if (unitStageFSData == null || !unitStageFSData.ContainsKey($"unit{unitLevel}"))
        {
            Logger.LogError($"Unit{unitLevel} data not found in Firestore for minigames", context);
            return false;
        }

        currentUnitData = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];
        return true;
    }

    private void InitializeTriviaState()
    {
        quizzes = (Dictionary<string, object>)currentUnitData[FSMapField.quizzes.ToString()];
        triviaScript.trivias = (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];

        if (level == 1)
        {
            triviaPass = Convert.ToBoolean(Convert.ToString(
                ((Dictionary<string, object>)
                    ((Dictionary<string, object>)
                        ((Dictionary<string, object>)currentUnitData[FSMapField.maingame.ToString()])[MainGame.levels.ToString()])
                    [$"0{(int)UnitStageButtonStatus.minigames}"])[MainGame.minigames_trivia_collected.ToString()]));

            Logger.LogInfo($"Checking trivia pass for level {level}. Found collected: {triviaPass}", context);
        }
    }
    async Task LoadMinigameData()
    {
        try
        {


            if (!TryResolveCurrentUnitData())
            {
                return;
            }

            Logger.LogInfo($"Unit{unitLevel} data loaded from FS :: {Json.Serialize(currentUnitData)}", context);

            InitializeTriviaState();

            if (currentUnitData.ContainsKey(FSMapField.minigames.ToString()))
            {
                Dictionary<string, object> minigamesData = (Dictionary<string, object>)currentUnitData[FSMapField.minigames.ToString()];

                triviaScript.level = level = Convert.ToInt32(minigamesData[MiniGames.level.ToString()]);

                Logger.LogInfo($"Loaded Unit{unitLevel} contains minigames data and the current level is {level}", context);
                await CreateMiniGamesScreen(minigamesData);

            }
            else
            {
                Logger.LogInfo($"Minigames data for {$"Unit{unitLevel}"} not found..... ", context);
                StartCoroutine(LoadQuizJSONToCreateLevel(1));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unable to fetch player data", context, ex);
        }
    }
    private async Task CreateMiniGamesScreen(Dictionary<string, object> minigamesData)
    {
        string formattedLevel = level < 10 ? $"0{level}" : $"{level}";

        Dictionary<string, object> levelsData = (Dictionary<string, object>)minigamesData[MiniGames.levels.ToString()];

        Dictionary<string, object> levelGameData = (Dictionary<string, object>)levelsData[formattedLevel];

        played = Convert.ToBoolean(levelGameData[MiniGames.played.ToString()]);

        bool quizPlayed = false;
        if (quizzes != null && quizzes.ContainsKey(buttonName))
        {
            var btnMap = (Dictionary<string, object>)quizzes[buttonName];
            if (btnMap.ContainsKey(level.ToString()))
            {
                quizPlayed = Convert.ToBoolean(btnMap[level.ToString()]);
            }
        }

        Logger.LogInfo($"Minigames level {level} played status: {played}, trivia quiz played status: {quizPlayed}", context);

        if (played && quizPlayed)
        {
            DateTime.TryParse((string)levelGameData[MiniGames.date.ToString()], out DateTime gameDT);
            bool valid = DateTime.Compare(currentDT.Date, gameDT.Date) > 0;

            if (valid && ReturnQuizLevelPlayedStatus(level,
                         triviaPass,
                         (Dictionary<string, object>)quizzes[Quizzes.minigames.ToString()],
                         context)) //new day
            {
                level++;
                triviaScript.level = level;
                Logger.LogInfo($"All condition check for new level {level} passed, creating new minigame level..", context);
                StartCoroutine(LoadQuizJSONToCreateLevel(level));
                return;
            }
            else
            {
                Logger.LogInfo($"Minigames game level {level} is played alongwith quiz, but the date condition is not valid for new level...", context);
                gameQuizComplete.SetActive(true);
                loading.SetActive(false);
                return;
            }
        }
        else if (played && !quizPlayed)
        {
            gamePlayScreen.SetActive(false);
            if (PlayerInfo.SceneReload)
            {
                if (TryGetComponent<ScreenTimer>(out var sT)) { sT.startTimer = true; }
                PlayerInfo.SceneReload = false;
                finalizeScreen = true;
                Logger.LogInfo($"As this is quiz reattempt, starting screen from unanswered quiz..", context);
                ShowTriviaQuiz();
                return;
            }

            Logger.LogInfo($"Minigames level {level} is played but quiz completion is pending...checking other conditions", context);
            Dictionary<string, object> q = (Dictionary<string, object>)quizzes[Quizzes.minigames.ToString()];
            if (CheckQuizPlayedStatus(level, triviaPass, q, quizReadyToPlay, passesNotCollected, $"0{(int)UnitStageButtonStatus.minigames}", context))
            {
                quizReadyToPlay.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.FirstPartCompleteOnLoad;
                finalizeScreen = true;
            }
            else
            {
                passesNotCollected.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.CollectTriviaPassOnLoad($"0{(int)UnitStageButtonStatus.minigames}");
            }

            loading.SetActive(false);
        }
        else //same day
        {
            coinTemplate.GetComponent<MasterCoinGameManager>().coinsPerSession = level * 2;
            coinTemplate.GetComponent<MasterCoinGameManager>().timerManager.GetComponent<TimerManager>().initialTime = 15;//(level * 40) * 0.68f;
            if (level > 2)
            {
                hintManager.GetComponent<HintManager>().enableHints = false;
                Logger.LogInfo($"Disabling hints for minigame level {level}", context);
            }

            Logger.LogInfo($"Minigame level {level} not completed..loading same level data and creating screen.", context);

            gamePlayScreen.SetActive(true);

        }
        Logger.LogInfo($"Minigames level {level} is ready to play...", context);

        LoadCProfileAndFinalizeScreen(childPic, displayName, screenContent, loading);

    }
    IEnumerator LoadQuizJSONToCreateLevel(int __level)
    {
        string url_json_quiz = $"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/json/{buttonName}/{__level}.json";
        List<string> _quizData = new List<string>();
        Logger.LogInfo($"Trivia quiz data path is {url_json_quiz}", context);
        UnityWebRequest request = UnityWebRequest.Get(url_json_quiz);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            var loadedData = JsonUtility.FromJson<QuizzesData>(json: request.downloadHandler.text);
            for (int i = 0; i < loadedData.Quizzes.Length; i++)
            {
                //OLD quizCount.Add(i);
                //New Addtion below
                _quizData.Add(loadedData.Quizzes[i].Number);
            }

            Logger.LogInfo($"Trivia data json is loaded having quiz count to {_quizData.Count}", context);
        }
        TriggerCreateNewLevelData(_quizData);
    }
    async void TriggerCreateNewLevelData(List<string> __quizData)
    {
        await CreateNewLevelData(__quizData);
    }

    async Task CreateNewLevelData(List<string> quizData)
    {
        string debugMessage;// = string.Empty;
        Logger.LogInfo($"All condition check for new level {level} passed, creating new {buttonName} level..", context);
        Dictionary<string, object> newQuizData = FirestoreData.QuizQuestions(quizData);


        string newFormattedLevel = level < 10 ? $"0{level}" : $"{level}";
        Dictionary<string, object> data = new Dictionary<string, object>(){
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                 {$"unit{unitLevel}", new Dictionary<string, object>(){
                       {FSMapField.quizzes.ToString(), new Dictionary<string, object>(){
                        {buttonName, new Dictionary<string, object>(){
                            {Convert.ToString(level), false}
                            }}
                    }},
                    {FSMapField.minigames.ToString(), new Dictionary<string, object>(){
                        {MiniGames.level.ToString(), level},
                          {MiniGames.levels.ToString(), new Dictionary<string, object>(){
                                {$"{newFormattedLevel}",  new Dictionary<string, object>(){
                                     {MiniGames.date.ToString(), currentDT.ToShortDateString()},
                                 { MiniGames.played.ToString(), false},
                                }}
                            }}
                        }},
                    {
                            FSMapField.trivia.ToString(), new Dictionary <string, object>(){
                               {buttonName, new Dictionary<string, object>(){
                                  {MiniGames.levels.ToString(), new Dictionary<string, object>(){
                                       {level.ToString(), newQuizData}

                                }}}}}
                            }
                    }}
                }}
          };
        if (level == 1)
        {

            if (!currentUnitData.ContainsKey(FSMapField.trivia.ToString()))
            {
                currentUnitData.Add(FSMapField.trivia.ToString(), new Dictionary<string, object>() { });
                Logger.LogInfo($"There was no trivia map field present in FS..create one for {buttonName} with empty level map field", context);
            }

            var triviaDict = (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];
            if (!triviaDict.ContainsKey(buttonName))
            {
                triviaDict.Add(buttonName, new Dictionary<string, object>() { });
                Logger.LogInfo($"Trivia map field found but no {buttonName} map field was found...creating one with empty level map field");
            }

            var buttonDict = (Dictionary<string, object>)triviaDict[buttonName];
            if (!buttonDict.ContainsKey(MiniGames.levels.ToString()))
            {
                buttonDict.Add(MiniGames.levels.ToString(), new Dictionary<string, object>() { });
            }

            var levelsDict = (Dictionary<string, object>)buttonDict[MiniGames.levels.ToString()];
            // add the new level entry
            levelsDict.Add(level.ToString(), newQuizData);
            Dictionary<string, object> pDta = new Dictionary<string, object>(){
                    {ProgressData.current_stage_name.ToString(),  ToTitleCase(buttonName)}
                };
            data.Add(FSMapField.progress_data.ToString(), pDta);
            debugMessage = $"Creating {buttonName} level {level} data before creating game screen";
        }
        else
        {
            var triviaPath = (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()])[buttonName])[MiniGames.levels.ToString()];
            triviaPath.Add(level.ToString(), newQuizData);
            debugMessage = $"Updating {buttonName} with new level {level} data before creating game screen";
        }
        triviaScript.trivias = (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];
        triviaScript.level = level;
        coinTemplate.GetComponent<MasterCoinGameManager>().coinsPerSession = level * 2;
        coinTemplate.GetComponent<MasterCoinGameManager>().timerManager.GetComponent<TimerManager>().initialTime = (level * 40) * 0.68f; ;
        if (level > 4)
        {
            hintManager.GetComponent<HintManager>().enableHints = false;
        }
        Logger.LogInfo(debugMessage, context);


        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                data);
            Logger.LogInfo($"Success...Updated minigame data {Json.Serialize(data)}..", context);
            gamePlayScreen.SetActive(true);
            LoadCProfileAndFinalizeScreen(childPic, displayName, screenContent, loading);
            Logger.LogInfo($"Minigame new level {level} is now ready to play...", context);
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogInfo($"No connection found while saving new minigame level data", context);
        }
    }
    public async void MarkGamePlayed()
    {
        screenTimer = timerObject.GetComponent<ScreenTimer>();
        screenTimer.startTimer = false;
        string formattedLevel = level < 10 ? $"0{level}" : $"{level}";
        Dictionary<string, object> data = new Dictionary<string, object>(){
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                {$"unit{unitLevel}", new Dictionary<string, object>(){
                    {FSMapField.minigames.ToString(), new Dictionary<string, object>(){
                          {MiniGames.levels.ToString(), new Dictionary<string, object>(){
                                {$"{formattedLevel}",  new Dictionary<string, object>(){
                                    { MiniGames.played.ToString(), true}
                                }}
                            }}
                        }}
                    }}
                }}
   };
        Logger.LogInfo($"Marking minigame level {formattedLevel} played status as played...", context);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                data);


            Dictionary<string, object> q = (Dictionary<string, object>)quizzes[Quizzes.minigames.ToString()];
            if (CheckQuizPlayedStatus(level, triviaPass, q, playTriviaQuizButton, collectTriviaPassButton, $"0{(int)UnitStageButtonStatus.minigames}", context))
            {
                playTriviaQuizButton.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.FirstPartComplete;
            }
            else
            {
                collectTriviaPassButton.GetComponent<PrefabTextHandler>().display.text = Message.CollectTriviaPassOnMarked($"0{(int)UnitStageButtonStatus.minigames}");
            }
            await Task.Delay(2000);
            gamePlayScreen.SetActive(false);
            screenTimer.ResetTimer();
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogInfo($"No connection found while marking minigame level played", context);
        }
    }
    public async void ShowTriviaQuiz()
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            loading.SetActive(true);
            triviaScript.enabled = true;
            gamePlayScreen.SetActive(false);
            if (finalizeScreen)
            {
                LoadCProfileAndFinalizeScreen(childPic, displayName, screenContent, loading);
            }
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogInfo($"No connection found ", context);

        }
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogInfo("Connectivity restored in Minigames, retrying action", context);
        RetryTheAction(popup);
    }

    void OnDestroy()
    {
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
        }
    }
    public void ReplayGameToCollectPass()
    {
        int __level = (int)UnitStageButtonStatus.minigames;
        ReplayLevelForPassCollection(unitLevel, __level, buttonName, context);

    }
    public void ReloadScene()
    {
        PlayerInfo.SceneReload = true;
        Transition.LoadLevel(SceneName.MiniGames.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }

    public void GotohomeScene()
    {
        Transition.LoadLevel(SceneName.Level.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }



}

