using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//[RequireComponent(typeof(CalculatorFundamentals))]
public class Trivia_Calculator : TriviaQuizBase
{
    private readonly List<string> quizCount = new List<string>();
    public Dictionary<string, object> trivias;
    public int level;
    private Dictionary<string, object> buttonTrivia;
    private string context = "Trivia_Calculator";
    void Start()
    {
        loader.SetActive(true);
        FirestoreClient = new FirestoreDataOperationManager();
        foreach (KeyValuePair<string, string> button in PlayerInfo.UnitButtonInfo)
        {
            unitLevel = button.Key;
            buttonName = button.Value;
            Logger.LogInfo($"Unit level is {unitLevel} and stage name is {buttonName}", context);
        }
        baseLevel = level;
        StartCoroutine(LoadQuizJSON($"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/json/{buttonName}/{level}.json"));
    }
    IEnumerator LoadQuizJSON(string TriviaUrl)
    {

        Debug.Log($"Trivia quiz data path is {TriviaUrl}");
        UnityWebRequest request = UnityWebRequest.Get(TriviaUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            triviaQuizzes = JsonUtility.FromJson<TriviaQuizzes>(json: request.downloadHandler.text);
            for (int i = 0; i < triviaQuizzes.Quizzes.Length; i++)
            {
                quizCount.Add(triviaQuizzes.Quizzes[i].Number);
            }

            Debug.Log($"Trivia quiz data json is loaded having Trivia quiz count to {quizCount.Count}");
        }
        buttonTrivia = (Dictionary<string, object>)trivias[buttonName];
        currentQuizData = (Dictionary<string, object>)buttonTrivia[IFirestoreEnums.CalCulator.levels.ToString()];
        Logger.LogInfo($"Loaded currentQuizDatais {JsonUtility.ToJson(currentQuizData)}", context);



        FilterQuizQuestions();

    }


}
