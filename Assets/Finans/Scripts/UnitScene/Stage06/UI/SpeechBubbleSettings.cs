using UnityEngine;
using TMPro;

namespace Game.UI
{
	[CreateAssetMenu(menuName = "Game/UI/Speech Bubble Settings", fileName = "SpeechBubbleSettings")]
	public sealed class SpeechBubbleSettings : ScriptableObject
	{
		[SerializeField] private Color backgroundColor = new Color(1f, 1f, 1f, 0.6f);
		[SerializeField] private Color textColor = Color.black;
		[SerializeField] private float fontSize = 28f;
		[SerializeField] private TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft;

		public Color BackgroundColor => backgroundColor;
		public Color TextColor => textColor;
		public float FontSize => fontSize;
		public TextAlignmentOptions Alignment => alignment;
	}
}


