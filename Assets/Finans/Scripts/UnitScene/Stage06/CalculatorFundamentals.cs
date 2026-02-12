
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;

using Ricimi;
using static IFirestoreEnums;
using Google.MiniJSON;
using UnityEngine.Networking;
using System.Collections;
using Game.Categories.NumberKeys;

[RequireComponent(typeof(InternetConnectivityCheck))]

public class CalculatorFundamentals : Elevator
{
    /*  [Header("UI References")]
      [SerializeField] private TMP_Text displayName;
      [SerializeField] private GameObject loading;
      [SerializeField] private GameObject timerObject;
      [SerializeField] private Canvas canvas;
      [SerializeField] private GameObject screenContent;
      [SerializeField] private Image childPic;
      [SerializeField] private GameObject gamePlayScreen;
      [SerializeField] private Trivia_Calculator triviaScript;
      [SerializeField] private GameObject playTriviaQuizButton;
      [SerializeField] private GameObject collectTriviaPassButton;
      [SerializeField] private GameObject gameQuizComplete;
      [SerializeField] private GameObject quizReadyToPlay;
      [SerializeField] private GameObject passesNotCollected;
      [SerializeField] private GameObject[] operatorButtons;
      [SerializeField] private Sprite operatorButtonImage;
      [SerializeField] private GameObject bubbleSpawnerGO;
      [SerializeField] private GameObject questionButtonGO;
      [SerializeField] private GameObject numberKeysControllerGO;
      [SerializeField] private GameObject starContainer;
      [SerializeField] private GameObject messageBoxPopupPrefab;

      [Header("Private Fields")]
      private InternetConnectivityCheck internetConnectivityCheck;
      private IFirestoreOperator FirestoreClient;

      private GameObject popup;
      private DateTime currentDT;
      Dictionary<string, object> currentUnitData;
      private Dictionary<string, object> quizzes;
      private bool triviaPass = false;
      private bool played;
      private int timesNumButtonClicked = 0;
      private bool finalizeScreen = false;
      private string context = "CalculatorFundamentals";
      [SerializeField] private BubbleSpawner bubbleSpawner;
      [SerializeField] private NumberKeysController numberKeysController;

      [Header("Public Fields")]
      public string unitLevel = "";
      public string buttonName = "";
      public int level = 1;
  */

    [Header("UI References")]
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject timerObject;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject contentContainer;
    [SerializeField] private Image childPic;
    [SerializeField] private GameObject[] gamePlayScreen;
    [SerializeField] private Trivia_Calculator triviaScript;
    [SerializeField] private GameObject playTriviaQuizButton;
    [SerializeField] private GameObject collectTriviaPassButton;
    [SerializeField] private GameObject gameQuizComplete;
    [SerializeField] private GameObject quizReadyToPlay;
    [SerializeField] private GameObject passesNotCollected;
    [SerializeField] private GameObject starContainer;
    [SerializeField] private GameObject messageBoxPopupPrefab;
    [SerializeField] private BubbleSpawner bubbleSpawner;
    [SerializeField] private GameObject bubbleSpawnerGO;
    [SerializeField] private NumberKeysController numberKeysController;
    [SerializeField] private GameObject numberKeysControllerGO;

    [Header("Private Fields")]
    private InternetConnectivityCheck internetConnectivityCheck;
    private IFirestoreOperator FirestoreClient;
    private ScreenTimer screenTimer;
    private GameObject popup;
    private DateTime currentDT;
    Dictionary<string, object> currentUnitData;
    private Dictionary<string, object> quizzes;
    private readonly List<string> quizNumbers = new List<string>();
    private bool triviaPass = false;
    private bool played;
    private bool finalizeScreen = false;
    private readonly string context = "CalculatorFundamentals";


    [Header("Public Fields")]
    [SerializeField] public string unitLevel = "";
    [SerializeField] public string buttonName = "";
    [SerializeField] public int level = 1;
    private string FormattedLevel => level < 10 ? $"0{level}" : $"{level}";

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
        if (timerObject != null)
        {
            screenTimer = timerObject.GetComponent<ScreenTimer>();
        }
        bubbleSpawner = bubbleSpawnerGO.GetComponent<BubbleSpawner>();
        numberKeysController = numberKeysControllerGO.GetComponent<NumberKeysController>();
        if (numberKeysControllerGO == null)
        {
            Logger.LogError("NumberKeysController component not found in CalculatorFundamentals", context);
            return;
        }
        if (bubbleSpawnerGO == null)
        {
            Logger.LogError("BubbleSpawner component not found in CalculatorFundamentals", context);
            return;
        }
        numberKeysController.totalRounds = 4;
        numberKeysController.BubbleRiseSpeed = 1f;
        bubbleSpawner.BaseDistractors = 6;
        bubbleSpawner.MinSeparation = 5;
        bubbleSpawner.PlacementAttempts = 8;
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

        await LoadLevelData();
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
        await LoadLevelData();
    }

    private void InitializeTriviaState()
    {
        quizzes = (Dictionary<string, object>)currentUnitData[FSMapField.quizzes.ToString()];
        if (currentUnitData.ContainsKey(FSMapField.trivia.ToString()))
        {
            triviaScript.trivias = (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];
            Logger.LogInfo($"Initialized triviaScript.trivias for {JsonUtility.ToJson(triviaScript.trivias)} in CalculatorFundamentals", context);


        }

        if (level == 1)
        {
            triviaPass = Convert.ToBoolean(Convert.ToString(
                ((Dictionary<string, object>)
                    ((Dictionary<string, object>)
                        ((Dictionary<string, object>)currentUnitData[FSMapField.maingame.ToString()])[MainGame.levels.ToString()])
                    [$"0{(int)UnitStageButtonStatus.calculator}"])[MainGame.calculator_trivia_collected.ToString()]));

            Logger.LogInfo($"Checking trivia pass collected in maingame level 0{(int)UnitStageButtonStatus.calculator} for {buttonName} level {level}. Found collected: {triviaPass}", context);
        }
    }

    async Task LoadLevelData()
    {
        try
        {
            Dictionary<string, object> unitStageFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_data.ToString());
            currentUnitData = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];
            InitializeTriviaState();
            if (currentUnitData.ContainsKey(FSMapField.calculator.ToString()))
            {
                Dictionary<string, object> stageData =
                                        (Dictionary<string, object>)currentUnitData[FSMapField.calculator.ToString()];

                level = Convert.ToInt32(stageData[CalCulator.level.ToString()]);
                triviaScript.level = level;

                Logger.LogInfo($"Loaded Unit{unitLevel} contains {buttonName} data::and the current level is {level}", context);
                await EvaluateLevelScreen(stageData);
            }
            else
            {
                Logger.LogInfo($"Calclator data for {$"Unit{unitLevel}"} not found..... ", context);
                await CreateLevelData();

            }

        }
        catch (Exception ex)
        {
            Logger.LogError($"Unable to fetch player data", context, ex);
        }
    }

    private async Task CreateLevelScreen(bool gamePlay = true)
    {
        if (gamePlay)
        {
            LoadCProfileAndFinalizeScreen(childPic, displayName, contentContainer, loading);
        }
        else if (!gamePlay)
        {
            triviaScript.enabled = true;
            if (finalizeScreen)
            {
                LoadCProfileAndFinalizeScreen(childPic, displayName, contentContainer, loading);
            }
        }

    }
    private async Task CreateLevelData()
    {
        Logger.LogInfo("Starting async CreateLevelData task...", context);

        // Ensure we start from a clean list every time
        quizNumbers.Clear();

        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(LoadQuizJSON(() => tcs.SetResult(true)));
        await tcs.Task;

        Logger.LogInfo($"CreateLevelData task to LoadQuizJSON completed.=> {Json.Serialize(quizNumbers)}", context);

        string debugMessage;
        Dictionary<string, object> newQuizData = FirestoreData.QuizQuestions(quizNumbers);

        Logger.LogInfo($"Starting task to create {buttonName} level {level}..", context);

        string newFormattedLevel = level < 10 ? $"0{level}" : $"{level}";
        Dictionary<string, object> data = new Dictionary<string, object>(){
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                 {$"unit{unitLevel}", new Dictionary<string, object>(){
                       {FSMapField.quizzes.ToString(), new Dictionary<string, object>(){
                        {buttonName, new Dictionary<string, object>(){
                            {Convert.ToString(level), false}
                            }}
                        }},
                    {FSMapField.calculator.ToString(), new Dictionary<string, object>(){
                        {CalCulator.level.ToString(), level},
                          {CalCulator.levels.ToString(), new Dictionary<string, object>(){
                                {$"{newFormattedLevel}",  new Dictionary<string, object>(){
                                    {CalCulator.date.ToString(), currentDT.ToShortDateString()},
                                   { CalCulator.played.ToString(), false}
                                }},
                            }}
                        }},
                    {
                            FSMapField.trivia.ToString(), new Dictionary <string, object>(){
                               {buttonName, new Dictionary<string, object>(){
                                  {CalCulator.levels.ToString(), new Dictionary<string, object>(){
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
                currentUnitData.Add(
                    FSMapField.trivia.ToString(),
                    new Dictionary<string, object>() { });

                Logger.LogInfo(
                    $"There was no trivia map field present in FS..create one for {buttonName} with empty level map field",
                    context);
            }

            var triviaDict =
                    (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];

            if (!triviaDict.ContainsKey(buttonName))
            {
                triviaDict.Add(buttonName, new Dictionary<string, object>() { });
                Logger.LogInfo(
                    $"Trivia map field found but no {buttonName} map field was found...creating one with empty level map field",
                    context);
            }

            var buttonDict = (Dictionary<string, object>)triviaDict[buttonName];
            if (!buttonDict.ContainsKey(CalCulator.levels.ToString()))
            {
                buttonDict.Add(CalCulator.levels.ToString(), new Dictionary<string, object>() { });
            }

            var levelsDict =
                (Dictionary<string, object>)buttonDict[CalCulator.levels.ToString()];

            levelsDict.Add(level.ToString(), newQuizData);

            Dictionary<string, object> pDta = new Dictionary<string, object>(){
                    {ProgressData.current_stage_name.ToString(),  ToTitleCase(buttonName)}
                };

            data.Add(FSMapField.progress_data.ToString(), pDta);
            debugMessage = $"Creating {buttonName} level {level} data before creating game screen";
        }
        else
        {
            var triviaPath =
                    (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)currentUnitData[
                        FSMapField.trivia.ToString()])[buttonName])[CalCulator.levels.ToString()];

            triviaPath.Add(level.ToString(), newQuizData);
            debugMessage = $"Updating {buttonName} with new level {level} data before creating game screen";
        }

        triviaScript.trivias =
                (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];
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
            Logger.LogInfo(
                    $"No connection found while creating {buttonName} new level {level} data",
                    context);
        }
    }
    async Task EvaluateLevelScreen(Dictionary<string, object> stageData)
    {
        string formattedLevel = level < 10 ? $"0{level}" : $"{level}";

        Dictionary<string, object> levelsData =
                    (Dictionary<string, object>)stageData[CalCulator.levels.ToString()];

        Dictionary<string, object> levelGameData =
                    (Dictionary<string, object>)levelsData[formattedLevel];

        played = Convert.ToBoolean(levelGameData[CalCulator.played.ToString()]);

        bool quizPlayed = false;
        if (quizzes != null && quizzes.ContainsKey(buttonName))
        {
            var btnMap = (Dictionary<string, object>)quizzes[buttonName];
            if (btnMap.ContainsKey(level.ToString()))
            {
                quizPlayed = Convert.ToBoolean(btnMap[level.ToString()]);
            }
        }

        Logger.LogInfo(
            $"{buttonName} level {level} played status: {played}, trivia quiz played status: {quizPlayed}",
            context);

        if (played && quizPlayed)
        {
            DateTime.TryParse((string)levelGameData[CalCulator.date.ToString()],
            out DateTime gameDT);

            bool valid = DateTime.Compare(currentDT.Date, gameDT.Date) > 0;

            if (valid && ReturnQuizLevelPlayedStatus(
                 level,
                 triviaPass,
                 (Dictionary<string, object>)quizzes[buttonName],
                 context))//new day
            {
                level++;
                triviaScript.level = level;

                Logger.LogInfo(
                    $"All condition check for new level {level} passed, creating new {buttonName} level..",
                    context);

                await CreateLevelData();
                return;
            }
            else
            {
                Logger.LogInfo(
                    $"{buttonName} game level {level} is played alongwith quiz, but the date condition is not valid for new level...",
                    context);

                gameQuizComplete.SetActive(true);
                loading.SetActive(false);
                return;
            }

        }
        else if (played && !quizPlayed)
        {
            if (PlayerInfo.SceneReload)
            {
                if (screenTimer != null)
                {
                    screenTimer.startTimer = true;
                }

                PlayerInfo.SceneReload = false;
                finalizeScreen = true;

                Logger.LogInfo(
                    $"As this is quiz reattempt, starting screen from unanswered quiz..",
                    context);

                await CreateLevelScreen(false);
                return;
            }

            Logger.LogInfo(
                $"{buttonName} game level {level} is played but quiz completion is pending...checking other conditions",
                context);

            Dictionary<string, object> q = (Dictionary<string, object>)quizzes[buttonName];


            if (CheckQuizPlayedStatus(
                level,
            triviaPass,
            q,
            quizReadyToPlay,
            passesNotCollected,
            $"0{(int)UnitStageButtonStatus.calculator}",
            context))
            {
                quizReadyToPlay.GetComponent<UserMessagePrefabTextHandler>().message.text =
                Message.FirstPartCompleteOnLoad;

                finalizeScreen = true;

            }
            else
            {
                passesNotCollected.GetComponent<UserMessagePrefabTextHandler>().message.text =
                Message.CollectTriviaPassOnLoad($"0{(int)UnitStageButtonStatus.calculator}");
            }

            loading.SetActive(false);
            return;
        }
        // Same day, game not completed - show gameplay
        Logger.LogInfo(
            $"{buttonName} game level {level} not completed..loading same level data and creating screen.",
            context);

        gamePlayScreen[level].SetActive(true);
        await CreateLevelScreen();

    }


    IEnumerator LoadQuizJSON(Action onQuizJSONLoaded)
    {
        string url_json_quiz =
        $"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/json/{buttonName}/{level}.json";

        Logger.LogInfo(
            $"Trivia quiz data path is {url_json_quiz}",
            context);

        UnityWebRequest request = UnityWebRequest.Get(url_json_quiz);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var loadedData = JsonUtility.FromJson<QuizzesData>(json: request.downloadHandler.text);

            for (int i = 0; i < loadedData.Quizzes.Length; i++)
            {
                quizNumbers.Add(loadedData.Quizzes[i].Number);
            }

            Logger.LogInfo(
                $"Trivia data json is loaded having quiz count to {quizNumbers.Count}",
                context);
        }

        Logger.LogInfo("JSON quiz data loading coroutine finished.", context);
        onQuizJSONLoaded?.Invoke(); // Invoke callback when done
    }

    public async void MarkGamePlayed()
    {
        screenTimer.startTimer = false;

        string formattedLevel = level < 10 ? $"0{level}" : $"{level}";

        Logger.LogInfo(
            $"The level {formattedLevel} will now be marked as played....",
            context);

        Dictionary<string, object> data = new Dictionary<string, object>(){
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                {$"unit{unitLevel}", new Dictionary<string, object>(){
                    {FSMapField.calculator.ToString(), new Dictionary<string, object>(){
                          {CalCulator.levels.ToString(), new Dictionary<string, object>(){
                                {$"{formattedLevel}",  new Dictionary<string, object>(){
                                  { CalCulator.played.ToString(), true}
                                }}
                            }}
                        }}
                    }}
                }}
   };

        Logger.LogInfo(
            $"Marking {buttonName} level {formattedLevel} played status as played...",
            context);


        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await ActionOnGamePlayed(data);
        }
        else
        {
            retryAction += async () => await ActionOnGamePlayed(data);
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
            Logger.LogInfo(
                $"No connection found while marking {buttonName} game as played",
                context);
        }
    }

    #region Connectivity
    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogInfo($"Connectivity restored in {buttonName}, retrying action", context);
        RetryTheAction(popup);
    }
    #endregion
    private void OnDestroy()
    {
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
        }
    }

    #region UI Actions
    public async void ShowTriviaQuiz()
    {
        loading.SetActive(true);
        if (await HaveConnectivity(canvas, messageBoxPopupPrefab, loading))
        {
            await CreateLevelScreen(false);
        }

    }
    public void ReplayGameToCollectPass()
    {
        int __level = (int)UnitStageButtonStatus.calculator;
        ReplayLevelForPassCollection(unitLevel, __level, buttonName, context);

    }
    public void ReloadScene()
    {
        PlayerInfo.SceneReload = true;
        Transition.LoadLevel("BlueprintForLevel", Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }

    public void GotohomeScene()
    {
        Transition.LoadLevel(SceneName.Level.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }
    #endregion


    /********************Actions on connection found*******************************************/

    #region Firestore Operations

    async Task ActionOnCreateNewLevelData(Dictionary<string, object> data)
    {
        await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                data);
        Logger.LogInfo($"Success...Updated new level {level} {buttonName} data {Json.Serialize(data)}..", context);
        gamePlayScreen[level].SetActive(true);
        LoadCProfileAndFinalizeScreen(childPic, displayName, contentContainer, loading);
        Logger.LogInfo($"{buttonName} new level {level} is now ready to play...", context);


    }
    async Task ActionOnGamePlayed(Dictionary<string, object> data)
    {
        await FirestoreClient.FirestoreDataSave(
                     FSCollection.parent.ToString(),
                     PlayerInfo.AuthenticatedID,
                     FSCollection.children.ToString(),
                     PlayerInfo.AuthenticatedChildID,
                     data);
        Dictionary<string, object> q = (Dictionary<string, object>)quizzes[Quizzes.calculator.ToString()];
        if (CheckQuizPlayedStatus(level, triviaPass, q, playTriviaQuizButton, collectTriviaPassButton, $"0{(int)UnitStageButtonStatus.calculator}", context))
        {
            playTriviaQuizButton.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.FirstPartComplete;
        }
        else
        {
            collectTriviaPassButton.GetComponent<PrefabTextHandler>().display.text = Message.CollectTriviaPassOnMarked($"0{(int)UnitStageButtonStatus.calculator}");
        }

        gamePlayScreen[level].SetActive(false);
        screenTimer.ResetTimer();
    }
    #endregion
    /*******************Instructions*******************************************/
    private float autoCloseSeconds = 6f;

    string PopupRootName = "Calculator_InstructionsPopup";
    string PopupTextName = "InstructionsText";
    private GameObject popupRoot;
    private TMP_Text popupText;
    private Coroutine autoCloseRoutine;

    private static readonly string InstMessage =
        "Find the flashing bubble and click the number on calculator keypad\n" +
        "The bubble can hide behind the calculator, you can move calculator grabbing the move icon or calculator panel";

    public void Show()
    {
        EnsurePopupUI();
        if (popupRoot == null || popupText == null) return;
        Logger.LogInfo($"Showing Calculator Instructions popup with name {popupRoot.name}", context);
        popupText.text = InstMessage;
        popupRoot.SetActive(true);

        if (autoCloseRoutine != null) StopCoroutine(autoCloseRoutine);
        autoCloseRoutine = StartCoroutine(AutoCloseAfterDelay());
    }

    private IEnumerator AutoCloseAfterDelay()
    {
        var seconds = Mathf.Max(0.5f, autoCloseSeconds);
        yield return new WaitForSecondsRealtime(seconds);

        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
        autoCloseRoutine = null;
    }

    private void EnsurePopupUI()
    {
        if (popupRoot != null && popupText != null) return;

        // Reuse existing popup if a designer already created one.
        var existing = GameObject.Find(PopupRootName);
        if (existing != null)
        {
            popupRoot = existing;
            popupText = popupRoot.GetComponentInChildren<TMP_Text>(true);
            return;
        }

        //var canvas = FindFirstObjectByType<Canvas>();
        //if (canvas == null) return;

        // Root panel
        popupRoot = new GameObject(PopupRootName, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        popupRoot.transform.SetParent(canvas.transform, false);

        var cg = popupRoot.GetComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        var bg = popupRoot.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);

        var rt = (RectTransform)popupRoot.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(900f, 220f);

        // Text
        var textGo = new GameObject(PopupTextName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(popupRoot.transform, false);

        var textRt = (RectTransform)textGo.transform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.pivot = new Vector2(0.5f, 0.5f);
        textRt.offsetMin = new Vector2(24f, 18f);
        textRt.offsetMax = new Vector2(-24f, -18f);

        popupText = textGo.GetComponent<TextMeshProUGUI>();
        popupText.text = InstMessage;
        popupText.color = Color.white;
        popupText.fontSize = 32f;
        popupText.textWrappingMode = TextWrappingModes.Normal;
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.raycastTarget = false;

        popupRoot.SetActive(false);
    }

}