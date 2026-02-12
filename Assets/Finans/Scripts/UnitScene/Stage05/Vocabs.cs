
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Ricimi;
using static IFirestoreEnums;
using Google.MiniJSON;

[RequireComponent(typeof(InternetConnectivityCheck))]
public class Vocabs : Elevator
{
    [Header("UI References")]
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject timerObject;
    [SerializeField] public Canvas canvas;
    [SerializeField] private GameObject screenContent;
    [SerializeField] private Image childPic;
    [SerializeField] private GameObject gamePlayScreen;
    [SerializeField] private Trivia_Vocabs triviaScript;
    [SerializeField] private GameObject playTriviaQuizButton;
    [SerializeField] private GameObject collectTriviaPassButton;
    [SerializeField] private GameObject gameQuizComplete;
    [SerializeField] private GameObject quizReadyToPlay;
    [SerializeField] private GameObject passesNotCollected;
    [SerializeField] private GameObject messageBoxPopupPrefab;

    [Header("Private Fields")]
    private InternetConnectivityCheck internetConnectivityCheck;
    private IFirestoreOperator FirestoreClient;
    private GameObject popup;
    private DateTime currentDT;
    Dictionary<string, object> currentUnitData;
    private Dictionary<string, object> vocabWordData = new Dictionary<string, object>();
    private Dictionary<string, object> loadedVocabsWordData = new Dictionary<string, object>();
    private Dictionary<string, object> quizzes;
    private bool triviaPass = false;
    private bool played;
    private bool finalizeScreen = false;
    private readonly string context = "Vocabs";

    [Header("Public Fields")]

    public Image backgroundHideImage;
    public GameObject gameManagerGO;
    public GameObject starContainer;
    public string unitLevel = "";
    public string buttonName = "";
    public WordData[] wordsData;
    public Image questionDisplayImg;
    public int level = 1;


    void Awake()
    {
        if (!PlayerInfo.IsAppAuthenticated)
        {
            Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
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
        await LoadVocabsWordData();
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
        Logger.LogInfo($"Re Loading data from FS", context);
        FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);
        Logger.LogInfo($"Child data reloaded from Firestore", context);
        await LoadVocabsWordData();
    }
    private bool TryResolveCurrentUnitData()
    {
        Dictionary<string, object> unitStageFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_data.ToString());
        if (unitStageFSData == null || !unitStageFSData.ContainsKey($"unit{unitLevel}"))
        {
            Logger.LogError($"Unit{unitLevel} data not found in Firestore", context);
            return false;
        }

        currentUnitData = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];
        return true;
    }

    private async Task InitializeTriviaState()
    {
        quizzes = (Dictionary<string, object>)currentUnitData[FSMapField.quizzes.ToString()];
        triviaScript.trivias = (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];

        if (level == 1)
        {
            triviaPass = Convert.ToBoolean(Convert.ToString(
                ((Dictionary<string, object>)
                    ((Dictionary<string, object>)
                        ((Dictionary<string, object>)currentUnitData[FSMapField.maingame.ToString()])[MainGame.levels.ToString()])
                    [$"0{(int)UnitStageButtonStatus.vocabs}"])[MainGame.vocabs_trivia_collected.ToString()]));

            Logger.LogInfo($"Checking trivia pass for level {level}. Found collected: {triviaPass}", context);
        }
    }

    async Task LoadVocabsWordData()
    {
        try
        {
            if (!TryResolveCurrentUnitData())
            {
                return;
            }

            Logger.LogInfo($"Unit{unitLevel} data loaded from FS :: {Json.Serialize(currentUnitData)}", context);

            await InitializeTriviaState();

            if (currentUnitData.ContainsKey(FSMapField.vocabs.ToString()))
            {
                Dictionary<string, object> vocabsData = (Dictionary<string, object>)currentUnitData[FSMapField.vocabs.ToString()];

                triviaScript.level = level = Convert.ToInt32(vocabsData[Vocab.level.ToString()]);

                Logger.LogInfo($"Loaded Unit{unitLevel} contains vocabs data::and the current level is {level}", context);
                await CreateVocabsScreen(vocabsData);
            }
            else
            {
                Logger.LogInfo($"{buttonName} data for {$"Unit{unitLevel}"} not found..... ", context);
                StartCoroutine(LoadVocabJSONToCreateLevel(1));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unable to fetch player data", context, ex);
        }
    }

    private async Task CreateVocabsScreen(Dictionary<string, object> vocabsData)
    {
        string formattedLevel = level < 10 ? $"0{level}" : $"{level}";

        Dictionary<string, object> levelsData = (Dictionary<string, object>)vocabsData[Vocab.levels.ToString()];

        Dictionary<string, object> levelGameData = (Dictionary<string, object>)levelsData[formattedLevel];

        played = Convert.ToBoolean(levelGameData[Vocab.played.ToString()]);

        bool quizPlayed = false;
        if (quizzes != null && quizzes.ContainsKey(buttonName))
        {
            var btnMap = (Dictionary<string, object>)quizzes[buttonName];
            if (btnMap.ContainsKey(level.ToString()))
            {
                quizPlayed = Convert.ToBoolean(btnMap[level.ToString()]);
            }
        }

        Logger.LogInfo($"Vocabs level {level} played status: {played}, trivia quiz played status: {quizPlayed}", context);
        loadedVocabsWordData = (Dictionary<string, object>)levelGameData[Vocab.words.ToString()];
        if (played && quizPlayed)
        {
            DateTime.TryParse((string)levelGameData[Vocab.date.ToString()], out DateTime gameDT);

            bool valid = DateTime.Compare(currentDT.Date, gameDT.Date) > 0;

            if (valid
             && ReturnQuizLevelPlayedStatus(
                 level,
                 triviaPass,
                 (Dictionary<string, object>)quizzes[Quizzes.vocabs.ToString()],
                 context)) //new day
            {
                level++;
                triviaScript.level = level;
                Logger.LogInfo($"All condition check for new level {level} passed, creating new vocabs level..", context);
                StartCoroutine(LoadVocabJSONToCreateLevel(level));
                return;
            }
            else
            {
                Logger.LogInfo($"Vocabs game level {level} is played alongwith quiz, but the date condition is not valid for new level...", context);
                gameQuizComplete.SetActive(true);
                loading.SetActive(false);
                return;
            }
        }
        else if (played && !quizPlayed)
        {
            if (PlayerInfo.SceneReload)
            {
                if (TryGetComponent<ScreenTimer>(out var sT)) { sT.startTimer = true; }
                PlayerInfo.SceneReload = false;
                finalizeScreen = true;
                Logger.LogInfo($"As this is quiz reattempt, starting screen from unanswered quiz..", context);
                ShowTriviaQuiz();
                return;
            }

            Logger.LogInfo($"Vocabs game level {level} is played but quiz completion is pending...checking other conditions", context);

            Dictionary<string, object> q = (Dictionary<string, object>)quizzes[Quizzes.vocabs.ToString()];


            if (CheckQuizPlayedStatus(level, triviaPass, q, quizReadyToPlay, passesNotCollected, $"0{(int)UnitStageButtonStatus.vocabs}", context))
            {
                quizReadyToPlay.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.FirstPartCompleteOnLoad;
                finalizeScreen = true;

            }
            else
            {
                passesNotCollected.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.CollectTriviaPassOnLoad($"0{(int)UnitStageButtonStatus.vocabs}");
            }

            loading.SetActive(false);
        }
        else //same day
        {
            Logger.LogInfo($"Vocabs game level {level} not completed..loading same level data and creating screen.", context);
            GetStreamingAssetRefAndLoad(level.ToString());
        }
    }

    IEnumerator LoadVocabJSONToCreateLevel(int __level)
    {
        string url_json = $"{Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}/json/{__level}.json";
        UnityWebRequest request = UnityWebRequest.Get(url_json);
        request.downloadHandler = new DownloadHandlerBuffer();
        Logger.LogInfo($"Loading json for vocabs word from path {url_json}", context);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            wordsData = JsonUtility.FromJson<WordsData>(request.downloadHandler.text).VocabQuestionsData;
            for (int i = 0; i < wordsData.Length; i++)
            {
                Logger.LogInfo($"Loaded vocabs questions no {wordsData[i].QuesNo} has word {wordsData[i].QuesWord}", context);
                vocabWordData.Add(wordsData[i].QuesNo, true);
                StartCoroutine(LoadVocabImage(wordsData[i].ImageURL));
            }
        }
        else
        {
            Logger.LogError(request.error);
        }
        StartCoroutine(LoadQuizJSONToCreateLevel(level));

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
                    {FSMapField.vocabs.ToString(), new Dictionary<string, object>(){
                        {Vocab.level.ToString(), level},
                          {Vocab.levels.ToString(), new Dictionary<string, object>(){
                                {$"{newFormattedLevel}",  new Dictionary<string, object>(){
                                    {Vocab.date.ToString(), currentDT.ToShortDateString()},
                                   { Vocab.played.ToString(), false},
                                    {Vocab.words.ToString(), vocabWordData}
                                }},
                            }}
                        }},
                    {
                            FSMapField.trivia.ToString(), new Dictionary <string, object>(){
                               {buttonName, new Dictionary<string, object>(){
                                  {Vocab.levels.ToString(), new Dictionary<string, object>(){
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
            if (!buttonDict.ContainsKey(Vocab.levels.ToString()))
            {
                buttonDict.Add(Vocab.levels.ToString(), new Dictionary<string, object>() { });
            }

            var levelsDict = (Dictionary<string, object>)buttonDict[Vocab.levels.ToString()];
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
            var triviaPath = (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()])[buttonName])[Vocab.levels.ToString()];
            triviaPath.Add(level.ToString(), newQuizData);
            debugMessage = $"Updating {buttonName} with new level {level} data before creating game screen";
        }

        triviaScript.trivias = (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];
        triviaScript.level = level;
        Logger.LogInfo(debugMessage, context);

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await ActionOnCreateNewLevelData(data);
        }
        else
        {
            retryAction += async () => await ActionOnCreateNewLevelData(data);
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
            Logger.LogInfo($"No connection found while saving new vocabs level data", context);
        }
    }

    IEnumerator LoadVocabJSON(string JSONUrl)
    {
        UnityWebRequest request = UnityWebRequest.Get(JSONUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        Logger.LogInfo($"Loading json for vocabs word from path {JSONUrl}", context);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            //wordsData = JsonUtility.FromJson<WordsData>(request.downloadHandler.text).VocabQuestionsData;
            WordData[] tempWordsData = JsonUtility.FromJson<WordsData>(request.downloadHandler.text).VocabQuestionsData;
            List<WordData> wordDataList = new List<WordData>();

            for (int i = 0; i < tempWordsData.Length; i++)
            {
                if (loadedVocabsWordData != null && loadedVocabsWordData.ContainsKey(tempWordsData[i].QuesNo) && Convert.ToBoolean(loadedVocabsWordData[tempWordsData[i].QuesNo]))
                {
                    Logger.LogInfo($"Loaded vocabs word data has question {tempWordsData[i].QuesWord} available", context);
                    wordDataList.Add(tempWordsData[i]);

                    StartCoroutine(LoadVocabImage(tempWordsData[i].ImageURL));
                }
            }
            wordsData = wordDataList.ToArray();
        }
        else
        {
            Logger.LogError("Error occured during JSON loading", request.error);
        }
        gameManagerGO.GetComponent<GameManagerGF>().enabled = true;

        LoadCProfileAndFinalizeScreen(childPic, displayName, screenContent, loading);
        Logger.LogInfo($"Ready to play vocabs word puzzle", context);
    }
    public IEnumerator LoadVocabImage(string image_name)
    {
        string url_image = $"{Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}/images/{image_name}.png";

        UnityWebRequest uwr = new UnityWebRequest(url_image);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        uwr.downloadHandler = texDl;
        yield return uwr.SendWebRequest();
        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Texture2D t = texDl.texture;
            //t.alphaIsTransparency = true;
            Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                Vector2.zero, 1f);
            questionDisplayImg.sprite = s;
            Logger.LogInfo($"Loading image for vocabs word question from {Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}/images/{image_name}.png ", context);
        }
    }

    public async void MarkGamePlayed()
    {
        ScreenTimer screenTimer = timerObject.GetComponent<ScreenTimer>();
        screenTimer.startTimer = false;
        string formattedLevel = level < 10 ? $"0{level}" : $"{level}";
        Logger.LogInfo($"The level {formattedLevel} will now be marked as played....", context);
        Dictionary<string, object> data = new Dictionary<string, object>(){
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                {$"unit{unitLevel}", new Dictionary<string, object>(){
                    {FSMapField.vocabs.ToString(), new Dictionary<string, object>(){
                          {Vocab.levels.ToString(), new Dictionary<string, object>(){
                                {$"{formattedLevel}",  new Dictionary<string, object>(){
                                    { Vocab.played.ToString(), true}
                                }}
                            }}
                        }}
                    }}
                }}
   };
        Logger.LogInfo($"Marking vocabs level {formattedLevel} played status as played...", context);

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                data);

            Dictionary<string, object> q = (Dictionary<string, object>)quizzes[Quizzes.vocabs.ToString()];
            if (CheckQuizPlayedStatus(level, triviaPass, q, playTriviaQuizButton, collectTriviaPassButton, $"0{(int)UnitStageButtonStatus.vocabs}", context))
            {
                //  notPlayed.SetActive(true);
                playTriviaQuizButton.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.FirstPartComplete;
            }
            else
            {
                collectTriviaPassButton.GetComponent<PrefabTextHandler>().display.text = Message.CollectTriviaPassOnMarked($"0{(int)UnitStageButtonStatus.vocabs}");
            }
            await Task.Delay(3000);
            gamePlayScreen.SetActive(false);
            screenTimer.ResetTimer();
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogInfo($"No connection found while marking vocabs level played", context);
        }
    }

    public async void UpdateScore(string _quesNo)
    {
        string newFormattedLevel = level < 10 ? $"0{level}" : $"{level}";
        Dictionary<string, object> newLevelData = new Dictionary<string, object>(){
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                 {$"unit{unitLevel}", new Dictionary<string, object>(){
                    {FSMapField.vocabs.ToString(), new Dictionary<string, object>(){
                          {Vocab.levels.ToString(), new Dictionary<string, object>(){
                                {$"{newFormattedLevel}",  new Dictionary<string, object>(){
                                    {Vocab.words.ToString(), new Dictionary<string, object>(){
                                        {_quesNo, false }
                                    } }
                                }},
                            }}
                        }}
                    }}
                }}
           };
        foreach (var kvp in FirestoreData.UpdatePointScore(ScorePoint.VOCABS))
        {
            newLevelData[kvp.Key] = kvp.Value;
        }

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await ActionOnUpdateScore(newLevelData);
        }
        else
        {
            retryAction += async () => await ActionOnUpdateScore(newLevelData);
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
            Logger.LogInfo($"No connection found while updating vocabs score points", context);
        }
    }

    public async void DebitScore(int _points)
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                FirestoreData.DebitPointScore(ScorePoint.VOCABS, _points));
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogInfo($"No connection found while debiting vocabs score points", context);
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
    void GetStreamingAssetRefAndLoad(string fileName)
    {
        string url_json = $"{Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}/json/{fileName}.json";
        StartCoroutine(LoadVocabJSON(url_json));
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogInfo("Connectivity restored in Vocabs, retrying action", context);
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
        int __level = (int)UnitStageButtonStatus.vocabs;
        ReplayLevelForPassCollection(unitLevel, __level, buttonName, context);

    }
    public void ReloadScene()
    {
        PlayerInfo.SceneReload = true;
        Transition.LoadLevel(SceneName.Vocabs.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }

    public void GotohomeScene()
    {
        Transition.LoadLevel(SceneName.Level.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }
    /********************Actions on connection found*******************************************/

    async Task ActionOnCreateNewLevelData(Dictionary<string, object> data)
    {
        await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                data);
        Logger.LogInfo($"Success...Updated new level vocabs data {Json.Serialize(data)}..", context);
        gameManagerGO.GetComponent<GameManagerGF>().enabled = true;
        LoadCProfileAndFinalizeScreen(childPic, displayName, screenContent, loading);
        Logger.LogInfo($"Vocabs new level {level} is now ready to play...", context);


    }


    async Task ActionOnUpdateScore(Dictionary<string, object> data)
    {
        await FirestoreClient.FirestoreDataSave(
              FSCollection.parent.ToString(),
              PlayerInfo.AuthenticatedID,
              FSCollection.children.ToString(),
              PlayerInfo.AuthenticatedChildID,
              data);
        Logger.LogInfo($"Updated vocabs score points", context);


    }
    [Serializable]
    public class WordData
    {

        public string QuesWord;
        public string ImageURL;
        public Sprite Image;
        public string Hint;
        public string SubTitle;
        public string QuesNo;

    }
    [Serializable]
    public class WordsData
    {
        public WordData[] VocabQuestionsData;
    }


}
