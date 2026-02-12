using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.MiniJSON;
using Ricimi;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static IFirestoreEnums;
[RequireComponent(typeof(InternetConnectivityCheck))]
public class GameLoader : Elevator
{
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject messageBoxPopupPrefab;
    [SerializeField] private GameObject playVideoForLife;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject playGame;
    [SerializeField] private GameObject levelsScreen;
    [SerializeField] private GameObject levelButtonsParent;
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject gameLevelBtnParent;

    [Header("Private Fields")]
    private IFirestoreOperator FirestoreClient;
    private InternetConnectivityCheck internetConnectivityCheck;
    private int levelToLoad;
    private int level = 1;
    private string unitLevel = "";
    private string buttonName = "";
    private DateTime currentDT;
    private IClock clock = new TimeProvider();
    private GameObject popup;
    private Dictionary<string, object> gameData;
    private bool needToWatchVideoForLife = false;
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
        if (PlayerInfo.NeedPlayerLifeReset)
        {
            PlayerInfo.NeedPlayerLifeReset = false;
            levelToLoad = int.TryParse(PlayerInfo.GameLevel, out int parsedLevel) ? parsedLevel : 1;
            Logger.LogInfo($"Playing video to gain extra life for level {levelToLoad}", "GameLoader");
            playGame.SetActive(value: false);
            playVideoForLife.SetActive(true);
            loading.SetActive(false);
            return;
        }

        GameObject content = GameObject.FindWithTag(Tags.Level.ToString());
        if (content) { content.GetComponent<UnloadSceneAdditiveAsync>().toggleContent.SetActive(false); }
        FirestoreClient = new FirestoreDataOperationManager();
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
        if (!Params.ChildDataloaded)
        {
            if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
            {
                await ReloadChildData();
                Logger.LogInfo($"Child data reloaded from Firestore", "GameLoader"); return;

            }
            else
            {
                retryAction += async () => await ReloadChildData();
                popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
                loading.SetActive(false);
                Logger.LogError($"No connectivity found", "GameLoader"); return;
            }
        }
        LoadGameData();
    }
    private async void LoadGameData()
    {

        try
        {
            // Prefer TimeProvider for localizable time; fallback to ServerDateTime
            try { currentDT = clock.NowInTimeZone(FirestoreDatabase.GetChildTimeZoneOrDefault()); }
            catch { currentDT = ServerDateTime.GetFastestNISTDate(); }

            Dictionary<string, object> unitStageFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_data.ToString());
            if (unitStageFSData == null || !unitStageFSData.ContainsKey($"unit{unitLevel}"))
            {
                Logger.LogWarning($"Unit{unitLevel} data not found. Creating initial game data.", "GameLoader");
                CreateGameData();
                return;
            }
            Dictionary<string, object> currentUnitData = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];
            Logger.LogInfo($"Unit{unitLevel} data found: {Json.Serialize(currentUnitData)}", "GameLoader");
            if (!currentUnitData.ContainsKey(FSMapField.maingame.ToString()))
            {
                Logger.LogWarning("Maingame map not present. Creating initial game data.", "GameLoader");
                CreateGameData();
                return;
            }
            Dictionary<string, object> mainGameData = (Dictionary<string, object>)currentUnitData[FSMapField.maingame.ToString()];
            Logger.LogInfo($"Maingame data loaded: {Json.Serialize(mainGameData)}", "GameLoader");
            if (PlayerInfo.MainGameSelectedLevelLoaded)
            {
                level = PlayerInfo.MainGameSelectedLevelToLoad;
            }
            else
            {
                level = Convert.ToInt32(mainGameData[MainGame.level.ToString()]);
            }

            if (level != 0)
            {
                if (!mainGameData.ContainsKey(FSMapField.levels.ToString()))
                {
                    Logger.LogWarning("Levels map missing in maingame. Creating initial data.", "GameLoader");
                    CreateGameData();
                    return;
                }
                gameData = (Dictionary<string, object>)mainGameData[FSMapField.levels.ToString()];
                FirestoreClient.LoadGamePointsScore(mainGameData);
                string formattedLevel = level < 10 ? $"0{level}" : $"{level}";
                if (!gameData.ContainsKey(formattedLevel))
                {
                    Logger.LogWarning($"Level {formattedLevel} data missing. Creating initial data.", "GameLoader");
                    CreateGameData();
                    return;
                }
                Dictionary<string, object> gameLevelData = (Dictionary<string, object>)gameData[formattedLevel];
                var allLevels = (Dictionary<string, object>)mainGameData[MainGame.levels.ToString()];
                if (!allLevels.ContainsKey(formattedLevel))
                {
                    Logger.LogWarning($"Level {formattedLevel} mapping missing in levels. Creating initial data.", "GameLoader");
                    CreateGameData();
                    return;
                }
                Dictionary<string, object> levelDT = (Dictionary<string, object>)allLevels[formattedLevel];
                //  DateTime gameDT = currentDT;
                bool valid = false;
                Logger.LogInfo($"Current date is {currentDT}, checking maingame level {formattedLevel} date", "GameLoader");

                if (levelDT.ContainsKey(MainGame.date.ToString()))
                {
                    DateTime.TryParse((string)levelDT[MainGame.date.ToString()], out DateTime gameDT);
                    Logger.LogInfo($"Current game level {formattedLevel} date is {gameDT}", "GameLoader");
                    /*
                        <0 − If date1 is earlier than date2
                        0 − If date1 is the same as date2
                        >0 − If date1 is later than date2
                    */
                    valid = DateTime.Compare(currentDT.Date, gameDT.Date) > 0;
                }


                int life = PointSystem.Life = Convert.ToInt32(gameLevelData[MainGame.life.ToString()]);
                Dictionary<string, object> levelData = new Dictionary<string, object>(){
                                    {MainGame.life.ToString(), 3},
                                    {MainGame.played.ToString(), false},
                                    {MainGame.powerup_health.ToString(), 0}
            };
                Inference.SetPlayerInfoForPCollected(gameLevelData);

                if (gameLevelBtnParent != null)
                {
                    gameLevelBtnParent.GetComponent<GameLevelBtnStatus>().GameLevelBtnStatusData = (Dictionary<string, object>)gameData[FSMapField.locked.ToString()];
                }
                PlayerInfo.MainGameLevelPlayed = Convert.ToBoolean(gameLevelData[MainGame.played.ToString()]);
                if (level <= 10)
                {
                    Logger.LogInfo($"Current level {level} played is={PlayerInfo.MainGameLevelPlayed}; date comparision is= {valid};", "GameLoader");
                    if (valid && PlayerInfo.MainGameLevelPlayed && !PlayerInfo.GameLeveleLoadedForPassCollection) //new day
                    {
                        PointSystem.Life = 3;
                        levelToLoad = level + 1;
                        string formattedNewLevel = levelToLoad < 10 ? $"0{levelToLoad}" : $"{levelToLoad}";
                        PlayerInfo.MainGameLevelPlayed = false;
                        switch (levelToLoad)
                        {
                            case 2:
                                levelData.Add(MainGame.minigames_pass_collected.ToString(), false);
                                levelData.Add(MainGame.minigames_trivia_collected.ToString(), false);
                                break;
                            case 3:
                                levelData.Add(MainGame.vocabs_pass_collected.ToString(), false);
                                levelData.Add(MainGame.vocabs_trivia_collected.ToString(), false);
                                break;
                            case 4:
                                levelData.Add(MainGame.calculator_pass_collected.ToString(), false);
                                levelData.Add(MainGame.calculator_trivia_collected.ToString(), false);
                                break;
                            case 5:
                                levelData.Add(MainGame.video_pass_collected.ToString(), false);
                                levelData.Add(MainGame.video_trivia_collected.ToString(), false);
                                break;
                            default: break;
                        }

                        // Use centralized helper to build next-level update payload
                        Dictionary<string, object> updateData =
                            FirestoreData.BuildMainGameNextLevelUpdate($"unit{unitLevel}", levelToLoad, currentDT, levelData);

                        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
                        {
                            await ActionOnCreateNewLevelData(updateData);
                        }
                        else
                        {
                            retryAction += async () => await ActionOnCreateNewLevelData(updateData);
                            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);

                            Logger.LogError("No internet connectivity while updating main game next level..Waiting for connection", "GameLoader");
                            loading.SetActive(false);
                            return;
                        }
                    }
                    else //same day
                    {
                        levelToLoad = level;
                        if (life > 0 && life <= 3)
                        {
                            Logger.LogInfo($"Same level {level} available to play, remaining life={PointSystem.Life}", "GameLoader");
                            needToWatchVideoForLife = false;
                            if (PlayerInfo.GameLeveleLoadedForPassCollection)
                            {
                                Logger.LogInfo("Since this level is loaded for pass collection and player has enough life, exiting new level logic and directly loading the scene.", "GameLoader");
                                OnPlay();
                                loading.SetActive(false);
                                return;
                            }
                        }
                        else
                        {
                            Logger.LogInfo("No life left to play. You need to watch video to gain extra life", "GameLoader");

                            needToWatchVideoForLife = true;
                            if (PlayerInfo.SameSessionLifeReset)
                            {
                                PlayerInfo.SameSessionLifeReset = false;
                                OnPlay();
                                loading.SetActive(false);
                                return;
                            }
                        }
                    }
                    playGame.SetActive(true);
                }
                else
                {
                    if (gameOver != null) gameOver.SetActive(true);
                    Logger.LogInfo("All available levels completed", "GameLoader");
                }
                loading.SetActive(false);
            }
            else
            {

                Logger.LogWarning($"No maingame level {level} data present; creating initial data for level {level}", "GameLoader");
                CreateGameData();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Start failed in GameLoader", "GameLoader", ex);
            loading.SetActive(false);
        }
    }
    private async Task ReloadChildData()
    {
        FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);
        Logger.LogInfo($"Re Loading data from FS", "GameLoader");
        LoadGameData();
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogInfo("Connectivity restored in GameLevels, retrying action", "GameLoader");
        RetryTheAction(popup);
    }
    public void OnPlay()
    {
        PlayerInfo.GameLevel = levelToLoad < 10 ? $"0{levelToLoad}" : $"{levelToLoad}";
        if (needToWatchVideoForLife)
        {
            Logger.LogInfo("Need to watch video to gain life", "GameLoader");
            playGame.SetActive(value: false);
            playVideoForLife.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(GameSceneName[levelToLoad].ToString());
            Logger.LogInfo($"Loading maingame level {PlayerInfo.GameLevel}", "GameLoader");
        }
    }

    public void ResetPlayerLife()
    {
        loading.SetActive(true);
        PointSystem.Life = 3;
        // Centralized helper for maingame life reset payload
        Dictionary<string, object> resetData =
            FirestoreData.BuildMainGameLifeResetUpdate($"unit{unitLevel}", PlayerInfo.GameLevel, PointSystem.Life);
        ResettingLife(resetData);
    }

    async void ResettingLife(Dictionary<string, object> resetlifeData)
    {
        try
        {
            if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
            {
                await ActionOnResetLife(resetlifeData);
            }
            else
            {
                retryAction += async () => await ActionOnResetLife(resetlifeData);
                popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
                loading.SetActive(false);
                Logger.LogError("No internet connectivity while resetting life; skipping Firestore save", "GameLoader");
            }

        }
        catch (Exception ex)
        {
            Logger.LogError("ResettingLife failed in GameLoader", "GameLoader", ex);
        }
        finally
        {
            loading.SetActive(false);
        }
    }
    async void CreateGameData()
    {
        try
        {
            PointSystem.Life = 3;
            // Centralized helper to build initial maingame payload
            Dictionary<string, object> newdata =
                FirestoreData.BuildMainGameInitialData($"unit{unitLevel}", currentDT, PointSystem.Life);

            if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
            {
                await ActionOnCreateGameData(newdata);
            }
            else
            {
                retryAction += async () => await ActionOnCreateGameData(newdata);
                popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
                Logger.LogError("No internet connectivity while creating initial main game data; waiting for connection", "GameLoader");
            }

        }
        catch (Exception ex)
        {
            Logger.LogError("CreateGameData failed in GameLoader", "GameLoader", ex);
        }
        finally
        {
            loading.SetActive(false);
        }
    }
    //Attached to button as onClick in inspector
    public void BackToPlayGameScreen()
    {

        Logger.LogInfo("BackToPlayGameScreen clicked; showing play game screen", "GameLoader");
        playGame.SetActive(true);
        levelsScreen.SetActive(false);

    }
    //Attached to button as onClick in inspector 
    public void ShowLevelsScreen()
    {
        Logger.LogInfo("ShowLevelsScreen clicked; showing levels screen", "GameLoader");
        levelsScreen.SetActive(true);
        playGame.SetActive(true);
    }

    public void OnSelectedLevelPlay(Button btn)
    {
        loading.SetActive(true);
        PlayerInfo.MainGameSelectedLevelLoaded = true;
        PlayerInfo.MainGameSelectedLevelToLoad = levelToLoad = btn.GetComponent<StageGameLevelNumber>().levelNumber;
        string _formattedLevel = levelToLoad < 10 ? $"0{levelToLoad}" : $"{levelToLoad}";
        Dictionary<string, object> __gameLevelData = (Dictionary<string, object>)gameData[_formattedLevel];

        int _life = Convert.ToInt32(__gameLevelData[MainGame.life.ToString()]);
        Inference.SetPlayerInfoForPCollected(__gameLevelData);

        PlayerInfo.MainGameLevelPlayed = Convert.ToBoolean(__gameLevelData[MainGame.played.ToString()]);
        Logger.LogInfo($"Remaining life is {PointSystem.Life}", "GameLoader");

        if (_life > 0 && _life <= 3)
        {
            needToWatchVideoForLife = false;
            Logger.LogInfo($"Loading selected game level {GameSceneName[levelToLoad]} ... ", "GameLoader");


        }
        else
        {
            Logger.LogInfo("You have to watch video to get more life", "GameLoader");
            needToWatchVideoForLife = true;
            if (PlayerInfo.SameSessionLifeReset)
            {
                PlayerInfo.SameSessionLifeReset = false;

            }


        }

        OnPlay(); loading.SetActive(false);
    }

    /********************Actions on connection found*******************************************/

    async Task ActionOnCreateNewLevelData(Dictionary<string, object> _data)
    {
        await FirestoreClient.FirestoreDataSave(
                                FSCollection.parent.ToString(),
                                PlayerInfo.AuthenticatedID,
                                FSCollection.children.ToString(),
                                PlayerInfo.AuthenticatedChildID,
                                _data);
        Logger.LogInfo($"New level available: {levelToLoad} (previous level {level})", "GameLoader");

    }

    async Task ActionOnCreateGameData(Dictionary<string, object> _data)
    {
        await FirestoreClient.FirestoreDataSave(
                   FSCollection.parent.ToString(),
                   PlayerInfo.AuthenticatedID,
                   FSCollection.children.ToString(),
                   PlayerInfo.AuthenticatedChildID,
                   _data);

        levelToLoad = 1;
        if (playGame != null) playGame.SetActive(true);

    }

    async Task ActionOnResetLife(Dictionary<string, object> _data)
    {
        await FirestoreClient.FirestoreDataSave(
     FSCollection.parent.ToString(),
     PlayerInfo.AuthenticatedID,
     FSCollection.children.ToString(),
     PlayerInfo.AuthenticatedChildID,
     _data);
        if (playVideoForLife != null) playVideoForLife.SetActive(false);
        Logger.LogInfo($"Life reset; loading level {GameSceneName[levelToLoad]}", "GameLoader");
        needToWatchVideoForLife = false;
        SceneManager.LoadScene(GameSceneName[levelToLoad].ToString());
    }

}