using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static IFirestoreEnums;
using System;
public class Inference : MonoBehaviour
{
    public static int GenerateRandomNumber(int _count, HashSet<int> _exclude)
    {
        foreach (var item in _exclude)
        {
            Logger.LogInfo($"Found int {item} as exclude numers", "Inference");
        }
        int available = _count - _exclude.Count;
        if (available <= 0)
        {
            Logger.LogWarning("No available numbers to generate (exclude covers all). Returning 0.", "Inference");
            return 0;
        }
        var range = Enumerable.Range(0, _count).Where(i => !_exclude.Contains(i));

        var rand = new System.Random();
        int index = rand.Next(0, available);
        return range.ElementAt(index);
    }

    public static GameObject OpenPopup(Canvas _m_canvas, GameObject _messageBoxPopupPrefab)
    {
        GameObject _popup = Instantiate(_messageBoxPopupPrefab) as GameObject;
        _popup.SetActive(true);
        _popup.transform.localScale = Vector3.zero;
        _popup.transform.SetParent(_m_canvas.transform, false);
        _popup.GetComponent<Ricimi.Popup>().Open(); return _popup;
    }

    public static void SetPlayerInfoForPCollected(Dictionary<string, object> __gameLevelData)
    {
        if (__gameLevelData.ContainsKey(MainGame.lesson_pass_collected.ToString()))
        {
            PlayerInfo.LessonPassCollected = Convert.ToBoolean(__gameLevelData[MainGame.lesson_pass_collected.ToString()]);
        }
        if (__gameLevelData.ContainsKey(MainGame.flash_pass_collected.ToString()))
        {
            PlayerInfo.FlashPassCollected = Convert.ToBoolean(__gameLevelData[MainGame.flash_pass_collected.ToString()]);
        }
        if (__gameLevelData.ContainsKey(MainGame.flash_trivia_collected.ToString()))
        {
            PlayerInfo.FlashTriviaCollected = Convert.ToBoolean(__gameLevelData[MainGame.flash_trivia_collected.ToString()]);
        }
        if (__gameLevelData.ContainsKey(MainGame.minigames_pass_collected.ToString()))
        {
            PlayerInfo.MiniGamesPassCollected = Convert.ToBoolean(__gameLevelData[MainGame.minigames_pass_collected.ToString()]);
        }
        if (__gameLevelData.ContainsKey(MainGame.minigames_trivia_collected.ToString()))
        {
            PlayerInfo.MiniGamesTriviaCollected = Convert.ToBoolean(__gameLevelData[MainGame.minigames_trivia_collected.ToString()]);
        }

        if (__gameLevelData.ContainsKey(MainGame.vocabs_pass_collected.ToString()))
        {
            PlayerInfo.VocabsPassCollected = Convert.ToBoolean(__gameLevelData[MainGame.vocabs_pass_collected.ToString()]);
        }
        if (__gameLevelData.ContainsKey(MainGame.vocabs_trivia_collected.ToString()))
        {
            PlayerInfo.VocabsTriviaCollected = Convert.ToBoolean(__gameLevelData[MainGame.vocabs_trivia_collected.ToString()]);
        }

        if (__gameLevelData.ContainsKey(MainGame.calculator_pass_collected.ToString()))
        {
            PlayerInfo.CalcPassCollected = Convert.ToBoolean(__gameLevelData[MainGame.calculator_pass_collected.ToString()]);
        }
        if (__gameLevelData.ContainsKey(MainGame.calculator_trivia_collected.ToString()))
        {
            PlayerInfo.CalcTriviaCollected = Convert.ToBoolean(__gameLevelData[MainGame.calculator_trivia_collected.ToString()]);
        }
        if (__gameLevelData.ContainsKey(MainGame.video_pass_collected.ToString()))
        {
            PlayerInfo.VideoPassCollected = Convert.ToBoolean(__gameLevelData[MainGame.video_pass_collected.ToString()]);
        }
        if (__gameLevelData.ContainsKey(MainGame.video_trivia_collected.ToString()))
        {
            PlayerInfo.VideoTriviaCollected = Convert.ToBoolean(__gameLevelData[MainGame.video_trivia_collected.ToString()]);
        }


    }


}
