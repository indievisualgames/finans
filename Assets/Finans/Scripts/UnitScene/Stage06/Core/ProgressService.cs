using UnityEngine;

namespace Game.Core
{
    public static class ProgressService
    {
        private const string LastUnlockedKey = "progress.lastUnlocked";

        public static int GetLastUnlocked()
        {
            return PlayerPrefs.GetInt(LastUnlockedKey, 0);
        }

        public static void SetLastUnlocked(int categoryIndex)
        {
            PlayerPrefs.SetInt(LastUnlockedKey, categoryIndex);
            PlayerPrefs.Save();
        }

        public static int GetBestScore(string categoryKey)
        {
            return PlayerPrefs.GetInt($"category.{categoryKey}.best", 0);
        }

        public static void SetBestScore(string categoryKey, int score)
        {
            var best = GetBestScore(categoryKey);
            if (score > best)
            {
                PlayerPrefs.SetInt($"category.{categoryKey}.best", score);
                PlayerPrefs.Save();
            }
        }
    }
}


