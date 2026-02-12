using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;
using UnityEngine;
using UnityEngine.Networking;
using static IFirestoreEnums;

[RequireComponent(typeof(Video))]
public class Trivia_Video : TriviaQuizBase
{
    //  Video _video;
    List<string> quizCount = new List<string>();
    public Dictionary<string, object> trivias;
    public int level;
    private string context = "Trivia_Video";

    void Start()
    {
        loader.SetActive(true);
        FirestoreClient = new FirestoreDataOperationManager();
        // m_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
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

        Logger.LogInfo($"Tvivia quiz data path is {TriviaUrl}", context);
        UnityWebRequest request = UnityWebRequest.Get(TriviaUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            triviaQuizzes = JsonUtility.FromJson<TriviaQuizzes>(json: request.downloadHandler.text);
            for (int i = 0; i < triviaQuizzes.Quizzes.Length; i++)
            {
                // quizCount.Add(i);
                quizCount.Add(triviaQuizzes.Quizzes[i].Number);
            }

            Logger.LogInfo($"Trivia data json is loaded having quiz count to {quizCount.Count}", context);
            currentQuizData = (Dictionary<string, object>)((Dictionary<string, object>)trivias[buttonName])[IFirestoreEnums.Videos.levels.ToString()];
            FilterQuizQuestions();
        }
        // CheckQuizDataAsync();

    }

    /* async void CheckQuizDataAsync()
     {
         if (trivias.ContainsKey(buttonName))
         {
             if (((Dictionary<string, object>)trivias[buttonName]).ContainsKey(level.ToString()))
             {
                 currentQuizData = (Dictionary<string, object>)trivias[buttonName];
                 Logger.LogInfo($"Found vocabs trivia quiz data for level {level} {Json.Serialize(currentQuizData)}", context);
             }
             else
             {
                 currentQuizData = new Dictionary<string, object>(){
                     {level.ToString(), FirestoreData.QuizQuestions(quizCount)}
                     };
                 Dictionary<string, object> quizData = new Dictionary<string, object>(){
                 {
                     FSMapField.unit_stage_data.ToString(),new Dictionary<string, object>(){
                      {$"unit{unitLevel}", new Dictionary<string, object>(){
                         {
                             FSMapField.trivia.ToString(), new Dictionary <string, object>(){
                                {buttonName, new Dictionary<string, object>(){
                                  {level.ToString(), FirestoreData.QuizQuestions(quizCount)
                                             }
                                         }

                                      }
                                 }
                             }
                         }
                     }
                 }
             }
         };

         if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
         {
             await FirestoreClient.FirestoreDataSave(
                 FSCollection.parent.ToString(),
                 PlayerInfo.AuthenticatedID,
                 FSCollection.children.ToString(),
                 PlayerInfo.AuthenticatedChildID,
                 quizData);
         }
         else
         {
             popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
             Logger.LogError($"No connection found while creating video trivia quiz data", context);
         }

         }
         }
         FilterQuizQuestions();
     }*/

}
