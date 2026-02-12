using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class FC : TriviaQuizBase
{
    private int[] quizCardNumber = new int[2] { 1, 4 }; 
     string _unitLevel = "";
    string _buttonName = "";
     List<TriviaQuiz> filteredQuizzes = new List<TriviaQuiz>();
     private string _context = "Trivia_Flashcard";
public int[] QuizCardNumber {
        get
        {
            return quizCardNumber;
        }
        set
        {
            quizCardNumber = value;
        }
}
    void Awake()
    {
        /* currentQuizData = new Dictionary<string, object>()
         {
             {"U1_1_1",false },
             {"U1_1_2",true },
             {"U1_1_3",true },
             {"U1_1_4",false },
             {"U1_4_1",false },
             {"U1_4_2",false },
             {"U1_4_3",true },
             {"U1_4_4",false }


         };*/
           foreach (KeyValuePair<string, string> button in PlayerInfo.UnitButtonInfo)
             {
                 _unitLevel = button.Key;
                 _buttonName = button.Value;
             }
       
            
    }

    void Start()
    {
        StartCoroutine(LoadQuizJSON());
    }

    IEnumerator LoadQuizJSON()
    {
        List<TriviaQuiz> mergedQuiz = new List<TriviaQuiz>();
        string triviaUrl0 = $"{Application.streamingAssetsPath}/unit/{_unitLevel}/trivia/json/{_buttonName}/{QuizCardNumber[0]}.json";
        string triviaUrl1 = $"{Application.streamingAssetsPath}/unit/{_unitLevel}/trivia/json/{_buttonName}/{QuizCardNumber[1]}.json";

        yield return LoadSingleQuizFile(triviaUrl0, mergedQuiz);
        yield return LoadSingleQuizFile(triviaUrl1, mergedQuiz);
        /* TriviaQuizzes triviaQuizzes_0 = new TriviaQuizzes();
         TriviaQuizzes triviaQuizzes_1 = new TriviaQuizzes();
         UnityWebRequest request_0 = UnityWebRequest.Get(TriviaUrl_0);
         UnityWebRequest request_1 = UnityWebRequest.Get(TriviaUrl_1);
         request_0.downloadHandler = new DownloadHandlerBuffer();
         request_1.downloadHandler = new DownloadHandlerBuffer();
         yield return request_0.SendWebRequest();
         if (request_0.result == UnityWebRequest.Result.Success)
         {
             triviaQuizzes_0 = JsonUtility.FromJson<TriviaQuizzes>(json: request_0.downloadHandler.text);
            if (triviaQuizzes_0?.Quizzes != null){ mergedQuiz.AddRange(triviaQuizzes_0.Quizzes);}
             Logger.LogInfo($"Trivia data json is loaded having quiz count to {triviaQuizzes_0.Quizzes.Length}", _context);
         }
         else
         {
             Logger.LogError($"Failed to load trivia quiz data from {TriviaUrl_0} with error {request_0.error}", _context);
         }

         yield return request_1.SendWebRequest();
         if (request_1.result == UnityWebRequest.Result.Success)
         {
             triviaQuizzes_1 = JsonUtility.FromJson<TriviaQuizzes>(json: request_1.downloadHandler.text);
             if (triviaQuizzes_1?.Quizzes != null) { mergedQuiz.AddRange(triviaQuizzes_1.Quizzes); }
             Logger.LogInfo($"Trivia data json is loaded having quiz count to {triviaQuizzes_1.Quizzes.Length}", _context);
         }
         else
         {
             Logger.LogError($"Failed to load trivia quiz data from {TriviaUrl_1} with error {request_1.error}", _context);
         }
 */
        Logger.LogInfo($"Total merged quiz count is {mergedQuiz.Count}", _context);
        FilterUnansweredQuizzes(mergedQuiz);


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

    void FilterUnansweredQuizzes(List<TriviaQuiz> mergedQuiz)
    {   if (mergedQuiz == null || mergedQuiz.Count == 0)
        {
            Logger.LogWarning("Merged quiz list is empty — skipping filter.", _context);
            return;
        }

        filteredQuizzes.Clear();
        var quizLookup = mergedQuiz.ToDictionary(q => q.Number, q => q);

       /* foreach (KeyValuePair<string, object> quiz in currentQuizData)
        {
            if (Convert.ToBoolean(quiz.Value))
            {
                filteredQuizzes.Add(_mergedQuiz.Find(q => q.Number.ToString() == quiz.Key));
            }
            else
            {
                Logger.LogInfo($"Quiz number {quiz.Key} already answered", _context);
            }


        }*/

        foreach (var kvp in currentQuizData)
        {
            bool isUnanswered = Convert.ToBoolean(kvp.Value);
            if (isUnanswered && quizLookup.TryGetValue(kvp.Key, out TriviaQuiz foundQuiz))
            {
                filteredQuizzes.Add(foundQuiz);
            }
            else
            {
                Logger.LogInfo($"Quiz {kvp.Key} already answered or not found.", _context);
            }
        }
        triviaQuizzes = new TriviaQuizzes { Quizzes = filteredQuizzes.ToArray() };
        Logger.LogInfo($"Total unAnswered triviaQuizzes count is {triviaQuizzes.Quizzes.Length}", _context);
    }
}
/*Multiple JSON loading...not limited to 2
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class FC : MonoBehaviour
{
    [Serializable]
    public class TriviaQuiz
    {
        public string Id; // Unique identifier like "U1_1_1"
        public string Question;
        public string A;
        public string B;
        public string C;
        public string D;
        public string Answer;
        public int Number;
        public string Image;
    }

    [Serializable]
    public class TriviaQuizzes
    {
        public TriviaQuiz[] Quizzes;
    }

    [Header("Quiz Settings")]
    [SerializeField] private int[] QuizCardNumbers = new int[] { 1, 4 }; // Can have any number of entries
    [SerializeField] private GameObject flashCardScript;

    private string unitLevel = "";
    private string buttonName = "";
    private TriviaQuizzes triviaQuizzes;
    private List<TriviaQuiz> filteredQuizzes = new List<TriviaQuiz>();
    private Dictionary<string, bool> currentQuizData;
    private const string _context = "Trivia_Flashcard";

    void Awake()
    {
        // Initialize example quiz data (true = unanswered)
        currentQuizData = new Dictionary<string, bool>()
        {
            {"U1_1_1", false},
            {"U1_1_2", true},
            {"U1_1_3", true},
            {"U1_1_4", false},
            {"U1_4_1", false},
            {"U1_4_2", false},
            {"U1_4_3", true},
            {"U1_4_4", false}
        };

        // Get first valid pair from PlayerInfo.UnitButtonInfo
        if (PlayerInfo.UnitButtonInfo != null && PlayerInfo.UnitButtonInfo.Count > 0)
        {
            var first = PlayerInfo.UnitButtonInfo.First();
            unitLevel = first.Key;
            buttonName = first.Value;
        }
        else
        {
            Logger.LogError("PlayerInfo.UnitButtonInfo is empty or null", _context);
        }
    }

    void Start()
    {
        StartCoroutine(LoadAllQuizJSONs());
    }

    private IEnumerator LoadAllQuizJSONs()
    {
        if (QuizCardNumbers == null || QuizCardNumbers.Length == 0)
        {
            Logger.LogError("QuizCardNumbers array is empty. No JSON files to load.", _context);
            yield break;
        }

        List<TriviaQuiz> mergedQuiz = new List<TriviaQuiz>();
        List<UnityWebRequest> requests = new List<UnityWebRequest>();

        // Prepare all requests
        foreach (int quizNum in QuizCardNumbers)
        {
            string url = $"{Application.streamingAssetsPath}/unit/{unitLevel}/trivia/json/{buttonName}/{quizNum}.json";
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            requests.Add(request);
        }

        // Send all requests in sequence (or could use parallel with UnityWebRequestMultimedia)
        foreach (var req in requests)
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var loadedData = JsonUtility.FromJson<TriviaQuizzes>(req.downloadHandler.text);
                    if (loadedData?.Quizzes != null && loadedData.Quizzes.Length > 0)
                    {
                        mergedQuiz.AddRange(loadedData.Quizzes);
                        Logger.LogInfo($"Loaded {loadedData.Quizzes.Length} quizzes from {req.url}", _context);
                    }
                    else
                    {
                        Logger.LogWarning($"Loaded file {req.url} but found no quizzes.", _context);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error parsing JSON from {req.url}: {ex.Message}", _context);
                }
            }
            else
            {
                Logger.LogError($"Failed to load quiz data from {req.url}. Error: {req.error}", _context);
            }
        }

        Logger.LogInfo($"✅ Total merged quiz count: {mergedQuiz.Count}", _context);

        // Now filter unanswered quizzes
        FilterUnansweredQuizzes(mergedQuiz);
    }

    private void FilterUnansweredQuizzes(List<TriviaQuiz> mergedQuiz)
    {
        if (mergedQuiz == null || mergedQuiz.Count == 0)
        {
            Logger.LogWarning("Merged quiz list is empty — skipping filter.", _context);
            return;
        }

        filteredQuizzes.Clear();

        // Build a dictionary for fast lookup
        var quizLookup = mergedQuiz
            .Where(q => !string.IsNullOrEmpty(q.Id))
            .GroupBy(q => q.Id)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var kvp in currentQuizData)
        {
            bool isUnanswered = kvp.Value;
            if (isUnanswered && quizLookup.TryGetValue(kvp.Key, out TriviaQuiz foundQuiz))
            {
                filteredQuizzes.Add(foundQuiz);
            }
            else
            {
                Logger.LogInfo($"Quiz {kvp.Key} already answered or not found.", _context);
            }
        }

        triviaQuizzes = new TriviaQuizzes { Quizzes = filteredQuizzes.ToArray() };

        Logger.LogInfo($"✅ Total unanswered trivia quizzes: {triviaQuizzes.Quizzes.Length}", _context);
    }
}
*/