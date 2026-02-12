using UnityEngine;

namespace FinansGames.Core
{
    public abstract class MiniGameManager : MonoBehaviour
    {
        public static MiniGameManager Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public abstract void StartGame();
        public abstract void EndGame(bool isWin);
    }
}
