using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System;

/// <summary>
/// Enhanced Educational Performance Tracker for all mini-games and activities
/// Supports: Flashcards, Trivia Quiz, Video, Word Vocabulary, Story Book, Calculator, Audio Songs, Mega Quiz
/// </summary>
public class EducationalPerformanceTracker_Enhanced : MonoBehaviour
{
    [System.Serializable]
    public enum ActivityType
    {
        CoinGame,
        Flashcards,
        TriviaQuiz,
        Video,
        WordVocabulary,
        StoryBook,
        Calculator,
        AudioSongs,
        MegaQuiz,
        General
    }

    [System.Serializable]
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Expert
    }

    [System.Serializable]
    public class LearningSession
    {
        public DateTime sessionDate;
        public ActivityType activityType;
        public DifficultyLevel difficulty;
        public int totalItems;
        public int correctItems;
        public int mistakes;
        public float timeTaken;
        public float timeGiven;
        public int score;
        public List<string> conceptsCovered = new List<string>();
        public List<float> responseTimes = new List<float>();
        public List<bool> correctAnswers = new List<bool>();
        public float averageResponseTime;
        public float accuracyPercentage;
        public float efficiencyScore;

        // Enhanced metrics for detailed analysis
        public int wrongAnswers;
        public int timeBasedCorrectAnswers;
        public int timeBasedWrongAnswers;
        public float averageTimeForCorrect;
        public float averageTimeForWrong;
        public float fastestCorrectAnswer;
        public float slowestWrongAnswer;
        public int consecutiveCorrectStreak;
        public int consecutiveWrongStreak;
        public float timePressureAccuracy;
        public float relaxedAccuracy;
        public Dictionary<string, int> wrongAnswersByConcept = new Dictionary<string, int>();
        public Dictionary<string, float> averageTimeByConcept = new Dictionary<string, float>();
        public List<float> timeBasedScores = new List<float>();

        // Time extension usage within the session
        public float originalTimeGiven; // Time limit before any extensions
        public int adTimeExtensionsCount;
        public float adExtraTimeSeconds;
        public int purchaseTimeExtensionsCount;
        public float purchaseExtraTimeSeconds;
        public float totalExtraTimeSeconds;

        // Activity-specific metrics
        public int wrongPlacements; // For drag-drop activities
        public int skippedQuestions;
        public int hintsUsed;
        public float engagementTime; // Time spent actually engaged
        public List<string> attemptedConcepts = new List<string>();
        public Dictionary<string, int> conceptAttempts = new Dictionary<string, int>();
        public Dictionary<string, int> conceptCorrect = new Dictionary<string, int>();
    }

    [System.Serializable]
    public class ConceptMastery
    {
        public string conceptName;
        public ActivityType primaryActivity;
        public int totalAttempts;
        public int correctAttempts;
        public float averageResponseTime;
        public float masteryLevel; // 0-1 scale
        public DateTime lastPracticed;
        public int consecutiveCorrect;
        public int consecutiveIncorrect;
        public List<float> recentScores = new List<float>();

        // Enhanced tracking
        public int wrongAttempts;
        public float averageTimeForCorrect;
        public float averageTimeForWrong;
        public float fastestCorrectAnswer;
        public float slowestWrongAnswer;
        public int timeBasedCorrectCount;
        public int timeBasedWrongCount;
        public float timePressureAccuracy;
        public float relaxedAccuracy;
        public List<float> responseTimeHistory = new List<float>();
        public List<bool> answerHistory = new List<bool>();
        public Dictionary<string, int> wrongAnswerTypes = new Dictionary<string, int>();

        // Activity-specific mastery
        public Dictionary<ActivityType, int> attemptsByActivity = new Dictionary<ActivityType, int>();
        public Dictionary<ActivityType, int> correctByActivity = new Dictionary<ActivityType, int>();
        public Dictionary<ActivityType, float> averageTimeByActivity = new Dictionary<ActivityType, float>();
    }

    [System.Serializable]
    public class StudentProfile
    {
        public string studentId = "default_student";
        public string studentName = "Student";
        public int age = 8;
        public string grade = "3rd Grade";
        public DateTime joinDate = DateTime.Now;
        public int totalSessions;
        public float totalPlayTime;
        public int totalItemsCompleted;
        public float overallAccuracy;
        public float averageSessionScore;
        public List<string> strengths = new List<string>();
        public List<string> areasForImprovement = new List<string>();
        public Dictionary<string, ConceptMastery> conceptMastery = new Dictionary<string, ConceptMastery>();
        public List<LearningSession> learningHistory = new List<LearningSession>();

        // Activity-specific tracking
        public Dictionary<ActivityType, int> sessionsByActivity = new Dictionary<ActivityType, int>();
        public Dictionary<ActivityType, float> totalTimeByActivity = new Dictionary<ActivityType, float>();
        public Dictionary<ActivityType, float> averageScoreByActivity = new Dictionary<ActivityType, float>();
        public Dictionary<ActivityType, float> averageAccuracyByActivity = new Dictionary<ActivityType, float>();

        // Weekly progress tracking
        public Dictionary<string, float> weeklyProgress = new Dictionary<string, float>(); // Week as key
        public Dictionary<string, List<string>> weeklyStrengths = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> weeklyWeaknesses = new Dictionary<string, List<string>>();

        // Aggregated time extension analytics
        public int totalAdExtensions;
        public float totalAdExtraTime;
        public int totalPurchaseExtensions;
        public float totalPurchaseExtraTime;
        public int sessionsWithTimeExtensions;
    }

    public static EducationalPerformanceTracker_Enhanced Instance { get; private set; }

    [Header("Educational Settings")]
    public bool trackResponseTimes = true;
    public bool trackConceptMastery = true;
    public bool enableAdaptiveDifficulty = true;
    public bool trackActivitySpecificMetrics = true;
    public bool enableWeeklyProgressTracking = true;

    [Header("Learning Metrics")]
    [Range(0.1f, 2f)]
    public float timePressureThreshold = 5.0f;
    [Range(0.1f, 2f)]
    public float relaxedThreshold = 10.0f;

    [Header("Dashboard UI References")]
    public TextMeshProUGUI conceptMasteryText;
    public TextMeshProUGUI learningCurveText;
    public TextMeshProUGUI strengthsText;
    public TextMeshProUGUI improvementAreasText;
    public TextMeshProUGUI sessionHistoryText;
    public TextMeshProUGUI performanceTrendText;
    public TextMeshProUGUI knowledgeGapsText;
    public TextMeshProUGUI cognitiveScoreText;
    public TextMeshProUGUI efficiencyScoreText;
    public TextMeshProUGUI overallProgressText;

    [Header("Enhanced Performance Metrics UI")]
    public TextMeshProUGUI timeBasedAccuracyText;
    public TextMeshProUGUI responseTimeAnalysisText;
    public TextMeshProUGUI mistakePatternsText;
    public TextMeshProUGUI timePressureText;
    public TextMeshProUGUI learningSpeedText;
    public TextMeshProUGUI conceptDifficultyText;
    public TextMeshProUGUI improvementRateText;
    public TextMeshProUGUI sessionComparisonText;
    public TextMeshProUGUI adaptiveRecommendationsText;
    public TextMeshProUGUI wrongAnswersText;
    public TextMeshProUGUI mostDifficultConceptsText;

    [Header("Parent Dashboard UI")]
    public TextMeshProUGUI weeklyProgressText;
    public TextMeshProUGUI monthlyTrendText;
    public TextMeshProUGUI learningRecommendationsText;
    public TextMeshProUGUI timeSpentText;
    public TextMeshProUGUI improvementSuggestionsText;
    public TextMeshProUGUI strengthsParentText;

    [Header("Events")]
    public UnityEvent onLearningMilestone;
    public UnityEvent onPerformanceImprovement;
    public UnityEvent onKnowledgeGapIdentified;

    // Private variables
    private StudentProfile currentStudent;
    private LearningSession currentSession;
    private List<float> responseTimes = new List<float>();
    private List<bool> correctAnswers = new List<bool>();
    private Dictionary<string, float> conceptResponseTimes = new Dictionary<string, float>();
    private float sessionStartTime;
    private int sessionMistakes = 0;
    private int sessionCorrect = 0;
    private int sessionWrongAnswers = 0;
    private int timeBasedCorrectAnswers = 0;
    private int timeBasedWrongAnswers = 0;
    private float sessionStartTimeForItem;
    private List<float> correctResponseTimes = new List<float>();
    private List<float> wrongResponseTimes = new List<float>();
    private Dictionary<string, List<float>> conceptCorrectTimes = new Dictionary<string, List<float>>();
    private Dictionary<string, List<float>> conceptWrongTimes = new Dictionary<string, List<float>>();
    private Dictionary<string, int> wrongAnswerTypes = new Dictionary<string, int>();
    private int consecutiveCorrectStreak = 0;
    private int consecutiveWrongStreak = 0;
    private const float TIME_PRESSURE_THRESHOLD = 5.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        EnsureInitialized();
        LoadStudentProfile();
    }

    public void EnsureInitialized()
    {
        if (currentStudent == null)
        {
            currentStudent = new StudentProfile();
        }

        if (currentSession == null)
        {
            currentSession = null;
        }
    }

    private void LoadStudentProfile()
    {
        currentStudent = new StudentProfile();

        // Load saved data if exists
        string savedData = PlayerPrefs.GetString("StudentProfile_Enhanced", "");
        if (!string.IsNullOrEmpty(savedData))
        {
            try
            {
                currentStudent = JsonUtility.FromJson<StudentProfile>(savedData);

                // Validate loaded data
                if (currentStudent == null)
                {
                    Debug.LogWarning("EducationalPerformanceTracker: Loaded student profile is null, creating new profile");
                    currentStudent = new StudentProfile();
                }
                else
                {
                    Debug.Log($"EducationalPerformanceTracker: Student profile loaded successfully ({savedData.Length} characters)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"EducationalPerformanceTracker: Failed to load student profile: {e.Message}");

                // Try to load backup data
                try
                {
                    string backupData = PlayerPrefs.GetString("StudentProfile_Enhanced_Backup", "");
                    if (!string.IsNullOrEmpty(backupData))
                    {
                        currentStudent = JsonUtility.FromJson<StudentProfile>(backupData);
                        Debug.Log("EducationalPerformanceTracker: Loaded backup student profile");
                    }
                    else
                    {
                        currentStudent = new StudentProfile();
                        Debug.Log("EducationalPerformanceTracker: No backup data available, creating new profile");
                    }
                }
                catch (System.Exception backupException)
                {
                    Debug.LogError($"EducationalPerformanceTracker: Failed to load backup data: {backupException.Message}");
                    currentStudent = new StudentProfile();
                }
            }
        }
        else
        {
            Debug.Log("EducationalPerformanceTracker: No saved data found, creating new student profile");
        }

        // Initialize collections if null
        if (currentStudent.conceptMastery == null)
            currentStudent.conceptMastery = new Dictionary<string, ConceptMastery>();
        if (currentStudent.learningHistory == null)
            currentStudent.learningHistory = new List<LearningSession>();
        if (currentStudent.strengths == null)
            currentStudent.strengths = new List<string>();
        if (currentStudent.areasForImprovement == null)
            currentStudent.areasForImprovement = new List<string>();
        if (currentStudent.sessionsByActivity == null)
            currentStudent.sessionsByActivity = new Dictionary<ActivityType, int>();
        if (currentStudent.totalTimeByActivity == null)
            currentStudent.totalTimeByActivity = new Dictionary<ActivityType, float>();
        if (currentStudent.averageScoreByActivity == null)
            currentStudent.averageScoreByActivity = new Dictionary<ActivityType, float>();
        if (currentStudent.averageAccuracyByActivity == null)
            currentStudent.averageAccuracyByActivity = new Dictionary<ActivityType, float>();
        if (currentStudent.weeklyProgress == null)
            currentStudent.weeklyProgress = new Dictionary<string, float>();
        if (currentStudent.weeklyStrengths == null)
            currentStudent.weeklyStrengths = new Dictionary<string, List<string>>();
        if (currentStudent.weeklyWeaknesses == null)
            currentStudent.weeklyWeaknesses = new Dictionary<string, List<string>>();
    }

    /// <summary>
    /// Initialize a learning session for any activity type
    /// </summary>
    public void InitializeLearningSession(ActivityType activityType, int totalItems, float timeLimit, DifficultyLevel difficulty)
    {
        // Validate input parameters
        if (totalItems <= 0)
        {
            Debug.LogError($"EducationalPerformanceTracker: Invalid totalItems ({totalItems}). Must be greater than 0.");
            totalItems = 1; // Set minimum valid value
        }

        if (timeLimit < 0)
        {
            Debug.LogWarning($"EducationalPerformanceTracker: Negative time limit ({timeLimit}) provided. Setting to 0 (no time limit).");
            timeLimit = 0;
        }

        // Check if there's already an active session
        if (currentSession != null)
        {
            Debug.LogWarning("EducationalPerformanceTracker: Initializing new session while previous session was active. Completing previous session first.");
            CompleteLearningSession(0); // Complete with 0 score
        }

        // Reset session counters
        sessionStartTime = Time.time;
        sessionMistakes = 0;
        sessionCorrect = 0;
        sessionWrongAnswers = 0;
        timeBasedCorrectAnswers = 0;
        timeBasedWrongAnswers = 0;
        consecutiveCorrectStreak = 0;
        consecutiveWrongStreak = 0;

        // Clear previous session data
        responseTimes.Clear();
        correctAnswers.Clear();
        conceptResponseTimes.Clear();
        correctResponseTimes.Clear();
        wrongResponseTimes.Clear();
        conceptCorrectTimes.Clear();
        conceptWrongTimes.Clear();
        wrongAnswerTypes.Clear();

        // Initialize current session
        currentSession = new LearningSession
        {
            sessionDate = DateTime.Now,
            activityType = activityType,
            difficulty = difficulty,
            totalItems = totalItems,
            timeGiven = timeLimit,
            originalTimeGiven = timeLimit,
            adTimeExtensionsCount = 0,
            adExtraTimeSeconds = 0f,
            purchaseTimeExtensionsCount = 0,
            purchaseExtraTimeSeconds = 0f,
            totalExtraTimeSeconds = 0f,
            conceptsCovered = new List<string>(),
            responseTimes = new List<float>(),
            correctAnswers = new List<bool>(),
            wrongAnswersByConcept = new Dictionary<string, int>(),
            averageTimeByConcept = new Dictionary<string, float>(),
            timeBasedScores = new List<float>(),
            attemptedConcepts = new List<string>(),
            conceptAttempts = new Dictionary<string, int>(),
            conceptCorrect = new Dictionary<string, int>()
        };

        //        Debug.Log($"EducationalPerformanceTracker: Session initialized - {activityType}, {totalItems} items, {timeLimit}s time limit, {difficulty} difficulty");
    }

    /// <summary>
    /// Record extra time added by watching an ad and extend the current session's allowed time.
    /// </summary>
    public void RecordAdTimeExtension(float extraSeconds)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("EducationalPerformanceTracker: No active session to extend (ad)");
            return;
        }

        if (extraSeconds <= 0f) return;

        currentSession.adTimeExtensionsCount++;
        currentSession.adExtraTimeSeconds += extraSeconds;
        currentSession.totalExtraTimeSeconds += extraSeconds;
        currentSession.timeGiven += extraSeconds;

        Debug.Log($"EducationalPerformanceTracker: Ad time extension applied +{extraSeconds:F1}s (total extra {currentSession.totalExtraTimeSeconds:F1}s)");
    }

    /// <summary>
    /// Record extra time added by a purchase and extend the current session's allowed time.
    /// </summary>
    public void RecordPurchaseTimeExtension(float extraSeconds)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("EducationalPerformanceTracker: No active session to extend (purchase)");
            return;
        }

        if (extraSeconds <= 0f) return;

        currentSession.purchaseTimeExtensionsCount++;
        currentSession.purchaseExtraTimeSeconds += extraSeconds;
        currentSession.totalExtraTimeSeconds += extraSeconds;
        currentSession.timeGiven += extraSeconds;

        Debug.Log($"EducationalPerformanceTracker: Purchase time extension applied +{extraSeconds:F1}s (total extra {currentSession.totalExtraTimeSeconds:F1}s)");
    }

    /// <summary>
    /// Record an attempt for any activity type with detailed metrics
    /// </summary>
    public void RecordItemAttempt(string conceptName, bool isCorrect, float responseTime, string wrongAnswerType = "",
                                 bool isTimePressure = false, bool usedHint = false, bool wasSkipped = false)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("EducationalPerformanceTracker: No active session, cannot record attempt");
            return;
        }

        if (string.IsNullOrEmpty(conceptName))
        {
            Debug.LogError("EducationalPerformanceTracker: conceptName is null or empty");
            return;
        }

        // Validate and clamp response time
        if (responseTime < 0)
        {
            Debug.LogWarning($"EducationalPerformanceTracker: Negative response time ({responseTime}) provided. Setting to 0.");
            responseTime = 0f;
        }
        else if (responseTime > 300f) // 5 minutes max
        {
            Debug.LogWarning($"EducationalPerformanceTracker: Unusually long response time ({responseTime}s) provided. Clamping to 300s.");
            responseTime = 300f;
        }

        // Validate concept name length
        if (conceptName.Length > 100)
        {
            Debug.LogWarning($"EducationalPerformanceTracker: Concept name too long ({conceptName.Length} chars). Truncating.");
            conceptName = conceptName.Substring(0, 100);
        }

        // Record basic metrics
        responseTimes.Add(responseTime);
        correctAnswers.Add(isCorrect);
        currentSession.conceptsCovered.Add(conceptName);
        currentSession.responseTimes.Add(responseTime);
        currentSession.correctAnswers.Add(isCorrect);
        currentSession.attemptedConcepts.Add(conceptName);

        // Update concept attempts tracking
        if (!currentSession.conceptAttempts.ContainsKey(conceptName))
            currentSession.conceptAttempts[conceptName] = 0;
        currentSession.conceptAttempts[conceptName]++;

        if (isCorrect)
        {
            sessionCorrect++;
            consecutiveCorrectStreak++;
            consecutiveWrongStreak = 0;

            if (!currentSession.conceptCorrect.ContainsKey(conceptName))
                currentSession.conceptCorrect[conceptName] = 0;
            currentSession.conceptCorrect[conceptName]++;

            correctResponseTimes.Add(responseTime);
        }
        else
        {
            sessionMistakes++;
            sessionWrongAnswers++;
            consecutiveWrongStreak++;
            consecutiveCorrectStreak = 0;

            wrongResponseTimes.Add(responseTime);

            // Track wrong answer types
            if (!string.IsNullOrEmpty(wrongAnswerType))
            {
                if (!wrongAnswerTypes.ContainsKey(wrongAnswerType))
                    wrongAnswerTypes[wrongAnswerType] = 0;
                wrongAnswerTypes[wrongAnswerType]++;
            }
        }

        // Track time-based metrics
        if (isTimePressure || responseTime <= TIME_PRESSURE_THRESHOLD)
        {
            if (isCorrect)
                timeBasedCorrectAnswers++;
            else
                timeBasedWrongAnswers++;
        }

        // Track concept-specific response times
        if (!conceptResponseTimes.ContainsKey(conceptName))
            conceptResponseTimes[conceptName] = 0f;
        conceptResponseTimes[conceptName] += responseTime;

        // Update concept mastery
        UpdateConceptMastery(conceptName, isCorrect, responseTime, wrongAnswerType);

        // Track session-specific metrics
        if (usedHint)
            currentSession.hintsUsed++;
        if (wasSkipped)
            currentSession.skippedQuestions++;

        Debug.Log($"EducationalPerformanceTracker: Recorded {conceptName} - Correct: {isCorrect}, Time: {responseTime:F2}s, Streak: {consecutiveCorrectStreak}");
    }

    /// <summary>
    /// Complete the learning session and calculate final metrics
    /// </summary>
    public void CompleteLearningSession(int finalScore)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("EducationalPerformanceTracker: No active session to complete");
            return;
        }

        // Validate final score
        if (finalScore < 0)
        {
            Debug.LogWarning($"EducationalPerformanceTracker: Negative final score ({finalScore}) provided. Setting to 0.");
            finalScore = 0;
        }

        // Calculate session metrics
        currentSession.timeTaken = Mathf.Max(0, Time.time - sessionStartTime);
        currentSession.score = finalScore;
        currentSession.correctItems = sessionCorrect;
        currentSession.mistakes = sessionMistakes;
        currentSession.wrongAnswers = sessionWrongAnswers;
        currentSession.timeBasedCorrectAnswers = timeBasedCorrectAnswers;
        currentSession.timeBasedWrongAnswers = timeBasedWrongAnswers;
        currentSession.consecutiveCorrectStreak = consecutiveCorrectStreak;
        currentSession.consecutiveWrongStreak = consecutiveWrongStreak;

        // Validate data consistency
        if (currentSession.correctItems + currentSession.mistakes > currentSession.totalItems)
        {
            Debug.LogWarning($"EducationalPerformanceTracker: Data inconsistency detected - Total attempts ({currentSession.correctItems + currentSession.mistakes}) exceeds total items ({currentSession.totalItems})");
        }

        // Calculate averages
        if (responseTimes.Count > 0)
        {
            currentSession.averageResponseTime = (float)responseTimes.Average();
        }

        if (correctResponseTimes.Count > 0)
        {
            currentSession.averageTimeForCorrect = (float)correctResponseTimes.Average();
            currentSession.fastestCorrectAnswer = correctResponseTimes.Min();
        }

        if (wrongResponseTimes.Count > 0)
        {
            currentSession.averageTimeForWrong = (float)wrongResponseTimes.Average();
            currentSession.slowestWrongAnswer = wrongResponseTimes.Max();
        }

        // Calculate accuracy
        currentSession.accuracyPercentage = currentSession.totalItems > 0 ?
            (float)currentSession.correctItems / currentSession.totalItems : 0f;

        // Calculate time pressure accuracy
        if (timeBasedCorrectAnswers + timeBasedWrongAnswers > 0)
        {
            currentSession.timePressureAccuracy = (float)timeBasedCorrectAnswers / (timeBasedCorrectAnswers + timeBasedWrongAnswers);
        }

        // Calculate relaxed accuracy
        int relaxedCorrect = sessionCorrect - timeBasedCorrectAnswers;
        int relaxedWrong = sessionWrongAnswers - timeBasedWrongAnswers;
        if (relaxedCorrect + relaxedWrong > 0)
        {
            currentSession.relaxedAccuracy = (float)relaxedCorrect / (relaxedCorrect + relaxedWrong);
        }

        // Calculate efficiency score
        currentSession.efficiencyScore = CalculateEfficiencyScore();

        // Copy concept-specific data
        currentSession.wrongAnswersByConcept = new Dictionary<string, int>(wrongAnswerTypes);
        currentSession.averageTimeByConcept = new Dictionary<string, float>(conceptResponseTimes);

        // Add to learning history
        currentStudent.learningHistory.Add(currentSession);
        currentStudent.totalSessions++;
        currentStudent.totalPlayTime += currentSession.timeTaken;
        currentStudent.totalItemsCompleted += currentSession.totalItems;

        // Aggregate time extension analytics
        if (currentSession.totalExtraTimeSeconds > 0f)
        {
            currentStudent.sessionsWithTimeExtensions++;
        }
        currentStudent.totalAdExtensions += currentSession.adTimeExtensionsCount;
        currentStudent.totalAdExtraTime += currentSession.adExtraTimeSeconds;
        currentStudent.totalPurchaseExtensions += currentSession.purchaseTimeExtensionsCount;
        currentStudent.totalPurchaseExtraTime += currentSession.purchaseExtraTimeSeconds;

        // Update activity-specific tracking
        UpdateActivitySpecificTracking();

        // Update student profile
        UpdateStudentProfile();

        // Analyze learning patterns
        AnalyzeLearningPatterns();

        // Update weekly progress
        if (enableWeeklyProgressTracking)
        {
            UpdateWeeklyProgress();
        }

        // Save data
        SaveStudentData();

        // Update UI
        UpdateDashboardUI();

        Debug.Log($"EducationalPerformanceTracker: Session completed - Score: {finalScore}, Accuracy: {currentSession.accuracyPercentage:P0}, Time: {currentSession.timeTaken:F1}s");

        // Reset session
        currentSession = null;
    }

    private void UpdateActivitySpecificTracking()
    {
        if (currentSession == null) return;

        ActivityType activityType = currentSession.activityType;

        // Update sessions count
        if (!currentStudent.sessionsByActivity.ContainsKey(activityType))
            currentStudent.sessionsByActivity[activityType] = 0;
        currentStudent.sessionsByActivity[activityType]++;

        // Update total time
        if (!currentStudent.totalTimeByActivity.ContainsKey(activityType))
            currentStudent.totalTimeByActivity[activityType] = 0f;
        currentStudent.totalTimeByActivity[activityType] += currentSession.timeTaken;

        // Update average score
        var activitySessions = currentStudent.learningHistory
            .Where(s => s.activityType == activityType)
            .ToList();

        if (activitySessions.Count > 0)
        {
            currentStudent.averageScoreByActivity[activityType] = (float)activitySessions.Average(s => s.score);
            currentStudent.averageAccuracyByActivity[activityType] = (float)activitySessions.Average(s => s.accuracyPercentage);
        }
    }

    private void UpdateWeeklyProgress()
    {
        string weekKey = GetWeekKey(DateTime.Now);

        // Calculate weekly accuracy
        var thisWeekSessions = currentStudent.learningHistory
            .Where(s => GetWeekKey(s.sessionDate) == weekKey)
            .ToList();

        if (thisWeekSessions.Count > 0)
        {
            float weeklyAccuracy = (float)thisWeekSessions.Average(s => s.accuracyPercentage);
            currentStudent.weeklyProgress[weekKey] = weeklyAccuracy;

            // Identify weekly strengths and weaknesses
            var weeklyConcepts = thisWeekSessions
                .SelectMany(s => s.conceptsCovered)
                .GroupBy(c => c)
                .ToDictionary(g => g.Key, g => g.Count());

            var strongConcepts = weeklyConcepts
                .Where(kvp => currentStudent.conceptMastery.ContainsKey(kvp.Key) &&
                              currentStudent.conceptMastery[kvp.Key].masteryLevel >= 0.7f)
                .Select(kvp => kvp.Key)
                .ToList();

            var weakConcepts = weeklyConcepts
                .Where(kvp => currentStudent.conceptMastery.ContainsKey(kvp.Key) &&
                              currentStudent.conceptMastery[kvp.Key].masteryLevel < 0.5f)
                .Select(kvp => kvp.Key)
                .ToList();

            currentStudent.weeklyStrengths[weekKey] = strongConcepts;
            currentStudent.weeklyWeaknesses[weekKey] = weakConcepts;
        }
    }

    private string GetWeekKey(DateTime date)
    {
        // Get the start of the week (Monday)
        int daysSinceMonday = (int)date.DayOfWeek - 1;
        if (daysSinceMonday < 0) daysSinceMonday += 7;
        DateTime weekStart = date.AddDays(-daysSinceMonday);
        return weekStart.ToString("yyyy-MM-dd");
    }

    private void UpdateConceptMastery(string conceptName, bool isCorrect, float responseTime, string wrongAnswerType = "")
    {
        if (!trackConceptMastery) return;

        if (!currentStudent.conceptMastery.ContainsKey(conceptName))
        {
            currentStudent.conceptMastery[conceptName] = new ConceptMastery
            {
                conceptName = conceptName,
                totalAttempts = 0,
                correctAttempts = 0,
                averageResponseTime = 0f,
                masteryLevel = 0f,
                lastPracticed = DateTime.Now,
                consecutiveCorrect = 0,
                consecutiveIncorrect = 0,
                recentScores = new List<float>(),
                wrongAttempts = 0,
                averageTimeForCorrect = 0f,
                averageTimeForWrong = 0f,
                fastestCorrectAnswer = float.MaxValue,
                slowestWrongAnswer = 0f,
                timeBasedCorrectCount = 0,
                timeBasedWrongCount = 0,
                timePressureAccuracy = 0f,
                relaxedAccuracy = 0f,
                responseTimeHistory = new List<float>(),
                answerHistory = new List<bool>(),
                wrongAnswerTypes = new Dictionary<string, int>(),
                attemptsByActivity = new Dictionary<ActivityType, int>(),
                correctByActivity = new Dictionary<ActivityType, int>(),
                averageTimeByActivity = new Dictionary<ActivityType, float>()
            };
        }

        var mastery = currentStudent.conceptMastery[conceptName];
        mastery.totalAttempts++;
        mastery.lastPracticed = DateTime.Now;
        mastery.responseTimeHistory.Add(responseTime);
        mastery.answerHistory.Add(isCorrect);

        // Update activity-specific tracking
        if (currentSession != null)
        {
            ActivityType activityType = currentSession.activityType;

            if (!mastery.attemptsByActivity.ContainsKey(activityType))
                mastery.attemptsByActivity[activityType] = 0;
            mastery.attemptsByActivity[activityType]++;

            if (isCorrect)
            {
                mastery.correctAttempts++;
                mastery.consecutiveCorrect++;
                mastery.consecutiveIncorrect = 0;

                if (!mastery.correctByActivity.ContainsKey(activityType))
                    mastery.correctByActivity[activityType] = 0;
                mastery.correctByActivity[activityType]++;
            }
            else
            {
                mastery.wrongAttempts++;
                mastery.consecutiveIncorrect++;
                mastery.consecutiveCorrect = 0;

                if (!string.IsNullOrEmpty(wrongAnswerType))
                {
                    if (!mastery.wrongAnswerTypes.ContainsKey(wrongAnswerType))
                        mastery.wrongAnswerTypes[wrongAnswerType] = 0;
                    mastery.wrongAnswerTypes[wrongAnswerType]++;
                }
            }

            // Update average time by activity
            if (!mastery.averageTimeByActivity.ContainsKey(activityType))
                mastery.averageTimeByActivity[activityType] = 0f;

            var activityTimes = mastery.responseTimeHistory
                .Where((_, index) => mastery.answerHistory[index] == isCorrect)
                .ToList();

            if (activityTimes.Count > 0)
            {
                mastery.averageTimeByActivity[activityType] = (float)activityTimes.Average();
            }
        }

        // Calculate mastery level (0-1 scale)
        mastery.masteryLevel = mastery.totalAttempts > 0 ? (float)mastery.correctAttempts / mastery.totalAttempts : 0f;

        // Update average response time
        if (mastery.responseTimeHistory.Count > 0)
        {
            mastery.averageResponseTime = (float)mastery.responseTimeHistory.Average();
        }

        // Update time-based metrics
        if (responseTime <= TIME_PRESSURE_THRESHOLD)
        {
            if (isCorrect)
                mastery.timeBasedCorrectCount++;
            else
                mastery.timeBasedWrongCount++;
        }

        // Calculate time pressure accuracy
        if (mastery.timeBasedCorrectCount + mastery.timeBasedWrongCount > 0)
        {
            mastery.timePressureAccuracy = (float)mastery.timeBasedCorrectCount / (mastery.timeBasedCorrectCount + mastery.timeBasedWrongCount);
        }

        // Update fastest/slowest times
        if (isCorrect && responseTime < mastery.fastestCorrectAnswer)
            mastery.fastestCorrectAnswer = responseTime;
        if (!isCorrect && responseTime > mastery.slowestWrongAnswer)
            mastery.slowestWrongAnswer = responseTime;

        // Keep recent scores (last 10)
        if (currentSession != null)
        {
            mastery.recentScores.Add(currentSession.accuracyPercentage);
            if (mastery.recentScores.Count > 10)
                mastery.recentScores.RemoveAt(0);
        }
    }

    private float CalculateEfficiencyScore()
    {
        if (currentSession == null) return 0f;

        float accuracyWeight = 0.4f;
        float speedWeight = 0.3f;
        float consistencyWeight = 0.3f;

        // Accuracy component
        float accuracyScore = currentSession.accuracyPercentage;

        // Speed component (inverse of average response time, normalized)
        float speedScore = 0f;
        if (currentSession.averageResponseTime > 0)
        {
            speedScore = Mathf.Clamp01(1f - (currentSession.averageResponseTime / 10f));
        }

        // Consistency component (based on streak)
        float consistencyScore = Mathf.Clamp01((float)consecutiveCorrectStreak / currentSession.totalItems);
        float baseScore = (accuracyScore * accuracyWeight) + (speedScore * speedWeight) + (consistencyScore * consistencyWeight);

        // Penalize reliance on extended time (up to 25% reduction)
        float extensionPenaltyMultiplier = 1f;
        if (currentSession.originalTimeGiven > 0f && currentSession.totalExtraTimeSeconds > 0f)
        {
            float extensionRatio = Mathf.Clamp01(currentSession.totalExtraTimeSeconds / currentSession.originalTimeGiven);
            float maxPenalty = 0.25f; // cap reduction to 25%
            extensionPenaltyMultiplier = 1f - (extensionRatio * maxPenalty);
        }

        return baseScore * extensionPenaltyMultiplier;
    }

    private void UpdateStudentProfile()
    {
        if (currentStudent.learningHistory.Count == 0) return;

        // Calculate overall accuracy
        int totalCorrect = currentStudent.learningHistory.Sum(s => s.correctItems);
        int totalItems = currentStudent.learningHistory.Sum(s => s.totalItems);
        currentStudent.overallAccuracy = totalItems > 0 ? (float)totalCorrect / totalItems : 0f;

        // Calculate average session score
        currentStudent.averageSessionScore = (float)currentStudent.learningHistory.Average(s => s.score);

        // Ensure cumulative time extension fields are non-negative and consistent
        currentStudent.totalAdExtensions = Mathf.Max(0, currentStudent.totalAdExtensions);
        currentStudent.totalPurchaseExtensions = Mathf.Max(0, currentStudent.totalPurchaseExtensions);
        currentStudent.totalAdExtraTime = Mathf.Max(0f, currentStudent.totalAdExtraTime);
        currentStudent.totalPurchaseExtraTime = Mathf.Max(0f, currentStudent.totalPurchaseExtraTime);
        currentStudent.sessionsWithTimeExtensions = Mathf.Max(0, currentStudent.sessionsWithTimeExtensions);

        // Analyze learning patterns
        AnalyzeLearningPatterns();
    }

    private void AnalyzeLearningPatterns()
    {
        if (currentStudent.learningHistory.Count < 2) return;

        // Find strengths (concepts with high mastery)
        var strongConcepts = currentStudent.conceptMastery
            .Where(kvp => kvp.Value.masteryLevel >= 0.7f && kvp.Value.totalAttempts >= 5)
            .Select(kvp => kvp.Key)
            .ToList();

        currentStudent.strengths = strongConcepts;

        // Find areas for improvement (concepts with low mastery)
        var weakConcepts = currentStudent.conceptMastery
            .Where(kvp => kvp.Value.masteryLevel < 0.5f && kvp.Value.totalAttempts >= 3)
            .Select(kvp => kvp.Key)
            .ToList();

        currentStudent.areasForImprovement = weakConcepts;
    }

    private void UpdateDashboardUI()
    {
        if (currentStudent == null)
        {
            LoadStudentProfile();
        }

        // Overall Progress
        if (overallProgressText != null)
        {
            string extensionSummary =
                $"Ad Ext: {currentStudent.totalAdExtensions} (+{currentStudent.totalAdExtraTime:F0}s) | " +
                $"IAP Ext: {currentStudent.totalPurchaseExtensions} (+{currentStudent.totalPurchaseExtraTime:F0}s) | " +
                $"Sessions Used: {currentStudent.sessionsWithTimeExtensions}";

            overallProgressText.text = $"Overall Progress: {currentStudent.overallAccuracy:P0}\n" +
                                      $"Total Sessions: {currentStudent.totalSessions}\n" +
                                      $"Total Items: {currentStudent.totalItemsCompleted}\n" +
                                      $"Time Extensions: {extensionSummary}";
        }

        // Concept Mastery
        if (conceptMasteryText != null)
        {
            var topConcepts = currentStudent.conceptMastery
                .OrderByDescending(kvp => kvp.Value.masteryLevel)
                .Take(5)
                .Select(kvp => $"{kvp.Key}: {kvp.Value.masteryLevel:P0} ({kvp.Value.totalAttempts} attempts)")
                .ToList();

            conceptMasteryText.text = $"Top Concepts:\n{string.Join("\n", topConcepts)}";
        }

        // Strengths
        if (strengthsText != null)
        {
            if (currentStudent.strengths.Count > 0)
            {
                strengthsText.text = $"Strengths:\n{string.Join("\n", currentStudent.strengths)}";
            }
            else
            {
                strengthsText.text = "Strengths:\nKeep practicing to identify strengths!";
            }
        }

        // Areas for Improvement
        if (improvementAreasText != null)
        {
            if (currentStudent.areasForImprovement.Count > 0)
            {
                improvementAreasText.text = $"Areas for Improvement:\n{string.Join("\n", currentStudent.areasForImprovement)}";
            }
            else
            {
                improvementAreasText.text = "Areas for Improvement:\nGreat job! No areas need improvement.";
            }
        }

        // Update enhanced metrics
        UpdateEnhancedMetricsUI();
        UpdateParentDashboardUI();
    }

    private void UpdateEnhancedMetricsUI()
    {
        if (currentStudent == null) return;

        // Wrong Answers Analysis
        if (wrongAnswersText != null)
        {
            var wrongAnswerAnalysis = wrongAnswerTypes
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => $"{kvp.Key}: {kvp.Value} times")
                .ToList();

            if (wrongAnswerAnalysis.Count > 0)
            {
                wrongAnswersText.text = $"Common Mistakes:\n{string.Join("\n", wrongAnswerAnalysis)}";
            }
            else
            {
                wrongAnswersText.text = "Common Mistakes:\nNo mistakes recorded yet.";
            }
        }

        // Most Difficult Concepts
        if (mostDifficultConceptsText != null)
        {
            var difficultConcepts = currentStudent.conceptMastery
                .Where(kvp => kvp.Value.masteryLevel < 0.5f && kvp.Value.totalAttempts >= 3)
                .OrderBy(kvp => kvp.Value.masteryLevel)
                .Take(5)
                .Select(kvp => $"{kvp.Key}: {kvp.Value.masteryLevel:P0} mastery")
                .ToList();

            if (difficultConcepts.Count > 0)
            {
                mostDifficultConceptsText.text = $"Most Difficult:\n{string.Join("\n", difficultConcepts)}";
            }
            else
            {
                mostDifficultConceptsText.text = "Most Difficult:\nNo difficult concepts identified.";
            }
        }

        // Response Time Analysis
        if (responseTimeAnalysisText != null)
        {
            if (currentSession != null && responseTimes.Count > 0)
            {
                float avgResponseTime = (float)responseTimes.Average();
                float fastestTime = responseTimes.Min();
                float slowestTime = responseTimes.Max();

                responseTimeAnalysisText.text = $"Response Time Analysis:\n" +
                                              $"Average: {avgResponseTime:F2}s\n" +
                                              $"Fastest: {fastestTime:F2}s\n" +
                                              $"Slowest: {slowestTime:F2}s";
            }
            else
            {
                responseTimeAnalysisText.text = "Response Time Analysis:\nNo data available";
            }
        }

        // Time Pressure Analysis
        if (timePressureText != null)
        {
            if (currentSession != null)
            {
                string extensions =
                    $"Ext Used: Ad x{currentSession.adTimeExtensionsCount} (+{currentSession.adExtraTimeSeconds:F0}s), " +
                    $"IAP x{currentSession.purchaseTimeExtensionsCount} (+{currentSession.purchaseExtraTimeSeconds:F0}s)";

                timePressureText.text = $"Time Pressure Analysis:\n" +
                                       $"Under Pressure: {currentSession.timePressureAccuracy:P0}\n" +
                                       $"Relaxed: {currentSession.relaxedAccuracy:P0}\n" +
                                       $"Time-based Correct: {timeBasedCorrectAnswers}\n" +
                                       $"Time-based Wrong: {timeBasedWrongAnswers}\n" +
                                       $"Time Extensions: {extensions}";
            }
            else
            {
                timePressureText.text = "Time Pressure Analysis:\nNo data available";
            }
        }
    }

    private void UpdateParentDashboardUI()
    {
        if (currentStudent == null) return;

        // Weekly Progress
        if (weeklyProgressText != null)
        {
            string currentWeek = GetWeekKey(DateTime.Now);
            if (currentStudent.weeklyProgress.ContainsKey(currentWeek))
            {
                float weeklyAccuracy = currentStudent.weeklyProgress[currentWeek];
                var weeklySessions = currentStudent.learningHistory
                    .Where(s => GetWeekKey(s.sessionDate) == currentWeek)
                    .ToList();

                weeklyProgressText.text = $"This Week's Progress:\n" +
                                         $"Accuracy: {weeklyAccuracy:P0}\n" +
                                         $"Sessions: {weeklySessions.Count}\n" +
                                         $"Time Spent: {FormatTime(weeklySessions.Sum(s => s.timeTaken))}";
            }
            else
            {
                weeklyProgressText.text = "This Week's Progress:\nNo sessions this week";
            }
        }

        // Strengths for Parents
        if (strengthsParentText != null)
        {
            if (currentStudent.strengths.Count > 0)
            {
                strengthsParentText.text = $"Your Child's Strengths:\n{string.Join("\n", currentStudent.strengths)}";
            }
            else
            {
                strengthsParentText.text = "Your Child's Strengths:\nKeep practicing to identify strengths!";
            }
        }

        // Learning Recommendations
        if (learningRecommendationsText != null)
        {
            var recommendations = GenerateLearningRecommendations();
            learningRecommendationsText.text = $"Learning Recommendations:\n{string.Join("\n", recommendations)}";
        }

        // Time Spent
        if (timeSpentText != null)
        {
            timeSpentText.text = $"Total Learning Time:\n{FormatTime(currentStudent.totalPlayTime)}";
        }
    }

    private List<string> GenerateLearningRecommendations()
    {
        var recommendations = new List<string>();

        if (currentStudent.learningHistory.Count == 0)
        {
            recommendations.Add("Start learning to get recommendations");
            return recommendations;
        }

        // Check overall accuracy
        if (currentStudent.overallAccuracy < 0.7f)
        {
            recommendations.Add("Focus on accuracy over speed");
        }

        // Check session frequency
        if (currentStudent.totalSessions < 5)
        {
            recommendations.Add("Practice more frequently");
        }

        // Check for specific weak areas
        if (currentStudent.areasForImprovement.Count > 0)
        {
            recommendations.Add($"Practice: {string.Join(", ", currentStudent.areasForImprovement.Take(2))}");
        }

        // Time extension reliance recommendations
        int totalSessions = Mathf.Max(1, currentStudent.totalSessions);
        int sessionsUsingExtensions = currentStudent.sessionsWithTimeExtensions;
        float extensionSessionRatio = (float)sessionsUsingExtensions / totalSessions;
        float totalExtraTime = currentStudent.totalAdExtraTime + currentStudent.totalPurchaseExtraTime;

        if (sessionsUsingExtensions >= 2 && extensionSessionRatio >= 0.3f)
        {
            recommendations.Add("Improve pacing to rely less on extended time");
        }
        if (extensionSessionRatio >= 0.6f)
        {
            recommendations.Add("Try timed practice without extensions to build speed and confidence");
        }
        if (currentStudent.totalAdExtensions > 0)
        {
            recommendations.Add($"Aim to complete levels without ad-based extensions (used {currentStudent.totalAdExtensions}x)");
        }
        if (currentStudent.totalPurchaseExtensions > 0)
        {
            recommendations.Add($"Reduce purchased time boosts (used {currentStudent.totalPurchaseExtensions}x; +{totalExtraTime:F0}s total)");
        }
        if (currentStudent.overallAccuracy >= 0.8f && extensionSessionRatio >= 0.3f)
        {
            recommendations.Add("Accuracy is strong — focus on speed drills");
        }

        // Activity-specific recommendations
        var leastPracticedActivity = currentStudent.sessionsByActivity
            .OrderBy(kvp => kvp.Value)
            .FirstOrDefault();

        if (leastPracticedActivity.Key != ActivityType.General && leastPracticedActivity.Value < 3)
        {
            recommendations.Add($"Try more {leastPracticedActivity.Key} activities");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("Excellent progress! Keep up the good work");
        }

        return recommendations;
    }

    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);

        if (hours > 0)
            return $"{hours}h {minutes}m";
        else
            return $"{minutes}m";
    }

    private void SaveStudentData()
    {
        if (currentStudent == null)
        {
            Debug.LogWarning("EducationalPerformanceTracker: Cannot save data - no student profile available");
            return;
        }

        try
        {
            // Validate data before saving
            ValidateStudentData();

            string jsonData = JsonUtility.ToJson(currentStudent, true);

            // Check if JSON data is valid
            if (string.IsNullOrEmpty(jsonData) || jsonData == "{}")
            {
                Debug.LogWarning("EducationalPerformanceTracker: Generated JSON data is empty or invalid");
                return;
            }

            PlayerPrefs.SetString("StudentProfile_Enhanced", jsonData);
            PlayerPrefs.Save();
            Debug.Log($"EducationalPerformanceTracker: Student data saved successfully ({jsonData.Length} characters)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EducationalPerformanceTracker: Failed to save student data - {e.Message}");

            // Try to save a minimal backup
            try
            {
                var backupData = new StudentProfile
                {
                    studentId = currentStudent.studentId,
                    studentName = currentStudent.studentName,
                    totalSessions = currentStudent.totalSessions,
                    totalPlayTime = currentStudent.totalPlayTime
                };

                string backupJson = JsonUtility.ToJson(backupData, true);
                PlayerPrefs.SetString("StudentProfile_Enhanced_Backup", backupJson);
                PlayerPrefs.Save();
                Debug.Log("EducationalPerformanceTracker: Backup data saved successfully");
            }
            catch (System.Exception backupException)
            {
                Debug.LogError($"EducationalPerformanceTracker: Failed to save backup data - {backupException.Message}");
            }
        }
    }

    /// <summary>
    /// Validates student data before saving to prevent corruption
    /// </summary>
    private void ValidateStudentData()
    {
        if (currentStudent == null) return;

        // Ensure non-null collections
        if (currentStudent.conceptMastery == null)
            currentStudent.conceptMastery = new Dictionary<string, ConceptMastery>();
        if (currentStudent.learningHistory == null)
            currentStudent.learningHistory = new List<LearningSession>();
        if (currentStudent.strengths == null)
            currentStudent.strengths = new List<string>();
        if (currentStudent.areasForImprovement == null)
            currentStudent.areasForImprovement = new List<string>();

        // Validate numeric values
        currentStudent.totalSessions = Mathf.Max(0, currentStudent.totalSessions);
        currentStudent.totalPlayTime = Mathf.Max(0, currentStudent.totalPlayTime);
        currentStudent.totalItemsCompleted = Mathf.Max(0, currentStudent.totalItemsCompleted);
        currentStudent.overallAccuracy = Mathf.Clamp01(currentStudent.overallAccuracy);
        currentStudent.averageSessionScore = Mathf.Max(0, currentStudent.averageSessionScore);

        // Validate time extension data
        currentStudent.totalAdExtensions = Mathf.Max(0, currentStudent.totalAdExtensions);
        currentStudent.totalPurchaseExtensions = Mathf.Max(0, currentStudent.totalPurchaseExtensions);
        currentStudent.totalAdExtraTime = Mathf.Max(0, currentStudent.totalAdExtraTime);
        currentStudent.totalPurchaseExtraTime = Mathf.Max(0, currentStudent.totalPurchaseExtraTime);
        currentStudent.sessionsWithTimeExtensions = Mathf.Max(0, currentStudent.sessionsWithTimeExtensions);

        Debug.Log("EducationalPerformanceTracker: Student data validation completed");
    }

    // Public getter methods
    public StudentProfile GetStudentProfile() => currentStudent;
    public LearningSession GetCurrentSession() => currentSession;
    public float GetOverallAccuracy() => currentStudent?.overallAccuracy ?? 0f;
    public float GetAverageSessionScore() => currentStudent?.averageSessionScore ?? 0f;
    public List<string> GetStrengths() => currentStudent?.strengths ?? new List<string>();
    public List<string> GetAreasForImprovement() => currentStudent?.areasForImprovement ?? new List<string>();
    public Dictionary<string, ConceptMastery> GetConceptMastery() => currentStudent?.conceptMastery ?? new Dictionary<string, ConceptMastery>();

    public void RefreshDashboardUI()
    {
        UpdateDashboardUI();
    }

    /// <summary>
    /// Get activity-specific performance data
    /// </summary>
    public Dictionary<ActivityType, float> GetActivityPerformance()
    {
        return currentStudent?.averageAccuracyByActivity ?? new Dictionary<ActivityType, float>();
    }

    /// <summary>
    /// Get weekly progress data
    /// </summary>
    public Dictionary<string, float> GetWeeklyProgress()
    {
        return currentStudent?.weeklyProgress ?? new Dictionary<string, float>();
    }

    /// <summary>
    /// Get most difficult concepts
    /// </summary>
    public List<string> GetMostDifficultConcepts(int count = 5)
    {
        if (currentStudent?.conceptMastery == null) return new List<string>();

        return currentStudent.conceptMastery
            .Where(kvp => kvp.Value.masteryLevel < 0.5f && kvp.Value.totalAttempts >= 3)
            .OrderBy(kvp => kvp.Value.masteryLevel)
            .Take(count)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    /// <summary>
    /// Get wrong answer patterns
    /// </summary>
    public Dictionary<string, int> GetWrongAnswerPatterns()
    {
        return wrongAnswerTypes ?? new Dictionary<string, int>();
    }

    /// <summary>
    /// Comprehensive test method to validate all functionality.
    /// Call this method to test the educational performance tracker's robustness.
    /// </summary>
    [ContextMenu("Test Educational Performance Tracker")]
    public void TestEducationalPerformanceTracker()
    {
        Debug.Log("=== EducationalPerformanceTracker Test Started ===");

        // Test 1: Initialize session
        Debug.Log("Test 1: Initializing learning session");
        InitializeLearningSession(ActivityType.CoinGame, 5, 60f, DifficultyLevel.Medium);

        // Test 2: Record item attempts
        Debug.Log("Test 2: Recording item attempts");
        RecordItemAttempt("Addition", true, 2.5f, "", false, false, false);
        RecordItemAttempt("Subtraction", true, 1.8f, "", false, false, false);
        RecordItemAttempt("Multiplication", false, 4.2f, "Calculation Error", true, false, false);
        RecordItemAttempt("Division", true, 3.1f, "", false, true, false);
        RecordItemAttempt("Fractions", false, 5.5f, "Concept Error", false, false, true);

        // Test 3: Record time extensions
        Debug.Log("Test 3: Recording time extensions");
        RecordAdTimeExtension(15f);
        RecordPurchaseTimeExtension(10f);

        // Test 4: Complete session
        Debug.Log("Test 4: Completing learning session");
        CompleteLearningSession(85);

        // Test 5: Test edge cases
        Debug.Log("Test 5: Testing edge cases");

        // Test negative values
        InitializeLearningSession(ActivityType.Flashcards, -1, -5f, DifficultyLevel.Easy);
        Debug.Log($"  Negative inputs handled: TotalItems={currentSession?.totalItems}, TimeLimit={currentSession?.timeGiven}");

        // Test empty concept name
        InitializeLearningSession(ActivityType.TriviaQuiz, 3, 30f, DifficultyLevel.Hard);
        RecordItemAttempt("", true, 2.0f); // Should be rejected
        RecordItemAttempt("Valid Concept", true, 2.0f);

        // Test extreme response times
        RecordItemAttempt("Test Concept", true, -1f); // Negative time
        RecordItemAttempt("Test Concept 2", true, 500f); // Very long time

        // Test very long concept name
        string longConceptName = new string('A', 150);
        RecordItemAttempt(longConceptName, true, 2.0f);

        CompleteLearningSession(75);

        // Test 6: Test data retrieval
        Debug.Log("Test 6: Testing data retrieval methods");
        Debug.Log($"  Overall Accuracy: {GetOverallAccuracy():P0}");
        Debug.Log($"  Average Session Score: {GetAverageSessionScore():F1}");
        Debug.Log($"  Strengths: {string.Join(", ", GetStrengths())}");
        Debug.Log($"  Areas for Improvement: {string.Join(", ", GetAreasForImprovement())}");
        Debug.Log($"  Most Difficult Concepts: {string.Join(", ", GetMostDifficultConcepts(3))}");

        // Test 7: Test UI updates
        Debug.Log("Test 7: Testing UI updates");
        RefreshDashboardUI();

        Debug.Log("=== EducationalPerformanceTracker Test Completed ===");
    }
}

// Extension method for variance calculation
public static class EducationalPerformanceTrackerEnhancedExtensions
{
    public static float Variance(this IEnumerable<float> values)
    {
        var list = values.ToList();
        if (list.Count == 0) return 0f;

        float mean = (float)list.Average();
        float variance = list.Sum(x => Mathf.Pow(x - mean, 2)) / list.Count;
        return variance;
    }
}