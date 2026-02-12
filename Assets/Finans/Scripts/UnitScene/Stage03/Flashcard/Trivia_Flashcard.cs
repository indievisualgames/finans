using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Trivia_Flashcard : TriviaQuizBase
{
    private int[] quizCardNumber = new int[2];
    public Dictionary<string, object> trivias;
    public int level = 1;
    private string _context = "Trivia_Flashcard";
    public int[] QuizCardNumber
    {
        get
        {
            return quizCardNumber;
        }
        set
        {
            quizCardNumber = value;
        }
    }


    void Start()
    {
        loader.SetActive(true);
        FirestoreClient = new FirestoreDataOperationManager();
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        foreach (KeyValuePair<string, string> button in PlayerInfo.UnitButtonInfo)
        {
            unitLevel = button.Key;
            buttonName = button.Value;
        }
        Logger.LogInfo($"Unit level is {unitLevel} and stage name is {buttonName}", _context);

        baseLevel = level;
        StartCoroutine(LoadQuizJSON());
    }

    IEnumerator LoadQuizJSON()
    {
        List<TriviaQuiz> mergedQuiz = new List<TriviaQuiz>();
        Logger.LogInfo($"Unit level is {unitLevel} and stage name is {buttonName}", _context);
        string triviaUrl0 = $"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/json/{buttonName}/{QuizCardNumber[0]}.json";
        string triviaUrl1 = $"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/json/{buttonName}/{QuizCardNumber[1]}.json";

        yield return LoadSingleQuizFile(triviaUrl0, mergedQuiz);
        yield return LoadSingleQuizFile(triviaUrl1, mergedQuiz);

        Logger.LogInfo($"Total merged quiz count is {mergedQuiz.Count}", _context);

        triviaQuizzes = new TriviaQuizzes { Quizzes = mergedQuiz.ToArray() };
        currentQuizData = (Dictionary<string, object>)((Dictionary<string, object>)trivias[buttonName])[IFirestoreEnums.Flashcard.levels.ToString()];
        FilterQuizQuestions();


    }

    private IEnumerator LoadSingleQuizFile(string url, List<TriviaQuiz> mergedQuiz)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var loadedData = JsonUtility.FromJson<TriviaQuizzes>(request.downloadHandler.text);
                    if (loadedData?.Quizzes != null && loadedData.Quizzes.Length > 0)
                    {
                        mergedQuiz.AddRange(loadedData.Quizzes);
                        Logger.LogInfo($"Loaded {loadedData.Quizzes.Length} quizzes from {url}", _context);
                    }
                    else
                    {
                        Logger.LogWarning($"Loaded file {url} but found no quizzes.", _context);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error parsing JSON from {url}: {ex.Message}", _context);
                }
            }
            else
            {
                Logger.LogError($"Failed to load quiz data from {url}. Error: {request.error}", _context);
            }
        }
    }


}