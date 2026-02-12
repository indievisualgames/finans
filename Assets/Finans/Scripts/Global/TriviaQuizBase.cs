using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using static IFirestoreEnums;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor.Events;
#endif
[RequireComponent(typeof(InternetConnectivityCheck))]
public class TriviaQuizBase : Elevator
{
    protected IFirestoreOperator FirestoreClient;
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected GameObject loader;
    [SerializeField] protected GameObject timerObject;
    [SerializeField] Image questionImage;
    [SerializeField] TMP_Text question;
    [SerializeField] protected TMP_Text optionA;
    [SerializeField] protected TMP_Text optionB;
    [SerializeField] protected TMP_Text optionC;
    [SerializeField] protected TMP_Text optionD;
    [SerializeField] protected TMP_Text correctAnswer;
    [SerializeField] TMP_Text optionIsCorrect;
    [SerializeField] TMP_Text countDownTimer;
    public GameObject triviaScreenContent;
    [SerializeField] GameObject quizCompleted;

    [SerializeField] GameObject levelReAttempt;
    [SerializeField] GameObject levelCompleted;
    [SerializeField] GameObject quizFrame;
    [SerializeField] protected Button nextButton;
    [SerializeField] GameObject hud;
    [SerializeField] GameObject starCointainer;
    string answer;
    GameObject popup;
    protected InternetConnectivityCheck internetConnectivityCheck;
    public GameObject messageBoxPopupPrefab;

    List<TriviaQuiz> allQuizzes = new List<TriviaQuiz>();
    private string baseContext = "TriviaQuizBase";

    //protected Canvas m_canvas;
    protected RawImage clickedButtonBG;
    string currentQuestionNumber = "0";
    private int questionNumber = 0;
    protected string unitLevel = "";
    protected string buttonName = "";
    protected int baseLevel;
    [Serializable]
    public class CurrectAnswer
    {
        public string Answer;
    }
    [Serializable]
    public class TriviaQuiz
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
    public class TriviaQuizzes
    {
        public TriviaQuiz[] Quizzes;


    }

    protected List<string> questions = new List<string>();
    protected GameObject InstantiatedStars;
    public List<string> Questions
    {
        get { return questions; }
        set { questions = value; }
    }
    protected TriviaQuizzes triviaQuizzes;
    protected Dictionary<string, object> currentQuizData = new Dictionary<string, object>();
    bool allCorrect = true;

    IEnumerator LoadImage(string url)
    {
        Logger.LogInfo($"Loading ques {questionNumber} image from {url}", baseContext);

        UnityWebRequest uwr = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        uwr.downloadHandler = texDl;
        yield return uwr.SendWebRequest();
        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Texture2D t = texDl.texture;
            Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                Vector2.zero, 1f);
            questionImage.sprite = s;
            Logger.LogInfo("Loaded question image", baseContext);
        }
        else
        {
            Logger.LogWarning("Unable to load question image", baseContext);
        }


    }

    bool CreateQuizScreen()
    {
        try
        {
            question.text = allQuizzes[questionNumber].Question;
            optionA.text = allQuizzes[questionNumber].A;
            optionB.text = allQuizzes[questionNumber].B;
            optionC.text = allQuizzes[questionNumber].C;
            optionD.text = allQuizzes[questionNumber].D;
            answer = allQuizzes[questionNumber].Answer;
            currentQuestionNumber = allQuizzes[questionNumber].Number;
            Logger.LogInfo($"Showing quiz {questionNumber} form the available list having assigned number {currentQuestionNumber} in json", baseContext);
            Logger.LogInfo($"Correct answer for the current quiz number {currentQuestionNumber} is {answer}", baseContext);

            StartCoroutine(LoadImage($"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/images/{buttonName}/{allQuizzes[questionNumber].Image}.png"));

            switch (allQuizzes[questionNumber].Answer)
            {
                case "A":
                    correctAnswer.text = allQuizzes[questionNumber].A; optionIsCorrect.text = "A";
                    break;
                case "B":
                    correctAnswer.text = allQuizzes[questionNumber].B; optionIsCorrect.text = "B";
                    break;
                case "C":
                    correctAnswer.text = allQuizzes[questionNumber].C; optionIsCorrect.text = "C";
                    break;
                case "D":
                    correctAnswer.text = allQuizzes[questionNumber].D; optionIsCorrect.text = "D";
                    break;
                default:
                    break;
            }
            if (questionNumber < allQuizzes.Count)
            {
                questionNumber++;
            }
            Logger.LogInfo($"Showing quiz created for question number {questionNumber} having answer {answer}..", baseContext);
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error creating quiz screen: {ex.Message}", baseContext);
            return false;
        }

    }

    public async void OnSelectAnswer(Button button)
    {
        optionA.transform.parent.GetComponent<Button>().enabled = false;
        optionB.transform.parent.GetComponent<Button>().enabled = false;
        optionC.transform.parent.GetComponent<Button>().enabled = false;
        optionD.transform.parent.GetComponent<Button>().enabled = false;
        bool result = button.name.EndsWith(answer) ? true : false;


        clickedButtonBG = button.GetComponentInChildren<RawImage>();
        clickedButtonBG.enabled = true;

        if (result)
        {
            TriviaQuiz allImportantObjects = allQuizzes.FirstOrDefault(obj => obj.Number == currentQuestionNumber);
            clickedButtonBG.color = new Color32(108, 243, 15, 107);


            InstantiatedStars = Instantiate(starCointainer, new Vector3(0f, 0f, 0f), Quaternion.identity);
            InstantiatedStars.transform.SetParent(canvas.transform, false);
            // FirestoreClient.FirestoreDataSave(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID, FirestoreData.UpdatePointScore(ScorePoint.TRIVIA));

            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                {
                    FSMapField.unit_stage_data.ToString(),
                    new Dictionary<string, object>(){
                {$"unit{unitLevel}", new Dictionary<string, object>(){
                       {FSMapField.trivia.ToString(), new Dictionary<string, object>(){
                        {buttonName,new Dictionary<string, object>(){
                            {Flashcard.levels.ToString(),new Dictionary<string, object>(){
                            {baseLevel.ToString(), new Dictionary<string, object>(){
                                {allImportantObjects.Number.ToString(), false}
                                }
                                }

                       } }
                       }}
                    }
                    }}}}
                }
            };
            var pts = FirestoreData.UpdatePointScore(ScorePoint.TRIVIA);
            if (pts != null)
            {
                foreach (var kv in pts)
                {
                    if (!data.ContainsKey(kv.Key))
                        data.Add(kv.Key, kv.Value);
                    else
                        data[kv.Key] = kv.Value;
                }
                Logger.LogInfo($"Updated point score system in FS database", baseContext);
            }
            Logger.LogInfo($"Updated question no {currentQuestionNumber} as answered in FS database", baseContext);
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
                Logger.LogError($"No connection found while updating trivia question status", baseContext);
                return;
            }
        }
        else
        {
            allCorrect = false;
            clickedButtonBG.color = new Color32(250, 1, 1, 107);
            correctAnswer.transform.parent.gameObject.SetActive(true);
            Logger.LogInfo($"Your answer is incorrect", baseContext);
        }

        if (allQuizzes.Count > 1 && questionNumber < allQuizzes.Count)
        {
            Logger.LogInfo("Enabling next question button...", baseContext);
            nextButton.gameObject.SetActive(true);
        }
        else if (allQuizzes.Count == questionNumber)
        {
            Logger.LogInfo($"End of quiz for level {baseLevel}..Checking if all answers were answered correct..", baseContext);
            if (allCorrect)
            {
                await UpdateQuizAttempt();
            }
            else
            {
                StartCoroutine(CountDownBegins());
            }

        }
    }
    public async void NextQuestion()
    {

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {

            loader.SetActive(true);
            clickedButtonBG.enabled = false;
            correctAnswer.transform.parent.gameObject.SetActive(false);
            optionA.transform.parent.GetComponent<Button>().enabled = true;
            optionB.transform.parent.GetComponent<Button>().enabled = true;
            optionC.transform.parent.GetComponent<Button>().enabled = true;
            optionD.transform.parent.GetComponent<Button>().enabled = true;
            if (CreateQuizScreen())
            {
                Destroy(InstantiatedStars);
                nextButton.gameObject.SetActive(false);
                loader.SetActive(false);
            }
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogError($"Debug.Log:------------------Debug log: No connection found ", baseContext);

        }

    }


    float currCountdownValue = 3f;
    IEnumerator CountDownBegins()
    {
        while (currCountdownValue > 0)
        {
            Debug.Log("Countdown: " + currCountdownValue);
            countDownTimer.text = $"00:0{currCountdownValue}";
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }
        countDownTimer.text = string.Empty;
        hud.SetActive(false);
        triviaScreenContent.SetActive(false);
        if (allCorrect)
        {
            levelCompleted.SetActive(true);
            Logger.LogInfo($"Congrats!! This level {baseLevel} is now complete..", baseContext);
        }
        else { levelReAttempt.SetActive(true); Logger.LogInfo($"Some answered were incorrect..You can retry to answer them again", baseContext); }

    }
    async Task UpdateQuizAttempt()
    {
        // Use centralized helper to mark this trivia level as completed in Firestore.
        Dictionary<string, object> data =
            FirestoreData.BuildQuizLevelCompleted($"unit{unitLevel}", buttonName, baseLevel);
        Logger.LogInfo($"You answered all correct.. Updating stage {buttonName} level {baseLevel} status to completed", baseContext);

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                data);
            StartCoroutine(CountDownBegins());
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            Logger.LogError($"No connection found while marking trivia level complete", baseContext);
        }
    }

    protected void FilterQuizQuestions()
    {
        foreach (var quizItem in currentQuizData)
        {
            if (quizItem.Key == baseLevel.ToString())
            {
                foreach (var qDetails in quizItem.Value as Dictionary<string, object>)
                {
                    if (Convert.ToBoolean(qDetails.Value))
                    {
                        //// int.TryParse(qDetails.Key, out var question);
                        //// Questions.Add(question);
                        Questions.Add(qDetails.Key);
                        Logger.LogInfo($"Current quiz have unanswered question number {qDetails.Key} present in FS.....", baseContext);
                    }
                }
            }
        }
        LoadQuiz();
    }


    void LoadQuiz()
    {
        List<TriviaQuiz> tempQuizListHolder = new List<TriviaQuiz>();
        for (int i = 0; i < triviaQuizzes.Quizzes.Length; i++)
        {

            if (Questions.Contains(triviaQuizzes.Quizzes[i].Number))
            {
                Logger.LogInfo($"Creating quiz for question number : {i}", baseContext);
                TriviaQuiz bquiz = new TriviaQuiz();
                bquiz.Question = triviaQuizzes.Quizzes[i].Question;
                bquiz.A = triviaQuizzes.Quizzes[i].A;
                bquiz.B = triviaQuizzes.Quizzes[i].B;
                bquiz.C = triviaQuizzes.Quizzes[i].C;
                bquiz.D = triviaQuizzes.Quizzes[i].D;
                bquiz.Answer = triviaQuizzes.Quizzes[i].Answer;
                bquiz.Image = triviaQuizzes.Quizzes[i].Image;
                // bquiz.Number = i.ToString();
                bquiz.Number = triviaQuizzes.Quizzes[i].Number;
                tempQuizListHolder.Add(bquiz);
            }
        }
        allQuizzes = ShuffleTriviaQuiz(tempQuizListHolder);
        Logger.LogInfo($"Completed creating all {allQuizzes.Count} quiz", baseContext);
        if (CreateQuizScreen())
        {
            hud.SetActive(true);
            triviaScreenContent.SetActive(true);
            loader.SetActive(false);
            timerObject.GetComponent<ScreenTimer>().startTimer = true;
        }

    }

    List<TriviaQuiz> ShuffleTriviaQuiz(List<TriviaQuiz> inputList)
    {
        int i = 0;
        int t = inputList.Count;
        int r = 0;
        TriviaQuiz p = null;
        List<TriviaQuiz> tempList = new List<TriviaQuiz>();
        tempList.AddRange(inputList);

        while (i < t)
        {
            r = UnityEngine.Random.Range(i, tempList.Count);
            p = tempList[i];
            tempList[i] = tempList[r];
            tempList[r] = p;
            i++;
        }

        return tempList;
    }
}
