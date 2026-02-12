using UnityEngine;
using System.Collections;

/// <summary>
/// Example script showing how to integrate EducationalPerformanceTracker with all mini-games
/// This demonstrates the comprehensive data collection for performance metrics
/// </summary>
public class EducationalPerformanceTracker_UsageExample : MonoBehaviour
{
    [Header("Activity Settings")]
    public EducationalPerformanceTracker_Enhanced.ActivityType currentActivity = EducationalPerformanceTracker_Enhanced.ActivityType.CoinGame;
    public EducationalPerformanceTracker_Enhanced.DifficultyLevel difficulty = EducationalPerformanceTracker_Enhanced.DifficultyLevel.Medium;
    public int totalItems = 10;
    public float timeLimit = 300f;

    [Header("Test Data")]
    public bool generateTestData = false;
    public int testSessions = 5;

    void Start()
    {
        // Initialize the enhanced tracker
        if (EducationalPerformanceTracker_Enhanced.Instance == null)
        {
            GameObject trackerGO = new GameObject("EducationalPerformanceTracker_Enhanced");
            trackerGO.AddComponent<EducationalPerformanceTracker_Enhanced>();
        }

        if (generateTestData)
        {
            StartCoroutine(GenerateTestData());
        }
    }

    /// <summary>
    /// Example: Initialize a flashcard session
    /// </summary>
    public void StartFlashcardSession()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        tracker.InitializeLearningSession(EducationalPerformanceTracker_Enhanced.ActivityType.Flashcards, 20, 600f, EducationalPerformanceTracker_Enhanced.DifficultyLevel.Easy);
        
        Debug.Log("Flashcard session started - tracking vocabulary learning");
    }

    /// <summary>
    /// Example: Record a flashcard attempt
    /// </summary>
    public void RecordFlashcardAttempt(string word, bool isCorrect, float responseTime)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        string wrongAnswerType = isCorrect ? "" : "vocabulary_mistake";
        
        tracker.RecordItemAttempt(word, isCorrect, responseTime, wrongAnswerType, false, false, false);
        
        Debug.Log($"Flashcard recorded: {word} - Correct: {isCorrect}, Time: {responseTime:F2}s");
    }

    /// <summary>
    /// Example: Initialize a trivia quiz session
    /// </summary>
    public void StartTriviaQuizSession()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        tracker.InitializeLearningSession(EducationalPerformanceTracker_Enhanced.ActivityType.TriviaQuiz, 15, 300f, EducationalPerformanceTracker_Enhanced.DifficultyLevel.Medium);
        
        Debug.Log("Trivia quiz session started - tracking knowledge assessment");
    }

    /// <summary>
    /// Example: Record a trivia question attempt
    /// </summary>
    public void RecordTriviaAttempt(string question, bool isCorrect, float responseTime, string wrongAnswerType = "")
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        bool isTimePressure = responseTime < 5f; // Less than 5 seconds is time pressure
        
        tracker.RecordItemAttempt(question, isCorrect, responseTime, wrongAnswerType, isTimePressure, false, false);
        
        Debug.Log($"Trivia recorded: {question} - Correct: {isCorrect}, Time: {responseTime:F2}s, Pressure: {isTimePressure}");
    }

    /// <summary>
    /// Example: Initialize a video comprehension session
    /// </summary>
    public void StartVideoSession()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        tracker.InitializeLearningSession(EducationalPerformanceTracker_Enhanced.ActivityType.Video, 5, 1200f, EducationalPerformanceTracker_Enhanced.DifficultyLevel.Easy);
        
        Debug.Log("Video session started - tracking comprehension");
    }

    /// <summary>
    /// Example: Record video comprehension attempt
    /// </summary>
    public void RecordVideoComprehension(string concept, bool isCorrect, float responseTime)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        string wrongAnswerType = isCorrect ? "" : "comprehension_error";
        
        tracker.RecordItemAttempt(concept, isCorrect, responseTime, wrongAnswerType, false, false, false);
        
        Debug.Log($"Video comprehension recorded: {concept} - Correct: {isCorrect}, Time: {responseTime:F2}s");
    }

    /// <summary>
    /// Example: Initialize a word vocabulary session
    /// </summary>
    public void StartVocabularySession()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        tracker.InitializeLearningSession(EducationalPerformanceTracker_Enhanced.ActivityType.WordVocabulary, 25, 900f, EducationalPerformanceTracker_Enhanced.DifficultyLevel.Medium);
        
        Debug.Log("Vocabulary session started - tracking word learning");
    }

    /// <summary>
    /// Example: Record vocabulary attempt
    /// </summary>
    public void RecordVocabularyAttempt(string word, bool isCorrect, float responseTime, bool usedHint = false)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        string wrongAnswerType = isCorrect ? "" : "spelling_error";
        
        tracker.RecordItemAttempt(word, isCorrect, responseTime, wrongAnswerType, false, usedHint, false);
        
        Debug.Log($"Vocabulary recorded: {word} - Correct: {isCorrect}, Time: {responseTime:F2}s, Hint: {usedHint}");
    }

    /// <summary>
    /// Example: Initialize a story book session
    /// </summary>
    public void StartStoryBookSession()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        tracker.InitializeLearningSession(EducationalPerformanceTracker_Enhanced.ActivityType.StoryBook, 8, 1800f, EducationalPerformanceTracker_Enhanced.DifficultyLevel.Easy);
        
        Debug.Log("Story book session started - tracking reading comprehension");
    }

    /// <summary>
    /// Example: Record story comprehension attempt
    /// </summary>
    public void RecordStoryComprehension(string concept, bool isCorrect, float responseTime)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        string wrongAnswerType = isCorrect ? "" : "reading_comprehension_error";
        
        tracker.RecordItemAttempt(concept, isCorrect, responseTime, wrongAnswerType, false, false, false);
        
        Debug.Log($"Story comprehension recorded: {concept} - Correct: {isCorrect}, Time: {responseTime:F2}s");
    }

    /// <summary>
    /// Example: Initialize a calculator session
    /// </summary>
    public void StartCalculatorSession()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        tracker.InitializeLearningSession(EducationalPerformanceTracker_Enhanced.ActivityType.Calculator, 12, 600f, EducationalPerformanceTracker_Enhanced.DifficultyLevel.Hard);
        
        Debug.Log("Calculator session started - tracking math skills");
    }

    /// <summary>
    /// Example: Record calculator attempt
    /// </summary>
    public void RecordCalculatorAttempt(string operation, bool isCorrect, float responseTime)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        string wrongAnswerType = isCorrect ? "" : "calculation_error";
        bool isTimePressure = responseTime < 3f; // Less than 3 seconds for math
        
        tracker.RecordItemAttempt(operation, isCorrect, responseTime, wrongAnswerType, isTimePressure, false, false);
        
        Debug.Log($"Calculator recorded: {operation} - Correct: {isCorrect}, Time: {responseTime:F2}s, Pressure: {isTimePressure}");
    }

    /// <summary>
    /// Example: Initialize an audio songs session
    /// </summary>
    public void StartAudioSongsSession()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        tracker.InitializeLearningSession(EducationalPerformanceTracker_Enhanced.ActivityType.AudioSongs, 6, 900f, EducationalPerformanceTracker_Enhanced.DifficultyLevel.Easy);
        
        Debug.Log("Audio songs session started - tracking listening comprehension");
    }

    /// <summary>
    /// Example: Record audio comprehension attempt
    /// </summary>
    public void RecordAudioComprehension(string concept, bool isCorrect, float responseTime)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        string wrongAnswerType = isCorrect ? "" : "listening_error";
        
        tracker.RecordItemAttempt(concept, isCorrect, responseTime, wrongAnswerType, false, false, false);
        
        Debug.Log($"Audio comprehension recorded: {concept} - Correct: {isCorrect}, Time: {responseTime:F2}s");
    }

    /// <summary>
    /// Example: Initialize a mega quiz session
    /// </summary>
    public void StartMegaQuizSession()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        tracker.InitializeLearningSession(EducationalPerformanceTracker_Enhanced.ActivityType.MegaQuiz, 30, 1800f, EducationalPerformanceTracker_Enhanced.DifficultyLevel.Expert);
        
        Debug.Log("Mega quiz session started - tracking comprehensive knowledge");
    }

    /// <summary>
    /// Example: Record mega quiz attempt
    /// </summary>
    public void RecordMegaQuizAttempt(string question, bool isCorrect, float responseTime, string wrongAnswerType = "")
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        bool isTimePressure = responseTime < 10f; // Less than 10 seconds for complex questions
        
        tracker.RecordItemAttempt(question, isCorrect, responseTime, wrongAnswerType, isTimePressure, false, false);
        
        Debug.Log($"Mega quiz recorded: {question} - Correct: {isCorrect}, Time: {responseTime:F2}s, Pressure: {isTimePressure}");
    }

    /// <summary>
    /// Complete any session with final score
    /// </summary>
    public void CompleteSession(int finalScore)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        tracker.CompleteLearningSession(finalScore);
        
        Debug.Log($"Session completed with score: {finalScore}");
        
        // Refresh dashboard to show updated metrics
        tracker.RefreshDashboardUI();
    }

    /// <summary>
    /// Get performance data for analysis
    /// </summary>
    public void AnalyzePerformance()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        
        // Get activity-specific performance
        var activityPerformance = tracker.GetActivityPerformance();
        Debug.Log("Activity Performance:");
        foreach (var kvp in activityPerformance)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value:P0} accuracy");
        }
        
        // Get weekly progress
        var weeklyProgress = tracker.GetWeeklyProgress();
        Debug.Log("Weekly Progress:");
        foreach (var kvp in weeklyProgress)
        {
            Debug.Log($"Week {kvp.Key}: {kvp.Value:P0} accuracy");
        }
        
        // Get most difficult concepts
        var difficultConcepts = tracker.GetMostDifficultConcepts(5);
        Debug.Log("Most Difficult Concepts:");
        foreach (var concept in difficultConcepts)
        {
            Debug.Log($"- {concept}");
        }
        
        // Get wrong answer patterns
        var wrongPatterns = tracker.GetWrongAnswerPatterns();
        Debug.Log("Wrong Answer Patterns:");
        foreach (var kvp in wrongPatterns)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} times");
        }
    }

    /// <summary>
    /// Generate test data for demonstration
    /// </summary>
    private IEnumerator GenerateTestData()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        
        // Generate test sessions for different activities
        EducationalPerformanceTracker_Enhanced.ActivityType[] activities = { 
            EducationalPerformanceTracker_Enhanced.ActivityType.Flashcards, 
            EducationalPerformanceTracker_Enhanced.ActivityType.TriviaQuiz, 
            EducationalPerformanceTracker_Enhanced.ActivityType.Video, 
            EducationalPerformanceTracker_Enhanced.ActivityType.WordVocabulary 
        };
        string[] concepts = { "addition", "subtraction", "multiplication", "division", "vocabulary", "spelling", "comprehension" };
        
        for (int session = 0; session < testSessions; session++)
        {
            EducationalPerformanceTracker_Enhanced.ActivityType activity = activities[session % activities.Length];
            int items = Random.Range(5, 15);
            float timeLimit = Random.Range(300f, 900f);
            EducationalPerformanceTracker_Enhanced.DifficultyLevel difficulty = (EducationalPerformanceTracker_Enhanced.DifficultyLevel)(session % 4);
            
            // Initialize session
            tracker.InitializeLearningSession(activity, items, timeLimit, difficulty);
            
            // Generate attempts
            for (int item = 0; item < items; item++)
            {
                string concept = concepts[Random.Range(0, concepts.Length)];
                bool isCorrect = Random.Range(0f, 1f) > 0.3f; // 70% correct rate
                float responseTime = Random.Range(1f, 8f);
                string wrongAnswerType = isCorrect ? "" : "test_error";
                bool isTimePressure = responseTime < 3f;
                
                tracker.RecordItemAttempt(concept, isCorrect, responseTime, wrongAnswerType, isTimePressure, false, false);
                
                yield return new WaitForSeconds(0.1f); // Small delay for realistic timing
            }
            
            // Complete session
            int finalScore = Random.Range(60, 100);
            tracker.CompleteLearningSession(finalScore);
            
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log($"Generated {testSessions} test sessions with comprehensive data");
        
        // Analyze the generated data
        AnalyzePerformance();
    }

    /// <summary>
    /// Example: Track wrong placements for drag-drop activities
    /// </summary>
    public void RecordWrongPlacement(string concept, string wrongLocation)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        string wrongAnswerType = $"wrong_placement_{wrongLocation}";
        
        tracker.RecordItemAttempt(concept, false, 0f, wrongAnswerType, false, false, false);
        
        Debug.Log($"Wrong placement recorded: {concept} placed in {wrongLocation}");
    }

    /// <summary>
    /// Example: Track skipped questions
    /// </summary>
    public void RecordSkippedQuestion(string concept)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        
        tracker.RecordItemAttempt(concept, false, 0f, "skipped", false, false, true);
        
        Debug.Log($"Skipped question recorded: {concept}");
    }

    /// <summary>
    /// Example: Track hint usage
    /// </summary>
    public void RecordHintUsage(string concept, bool isCorrect, float responseTime)
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        string wrongAnswerType = isCorrect ? "" : "hint_used_incorrect";
        
        tracker.RecordItemAttempt(concept, isCorrect, responseTime, wrongAnswerType, false, true, false);
        
        Debug.Log($"Hint usage recorded: {concept} - Correct: {isCorrect}, Time: {responseTime:F2}s");
    }

    /// <summary>
    /// Get comprehensive performance report
    /// </summary>
    public void GetPerformanceReport()
    {
        var tracker = EducationalPerformanceTracker_Enhanced.Instance;
        var profile = tracker.GetStudentProfile();
        
        Debug.Log("=== PERFORMANCE REPORT ===");
        Debug.Log($"Total Sessions: {profile.totalSessions}");
        Debug.Log($"Total Play Time: {profile.totalPlayTime:F0} seconds");
        Debug.Log($"Total Items Completed: {profile.totalItemsCompleted}");
        Debug.Log($"Overall Accuracy: {profile.overallAccuracy:P0}");
        Debug.Log($"Average Session Score: {profile.averageSessionScore:F0}");
        
        Debug.Log("\n=== STRENGTHS ===");
        foreach (var strength in profile.strengths)
        {
            Debug.Log($"- {strength}");
        }
        
        Debug.Log("\n=== AREAS FOR IMPROVEMENT ===");
        foreach (var area in profile.areasForImprovement)
        {
            Debug.Log($"- {area}");
        }
        
        Debug.Log("\n=== ACTIVITY PERFORMANCE ===");
        foreach (var kvp in profile.averageAccuracyByActivity)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value:P0} accuracy");
        }
        
        Debug.Log("=== END REPORT ===");
    }
}

// Note: ActivityType and DifficultyLevel enums are defined in EducationalPerformanceTracker_Enhanced class
// Use EducationalPerformanceTracker_Enhanced.ActivityType and EducationalPerformanceTracker_Enhanced.DifficultyLevel 