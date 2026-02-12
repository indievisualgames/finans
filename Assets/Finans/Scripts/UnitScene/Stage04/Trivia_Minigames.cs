using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facebook.MiniJSON;
using UnityEngine;
using UnityEngine.Networking;
using static IFirestoreEnums;

[RequireComponent(typeof(MiniGames))]
[RequireComponent(typeof(Trivia_Minigames))]

public class Trivia_Minigames : TriviaQuizBase
{
    // Minigames _minigames;
    List<string> quizCount = new List<string>();
    public Dictionary<string, object> trivias;
    public int level;
    private string context = "Trivia_Minigames";

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
                //  quizCount.Add(i);
                quizCount.Add(triviaQuizzes.Quizzes[i].Number);
            }

            Logger.LogInfo($"Trivia data json is loaded having quiz count to {quizCount.Count}", context);
            currentQuizData = (Dictionary<string, object>)((Dictionary<string, object>)trivias[buttonName])[IFirestoreEnums.MiniGames.levels.ToString()];

            // CheckQuizDataAsync();
            /* if (trivias != null && trivias.ContainsKey(buttonName))
             {
                 var btn = (Dictionary<string, object>)trivias[buttonName];
                 if (btn.ContainsKey(IFirestoreEnums.Vocab.levels.ToString()))
                 {
                     currentQuizData = (Dictionary<string, object>)btn[IFirestoreEnums.Vocab.levels.ToString()];
                 }
             }
             if (currentQuizData == null || currentQuizData.Count == 0)
             {
                 // Build an in-memory map enabling all quiz numbers for this level
                 var map = new Dictionary<string, object>();
                 foreach (var q in quizCount)
                 {
                     map[q] = true;
                 }
                 currentQuizData = new Dictionary<string, object>()
           {
               { level.ToString(), map }
           };
                 Logger.LogWarning("Fallback quiz map constructed from JSON as trivias data missing", context);
             }*/
            FilterQuizQuestions();
        }
    }
}

/*async void CheckQuizDataAsync()
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
                  {_minigames.level.ToString(), FirestoreData.QuizQuestions(quizCount)}
                  };
              Dictionary<string, object> quizData = new Dictionary<string, object>(){
              {
                  FSMapField.unit_stage_data.ToString(),new Dictionary<string, object>(){
                   {$"unit{unitLevel}", new Dictionary<string, object>(){
                      {
                          FSMapField.trivia.ToString(), new Dictionary <string, object>(){
                             {buttonName, new Dictionary<string, object>(){
                               {_minigames.level.ToString(), FirestoreData.QuizQuestions(quizCount)
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

              await _minigames.FirestoreClient.FirestoreDataSave(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID, quizData);
            Logger.LogInfo($"No minigames trivia quiz data found for level {level}, creating new data", context);

            await CreateQuizDataAsync();

        }
    }
    else
    {
        Logger.LogInfo($"No minigames trivia field found..Creating field with level {level} data", context);
        await CreateQuizDataAsync();
    }
    FilterQuizQuestions();
}*/

/*  private async Task CreateQuizDataAsync()
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

      await FirestoreClient.FirestoreDataSave(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID, quizData);


  }

}*/
