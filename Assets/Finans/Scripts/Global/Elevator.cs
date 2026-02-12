using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Ricimi;
using System.Threading.Tasks;
using static IFirestoreEnums;
using System.Collections.Generic;
using System;


public class Elevator : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds2 = new WaitForSeconds(2);

    //Parent profile

    protected void LoadProfileAndFinalizeScreen(Image _parentPic, TMP_Text _displayName, GameObject _screenContent, GameObject _loading)
    {
        _displayName.text = $"Hello User";
        if (PlayerInfo.IsAppAuthenticated && Firebase.Auth.FirebaseAuth.DefaultInstance != null)
        {
            var currentUser = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
            if (currentUser != null && currentUser.IsValid() && !string.IsNullOrEmpty(currentUser.DisplayName))
            {
                _displayName.text = $"Hello {currentUser.DisplayName}";
            }
        }
        _parentPic.sprite = PlayerInfo.ProfileImageSprite;
        _parentPic.preserveAspect = true;
        OperationFinished(_screenContent, _loading);

    }

    //Child profile
    protected void LoadCProfileAndFinalizeScreen(Image _profilePic, TMP_Text _displayName, GameObject _screenContent, GameObject _loading, GameObject _hud = null)
    {
        Sprite[] _sprite = Resources.LoadAll<Sprite>(Params.AvatarSSName);
        for (int i = 0; i < _sprite.Length; i++)
        {
            if (_sprite[i].name == PlayerInfo.AvatarURL)
            {
                _profilePic.sprite = _sprite[i];
                _profilePic.preserveAspect = true;
                PlayerInfo.ProfileImageSprite = _sprite[i];
                _profilePic.sprite = _sprite[i];
            }
        }
        _displayName.text = $"Hello {PlayerInfo.ChildName}";
        OperationFinished(_screenContent, _loading, _hud);

    }
    protected IEnumerator LoadUnitOrStageImage(string url_image, Image image)
    {
        using (UnityWebRequest uwr = new UnityWebRequest(url_image))
        {
            uwr.disposeDownloadHandlerOnDispose = true;
            uwr.disposeUploadHandlerOnDispose = true;
            uwr.disposeCertificateHandlerOnDispose = true;

            DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
            uwr.downloadHandler = texDl;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D t = texDl.texture;
                Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                    Vector2.zero, 1f);
                if (image != null)
                {
                    image.sprite = s;
                }
            }
        }
        yield break;
    }


    protected void OperationFinished(GameObject screenContent, GameObject loading, GameObject hud = null)
    {
        if (hud != null) hud.SetActive(true);
        screenContent.SetActive(true);
        loading.SetActive(false);

    }
    protected void FinalizeScreen(GameObject _screenContent, GameObject _loading)
    {
        OperationFinished(_screenContent, _loading);

    }
    protected Action retryAction = null;
    protected GameObject ShowNoConnectivityPopup(Canvas canvas, InternetConnectivityCheck internetConnectivityCheck, GameObject messageBoxPopupPrefab, bool primary = false, bool secondary = false, bool teritary = false)
    {
        MessageBox msgBox = messageBoxPopupPrefab.GetComponent<MessageBox>();
        msgBox.Headline = Message.MBNoInternetHeadline;
        msgBox.Message = Message.MBNoInternetMessage;
        if (primary)
        {

            msgBox.ActionText = Message.MBActionButtonText;
            msgBox.actionButton.SetActive(true);
        }
        else if (secondary)
        {
            msgBox.SecondaryText = Message.MBSecondaryButtonText;
            msgBox.secondaryButton.SetActive(true);
        }
        else if (teritary)
        {
            msgBox.TertiaryText = Message.MBTeritaryButtonText;
            msgBox.tertiaryButton.SetActive(true);
        }

        GameObject popup = Inference.OpenPopup(canvas, messageBoxPopupPrefab);
        Logger.LogInfo("Message box opened (no connectivity)", "Elevator");
        internetConnectivityCheck.CheckNow(true);
        return popup;

    }
    protected string ToTitleCase(string text)
    {
        return char.ToUpper(text[0]) + text.Substring(1);
    }

    protected async Task<bool> HaveConnectivity(Canvas canvas, GameObject messageBoxPopupPrefab, GameObject loading = null)
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        { return true; }
        MessageBox msgBox = messageBoxPopupPrefab.GetComponent<MessageBox>();
        msgBox.Headline = Message.MBNoInternetHeadline;
        msgBox.Message = Message.MBNoInternetMessage;
        msgBox.actionButton.SetActive(false);
        msgBox.secondaryButton.SetActive(false);
        msgBox.tertiaryButton.SetActive(false);
        GameObject popup = Inference.OpenPopup(canvas, messageBoxPopupPrefab);
        if (loading != null) loading.SetActive(false);
        StartCoroutine(CloseInternetConnectivityPopup(popup));
        return false;
    }

    IEnumerator CloseInternetConnectivityPopup(GameObject popup)
    { //yield return new WaitForSeconds(2);
        yield return _waitForSeconds2;
        if (popup != null && popup.GetComponent<Popup>() != null)
        {
            popup.GetComponent<Popup>().Close();
        }
    }

    protected GameObject ShowMessagePopup(Canvas m_canvas, GameObject messageBoxPopupPrefab, string headline, string message, string actionText, string secondaryText, int buttonToEnable, UnityAction action = null)
    {
        MessageBox msgBox = messageBoxPopupPrefab.GetComponent<MessageBox>();
        msgBox.Headline = headline;
        msgBox.Message = message;
        Logger.LogInfo($"Opening message box: {headline}", "Elevator");
        switch (buttonToEnable)
        {
            case 1:
                msgBox.ActionText = actionText;
                msgBox.actionButton.SetActive(true);
                // if (action != null) { msgBox.actionButton.GetComponent<Button>().onClick.AddListener(action); }
                break;
            case 2:
                msgBox.ActionText = actionText;
                msgBox.SecondaryText = secondaryText;
                msgBox.actionButton.SetActive(true);
                msgBox.secondaryButton.SetActive(true);

                break;

            default: break;
        }
        if (action != null) { msgBox.actionButton.GetComponent<Button>().onClick.AddListener(action); }
        Logger.LogInfo($"Attached event listener to {headline} MessageBox", "Elevator");
        GameObject popup = Inference.OpenPopup(m_canvas, messageBoxPopupPrefab);
        Logger.LogInfo($"Popup for {headline} opened", "Elevator");
        return popup;

    }

    // Centralized toast/helper for short-lived user messages
    protected void ShowToast(Canvas m_canvas, GameObject messageBoxPopupPrefab, string message, float seconds = 2f, string headline = "Info")
    {
        MessageBox msgBox = messageBoxPopupPrefab.GetComponent<MessageBox>();
        msgBox.Headline = headline;
        msgBox.Message = message;
        msgBox.actionButton.SetActive(false);
        msgBox.secondaryButton.SetActive(false);
        GameObject popup = Inference.OpenPopup(m_canvas, messageBoxPopupPrefab);
        StartCoroutine(ClosePopupAfterSeconds(popup, seconds));
        Logger.LogInfo($"Toast shown: {message}", "Elevator");
    }

    private IEnumerator ClosePopupAfterSeconds(GameObject popup, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (popup != null && popup.GetComponent<Popup>() != null)
        {
            popup.GetComponent<Popup>().Close();
        }
    }
    public void ReplayLevelForPassCollection(string unitLevel, int __level, string buttonName, string context)
    {
        string gameLevel = __level < 10 ? $"0{__level}" : $"{__level}";
        string __unitLevel = unitLevel;
        PlayerInfo.GameLevel = gameLevel;
        PlayerInfo.UnitButtonInfo.Clear();
        PlayerInfo.UnitButtonInfo.Add(__unitLevel, buttonName);
        PlayerInfo.TriviaPassCollection = true;
        PlayerInfo.GameLeveleLoadedForPassCollection = true;
        PlayerInfo.MainGameSelectedLevelToLoad = __level;
        PlayerInfo.MainGameSelectedLevelLoaded = true;

        if (FetchLevelDetails(unitLevel, gameLevel))
        {
            //  PlayerInfo.TriviaPassCollection = true;
            Logger.LogInfo($"Replay game level {gameLevel} to unlock trivia quiz", context);
            Transition.LoadLevel(SceneName.MainGame.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
        else
        {
            Logger.LogInfo("Watch video to gain extra life", context);
            Transition.LoadLevel(SceneName.MainGame.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
    }

    public bool FetchLevelDetails(string unitLevel, string gameLevel, string context = "Elevator")
    {
        Dictionary<string, object> unitStageFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_data.ToString());
        if (unitStageFSData == null || !unitStageFSData.ContainsKey($"unit{unitLevel}"))
        {
            Logger.LogWarning($"Unit data missing for unit{unitLevel}", context);
            return false;
        }
        Dictionary<string, object> currentUnitData = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];
        if (!currentUnitData.ContainsKey(FSMapField.maingame.ToString()))
        {
            Logger.LogWarning("Maingame field missing in unit data", context);
            return false;
        }
        Dictionary<string, object> mainGameData = (Dictionary<string, object>)currentUnitData[FSMapField.maingame.ToString()];
        Logger.LogInfo($"Loading game data for level {gameLevel}", context);

        if (!mainGameData.ContainsKey(FSMapField.levels.ToString()))
        {
            Logger.LogWarning("Levels field missing in maingame", context);
            return false;
        }
        var levelsMap = (Dictionary<string, object>)mainGameData[FSMapField.levels.ToString()];
        if (!levelsMap.ContainsKey(gameLevel))
        {
            Logger.LogWarning($"Requested level {gameLevel} not present in levels map", context);
            return false;
        }
        Dictionary<string, object> gameLevelData = (Dictionary<string, object>)levelsMap[gameLevel];
        Inference.SetPlayerInfoForPCollected(gameLevelData);
        int life = PointSystem.Life = Convert.ToInt32(gameLevelData[MainGame.life.ToString()]);
        if (life == 0)
        {
            PlayerInfo.NeedPlayerLifeReset = true;
            Logger.LogInfo($"No life to play level, setting PlayerInfo.NeedPlayerLifeReset to {PlayerInfo.NeedPlayerLifeReset}", context);
            return false;
        }
        return true;
    }

    protected bool ReturnQuizLevelPlayedStatus(int _level, bool _triviaPass, Dictionary<string, object> _quizzes, string _context)
    {
        switch (_level)
        {
            case 1:
                /* if (!Convert.ToBoolean(((Dictionary<string, object>)quiz[Quizzes.flashcard.ToString()])["1"]))*/
                if (!_triviaPass)
                {
                    Logger.LogInfo($"New level available, but Trivia pass from penguin level {_level} not collected.....Cannot create new level ", _context);
                    return false;
                }
                else if (!Convert.ToBoolean(_quizzes["1"]))
                {
                    Logger.LogInfo($"New level available, trivia pass from penguin level 02 also collected, but the level 01 quiz is not played.....Cannot create new level ", _context);
                    return false;
                }
                break;
            default:
                int previousLevel = _level - 1;
                if (!Convert.ToBoolean(_quizzes[previousLevel.ToString()]))
                {
                    Logger.LogInfo($"Current level is {_level} and as previous level {previousLevel} quiz not completed, unable to create new level", _context);
                    return false;
                }

                break;
        }
        return true;
    }

    public bool CheckQuizPlayedStatus(int _level, bool _triviaPass, Dictionary<string, object> _quizzes, GameObject _takeQuizButton, GameObject _collectQuizPassButton, string _maingamelevel, string _context)
    {
        if (_level == 1)
        {

            if (!_triviaPass)
            {
                _collectQuizPassButton.SetActive(true);
                Logger.LogInfo($"Trivia pass from maingame level {_maingamelevel} not collected....Collect now., ", _context);
                return false;
            }
            else if (!Convert.ToBoolean(_quizzes["1"]))
            {
                _takeQuizButton.SetActive(true);
                Logger.LogInfo($"Trivia pass from maingame level {_maingamelevel} collected, enabling quiz", _context);
            }

        }

        else
        {
            if (_quizzes.ContainsKey(_level.ToString()))
            {
                if (Convert.ToBoolean(_quizzes[_level.ToString()]))
                {
                    Logger.LogInfo($"Current level {_level} quiz completed..", _context);
                }
                else
                {
                    _takeQuizButton.SetActive(true);
                    Logger.LogInfo($"Ready to play level {_level} quiz", _context);
                }
            }
            else
            {
                _takeQuizButton.SetActive(true);
                Logger.LogInfo($"Ready to play level {_level} quiz", _context);
            }
        }
        return true;
    }
    protected void RetryTheAction(GameObject popup)
    {
        retryAction?.Invoke();
        popup.GetComponent<Popup>().Close();
        Logger.LogInfo($"Retry action invoked inside and popup closed", "Elevator");


    }




    [Serializable]
    protected class QuizData
    {

        public string Question;
        public string A;
        public string B;
        public string C;
        public string D;
        public string Answer;
        public string Number;
        public string Image;

    }
    [Serializable]
    protected class QuizzesData
    {
        public QuizData[] Quizzes;
    }
}






