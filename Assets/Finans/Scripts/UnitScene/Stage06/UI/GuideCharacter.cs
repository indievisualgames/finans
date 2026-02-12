using UnityEngine;
using Game.UI;

namespace Game.UI
{
    public sealed class GuideCharacter : MonoBehaviour
    {
        [SerializeField] private SpeechBubbleView speechBubble;
        [SerializeField] private Animator animator; // optional

        public void ShowPrompt(string message)
        {
            speechBubble?.SetMessage(message);
            if (animator != null)
            {
                animator.SetTrigger("Prompt");
            }
        }

        public void ShowSuccess(string message)
        {
            speechBubble?.SetMessage(message);
            if (animator != null)
            {
                animator.SetTrigger("Success");
            }
        }

		public void PlayCompletionBubbleEffect()
		{
			speechBubble?.PlayCompletionEffect();
		}

        public void ShowError(string message)
        {
            speechBubble?.SetMessage(message);
            if (animator != null)
            {
                animator.SetTrigger("Error");
            }
        }
    }
}


