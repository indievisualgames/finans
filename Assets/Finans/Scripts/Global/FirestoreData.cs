using System;
using System.Collections.Generic;

using UnityEngine;
using static IFirestoreEnums;

public class FirestoreData : MonoBehaviour
{

    /* public static Dictionary<string, object> UnitButtonStatusBoolean(string _unit)
     {
         Dictionary<string, object> data = new Dictionary<string, object>(){
     {

             _unit,  new Dictionary<string, object>(){
                 {UnitButtonName.lesson.ToString(), "false"},
                 {UnitButtonName.flashcard.ToString(), "false"},
                {UnitButtonName.minigame.ToString(), "true"},
                {UnitButtonName.video.ToString(), "true"},
                {UnitButtonName.vocab.ToString(), "true"},
               { UnitButtonName.storybook.ToString(), "true"},
                {UnitButtonName.worksheet.ToString(), "true"},
                {UnitButtonName.calculator.ToString(), "true"},
                {UnitButtonName.megaquiz.ToString(), "true"},
                {UnitButtonName.finsong.ToString(), "true"},
                                   }
                }

            };
         return data;
     }*/
    public static Dictionary<string, object> CreateFlashcardData(int[] cardNumbers)
    {
        DateTime currentDT = ServerDateTime.GetFastestNISTDate();
        Dictionary<string, object> initialdata = new Dictionary<string, object>(){
                            {Flashcard.level.ToString(), 1},
                            {Flashcard.levels.ToString(), new Dictionary<string, object>{
                                { "01", new Dictionary<string, object> {
                                    {Flashcard.date.ToString(), currentDT.ToShortDateString()},
                                    {Flashcard.played.ToString(), false },
                                     {Flashcard.current_card_one.ToString(), cardNumbers[0]},
                                    {Flashcard.current_card_two.ToString() , cardNumbers[1]},
                                  /*  {Flashcard.validity_card_one.ToString(), currentDT.ToShortDateString()},
                                    {Flashcard.validity_card_two.ToString(), currentDT.ToShortDateString()},*/
                                    {Flashcard.card_one_scratched.ToString(), false},
                                    {Flashcard.card_two_scratched.ToString(), false}
                            }}

                  } } };
        return initialdata;
    }

    public static Dictionary<string, object> CreateFlashcards(int[] cardNumbers, HashSet<int> _expired, int _level)
    {
        DateTime currentDT = ServerDateTime.GetFastestNISTDate();
        string formattedLevel = _level < 10 ? $"0{_level}" : $"{_level}";
        Dictionary<string, object> data = new Dictionary<string, object>() {

                     {Flashcard.level.ToString(), _level },
                     {Flashcard.levels.ToString(), new Dictionary<string, object>{
                        {formattedLevel, new Dictionary<string, object> {
                            {Flashcard.date.ToString(), currentDT.ToShortDateString()},
                            {Flashcard.played.ToString(), false },
                            {Flashcard.current_card_one.ToString(), cardNumbers[0]},
                            {Flashcard.card_one_scratched.ToString(), false},
                            {Flashcard.current_card_two.ToString(), cardNumbers[1]},
                            {Flashcard.card_two_scratched.ToString(), false},
                            {Flashcard.expired.ToString(), _expired}
                        }},

                       } } };

        return data;

    }
    public static Dictionary<string, object> CreateFlashcard(string _unit, string _buttonname, int _randomGeneratedNumber, HashSet<int> _expired, int tabnumber, int _level)
    {
        Dictionary<string, object> data;
        DateTime currentDT = ServerDateTime.GetFastestNISTDate();
        HashSet<int> _includeNewCardInExclude = new HashSet<int>();
        string formattedLevel = _level < 10 ? $"0{_level}" : $"{_level}";
        foreach (int item in _expired)
        {
            _includeNewCardInExclude.Add(item);
        }
        _includeNewCardInExclude.Add(_randomGeneratedNumber);
        //  int tabCardTwoGeneratedNumber = 0;
        if (tabnumber == -1)
        {
            // int tabCardTwoGeneratedNumber = Inference.GenerateRandomNumber(Params.TotalFlashCard, _expired);
            //   int tabCardTwoGeneratedNumber = Inference.GenerateRandomNumber(Params.TotalFlashCard, _includeNewCardInExclude);
            // _includeNewCardInExclude.Add(tabCardTwoGeneratedNumber);
            Dictionary<string, object> initialdata = new Dictionary<string, object>(){
                    {Flashcard.level.ToString(), 1},
                            {Flashcard.levels.ToString(), new Dictionary<string, object>{
                                { "01", new Dictionary<string, object> {
                                    {Flashcard.date.ToString(), currentDT.ToShortDateString()},
                                    {Flashcard.played.ToString(), false }
                    }}
                }},

                   // {Flashcard.date.ToString(), currentDT.ToShortDateString()},
                    /*{Flashcard.current_card_one.ToString(), _randomGeneratedNumber},
                    {Flashcard.current_card_two.ToString() , tabCardTwoGeneratedNumber},
                    {Flashcard.validity_card_one.ToString(), currentDT.ToShortDateString()},
                   {Flashcard.validity_card_two.ToString(), currentDT.ToShortDateString()},
                   {Flashcard.card_one_scratched.ToString(), false},
                    {Flashcard.card_two_scratched.ToString(), false}*/
                    };

            data = initialdata;
            Logger.LogInfo($"Intial card creation without the expired map field in FS ", "FirestoreData");
        }
        else
        {
            Dictionary<string, object> __tabcard;
            if (tabnumber == 1)
            {
                __tabcard = new Dictionary<string, object>() {
                     {Flashcard.level.ToString(), _level },
                     {Flashcard.levels.ToString(), new Dictionary<string, object>{
                                {formattedLevel, new Dictionary<string, object> {
                        {Flashcard.current_card_one.ToString(), _randomGeneratedNumber},
                         {Flashcard.card_one_scratched.ToString(), false},
                       // {Flashcard.validity_card_one.ToString(), currentDT.ToShortDateString()},
                        {Flashcard.expired.ToString(), _expired}
                       } } } } }; data = __tabcard;
                Logger.LogInfo($"Creating card for tab A...... ", "FirestoreData");
            }
            else
            {
                __tabcard = new Dictionary<string, object>() {
                     {Flashcard.levels.ToString(), new Dictionary<string, object>{
                                {formattedLevel, new Dictionary<string, object> {
                            {Flashcard.current_card_two.ToString(), _randomGeneratedNumber},
                             {Flashcard.card_two_scratched.ToString(), false},
                          //  {Flashcard.validity_card_two.ToString(), currentDT.ToShortDateString()},
                            {Flashcard.expired.ToString(), _expired}
                        }} } } }; data = __tabcard;
                Logger.LogInfo($"Creating card for tab B...... ", "FirestoreData"); ;
            }

        }

        Dictionary<string, object> returndata;// = new Dictionary<string, object>();

        /* if (_createtrivia)
         {
             returndata = new Dictionary<string, object>(){
         {_unit,  new Dictionary<string, object>(){
             {_buttonname, data},
             {FSMapField.trivia.ToString(), TriviaInitialQuizData(_buttonname,  currentDT)}
            }}};
         }
         else
         {*/

        returndata = new Dictionary<string, object>(){
        {_unit,  new Dictionary<string, object>(){
            {_buttonname, data} }

           }};
        /*}*/

        return returndata;

    }

    // --- MainGame data builders (used by GameLoader) ---

    /// <summary>
    /// Build initial maingame payload for a unit with first level unlocked.
    /// </summary>
    public static Dictionary<string, object> BuildMainGameInitialData(string unitKey, DateTime currentDate, int initialLife)
    {
        var formattedLevel = "01";
        var levelData = new Dictionary<string, object>
        {
            { MainGame.life.ToString(), initialLife },
            { MainGame.played.ToString(), false },
            { MainGame.powerup_health.ToString(), 0 },
            { MainGame.lesson_pass_collected.ToString(), false },
            { MainGame.flash_pass_collected.ToString(), false },
            { MainGame.flash_trivia_collected.ToString(), false }
        };

        var data = new Dictionary<string, object>
        {
            { FSMapField.progress_data.ToString(), new Dictionary<string, object>
                {
                    { ProgressData.level_completed.ToString(), 1 }
                }
            },
            { FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>
                {
                    { unitKey, new Dictionary<string, object>
                        {
                            { FSMapField.maingame.ToString(), new Dictionary<string, object>
                                {
                                    { MainGame.date.ToString(), currentDate.ToShortDateString() },
                                    { MainGame.level.ToString(), 1 },
                                    { MainGame.levels.ToString(), new Dictionary<string, object>
                                        {
                                            { FSMapField.locked.ToString(), new Dictionary<string, object>
                                                {
                                                    { formattedLevel, true }
                                                }
                                            },
                                            { formattedLevel, levelData }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        return data;
    }

    /// <summary>
    /// Build an update payload for advancing maingame to a new level.
    /// </summary>
    public static Dictionary<string, object> BuildMainGameNextLevelUpdate(string unitKey, int newLevel, DateTime currentDate, Dictionary<string, object> levelData)
    {
        var formattedNewLevel = newLevel < 10 ? $"0{newLevel}" : $"{newLevel}";
        var data = new Dictionary<string, object>
        {
            { FSMapField.progress_data.ToString(), new Dictionary<string, object>
                {
                    { ProgressData.level_completed.ToString(), newLevel }
                }
            },
            { FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>
                {
                    { unitKey, new Dictionary<string, object>
                        {
                            { FSMapField.maingame.ToString(), new Dictionary<string, object>
                                {
                                    { MainGame.date.ToString(), currentDate.ToShortDateString() },
                                    { MainGame.level.ToString(), newLevel },
                                    { MainGame.levels.ToString(), new Dictionary<string, object>
                                        {
                                            { FSMapField.locked.ToString(), new Dictionary<string, object>
                                                {
                                                    { formattedNewLevel, true }
                                                }
                                            },
                                            { formattedNewLevel, levelData }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        return data;
    }

    /// <summary>
    /// Build a payload to reset life for a specific maingame level.
    /// </summary>
    public static Dictionary<string, object> BuildMainGameLifeResetUpdate(string unitKey, string levelKey, int life)
    {
        return new Dictionary<string, object>
        {
            {
                FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>
                {
                    {
                        unitKey, new Dictionary<string, object>
                        {
                            {
                                FSMapField.maingame.ToString(), new Dictionary<string, object>
                                {
                                    {
                                        MainGame.levels.ToString(), new Dictionary<string, object>
                                        {
                                            {
                                                levelKey, new Dictionary<string, object>
                                                {
                                                    { MainGame.life.ToString(), life }
                                                }
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
    }

    /* public static Dictionary<string, object> CreateLesson(string _unit, string _buttonname)
     {
         Dictionary<string, object> lessons = new Dictionary<string, object>();
         return lessons;
     }*/
    public static Dictionary<string, object> ParentProfileData(
        string _parentName, string _country, string _currency, string _language, string _countryCode, string _pin, string _pinHint
    )
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {ParentProfile.parentname.ToString(), _parentName},
            {ParentProfile.country.ToString(), _country},
            {ParentProfile.currency.ToString(),_currency},
            {ParentProfile.language.ToString(), _language},
            {ParentProfile.countrycode.ToString(), _countryCode},
            {ParentProfile.pin.ToString(),_pin},
            {ParentProfile.pinhint.ToString(),_pinHint}
        };
        return data;
    }

    public static Dictionary<string, object> ChildProfileData(string _firstName, string _lastName, string _age, string _avatar, string _pinHint, string _pin, string _grade, string _gender, string _plan, string _screentime)
    {
        Dictionary<string, object> data = new Dictionary<string, object>(){
            {ChildProfile.firstname.ToString(), _firstName},
            {ChildProfile.lastname.ToString(), _lastName},
            {ChildProfile.age.ToString(), _age},
            {ChildProfile.avatar.ToString(),_avatar },
            {ChildProfile.pinhint.ToString(), _pinHint},
            {ChildProfile.pin.ToString(), _pin},
            {ChildProfile.grade.ToString(), _grade},
            {ChildProfile.gender.ToString(), _gender},
            {ChildProfile.plan.ToString(), _plan},
            {ChildProfile.screentime.ToString(), _screentime}
        };
        return data;
    }

    public static Dictionary<string, object> UnitStageButtonStatus(string _unit, bool _default)
    {
        //   DateTime currentDT = ServerDateTime.GetFastestNISTDate();
        Dictionary<string, object> data = new Dictionary<string, object>(){
    {

            _unit,  new Dictionary<string, object>(){
                {UnitButtonName.maingame.ToString(), _default},
                {UnitButtonName.lesson.ToString(), false},
                {UnitButtonName.flashcard.ToString(), false},
               {UnitButtonName.minigames.ToString(), false},
               {UnitButtonName.vocabs.ToString(), false},
               {UnitButtonName.calculator.ToString(), false},
               {UnitButtonName.video.ToString(), false},
               { UnitButtonName.storybook.ToString(), false},
               {UnitButtonName.worksheets.ToString(), false},
               {UnitButtonName.funfact.ToString(), false},
               {UnitButtonName.finsong.ToString(), false},
               {UnitButtonName.megaquiz.ToString(), false},
                                  }
               }

           };
        return data;
    }
    public static Dictionary<string, object> QuizQuestions(List<string> _questionNumbers)
    {
        var result = new Dictionary<string, object>();
        foreach (var number in _questionNumbers)
        {
            result.Add(number.ToString(), true);
            Console.WriteLine(number);
        }
        return result;
    }

    /// <summary>
    /// Build a payload that marks a specific trivia level as completed for a given unit/stage.
    /// Used by TriviaQuizBase when all questions for a level are answered correctly.
    /// </summary>
    public static Dictionary<string, object> BuildQuizLevelCompleted(string unitKey, string stageKey, int level)
    {
        return new Dictionary<string, object>
        {
            {
                FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>
                {
                    {
                        unitKey, new Dictionary<string, object>
                        {
                            {
                                FSMapField.quizzes.ToString(), new Dictionary<string, object>
                                {
                                    { stageKey, new Dictionary<string, object>
                                        {
                                            { level.ToString(), true }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    public static Dictionary<string, object> CreateTriviaQuizData(string _unit, string _stagename, List<string> questionNumbers, HashSet<int> pending, HashSet<int> attempted, int quizForLevel)
    {
        DateTime currentDT = ServerDateTime.GetFastestNISTDate();
        Dictionary<string, object> data = new Dictionary<string, object>(){{
     FSMapField.unit_stage_data.ToString(), new Dictionary<string, object>(){
        {
        _unit, new Dictionary<string, object>(){
            {
                FSMapField.trivia.ToString(), new Dictionary<string, object>(){
                        {
                    _stagename, new Dictionary<string, object>(){
                        {TriviaData.level.ToString(), quizForLevel},
                        {TriviaData.validity.ToString(),   currentDT.ToShortDateString()},
                        {TriviaData.pending.ToString(), pending },
                        {TriviaData.attempted.ToString(),attempted},
                        {TriviaData.question.ToString(), QuizQuestions(questionNumbers)
                    }
                }
            }}
        }}
    }}
},
                        {FSMapField.points_score.ToString(),  new Dictionary<string, object>(){
                            {HUD.view.ToString(), PointSystem.View+1}
                        }
            }};
        return data;
    }

    public static Dictionary<string, object> TriviaInitialQuizData(string _stagename, DateTime currentDT)
    {
        List<string> questionNumbers = new List<string>();
        HashSet<int> attempted = new HashSet<int>();
        int generatedRandomNumber;
        for (int i = 0; i < Params.NumberofQuizPerDay; i++)
        {
            generatedRandomNumber = Inference.GenerateRandomNumber(Params.TriviaQuizQuestionsCount, attempted);
            // Debug.Log($"Number generated for triviaquiz is {generatedRandomNumber}");
            attempted.Add(generatedRandomNumber);
            questionNumbers.Add(generatedRandomNumber.ToString());

        }
        Dictionary<string, object> data = new Dictionary<string, object>(){
                       {
                    _stagename, new Dictionary<string, object>(){
                        {TriviaData.level.ToString(),1},
                        {TriviaData.validity.ToString(),  currentDT.ToShortDateString()},
                        {TriviaData.attempted.ToString(), attempted},
                        {TriviaData.question.ToString(), QuizQuestions(questionNumbers)
                    }}}};
        return data;
    }

    public static Dictionary<string, object> CreateInitialScorePoints(int _xp, int _coins, int _stars, int _view, int _visit, int _passes)
    {
        Dictionary<string, object> data = new Dictionary<string, object>(){
                {HUD.xp.ToString(), _xp},
                {HUD.coins.ToString(), _coins},
                {HUD.stars.ToString(), _stars},
                {HUD.view.ToString(), _view},
                {HUD.visit.ToString(), _visit},
                {MainGame.passes.ToString(), _passes}


        };

        return data;
    }
    public static Dictionary<string, object> CreateProgess(string _current_unit, string _current_unit_name, string _current_stage_name, DateTime _next_unit_date, string _rank, int _level_completed)
    {
        Dictionary<string, object> data = new Dictionary<string, object>(){
                 { ProgressData.current_unit.ToString(), _current_unit },
                                    { ProgressData.current_unit_name.ToString(), _current_unit_name },
                                    { ProgressData.current_stage_name.ToString(), _current_stage_name },
                                    { ProgressData.next_unit_date.ToString(), _next_unit_date.AddDays(1).ToShortDateString() },
                                    { ProgressData.rank.ToString(),  _rank.ToString() },
                                    { ProgressData.level_completed.ToString(), _level_completed }      };
        return data;
    }

    public static Dictionary<string, object> UpdatePointScore(ScorePoint on)
    {
        int _xp = 0;
        int _coins = 0;
        int _stars = 0;
        int _view = 0;
        int _visit = 0;

        switch (on)
        {
            case ScorePoint.LESSON:
                _xp = 5;
                _coins = 10;
                _stars = 10;
                _view = 0;
                _visit = 0;
                break;
            case ScorePoint.MAINGAME:
                _xp = 10;
                _coins = 10;
                _stars = 10;
                _view = 0;
                _visit = 0;
                break;
            case ScorePoint.SCRATCHCARD:
                _xp = 15;
                _coins = 15;
                _stars = 15;
                _view = 1;
                _visit = 0;
                break;
            case ScorePoint.TRIVIA:
                _xp = 10;
                _coins = 10;
                _stars = 10;
                _view = 0;
                _visit = 0;
                break;
            case ScorePoint.MINIGAME:
                _xp = 25;
                _coins = 20;
                _stars = 20;
                _view = 0;
                _visit = 0;
                break;
            case ScorePoint.VIDEOS:
                _xp = 30;
                _coins = 25;
                _stars = 25;
                _view = 1;
                _visit = 1;
                break;
            case ScorePoint.VOCABS:
                _xp = 5;
                _coins = 10;
                _stars = 10;
                _view = 0;
                _visit = 0;
                break;
            case ScorePoint.STORYBOOK:
                _xp = 50;
                _coins = 55;
                _stars = 55;
                _view = 1;
                _visit = 1;
                break;
            case ScorePoint.CALCULATOR:
                _xp = 60;
                _coins = 60;
                _stars = 60;
                _view = 1;
                _visit = 1;
                break;
            case ScorePoint.WORKSHEET:
                _xp = 75;
                _coins = 75;
                _stars = 75;
                _view = 1;
                _visit = 1;
                break;
            case ScorePoint.FINSONG:
                _xp = 90;
                _coins = 85;
                _stars = 85;
                _view = 0;
                _visit = 0;
                break;
            case ScorePoint.MEGAQUIZ:
                _xp = 100;
                _coins = 100;
                _stars = 100;
                _view = 5;
                _visit = 5;
                break;


        }
        PointSystem.XP += _xp;
        PointSystem.Coins += _coins;
        PointSystem.Stars += _stars;
        PointSystem.View += _view;
        PointSystem.Visit += _visit;
        Dictionary<string, object> _data = new Dictionary<string, object>(){
                {FSMapField.points_score.ToString(), new Dictionary<string, object>(){
                {HUD.xp.ToString(), PointSystem.XP},
                {HUD.coins.ToString(), PointSystem.Coins},
                {HUD.stars.ToString(), PointSystem.Stars},
                {HUD.view.ToString(), PointSystem.View},
                {HUD.visit.ToString(), PointSystem.Visit}
                }
            }
        };
        return _data;

    }

    public static Dictionary<string, object> DebitPointScore(ScorePoint on, int points)
    {

        int _coins = 0;


        switch (on)
        {

            case ScorePoint.VOCABS:
                _coins = points;


                break;
            default:

                break;


        }
        PointSystem.Coins -= _coins;

        Dictionary<string, object> _data = new Dictionary<string, object>(){
                {FSMapField.points_score.ToString(), new Dictionary<string, object>(){
                {HUD.coins.ToString(), PointSystem.Coins},

                }
            }
        };
        return _data;

    }


    public static Dictionary<string, object> DefaultQuizzes()
    {
        Dictionary<string, object> data = new Dictionary<string, object>() {
                    {Quizzes.flashcard.ToString(), new Dictionary<string, object>() {
                    {"1", false}}},
                    {Quizzes.minigames.ToString(), new Dictionary<string, object>() {
                    {"1", false}}},
                    {Quizzes.vocabs.ToString(), new Dictionary<string, object>() {
                    {"1", false}}},
                    { Quizzes.calculator.ToString(), new Dictionary<string, object>() {
                    {"1", false}}},
                    { Quizzes.video.ToString(), new Dictionary<string, object>() {
                    {"1", false}}},
                    {Quizzes.storybook.ToString(), new Dictionary<string, object>() {
                    {"1", false}}},
                    {Quizzes.worksheets.ToString(), new Dictionary<string, object>() {
                    {"1", false}}},
                    {Quizzes.finsong.ToString(), new Dictionary<string, object>() {
                    {"1", false}}},
                    {Quizzes.funfact.ToString(), new Dictionary<string, object>() {
                    {"1", false}}},
                    };
        return data;
    }
}