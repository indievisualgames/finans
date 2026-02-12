using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace Game.UI
{
    public sealed class SpeechBubbleView : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image background;
        [SerializeField] private SpeechBubbleSettings settings;
        [SerializeField] private bool applySettingsOnAwake = true;
        [SerializeField] private bool applySettingsOnValidate = true;
		
		// Optional UI completion effect (kept inactive in scene). Will be SetActive(true) briefly on success.
		[SerializeField] private GameObject completionEffect;
		[SerializeField] private float completionEffectDuration = 1.5f;
		private Coroutine completionRoutine;

        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }

		private void Awake()
		{
			if (applySettingsOnAwake)
			{
				ApplySettings();
			}
		}

		private void OnValidate()
		{
			if (applySettingsOnValidate && Application.isEditor && !Application.isPlaying)
			{
				ApplySettings();
			}
		}

		public void SetSettings(SpeechBubbleSettings newSettings)
		{
			settings = newSettings;
			ApplySettings();
		}

		public void ApplySettings()
		{
			if (settings == null) return;
			if (messageText != null)
			{
				messageText.color = settings.TextColor;
				messageText.fontSize = settings.FontSize;
				messageText.alignment = settings.Alignment;
			}
			if (background != null)
			{
				background.color = settings.BackgroundColor;
			}
		}

		public void PlayCompletionEffect()
		{
			if (completionEffect == null) return;
			if (completionRoutine != null)
			{
				StopCoroutine(completionRoutine);
				completionRoutine = null;
			}
			completionEffect.SetActive(true);
			if (completionEffectDuration > 0f && isActiveAndEnabled)
			{
				completionRoutine = StartCoroutine(DisableAfterDelay(completionEffectDuration));
			}
		}

		private IEnumerator DisableAfterDelay(float delay)
		{
			yield return new WaitForSeconds(delay);
			if (completionEffect != null)
			{
				completionEffect.SetActive(false);
			}
			completionRoutine = null;
		}

		private void OnDisable()
		{
			if (completionRoutine != null)
			{
				StopCoroutine(completionRoutine);
				completionRoutine = null;
			}
			if (completionEffect != null)
			{
				completionEffect.SetActive(false);
			}
		}
    }
}


