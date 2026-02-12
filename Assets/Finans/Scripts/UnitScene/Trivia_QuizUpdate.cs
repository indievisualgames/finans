using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Updated version of Trivia_Quiz with small refactors (no behavior change).
public class Trivia_QuizUpdate : TriviaQuizBase
{
    private readonly List<string> quizCount = new List<string>();

    // Populated externally before this script runs (kept same name as original Trivia_Quiz)
    public Dictionary<string, object> trivias;
    public int level = 1;

    private string context = "Trivia_QuizUpdate";
    private Dictionary<string, object> buttonTrivia;

    private void Start()
    {
        loader.SetActive(true);

        FirestoreClient = new FirestoreDataOperationManager();

        foreach (KeyValuePair<string, string> button in PlayerInfo.UnitButtonInfo)
        {
            unitLevel = button.Key;
            buttonName = button.Value;
            Logger.LogInfo($"Unit level is {unitLevel} and stage is {buttonName}", context);
        }

        baseLevel = level;

        string triviaUrl =
            $"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/json/{buttonName}/{level}.json";

        StartCoroutine(LoadQuizJSON(triviaUrl));
    }

    private IEnumerator LoadQuizJSON(string triviaUrl)
    {
        Debug.Log($"{context}: Trivia quiz data path is {triviaUrl}");

        UnityWebRequest request = UnityWebRequest.Get(triviaUrl);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            triviaQuizzes = JsonUtility.FromJson<TriviaQuizzes>(request.downloadHandler.text);

            for (int i = 0; i < triviaQuizzes.Quizzes.Length; i++)
            {
                quizCount.Add(triviaQuizzes.Quizzes[i].Number);
            }

            Debug.Log($"{context}: Trivia quiz data loaded, count = {quizCount.Count}");
        }

        // Cache the per-button trivia dictionary once
        buttonTrivia = (Dictionary<string, object>)trivias[buttonName];
        currentQuizData = (Dictionary<string, object>)buttonTrivia[IFirestoreEnums.CalCulator.levels.ToString()];

        // Let the base class handle question selection / UI
        FilterQuizQuestions();
    }
}



