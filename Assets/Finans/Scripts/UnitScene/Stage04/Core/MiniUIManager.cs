using UnityEngine;
using UnityEngine.UI;

namespace FinansGames.UI
{
    public class MiniUIManager : MonoBehaviour
    {
        public static MiniUIManager Instance { get; private set; }

        [SerializeField] private Text scoreText;
        [SerializeField] private Text timerText;
        [SerializeField] private GameObject winScreen;
        [SerializeField] private GameObject loseScreen;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void UpdateScore(int score)
        {
            scoreText.text = "Score: " + score;
        }

        public void UpdateTimer(float time)
        {
            timerText.text = "Time: " + time.ToString("F2");
        }

        public void ShowWinScreen() => winScreen.SetActive(true);
        public void ShowLoseScreen() => loseScreen.SetActive(true);
    }
}
