using UnityEngine;
using TMPro;

namespace Game.Categories.NumberKeys
{
	public sealed class NumberBubbleTextView : MonoBehaviour
	{
		[SerializeField] private TMP_Text label;
		[SerializeField] private NumberBubble target;

		private int lastShownDigit = -1;

		private void Reset()
		{
			if (target == null) target = GetComponentInParent<NumberBubble>();
			if (label == null) label = GetComponent<TMP_Text>();
		}

		private void OnEnable()
		{
			Refresh(true);
		}

		private void Update()
		{
			if (target == null || label == null) return;
			if (target.digit != lastShownDigit)
			{
				Refresh(false);
			}
		}

		private void OnValidate()
		{
			Refresh(true);
		}

		public void Refresh(bool force)
		{
			if (target == null || label == null) return;
			var clamped = Mathf.Clamp(target.digit, 0, 9);
			if (force || clamped != lastShownDigit)
			{
				lastShownDigit = clamped;
				label.text = clamped.ToString();
			}
		}
	}
}


