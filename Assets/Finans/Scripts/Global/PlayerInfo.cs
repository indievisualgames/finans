using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    private static string authenticatedid = "jhwsv8rXlbelpc9pdDwdXB14d9c2";//string.Empty; /////="M9f73WyEGaNf3K3VOErF2HHnzs13";// "qtqv29TLMvTKn5Z2ZZf0OwhIREJ3";  //string.Empty; //
                                                                           //private static string authenticatedid = "Hnvi6LC6M5MjrFY341Ptarji87a2"; //"jhwsv8rXlbelpc9pdDwdXB14d9c2"; //string.Empty; // //="M9f73WyEGaNf3K3VOErF2HHnzs13";// "qtqv29TLMvTKn5Z2ZZf0OwhIREJ3";  //string.Empty; //
    /// <summary>
    //  private static string authenticatedchildid = string.Empty; //"a27136f6fd16458f8937beaa09d6b881";//
    /// </summary>
    private static string authenticatedchildid = "af3bc05bbebb4d70a961d9a63a8b2434";// "8f9c7fe07cbb4e2ba56559613326a89c"; //"a27136f6fd16458f8937beaa09d6b881"; //string.Empty; //= "f6c3bc07a8c4439a9c5b08928bf6ee33";// "b85042ed305c4360a85051d6a58d9ded";"517d6cc7f5984f128cdef89cc7c0aaf0";//"1597b2d78a2440c5bb992e41be41cd45"; //string.Empty; //"8f9c7fe07cbb4e2ba56559613326a89c";// ;//="0f0189d4a14044d0a9a66fd812877087";//
    // 
    private static bool isappauthenticated = true;
    private static bool maingamelevelplayed = false;
    private static Dictionary<string, string> unitbuttoninfo = new Dictionary<string, string>() { { "01", "minigames" } };//{ { "01", "vocabs" } };// { {"01", "calculator" }};//{ { "01", "calculator" } }//;////{ { "01", "flashcard" } };  //{ { "01", "vocabs" } };////;// { { "01", "calculator" } };
    // private static Dictionary<string, object> currentUnitInfo = new Dictionary<string, object>();//{{"current_unit", "111"},{"current_unit_name", "Intronnn"},{"current_stage_name", "lessononn"},{"level_completed", "1"},{"rank", "1000"}};
    private static Sprite profileimagesprite = null;
    private static bool lpasscollected = false;
    private static bool fpasscollected = false;
    private static bool flashTriviaCollected = false;
    private static bool mgspasscollected = false;
    private static bool mgstriviacollected = false;
    private static bool vocabspasscollected = false;
    private static bool vocabstriviacollected = false;
    private static bool calcpasscollected = false;
    private static bool calctriviacollected = false;
    private static bool videopasscollected = false;
    private static bool videotriviacollected = false;
    private static bool storelpcollected = false;
    private static bool storefpcollected = false;
    private static bool storeftcollected = false;
    private static bool storemgpcollected = false;
    private static bool storemgtcollected = false;
    private static bool storevpcollected = false;
    private static bool storevtcollected = false;
    private static bool storecalcpcollected = false;
    private static bool storecalctcollected = false;
    private static bool storevideopcollected = false;
    private static bool storevideotcollected = false;
    private static int maingameSelectedLevelToLoad = -1;
    private static bool maingameSelectedLevelLoaded = false;
    private static bool needPlayerLifeReset = false;
    private static bool triviaPassCollection = false;
    private static bool gameLeveleLoadedForPassCollection = false;
    private static bool sameSessionLifeReset = false;
    private static string childname = string.Empty;

    private static bool sceneRelod = false;
    private static string avatarURL = string.Empty;
    //private static string triviaquiztitle = string.Empty;// = "intro";
    private static string gameLevel = string.Empty;
    private static string currentLoginAttempt = string.Empty;
    public static string CurrentLoginAttempt
    {
        get { return currentLoginAttempt; }
        set { currentLoginAttempt = value; }
    }
    public static string AuthenticatedID
    {
        get { return authenticatedid; }
        set { authenticatedid = value; }
    }
    public static bool IsAppAuthenticated
    {
        get { return isappauthenticated; }
        set { isappauthenticated = value; }
    }
    public static bool NeedPlayerLifeReset
    {
        get { return needPlayerLifeReset; }
        set { needPlayerLifeReset = value; }
    }
    public static bool TriviaPassCollection
    {
        get { return triviaPassCollection; }
        set { triviaPassCollection = value; }
    }
    public static bool SameSessionLifeReset
    {
        get { return sameSessionLifeReset; }
        set { sameSessionLifeReset = value; }
    }
    public static bool GameLeveleLoadedForPassCollection
    {
        get { return gameLeveleLoadedForPassCollection; }
        set { gameLeveleLoadedForPassCollection = value; }
    }
    public static string AuthenticatedChildID
    {
        get { return authenticatedchildid; }
        set { authenticatedchildid = value; }
    }
    public static string ChildName
    {
        get { return childname; }
        set { childname = value; }
    }
    public static string AvatarURL
    {
        get { return avatarURL; }
        set { avatarURL = value; }
    }
    public static Dictionary<string, string> UnitButtonInfo
    {
        get { return unitbuttoninfo; }
        set { unitbuttoninfo = value; }
    }

    public static void StorePassCollectedToFalse()
    {
        storelpcollected =
        storefpcollected =
        storemgpcollected =
        storemgtcollected =
        storevpcollected =
        storevtcollected =
        storecalcpcollected =
        storecalctcollected =
        storevideopcollected =
        storevideotcollected = false;
    }
    /* public static Dictionary<string, object> CurrentUnitInfo
     {
         get { return currentUnitInfo; }
         set { currentUnitInfo = value; }
     }*/
    /*  public static string TriviaQuizTitle
      {
          get { return triviaquiztitle; }
          set { triviaquiztitle = value; }
      }*/
    public static bool SceneReload
    {
        get { return sceneRelod; }
        set { sceneRelod = value; }
    }
    public static Sprite ProfileImageSprite
    {
        get { return profileimagesprite; }
        set { profileimagesprite = value; }
    }
    public static bool MainGameLevelPlayed
    {
        get { return maingamelevelplayed; }
        set { maingamelevelplayed = value; }
    }
    public static bool LessonPassCollected
    {
        get { return lpasscollected; }
        set { lpasscollected = value; }
    }

    public static bool FlashPassCollected
    {
        get { return fpasscollected; }
        set { fpasscollected = value; }
    }
    public static bool FlashTriviaCollected
    {
        get { return flashTriviaCollected; }
        set { flashTriviaCollected = value; }
    }
    public static bool MiniGamesPassCollected
    {
        get { return mgspasscollected; }
        set { mgspasscollected = value; }
    }
    public static bool MiniGamesTriviaCollected
    {
        get { return mgstriviacollected; }
        set { mgstriviacollected = value; }
    }
    public static bool VocabsPassCollected
    {
        get { return vocabspasscollected; }
        set { vocabspasscollected = value; }
    }
    public static bool VocabsTriviaCollected
    {
        get { return vocabstriviacollected; }
        set { vocabstriviacollected = value; }
    }
    public static bool CalcPassCollected
    {
        get { return calcpasscollected; }
        set { calcpasscollected = value; }
    }
    public static bool CalcTriviaCollected
    {
        get { return calctriviacollected; }
        set { calctriviacollected = value; }
    }

    public static bool VideoPassCollected
    {
        get { return videopasscollected; }
        set { videopasscollected = value; }
    }
    public static bool VideoTriviaCollected
    {
        get { return videotriviacollected; }
        set { videotriviacollected = value; }
    }

    public static bool StoreLPCollected
    {
        get { return storelpcollected; }
        set { storelpcollected = value; }
    }
    public static bool StoreFPCollected
    {
        get { return storefpcollected; }
        set { storefpcollected = value; }
    }
    public static bool StoreFTCollected
    {
        get { return storeftcollected; }
        set { storeftcollected = value; }
    }
    public static bool StoreMGsPassCollected
    {
        get { return storemgpcollected; }
        set { storemgpcollected = value; }
    }
    public static bool StoreMGsTriviaCollected
    {
        get { return storemgtcollected; }
        set { storemgtcollected = value; }
    }
    public static bool StoreVocabsPassCollected
    {
        get { return storevpcollected; }
        set { storevpcollected = value; }
    }
    public static bool StoreVocabsTriviaCollected
    {
        get { return storevtcollected; }
        set { storevtcollected = value; }
    }
    public static bool StoreCalcPassCollected
    {
        get { return storecalcpcollected; }
        set { storecalcpcollected = value; }
    }
    public static bool StoreCalcTriviaCollected
    {
        get { return storecalctcollected; }
        set { storecalctcollected = value; }
    }
    public static bool StoreVideoPassCollected
    {
        get { return storevideopcollected; }
        set { storevideopcollected = value; }
    }
    public static bool StoreVideoTriviaCollected
    {
        get { return storevideotcollected; }
        set { storevideotcollected = value; }
    }
    public static string GameLevel
    {
        get { return gameLevel; }
        set { Logger.LogInfo($"Setting GameLevel to {value}", "PlayerInfo"); gameLevel = value; }
    }
    public static int MainGameSelectedLevelToLoad
    {
        get { return maingameSelectedLevelToLoad; }
        set { maingameSelectedLevelToLoad = value; }
    }
    public static bool MainGameSelectedLevelLoaded
    {
        get { return maingameSelectedLevelLoaded; }
        set { maingameSelectedLevelLoaded = value; }
    }
}



