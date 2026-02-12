using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ricimi;
using ScratchCardAsset;
using TMPro;
using UI.Pagination.Examples;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static IFirestoreEnums;

[RequireComponent(typeof(InternetConnectivityCheck))]
public class FlashCard : Elevator
{
    [Header("UI References")]
    [SerializeField] private Image Card1Img;
    [SerializeField] private Image Card2Img;
    [SerializeField] private Image childPic;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private GameObject loadingActivity;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private GameObject timerObject;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject ScratchCardOne;
    [SerializeField] private GameObject ScratchCardTwo;
    [SerializeField] private TMP_Text Card1Title;
    [SerializeField] private TMP_Text Card1SubTitle;
    [SerializeField] private TMP_Text Card1Description;

    [SerializeField] private TMP_Text Card2Title;
    [SerializeField] private TMP_Text Card2SubTitle;
    [SerializeField] private TMP_Text Card2Description;
    [SerializeField] private GameObject galleryScriptable;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject cardScreenContent;
    [SerializeField] private GameObject playTriviaQuizButton;
    [SerializeField] private GameObject collectTriviaPassButton;
    [SerializeField] private GameObject gameQuizComplete;
    [SerializeField] private GameObject quizReadyToPlay;
    [SerializeField] private GameObject passesNotCollected;
    [SerializeField] private GameObject cardOne;
    [SerializeField] private GameObject cardTwo;
    [SerializeField] private GameObject noCards;
    [SerializeField] private GameObject radialShine;
    [SerializeField] private TMP_Text viewedCards;
    [SerializeField] private TMP_Text totalCards;
    [SerializeField] private GameObject triviaScriptableObject;
    [SerializeField] private GameObject starCointainer01;
    [SerializeField] private GameObject starCointainer02;
    [SerializeField] private GameObject card1Button;
    [SerializeField] private GameObject card2Button;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject messageBoxPopupPrefab;

    [Header("Private Fields")]

    private DateTime cardValidityDate;
    Dictionary<string, object> currentUnitData;
    private HashSet<int> expired = new HashSet<int>();
    private HashSet<int> alreadyPresentCards = new HashSet<int>();
    Dictionary<string, object> unitStageFSData = new Dictionary<string, object>();
    private bool triviaPass = false;
    private bool alreadyDone = false;
    private bool onEraseProgressCardOneExecution = false;
    private bool onEraseProgressCardTwoExecution = false;
    private bool isScreenLoadingComplete = false;
    private string formattedLevel = "01";
    private int currentCardTabOne = 0;
    private int currentCardTabTwo = 0;
    private bool cardOneScratched = false;
    private bool cardTwoScratched = false;
    private Dictionary<string, object> quizzes;
    private int cardnumber = 0;
    private GameObject popup;
    private DateTime currentDT;
    private int[] generatedRandomCardNumbers = new int[2];
    private Trivia_Flashcard triviaScript;
    private InternetConnectivityCheck internetConnectivityCheck;
    private string Logcontext = "FlashCard";

    [Header("Public Fields")]
    public IFirestoreOperator FirestoreClient;

    public string unitLevel = "";
    public string buttonName = "";
    public int level = 1;
    public Cards card1 = new Cards();
    public Cards card2 = new Cards();

    [Serializable]
    public class Cards
    {
        public string Title;
        public string SubTitle;
        public string Description;
    }
    public EraseProgress EraseProgressCardOne;
    public EraseProgress EraseProgressCardTwo;


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
        totalCards.text = Params.TotalFlashCard.ToString();
        if (content) { content.GetComponent<UnloadSceneAdditiveAsync>().toggleContent.SetActive(false); }

        FirestoreClient = new FirestoreDataOperationManager();
        InitializeConnectivityAndCanvas();
        InitializeEraseProgressHandlers();

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            currentDT = ServerDateTime.GetFastestNISTDate();
            if (!Params.ChildDataloaded)
            {
                await ReloadChildData();
                Logger.LogInfo($"Child data reloaded from Firestore", Logcontext); ;
                return;
            }
        }
        else
        {
            retryAction += async () => await ReloadChildData();
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            loadingActivity.SetActive(false);
            Logger.LogError($"No connectivity found", Logcontext);
            return;
        }
        await CardDataLoad();
    }

    private void InitializeConnectivityAndCanvas()
    {
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
        var found = FindFirstObjectByType<Canvas>();
        if (found != null) { canvas = found; }
        else { Logger.LogError("Canvas not found in scene during start", Logcontext); }

    }

    private void InitializeEraseProgressHandlers()
    {
        EraseProgressCardOne.OnProgress += OnEraseProgressCardOne;
        EraseProgressCardTwo.OnProgress += OnEraseProgressCardTwo;
    }

    private async Task ReloadChildData()
    {
        FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);
        Logger.LogInfo($"Re Loading data from FS", Logcontext);
        await CardDataLoad();
    }

    async Task CardDataLoad()
    {
        if (!TryInitializeTriviaScript())
        {
            return;
        }

        Logger.LogInfo($"Getting data details from FS for Unit{unitLevel}-> flashcard", Logcontext);

        unitStageFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_data.ToString());

        if (!CheckCardData())
        {
            Logger.LogInfo($"Flashcard data for level 01 not found!! Creating now...", Logcontext);
            HashSet<int> generatedNumbers = new HashSet<int>();
            Array.Clear(generatedRandomCardNumbers, 0, generatedRandomCardNumbers.Length);

            for (int i = 0; i < 2; i++)
            {
                int num = Inference.GenerateRandomNumber(Params.TotalFlashCard, generatedNumbers);
                generatedNumbers.Add(num);
                generatedRandomCardNumbers[i] = num;
            }
            triviaScript.QuizCardNumber = generatedRandomCardNumbers;
            cardOneScratched = cardTwoScratched = onEraseProgressCardOneExecution = onEraseProgressCardTwoExecution = false;
            StartCoroutine(LoadQuizJSONForDataCreation());
        }
        else
        {
            Logger.LogInfo($"Flashcard data found for Unit{unitLevel}..Loading existing data", Logcontext);
            if (!Params.ChildDataloaded)
            {
                Logger.LogInfo($"Reloading child data as it has changed since last load", Logcontext);
                FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(
                    FSCollection.parent.ToString(),
                    PlayerInfo.AuthenticatedID,
                    FSCollection.children.ToString(),
                    PlayerInfo.AuthenticatedChildID);
            }

            await LoadLevelData();
        }
    }

    private bool TryInitializeTriviaScript()
    {
        triviaScript = triviaScriptableObject.GetComponent<Trivia_Flashcard>();

        if (triviaScript == null)
        {
            Logger.LogError("Trivia scriptable object is not assigned in FlashCard script", Logcontext);
            return false;
        }

        return true;
    }

    bool CheckCardData()
    {
        try
        {
            Dictionary<string, object> unit = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];

            if (unit.ContainsKey(buttonName.ToString()))
            {
                return true;
            }

        }
        catch (Exception ex)
        {
            Logger.LogError($"Unable to fetch flashcard data", Logcontext, ex);
            return false;
        }
        return false;
    }
    void GetStreamingAssetRefAndLoad(string __cardIs)
    {
        string url_image = $"{Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}/images/{cardnumber}.png";
        string url_json = $"{Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}/json/{cardnumber}.json";
        StartCoroutine(LoadJSON(url_json, __cardIs));
        StartCoroutine(LoadImage(url_image, __cardIs));
    }

    IEnumerator LoadJSON(string JSONUrl, string _cardIs)
    {
        UnityWebRequest request = UnityWebRequest.Get(JSONUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        Debug.Log($"Loading json for flashcards from path {JSONUrl}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Cards card = JsonUtility.FromJson<Cards>(request.downloadHandler.text);

            if (_cardIs == "A")
            {
                Card1Title.text = card.Title; Card1SubTitle.text = card.SubTitle; Card1Description.text = card.Description;
            }
            else
            {
                Card2Title.text = card.Title; Card2SubTitle.text = card.SubTitle; Card2Description.text = card.Description;

            }
        }
        else
        {
            Debug.Log(request.error);
        }
    }
    IEnumerator LoadImage(string url, string _cardIs)
    {
        UnityWebRequest uwr = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        uwr.downloadHandler = texDl;
        yield return uwr.SendWebRequest();
        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Texture2D t = texDl.texture;
            Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                Vector2.zero, 1f);
            if (_cardIs == "A")
            {
                Card1Img.sprite = s;
                if (!cardOneScratched)
                {
                    Logger.LogInfo($"Card one is not scratched yet, enabling scratch for card one...", Logcontext);
                    ScratchCardOne.SetActive(true);
                }
                else
                {
                    Debug.Log($"Card one is already scratched");
                    ScratchCardOne.SetActive(false);
                }
            }
            else
            {
                Card2Img.sprite = s;
                if (!cardTwoScratched)
                {
                    Logger.LogInfo($"Card two is not scratched yet, So enabling scratch for scratch card two...", Logcontext);
                    ScratchCardTwo.SetActive(true);
                }
                else
                {
                    Logger.LogInfo($"Cardtwo already scratched : {cardTwoScratched}", Logcontext);
                    ScratchCardTwo.SetActive(value: false);
                }
            }
        }

        if (isScreenLoadingComplete)
        {
            viewedCards.text = expired.Count == 0 ? "2" : (expired.Count + 2).ToString();
            Logger.LogInfo($"Setting viewed card to {viewedCards.text}", Logcontext);
        }
        else
        {
            viewedCards.text = expired.Count > 1 ? (expired.Count + 2 - 1).ToString() : expired.Count == 0 ? "1" : (expired.Count + 2).ToString();
            Logger.LogInfo($"Setting viewed card to {viewedCards.text}", Logcontext);
            isScreenLoadingComplete = true;
        }
        LoadCProfileAndFinalizeScreen(childPic, displayName, cardScreenContent, GetActiveLoader(), hud);
    }

    GameObject GetActiveLoader()
    {
        if (loadingActivity != null && loadingActivity.activeInHierarchy) return loadingActivity;
        if (loadingText != null && loadingText.activeInHierarchy) return loadingText;
        return loadingActivity != null ? loadingActivity : loadingText;
    }

    public void CardTabButtonClick(Button btn)
    {
        string tabSelected = btn.name.Substring(btn.name.Length - 1);
        if (tabSelected == "A")
        {
            viewedCards.text = expired.Count > 1 ? (expired.Count + 2 - 1).ToString() : expired.Count == 0 ? "1" : (expired.Count + 2).ToString();
            Debug.Log($"Button tab {tabSelected} clicked...");
            return;
        }
        else if (tabSelected == "B")
        {
            if (!alreadyDone)
            {
                alreadyDone = true;
                Debug.Log($"Button tab {tabSelected} clicked, applying text and image to card 2....");
                cardnumber = currentCardTabTwo;
                CardTwoCreation();

            }
            else
            {

                viewedCards.text = expired.Count == 0 ? "2" : (expired.Count + 2).ToString();
                loadingActivity.SetActive(false);
                Debug.Log($"Button tab {tabSelected} clicked, Card 2 is already created..... ");
            }
        }
    }

    async void CardTwoCreation()
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {

            GetStreamingAssetRefAndLoad("B");
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
        }
    }

    private async void UpdateScratchCardScore(GameObject _scratchCard)
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (await FirestoreClient.FirestoreDataSave(
                    FSCollection.parent.ToString(),
                    PlayerInfo.AuthenticatedID,
                    FSCollection.children.ToString(),
                    PlayerInfo.AuthenticatedChildID,
                    FirestoreData.UpdatePointScore(ScorePoint.SCRATCHCARD)))
            {
                _scratchCard.SetActive(value: false);
            }
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogError($"No connection found while updating scratch card score", Logcontext);
            return;
        }

        if (cardOneScratched && cardTwoScratched)
        {
            nextButton.SetActive(true);
        }
    }

    private async void OnEraseProgressCardOne(float progress)
    {
        int value = Convert.ToInt32(Mathf.Round(progress * 100f));
        if (value >= 75)
        {
            EraseProgressCardOne.transform.parent.GetComponent<ScratchCardManager>().Card.FillInstantly();
            Dictionary<string, object> _data = new Dictionary<string, object>(){{
              FSMapField.unit_stage_data.ToString(),  new Dictionary<string, object>(){
                {$"unit{unitLevel}", new Dictionary<string, object>(){
                { buttonName, new Dictionary<string, object>(){
                      {Flashcard.levels.ToString(), new Dictionary<string, object>(){
                                {formattedLevel, new Dictionary<string, object>(){
                                    {Flashcard.card_one_scratched.ToString(), true}
                    }}}}}} } } } } };

            if (!onEraseProgressCardOneExecution)
            {
                onEraseProgressCardOneExecution = true;
                if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
                {
                    await ActionOnCardOneErased(_data);
                }
                else
                {
                    retryAction += async () => await ActionOnCardOneErased(_data);
                    popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
                    Logger.LogError($"No connection found while updating card one scratch status", Logcontext);
                }
            }
        }
    }
    private async void OnEraseProgressCardTwo(float progress)
    {
        int value = Convert.ToInt32(Mathf.Round(progress * 100f));
        if (value >= 75)
        {
            EraseProgressCardTwo.transform.parent.GetComponent<ScratchCardManager>().Card.FillInstantly();

            Dictionary<string, object> _data = new Dictionary<string, object>(){{
               FSMapField.unit_stage_data.ToString(),  new Dictionary<string, object>(){
                    {$"unit{unitLevel}", new Dictionary<string, object>(){
                        {buttonName, new Dictionary<string, object>(){
                            {Flashcard.levels.ToString(), new Dictionary<string, object>(){
                                {formattedLevel, new Dictionary<string, object>(){
                                        {Flashcard.card_two_scratched.ToString(), true}
                                }}}}}}}}}} };

            if (!onEraseProgressCardTwoExecution)
            {
                onEraseProgressCardTwoExecution = true;
                cardTwoScratched = true;
                if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
                {
                    await ActionOnCardTwoErased(_data);
                }
                else
                {
                    retryAction += async () => await ActionOnCardTwoErased(_data);
                    popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
                    Logger.LogError($"No connection found while updating card two scratch status", Logcontext);
                }
            }
        }
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogInfo("Connectivity restored in Flashcard, retrying action", Logcontext);
        RetryTheAction(popup);
    }

    void OnDestroy()
    {
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
        }
    }

    private async Task LoadLevelData()
    {

        currentUnitData = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];
        Dictionary<string, object> _stageData = (Dictionary<string, object>)currentUnitData[buttonName];
        quizzes = (Dictionary<string, object>)currentUnitData[FSMapField.quizzes.ToString()];
        triviaScript.trivias = (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];
        bool played = false;
        triviaScript.level = level = Convert.ToInt32(_stageData[Flashcard.level.ToString()]);
        if (level == 1)
        {
            triviaPass = Convert.ToBoolean(Convert.ToString(((Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)currentUnitData[FSMapField.maingame.ToString()])[MainGame.levels.ToString()])[$"0{(int)UnitStageButtonStatus.flashcard}"])[MainGame.flash_trivia_collected.ToString()]));
            string r = triviaPass ? "collected" : "not collected";
            Logger.LogInfo($"Checking if trivia pass from penguin game for flashcard level {level} collected..found:->  {r}", Logcontext);
        }
        else
        {
            // quizPlayed =Convert.ToBoolean((Dictionary<string, object>)((Dictionary<string, object>)quizzes[Quizzes.flashcard.ToString()])[level.ToString()]);

        }
        formattedLevel = level < 10 ? $"0{level}" : $"{level}";
        Dictionary<string, object> _stageLevelData;
        if (((Dictionary<string, object>)_stageData[Flashcard.levels.ToString()]).ContainsKey(formattedLevel))
        {
            _stageLevelData = (Dictionary<string, object>)((Dictionary<string, object>)_stageData[Flashcard.levels.ToString()])[formattedLevel];
        }
        else
        {
            Logger.LogError($"Level {level} data is not present in the database", Logcontext);
            return;
        }

        foreach (var cardDetails in _stageLevelData)
        {
            if (cardDetails.Key == Flashcard.date.ToString())
            {
                DateTime.TryParse((string)cardDetails.Value, out cardValidityDate);
                Logger.LogInfo($"Card validity date is {cardValidityDate}", Logcontext);

            }
            if (cardDetails.Key == Flashcard.current_card_one.ToString())
            {
                currentCardTabOne = Convert.ToInt32(cardDetails.Value);
                alreadyPresentCards.Add(currentCardTabOne);
                Logger.LogInfo($"Card one number found: {currentCardTabOne}", Logcontext);
            }
            if (cardDetails.Key == Flashcard.current_card_two.ToString())
            {
                currentCardTabTwo = Convert.ToInt32(cardDetails.Value);
                alreadyPresentCards.Add(currentCardTabTwo);
                Logger.LogInfo($"Card two numbers found: {currentCardTabTwo}", Logcontext);
            }

            if (cardDetails.Key == Flashcard.card_one_scratched.ToString())
            {
                cardOneScratched = Convert.ToBoolean(cardDetails.Value.ToString().ToLower());
            }
            if (cardDetails.Key == Flashcard.card_two_scratched.ToString())
            {
                cardTwoScratched = Convert.ToBoolean(cardDetails.Value.ToString().ToLower());
            }

            if (cardDetails.Key == Flashcard.expired.ToString())
            {
                foreach (var __item in cardDetails.Value as List<object>)
                {
                    expired.Add(Convert.ToInt32(__item));
                    galleryScriptable.GetComponent<FlashCardGallery>().expired.Add(Convert.ToInt32(__item));
                }
            }
            if (cardDetails.Key == Flashcard.played.ToString())
            {
                played = Convert.ToBoolean(cardDetails.Value.ToString().ToLower());

            }

        }
        Logger.LogInfo($"Loaded Unit{unitLevel} -> {buttonName}-> level {level} data", Logcontext);
        await GetCurrentCondition(played, quizzes, cardValidityDate);

    }

    private async Task GetCurrentCondition(bool played, Dictionary<string, object> _quizzes, DateTime cardValidityDate)
    {
        bool quizPlayed = false;
        if (_quizzes != null && _quizzes.ContainsKey(buttonName))
        {
            var buttonMap = (Dictionary<string, object>)_quizzes[buttonName];
            if (buttonMap.ContainsKey(level.ToString()))
            {
                quizPlayed = Convert.ToBoolean(buttonMap[level.ToString()]);
            }
        }

        if (played && quizPlayed)
        {

            bool valid = DateTime.Compare(currentDT.Date, cardValidityDate.Date) > 0;

            if (valid
             && ReturnQuizLevelPlayedStatus(
                 level,
                 triviaPass,
                 _quizzes != null && _quizzes.ContainsKey(Quizzes.flashcard.ToString()) ? (Dictionary<string, object>)_quizzes[Quizzes.flashcard.ToString()] : new Dictionary<string, object>(),
                 Logcontext)) //new day
            {
                level++;
                triviaScript.level = level;
                Logger.LogInfo($"All condition check for new level {level} passed, ready to create new level data..", Logcontext);
                await CreateNewLevelData();
            }
            else
            {
                Logger.LogInfo($"Flashcard game level {level} is played alongwith the quiz, but the date condition is not valid for new level...", Logcontext);
                gameQuizComplete.SetActive(true);
                loadingActivity.SetActive(false);
                return;
            }
        }
        else if (played && !quizPlayed)
        {
            Logger.LogInfo($"Flashcard game level {level} is played but quiz completion is pending...checking other conditions", Logcontext);

            Dictionary<string, object> q = (Dictionary<string, object>)_quizzes[Quizzes.flashcard.ToString()];
            if (PlayerInfo.SceneReload)
            {
                if (TryGetComponent<ScreenTimer>(out var sT)) { sT.startTimer = true; }
                PlayerInfo.SceneReload = false;
                ShowTriviaQuizOnLoad();
                return;
            }

            if (CheckQuizPlayedStatus(level, triviaPass, q, quizReadyToPlay, passesNotCollected, $"0{(int)UnitStageButtonStatus.flashcard}", Logcontext))
            {

                triviaScript.QuizCardNumber[0] = currentCardTabOne;
                triviaScript.QuizCardNumber[1] = currentCardTabTwo;
                quizReadyToPlay.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.FirstPartCompleteOnLoad;
                //finalizeScreen = true;
            }
            else
            {
                passesNotCollected.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.CollectTriviaPassOnLoad($"0{(int)UnitStageButtonStatus.flashcard}");
            }

            loadingActivity.SetActive(false);
        }
        else //same day
        {
            Logger.LogInfo($"Flashcard game level {level} not completed..loading same level data and creating screen.", Logcontext);
            currentDT = cardValidityDate;

            cardnumber = currentCardTabOne;
            if (cardOneScratched && cardTwoScratched)
            {
                int index = 0;
                foreach (int card in alreadyPresentCards)
                {
                    if (index >= triviaScript.QuizCardNumber.Length) break;
                    triviaScript.QuizCardNumber[index++] = card;
                }
                nextButton.SetActive(true);
            }
            GetStreamingAssetRefAndLoad("A");
        }

    }


    IEnumerator LoadNoCardAvailable(string _cardIs)
    {

        string url_image = $"{Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}/nocard.png";
        UnityWebRequest uwr = new UnityWebRequest(url_image);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        uwr.downloadHandler = texDl;
        yield return uwr.SendWebRequest();
        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Texture2D t = texDl.texture;
            Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                Vector2.zero, 1f);
            if (_cardIs == "A")
            {
                Card1Img.sprite = s;
                Card1Title.text = string.Empty;
                Card1SubTitle.text = string.Empty;
                Card1Description.text = string.Empty;
                ScratchCardOne.SetActive(value: false);
            }
            else
            {
                Card2Img.sprite = s;
                Card2Title.text = string.Empty;
                Card2SubTitle.text = string.Empty;
                Card2Description.text = string.Empty;
                ScratchCardTwo.SetActive(value: false);
            }
        }
        loadingActivity.SetActive(false);
    }

    async Task CreateNewLevelData()
    {
        Logger.LogInfo($"Creating new level card data ...", Logcontext);
        HashSet<int> previousCards = new HashSet<int>();
        expired.Add(currentCardTabOne);
        expired.Add(currentCardTabTwo);
        foreach (int numbers in alreadyPresentCards)
        {
            previousCards.Add(numbers);
        }
        previousCards.UnionWith(expired);

        if (Params.TotalFlashCard == previousCards.Count)
        {
            Logger.LogInfo($"All cards are exhausted...New card cannot be generated", Logcontext);
            StartCoroutine(LoadNoCardAvailable("A"));
            return;
        }
        else
        {

            Array.Clear(generatedRandomCardNumbers, 0, generatedRandomCardNumbers.Length);
            loadingActivity.SetActive(false);
            loadingText.SetActive(true);
            galleryScriptable.GetComponent<FlashCardGallery>().expired = previousCards;
            isScreenLoadingComplete = true;
            for (int i = 0; i < 2; i++)
            {
                int num = Inference.GenerateRandomNumber(Params.TotalFlashCard, previousCards);
                Logger.LogInfo($"Generated card number {num}", Logcontext);
                generatedRandomCardNumbers[i] = num;
                previousCards.Add(num);
            }
            cardOneScratched = cardTwoScratched = onEraseProgressCardOneExecution = onEraseProgressCardTwoExecution = false;
            StartCoroutine(LoadQuizJSONForDataCreation());

        }
    }
    public void ReplayGameToCollectPass()
    {
        int __level = (int)UnitStageButtonStatus.flashcard;
        ReplayLevelForPassCollection(unitLevel, __level, buttonName, Logcontext);

    }
    public async void ShowTriviaQuizOnLoad()
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            loadingActivity.SetActive(true);
            int index = 0;
            foreach (int card in alreadyPresentCards)
            {
                if (index >= triviaScript.QuizCardNumber.Length) break;
                triviaScript.QuizCardNumber[index++] = card;
            }

            triviaScript.gameObject.SetActive(true);
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogError($"No connection found ", Logcontext);
        }
    }

    public async void ShowTriviaQuizInContinuation()
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            loadingActivity.SetActive(true);

            triviaScript.gameObject.SetActive(true);
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogError($"No connection found ", Logcontext);
        }
    }
    public async void MarkGamePlayed()
    {
        timerObject.GetComponent<ScreenTimer>().startTimer = false;
        string formattedLevel = level < 10 ? $"0{level}" : $"{level}";
        Dictionary<string, object> data = new Dictionary<string, object>(){
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                {$"unit{unitLevel}", new Dictionary<string, object>(){
                    {FSMapField.flashcard.ToString(), new Dictionary<string, object>(){
                          {Flashcard.levels.ToString(), new Dictionary<string, object>(){
                                {$"{formattedLevel}",  new Dictionary<string, object>(){
                                    { Flashcard.played.ToString(), true}
                                }}
                            }}
                        }}
                    }}
                }}
   };
        Logger.LogInfo($"Marking flashcard level {formattedLevel} played status as played...", Logcontext);

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                data);

            Dictionary<string, object> q = (Dictionary<string, object>)quizzes[Quizzes.flashcard.ToString()];
            if (CheckQuizPlayedStatus(level, triviaPass, q, playTriviaQuizButton, collectTriviaPassButton, $"0{(int)UnitStageButtonStatus.flashcard}", Logcontext))
            {
                playTriviaQuizButton.GetComponent<UserMessagePrefabTextHandler>().message.text = Message.FirstPartComplete;

            }
            else
            {
                collectTriviaPassButton.GetComponent<PrefabTextHandler>().display.text = Message.CollectTriviaPassOnMarked($"0{(int)UnitStageButtonStatus.flashcard}");
            }
            timerObject.GetComponent<ScreenTimer>().ResetTimer();
            cardScreenContent.SetActive(false);
            loadingActivity.SetActive(false);
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogError($"No connection found while marking flashcard level played", Logcontext);
        }
    }
    public void ReloadScene()
    {
        PlayerInfo.SceneReload = true;
        Transition.LoadLevel(SceneName.FlashCard.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }
    public void GotohomeScene()
    {
        Transition.LoadLevel(SceneName.Level.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }

    IEnumerator LoadQuizJSONForDataCreation()
    {
        List<QuizData> mergedQuiz = new List<QuizData>();
        string triviaUrl0 = $"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/json/{buttonName}/{generatedRandomCardNumbers[0]}.json";
        string triviaUrl1 = $"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/json/{buttonName}/{generatedRandomCardNumbers[1]}.json";

        yield return LoadSingleQuizFile(triviaUrl0, mergedQuiz);
        yield return LoadSingleQuizFile(triviaUrl1, mergedQuiz);

        // ContinueSavingDatToFS is an async Task; start it and yield until it's completed to avoid 'await' inside IEnumerator
        var saveTask = ContinueSavingDatToFS(mergedQuiz);
        while (!saveTask.IsCompleted) yield return null;
        if (saveTask.IsFaulted)
        {
            Logger.LogError($"Error saving quiz data to Firestore: {saveTask.Exception}", Logcontext);
        }

        //New Addition

        for (int i = 0; i < generatedRandomCardNumbers.Length; i++)
        {
            string tab = i == 0 ? "A" : "B";
            cardnumber = generatedRandomCardNumbers[i];
            GetStreamingAssetRefAndLoad(tab);


        }
        Logger.LogInfo($"All DONE!!", Logcontext);
    }
    private IEnumerator LoadSingleQuizFile(string url, List<QuizData> mergedQuiz)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var loadedData = JsonUtility.FromJson<QuizzesData>(request.downloadHandler.text);
                    if (loadedData?.Quizzes != null && loadedData.Quizzes.Length > 0)
                    {
                        mergedQuiz.AddRange(loadedData.Quizzes);
                        Logger.LogInfo($"Loaded {loadedData.Quizzes.Length} quizzes from {url}", Logcontext);
                    }
                    else
                    {
                        Logger.LogWarning($"Loaded json file {url} but found no quizzes.", Logcontext);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error parsing JSON from {url}: {ex.Message}", Logcontext);
                }
            }
            else
            {
                Logger.LogError($"Failed to load json quiz data from {url}. Error: {request.error}", Logcontext);
            }
        }
    }

    async Task ContinueSavingDatToFS(List<QuizData> mergedQuiz)
    {
        Dictionary<string, object> quizLevelData = new Dictionary<string, object>();
        foreach (var q in mergedQuiz)
        {
            quizLevelData.Add(q.Number, true);
        }


        if (await PopulateDictionary(quizLevelData))
        {
            Dictionary<string, object> newCardData;
            if (level == 1)
            {
                newCardData = FirestoreData.CreateFlashcardData(generatedRandomCardNumbers);
            }
            else
            {
                newCardData = FirestoreData.CreateFlashcards(generatedRandomCardNumbers, expired, level);
            }
            Dictionary<string, object> dataChild = new Dictionary<string, object>() {
                {FSMapField.progress_data.ToString(), new Dictionary<string, object>(){
                    {ProgressData.current_stage_name.ToString(), buttonName}
                }},
                {FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
                    {$"unit{unitLevel}", new Dictionary<string, object>(){
                        {buttonName, newCardData},
                        {FSMapField.trivia.ToString(),new Dictionary<string,object>(){
                        {buttonName, new Dictionary<string, object>(){
                            {Flashcard.levels.ToString(), new Dictionary<string, object>(){
                                {level.ToString(), quizLevelData} }}
                    }} } },
                   {FSMapField.quizzes.ToString(), new Dictionary<string, object>(){
                    { buttonName, new Dictionary<string, object>(){
                    {level.ToString(), false }
                        }}}}
                    }
                }
                }}
               };

            if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
            {
                _ = await FirestoreClient.FirestoreDataSave(
                    FSCollection.parent.ToString(),
                    PlayerInfo.AuthenticatedID,
                    FSCollection.children.ToString(),
                    PlayerInfo.AuthenticatedChildID,
                    dataChild);
                Logger.LogInfo($"Success..Flashcard data for Unit{unitLevel} -> level 01 created in FS having card numbers {generatedRandomCardNumbers[0]} ", Logcontext);
                alreadyDone = true;
            }
            else
            {
                popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
                Logger.LogError($"No connection found while creating initial flashcard data", Logcontext);
            }
        }
    }
    private async Task<bool> PopulateDictionary(Dictionary<string, object> _quizLevelData)
    {
        Logger.LogInfo($"Populating dictionary to add newly created data ", Logcontext);
        currentUnitData = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];
        quizzes = (Dictionary<string, object>)currentUnitData[FSMapField.quizzes.ToString()];
        try
        {
            if (!currentUnitData.ContainsKey(FSMapField.trivia.ToString()))
            {

                currentUnitData.Add(FSMapField.trivia.ToString(), new Dictionary<string, object>() { });
                Logger.LogInfo($"There was no trivia map field present in FS..create one for {buttonName} with empty level map field", Logcontext);
            }

            var triviaDict = (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];
            if (!triviaDict.ContainsKey(buttonName))
            {
                triviaDict.Add(buttonName, new Dictionary<string, object>() { });
                Logger.LogInfo($"Trivia map field found but no {buttonName} map field was found...creating one with empty level map field");
            }

            var buttonDict = (Dictionary<string, object>)triviaDict[buttonName];
            if (!buttonDict.ContainsKey(Flashcard.levels.ToString()))
            {
                buttonDict.Add(Flashcard.levels.ToString(), new Dictionary<string, object>() { });
            }

            var levelsDict = (Dictionary<string, object>)buttonDict[Flashcard.levels.ToString()];
            // add the new level entry
            levelsDict.Add(level.ToString(), _quizLevelData);

            triviaScript.QuizCardNumber = generatedRandomCardNumbers;
            triviaScript.trivias = (Dictionary<string, object>)currentUnitData[FSMapField.trivia.ToString()];
            triviaScript.level = level;
            Logger.LogInfo($"Added newly created quiz data in triviaScript.trivias", Logcontext);
            if (level == 1)
            {
                triviaPass = Convert.ToBoolean(Convert.ToString(((Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)currentUnitData[FSMapField.maingame.ToString()])[MainGame.levels.ToString()])[$"0{(int)UnitStageButtonStatus.flashcard}"])[MainGame.flash_trivia_collected.ToString()]));
                string r = triviaPass ? "collected" : "not collected";
                Logger.LogInfo($"Checking if trivia pass from penguin game for flashcard level {level} collected..found:->  {r}", Logcontext);
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Unable to save data...", Logcontext, ex);
        }

        return false;

    }

    /********************Actions on connection found*******************************************/

    async Task ActionOnCardOneErased(Dictionary<string, object> _data)
    {
        await FirestoreClient.FirestoreDataSave(
                               FSCollection.parent.ToString(),
                               PlayerInfo.AuthenticatedID,
                               FSCollection.children.ToString(),
                               PlayerInfo.AuthenticatedChildID,
                               _data);
        Debug.Log($"Updating card one scratch status to true..");
        starCointainer01.SetActive(true);
        cardOneScratched = true;
        UpdateScratchCardScore(ScratchCardOne);
    }
    async Task ActionOnCardTwoErased(Dictionary<string, object> _data)
    {
        await FirestoreClient.FirestoreDataSave(
                        FSCollection.parent.ToString(),
                        PlayerInfo.AuthenticatedID,
                        FSCollection.children.ToString(),
                        PlayerInfo.AuthenticatedChildID,
                        _data);
        Debug.Log($"Scratch card two scratch status updated in FS.....");
        starCointainer02.SetActive(true);
        UpdateScratchCardScore(ScratchCardTwo);
    }


}