using Game.Core;
using UnityEngine;

namespace Game.Categories.OperatorMatch
{
    /// <summary>
    /// Hooks Operator Match category into GameFlowController via ICategoryController.
    /// </summary>
    public sealed class OperatorMatchCategoryController : MonoBehaviour, ICategoryController
    {
        [SerializeField] private OperatorMatchLevelController levelController;

        private void Awake()
        {
            if (levelController == null)
            {
                levelController = GetComponentInChildren<OperatorMatchLevelController>(true);
                if (levelController == null)
                {
                    levelController = GetComponentInParent<OperatorMatchLevelController>();
                }
            }
        }

        public void Initialize(ICalculatorInputSource inputSource)
        {
            if (levelController == null)
            {
                Awake();
                if (levelController == null)
                {
                    Debug.LogError("OperatorMatchCategoryController needs an OperatorMatchLevelController reference.");
                    enabled = false;
                    return;
                }
            }

            levelController.Initialize(inputSource);
        }

        public void StartCategory()
        {
            levelController?.StartLevel();
        }
    }
}


