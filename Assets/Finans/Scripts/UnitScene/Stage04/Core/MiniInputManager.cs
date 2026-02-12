using UnityEngine;

namespace FinansGames.Inputs
{
    public class MiniInputManager : MonoBehaviour
    {
        public static MiniInputManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public bool IsTouching()
        {
            return Input.GetMouseButton(0) || Input.touchCount > 0;
        }
    }
}
