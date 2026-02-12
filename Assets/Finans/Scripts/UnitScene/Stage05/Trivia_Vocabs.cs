using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static IFirestoreEnums;

[RequireComponent(typeof(Vocabs))]
public class Trivia_Vocabs : TriviaQuizBase
{
    List<string> quizCount = new List<string>();
    public Dictionary<string, object> trivias;
    public int level;
    private string context = "Trivia_Vocabs";
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

        Logger.LogInfo($"Trivia quiz data path is {TriviaUrl}", context);
        UnityWebRequest request = UnityWebRequest.Get(TriviaUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            triviaQuizzes = JsonUtility.FromJson<TriviaQuizzes>(json: request.downloadHandler.text);
            for (int i = 0; i < triviaQuizzes.Quizzes.Length; i++)
            {
                //OLD quizCount.Add(i);
                //New Addtion below
                quizCount.Add(triviaQuizzes.Quizzes[i].Number);
            }

            Logger.LogInfo($"Trivia quiz json is loaded having quiz count to {quizCount.Count}", context);
        }
        currentQuizData = (Dictionary<string, object>)((Dictionary<string, object>)trivias[buttonName])[Vocab.levels.ToString()];

        FilterQuizQuestions();
    }


}
