public interface IFirestoreEnums
{
    public enum ParentProfile
    {
        children, country, countrycode, currency, language, parentname, pin, pinhint
    }
    public enum Unit { unit01, unit02, unit03, unit04, unit05, unit06 }

    public enum ChildProfile
    { dob, age, avatar, firstname, gender, grade, lastname, pin, pinhint, plan, screentime }

    /*
        Dont change the sequence in UnitButtonName
    */
    public enum UnitButtonName
    {
        maingame, lesson, flashcard, minigames, vocabs, calculator, video, storybook, worksheets, finsong, funfact, megaquiz
    }


    public enum UnitStageButtonStatus
    {
        lesson, flashcard, minigames, vocabs, calculator, video, storybook, worksheets, finsong, funfact, megaquiz
    }
    public enum FSMapField
    {
        unit_stage_btn_status,
        unit_stage_data,
        maingame,
        lesson,
        flashcard,
        minigames,
        vocabs,
        calculator,
        video,
        trivia,
        quizzes,
        profile,
        points_score,
        progress_data,
        levels,
        locked
    }

    public enum FSCollection
    {
        parent,
        children
    }

    public enum SceneName
    {
        Level,
        HomeScene,
        AppAuthentication,
        Agreement,
        LanguageRegion,
        ChildAuthentication,
        ChildDashboard,
        ParentDashboard,
        AddChildDashboard,
        ChildProgressDashboard,
        ParentAccount,
        MainGame,
        Lesson,
        FlashCard,
        MiniGames,
        Vocabs,
        Calculator,
        Video,
        StoryBook,
        WorkSheets,
        MegaQuiz,
        FunFact,
        FinSong
    }

    public enum TriviaData
    {
        question, validity, pending, attempted, level

    }

    /*   public enum TriviaQuizTopic
       {
           intro,
           barter,
           money,
           currency,
           sourcing,
           valuation
       }*/

    /* public enum MainGame
     {
         date, played, level,  game_points, lesson_points, flash_points,video_points, life, powerup_health, lesson_points_collected, flash_points_collected, video_points_collected
     }*/

    public enum MainGame
    { date, played, level, levels, life, passes, lesson_pass_collected, flash_pass_collected, flash_trivia_collected, minigames_pass_collected, minigames_trivia_collected, vocabs_pass_collected, vocabs_trivia_collected, calculator_pass_collected, calculator_trivia_collected, video_pass_collected, video_trivia_collected, game_points, powerup_health }
    public enum Flashcard
    { date, played, level, levels, current_card_one, current_card_two, card_one_scratched, card_two_scratched, expired, trivia }
    public enum Vocab
    { date, played, levels, level, words }
    public enum MiniGames
    { date, played, levels, level }
    public enum CalCulator
    { date, played, levels, level }
    public enum Videos
    { id, date, played, levels, level }
    public enum StoryBook
    { date, played, levels, level }
    public enum HUD
    {
        xp, coins, stars, view, visit,
        /*game_score,
     activity_score,

     bonus,
     challenge,
     achievement,
     interaction,
     social,
     learning,
     collection*/
    }

    public enum GameScore
    { life, game_points, powerup_health }
    public enum ProgressData
    { current_unit, current_unit_name, current_stage_name, next_unit_date, rank, level_completed }
    public enum ScorePoint
    { MAINGAME, LESSON, SCRATCHCARD, TRIVIA, MINIGAME, VOCABS, CALCULATOR, VIDEOS, STORYBOOK, FINSONG, WORKSHEET, FUNFACT, MEGAQUIZ }
    public enum Quizzes
    { lesson, flashcard, minigames, vocabs, calculator, video, storybook, worksheets, finsong, funfact, megaquiz }
    public enum Tags
    { LessonPass, FlashPass, FlashTrivia, MinigamesPass, MinigamesTrivia, VocabsPass, VocabsTrivia, CalculatorPass, CalculatorTrivia, VideoPass, VideoTrivia, MessageBox, Level }
    /*    public static string[] UnitTitle = {
            "",
       "Intro",
        "Barter System",
        "Money",
        "Currency",
        "Sourcing",
        "Valuation",
        };*/
    public static string[] UnitLevelName = {
      "",
      "Intro",
        "Barter",
        "Money",
        "Currency"

    };
    public static string[] UnitStageName = {
       "", "MainGame", "Lesson", "Flashcard", "Minigames", "Vocabs", "Calculator", "Video",  "Storybook", "Worksheets", "Funfact", "Finsong", "Megaquiz"};

    /* public static string MapStageName(string stage)
     {
         if (string.IsNullOrEmpty(stage)) return stage;
         if (stage.Equals("Fashcard", System.StringComparison.OrdinalIgnoreCase)) return "Flashcard";
         return stage;
     }*/

    public static string[] GameSceneName =
      {"", "U1-1","U1-2","U1-3","U1-4" ,"U1-5","U1-6","U1-7","U1-8"  };
    //  public static string[] MiniGameSceneName =
    //   {"", "U1_L1_KYC_01","U1_L1_KYC_01","U1_L1_KYC_01","U1_L1_KYC_01" ,"U1_L1_KYC_01","U1_L1_KYC_01","U1_L1_KYC_01"  };
}
