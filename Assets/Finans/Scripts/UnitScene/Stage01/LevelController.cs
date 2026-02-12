using System;
using System.Collections;
using System.Collections.Generic;
using Google.MiniJSON;
using Ricimi;
using UnityEngine;
using static IFirestoreEnums;

public class LevelController : MonoBehaviour
{
    //[SerializeField] private GameObject menu;
    [SerializeField] private GameObject continueButton;
    public static LevelController instance;//global call
    private IFirestoreOperator FirestoreClient;
    string unitLevel = "";
    string buttonName = "";
    DateTime currentDT;
    private bool updatePasskeyData = false;
    private string continueToScene = "";
    private bool collectedPass = false;
    private string context = "LevelController";
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
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
    void Start()
    {
        currentDT = ServerDateTime.GetFastestNISTDate();
    }

    public void UpdateLevelStats(int __stars, int __coins, int __gamescore)
    {
        Dictionary<string, object> maingameDataOnLevelComplete;
        int __xp = 10;
        GameManager.Stars = 0;
        GameManager.Coins = 0;
        PointSystem.Stars += __stars;
        PointSystem.Coins += __coins;
        PointSystem.XP += __xp;
        Dictionary<string, object> gameScoreData = new Dictionary<string, object>(){
                {HUD.stars.ToString(),  PointSystem.Stars},
                 {HUD.coins.ToString(),  PointSystem.Coins},
            { HUD.xp.ToString(),  PointSystem.XP},
            {MainGame.passes.ToString(), PointSystem.Passes}
            };

        switch (PlayerInfo.MainGameLevelPlayed)
        {
            case true:
                maingameDataOnLevelComplete = new Dictionary<string, object>(){
                    {MainGame.game_points.ToString(), PointSystem.GamePoints },
                        {MainGame.levels.ToString(), new Dictionary<string, object>(){
                             {PlayerInfo.GameLevel,  new Dictionary<string, object>(){
                                {MainGame.powerup_health.ToString(), 101}
                        }}
                    }},
               };
                Logger.LogInfo($"Earned extra {__stars} stars in repeat play; total stars={PointSystem.Stars}", context);
                break;
            default:

                PlayerInfo.MainGameLevelPlayed = true;
                maingameDataOnLevelComplete = new Dictionary<string, object>(){
                    {MainGame.game_points.ToString(), PointSystem.GamePoints },
                        {MainGame.levels.ToString(), new Dictionary<string, object>(){
                            {FSMapField.locked.ToString(), new Dictionary<string, object>(){
                                {PlayerInfo.GameLevel, false}
                            }},
                            {PlayerInfo.GameLevel,  new Dictionary<string, object>(){
                                {MainGame.played.ToString(), PlayerInfo.MainGameLevelPlayed},
                                {MainGame.date.ToString(), currentDT.ToShortDateString()},
                        }}
                    }},
               };
                Logger.LogInfo($"Earned {__stars} stars on first play; total stars={PointSystem.Stars}", context);
                break;
        }
        PointSystem.GamePoints = PointSystem.GamePoints + __gamescore;
        Dictionary<string, object> nestedMaingameData = (Dictionary<string, object>)((Dictionary<string, object>)maingameDataOnLevelComplete[MainGame.levels.ToString()])[PlayerInfo.GameLevel];
        Dictionary<string, object> passkeyCollectedData = new Dictionary<string, object>(){
            {FSMapField.unit_stage_btn_status.ToString(), new Dictionary<string, object>(){
                {$"unit{unitLevel}", new Dictionary<string, object>(){}}
            }}
        };

        if (!PlayerInfo.LessonPassCollected && PlayerInfo.StoreLPCollected)
        {
            PlayerInfo.StoreLPCollected = false;
            PlayerInfo.LessonPassCollected = true;
            nestedMaingameData.Add(MainGame.lesson_pass_collected.ToString(), PlayerInfo.LessonPassCollected);
            collectedPass = true;
        }
        if (!PlayerInfo.FlashPassCollected && PlayerInfo.StoreFPCollected)
        {
            PlayerInfo.StoreFPCollected = false;
            PlayerInfo.FlashPassCollected = true;
            nestedMaingameData.Add(MainGame.flash_pass_collected.ToString(), PlayerInfo.FlashPassCollected);
            collectedPass = true;
        }
        if (PlayerInfo.LessonPassCollected && PlayerInfo.FlashPassCollected && collectedPass)
        {
            ((Dictionary<string, object>)((Dictionary<string, object>)passkeyCollectedData[FSMapField.unit_stage_btn_status.ToString()])[$"unit{unitLevel}"]).Add(UnitButtonName.lesson.ToString(), true);
            ((Dictionary<string, object>)((Dictionary<string, object>)passkeyCollectedData[FSMapField.unit_stage_btn_status.ToString()])[$"unit{unitLevel}"]).Add(UnitButtonName.flashcard.ToString(), true);
            Logger.LogInfo($"Both Lesson and Flashcard passes collected; unlocking both buttons", context);
            updatePasskeyData = true;
            collectedPass = false;
        }
        if (!PlayerInfo.FlashTriviaCollected && PlayerInfo.StoreFTCollected)
        {
            PlayerInfo.StoreFTCollected = false;
            PlayerInfo.FlashTriviaCollected = true;
            nestedMaingameData.Add(MainGame.flash_trivia_collected.ToString(), PlayerInfo.FlashTriviaCollected);
        }
        if (!PlayerInfo.MiniGamesPassCollected && PlayerInfo.StoreMGsPassCollected)
        {
            PlayerInfo.StoreMGsPassCollected = false;
            PlayerInfo.MiniGamesPassCollected = true;
            nestedMaingameData.Add(MainGame.minigames_pass_collected.ToString(), PlayerInfo.MiniGamesPassCollected);
            ((Dictionary<string, object>)((Dictionary<string, object>)passkeyCollectedData[FSMapField.unit_stage_btn_status.ToString()])[$"unit{unitLevel}"]).Add(UnitButtonName.minigames.ToString(), true);
            Logger.LogInfo($"MiniGames pass collected; unlocking minigames button", context);
            updatePasskeyData = true;
        }
        if (!PlayerInfo.MiniGamesTriviaCollected && PlayerInfo.StoreMGsTriviaCollected)
        {
            PlayerInfo.StoreMGsTriviaCollected = false;
            PlayerInfo.MiniGamesTriviaCollected = true;
            nestedMaingameData.Add(MainGame.minigames_trivia_collected.ToString(), PlayerInfo.MiniGamesTriviaCollected);
        }
        if (!PlayerInfo.VocabsPassCollected && PlayerInfo.StoreVocabsPassCollected)
        {
            PlayerInfo.StoreVocabsPassCollected = false;
            PlayerInfo.VocabsPassCollected = true;
            nestedMaingameData.Add(MainGame.vocabs_pass_collected.ToString(), PlayerInfo.VocabsPassCollected);
            ((Dictionary<string, object>)((Dictionary<string, object>)passkeyCollectedData[FSMapField.unit_stage_btn_status.ToString()])[$"unit{unitLevel}"]).Add(UnitButtonName.vocabs.ToString(), true);
            Logger.LogInfo($"Vocabs pass collected; unlocking vocabs button", context);
            updatePasskeyData = true;
        }
        if (!PlayerInfo.VocabsTriviaCollected && PlayerInfo.StoreVocabsTriviaCollected)
        {
            PlayerInfo.StoreVocabsTriviaCollected = false;
            PlayerInfo.VocabsTriviaCollected = true;
            nestedMaingameData.Add(MainGame.vocabs_trivia_collected.ToString(), PlayerInfo.VocabsTriviaCollected);
        }
        if (!PlayerInfo.CalcPassCollected && PlayerInfo.StoreCalcPassCollected)
        {
            PlayerInfo.StoreCalcPassCollected = false;
            PlayerInfo.CalcPassCollected = true;
            nestedMaingameData.Add(MainGame.calculator_pass_collected.ToString(), PlayerInfo.CalcPassCollected);
            ((Dictionary<string, object>)((Dictionary<string, object>)passkeyCollectedData[FSMapField.unit_stage_btn_status.ToString()])[$"unit{unitLevel}"]).Add(UnitButtonName.calculator.ToString(), true);
            updatePasskeyData = true;
        }
        if (!PlayerInfo.CalcTriviaCollected && PlayerInfo.StoreCalcTriviaCollected)
        {
            PlayerInfo.StoreCalcTriviaCollected = false;
            PlayerInfo.CalcTriviaCollected = true;
            nestedMaingameData.Add(MainGame.calculator_trivia_collected.ToString(), PlayerInfo.CalcTriviaCollected);
        }
        if (!PlayerInfo.VideoPassCollected && PlayerInfo.StoreVideoPassCollected)
        {
            PlayerInfo.StoreVideoPassCollected = false;
            PlayerInfo.VideoPassCollected = true;
            nestedMaingameData.Add(MainGame.video_pass_collected.ToString(), PlayerInfo.VideoPassCollected);
            ((Dictionary<string, object>)((Dictionary<string, object>)passkeyCollectedData[FSMapField.unit_stage_btn_status.ToString()])[$"unit{unitLevel}"]).Add(UnitButtonName.video.ToString(), true);
            updatePasskeyData = true;
        }
        if (!PlayerInfo.VideoTriviaCollected && PlayerInfo.StoreVideoTriviaCollected)
        {
            PlayerInfo.StoreVideoTriviaCollected = false;
            PlayerInfo.VideoTriviaCollected = true;
            nestedMaingameData.Add(MainGame.video_trivia_collected.ToString(), PlayerInfo.VideoTriviaCollected);
        }
        Dictionary<string, object> mainGameData = new Dictionary<string, object>(){
            { FSMapField.points_score.ToString(), gameScoreData},
            {FSMapField.unit_stage_data.ToString(), new Dictionary<string,object>(){
                {$"unit{unitLevel}", new Dictionary<string,object>(){
                    {FSMapField.maingame.ToString(),maingameDataOnLevelComplete}
            }}}}};

        Logger.LogInfo($"updatePasskeyData value is {updatePasskeyData}", context);
        UpdateFSData(mainGameData);

        if (updatePasskeyData)
        {
            UpdateStageButtonStatus(passkeyCollectedData); CanContinueGame();
        }
        else
        {
            Logger.LogInfo($"No pass collected during the play..Checking database for stored pass", context);
            CheckDatabaseToContinueGame();
        }
    }
    public void UpdatePlayerLife(int _starcount)
    {
        PointSystem.Life = PointSystem.Life + _starcount;
        Dictionary<string, object> lifedata = new Dictionary<string, object>(){
        {FSMapField.unit_stage_data.ToString(), new Dictionary<string,object>(){
            {$"unit{unitLevel}", new Dictionary<string,object>(){
                {FSMapField.maingame.ToString(),new Dictionary<string, object>(){
                      {MainGame.levels.ToString(), new Dictionary<string, object>(){
                        {PlayerInfo.GameLevel,  new Dictionary<string, object>(){
                            {MainGame.life.ToString(), PointSystem.Life}
                            }}
                        }}
                    }}
                }}
            }}
        };
        Logger.LogInfo($"Player life for maingame level {PlayerInfo.GameLevel} left: {PointSystem.Life}", context);
        UpdateFSData(lifedata);
    }
    async void UpdateFSData(Dictionary<string, object> UpdatedMaingameData)
    {
        Logger.LogDebug($"Updating maingame level {PlayerInfo.GameLevel} data status ... {Json.Serialize(UpdatedMaingameData)}", context);
        if (PointSystem.Life == 0)
        {
            PlayerInfo.SameSessionLifeReset = true;
        }
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                UpdatedMaingameData);
        }
        else
        {
            Logger.LogError("No internet connectivity while updating main game data; skipping Firestore save", context);
        }
    }

    async void UpdateStageButtonStatus(Dictionary<string, object> UpdateStageStatusFSData)
    {

        updatePasskeyData = false;
        Logger.LogDebug($"Updating stage locked unlocked status {Json.Serialize(UpdateStageStatusFSData)}", context);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                UpdateStageStatusFSData);
        }
        else
        {
            Logger.LogError("No internet connectivity while updating stage button status; skipping Firestore save", context);
        }
    }

    private void ContinueGame(string __continueToScene = "")
    {
        if (__continueToScene != "")
        {
            continueToScene = __continueToScene;
            continueButton.SetActive(true);
        }
    }
    public void ContinueToScene()
    {
        string __unitLevel = unitLevel;
        PlayerInfo.UnitButtonInfo.Clear();
        PlayerInfo.UnitButtonInfo.Add(__unitLevel, continueToScene.ToLower());
        Transition.LoadLevel(continueToScene, Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }
    private void CanContinueGame()
    {
        string sceneName = "";

        switch (PlayerInfo.GameLevel)
        {
            case "01":
                if (PlayerInfo.LessonPassCollected && PlayerInfo.FlashPassCollected)
                {
                    Logger.LogInfo($"Just collected the pass..Skipping database pass check and enabling continue for {SceneName.FlashCard.ToString()} scene", context);
                    sceneName = SceneName.FlashCard.ToString();
                }
                break;
            case "02":
                if (PlayerInfo.MiniGamesPassCollected)
                {
                    Logger.LogInfo($"Just collected MiniGamesPass..Skipping database pass check and enabling continue for {SceneName.MiniGames.ToString()} scene", context);
                    sceneName = SceneName.MiniGames.ToString();
                }
                break;
            case "03":
                if (PlayerInfo.VocabsPassCollected)
                {
                    Logger.LogInfo($"Just collected VocabsPass..Skipping database pass check and enabling continue for {SceneName.Vocabs.ToString()} scene", context);
                    sceneName = SceneName.Vocabs.ToString();
                }
                break;
            case "04":
                if (PlayerInfo.CalcPassCollected)
                {
                    Logger.LogInfo($"Just collected CalculatorPass..Skipping database pass check and enabling continue for {SceneName.Calculator.ToString()} scene", context);
                    sceneName = SceneName.Calculator.ToString();
                }
                break;
            case "05":
                if (PlayerInfo.VideoPassCollected)
                {
                    Logger.LogInfo($"Just collected VideoPass..Skipping database pass check and enabling continue for {SceneName.Video.ToString()} scene", context);
                    sceneName = SceneName.Video.ToString();
                }
                break;
            default:
                break;

        }
        ContinueGame(sceneName);
    }


    private async void CheckDatabaseToContinueGame()
    {
        FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);
        if (FirestoreDatabase.ChildData.ContainsKey(FSMapField.unit_stage_data.ToString()))
        {
            var unitStageData = FirestoreDatabase.ChildData[FSMapField.unit_stage_data.ToString()] as Dictionary<string, object>;
            if (unitStageData != null && unitStageData.ContainsKey($"unit{unitLevel}"))
            {
                var unitData = unitStageData[$"unit{unitLevel}"] as Dictionary<string, object>;
                if (unitData != null && unitData.ContainsKey(FSMapField.maingame.ToString()))
                {
                    var mainGameData = unitData[FSMapField.maingame.ToString()] as Dictionary<string, object>;
                    if (mainGameData != null && mainGameData.ContainsKey(FSMapField.levels.ToString()))
                    {
                        var levelsData = mainGameData[FSMapField.levels.ToString()] as Dictionary<string, object>;
                        if (levelsData != null && levelsData.ContainsKey(PlayerInfo.GameLevel))
                        {
                            string passName = "";
                            string passName2 = "";
                            string sceneName = "";
                            switch (PlayerInfo.GameLevel)
                            {
                                case "01":
                                    passName2 = MainGame.flash_pass_collected.ToString();
                                    passName = MainGame.lesson_pass_collected.ToString();
                                    sceneName = SceneName.FlashCard.ToString();
                                    break;
                                case "02":
                                    passName = MainGame.minigames_pass_collected.ToString();
                                    sceneName = SceneName.MiniGames.ToString();
                                    break;
                                case "03":
                                    passName = MainGame.vocabs_pass_collected.ToString();
                                    sceneName = SceneName.Vocabs.ToString();
                                    break;
                                case "04":
                                    passName = MainGame.calculator_pass_collected.ToString();
                                    sceneName = SceneName.Calculator.ToString();
                                    break;
                                case "05":
                                    passName = MainGame.video_pass_collected.ToString();
                                    sceneName = SceneName.Video.ToString();
                                    break;
                                default:
                                    break;
                            }
                            Dictionary<string, object> __currentLevelData = levelsData[PlayerInfo.GameLevel] as Dictionary<string, object>;

                            if (PlayerInfo.GameLevel == "01")
                            {
                                if (Convert.ToBoolean(__currentLevelData[passName].ToString()) == true && Convert.ToBoolean(__currentLevelData[passName2].ToString()) == true)
                                {
                                    Logger.LogInfo($"Found valid pass in the database; enabling continue", context);
                                    ContinueGame(sceneName);
                                }
                                else
                                {
                                    Logger.LogInfo($"No or incomplete passes collected for {PlayerInfo.GameLevel}; cannot continue", context);
                                }
                            }
                            else
                            {
                                if (Convert.ToBoolean(__currentLevelData[passName]) == true)
                                {
                                    Logger.LogInfo($"Found valid pass in the database; enabling continue", context);
                                    ContinueGame(sceneName);
                                }
                                else
                                {
                                    Logger.LogInfo($"No stored pass found in database for level {PlayerInfo.GameLevel}; cannot continue", context);
                                }
                            }


                        }
                    }
                }
            }
        }

    }
}

