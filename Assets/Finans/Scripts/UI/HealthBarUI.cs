using UnityEngine;
using UnityEngine.UI;
using MagicPigGames;

/// <summary>
/// UI Controller for Health Progress Bar.
/// 
/// LOGIC:
/// - Listens to PlayerHealth.OnHealthChanged event
/// - Updates progress bar to reflect current health (0-1 range)
/// - Progress bar uses invertProgress = true (bar shrinks as health decreases)
/// - Supports HorizontalProgressBar, Slider, or Image as fallbacks
/// </summary>
public class HealthBarUI : MonoBehaviour
{
	[SerializeField]
	private HorizontalProgressBar healthBar; // Primary progress bar component

	// Fallbacks if HorizontalProgressBar is not present in the scene
	[SerializeField]
	private Slider sliderBar; // Fallback UI element

	[SerializeField]
	private Image fillImage; // Fallback UI element

	[SerializeField]
	private PlayerHealth playerHealth; // Reference to health system

	private void Awake()
	{
		if (playerHealth == null)
		{
			playerHealth = FindFirstObjectByType<PlayerHealth>();
		}

		// Auto-bind the health bar if not assigned in the inspector
		if (healthBar == null)
		{
			healthBar = GetComponent<HorizontalProgressBar>();
			if (healthBar == null)
			{
				healthBar = GetComponentInChildren<HorizontalProgressBar>(true);
			}
		}

		// Auto-bind fallbacks if needed
		if (healthBar == null)
		{
			sliderBar = sliderBar != null ? sliderBar : GetComponentInChildren<Slider>(true);
			if (sliderBar != null)
			{
				sliderBar.minValue = 0f;
				sliderBar.maxValue = 1f;
			}

			if (fillImage == null)
			{
				fillImage = GetComponentInChildren<Image>(true);
			}
		}

		// Configure overlay to represent missing health (right-anchored, invert on)
		if (healthBar != null)
		{
			if (healthBar.rectTransform == null)
			{
				healthBar.rectTransform = healthBar.GetComponent<RectTransform>();
			}
			healthBar.invertProgress = true;
			// Ensure overlayBar is assigned; if not, try to pick a child RectTransform
			if (healthBar.overlayBar == null)
			{
				var allRects = healthBar.GetComponentsInChildren<RectTransform>(true);
				foreach (var rt in allRects)
				{
					if (rt != healthBar.rectTransform)
					{
						healthBar.overlayBar = rt;
						break;
					}
				}
			}
			if (healthBar.overlayBar != null)
			{
				var overlay = healthBar.overlayBar;
				overlay.anchorMin = new Vector2(1f, 0f);
				overlay.anchorMax = new Vector2(1f, 1f);
				overlay.pivot = new Vector2(1f, 0.5f);
			}
		}
	}

	private void OnEnable()
	{
		if (playerHealth != null)
		{
			playerHealth.OnHealthChanged += HandleHealthChanged;
			// Sync immediately in case event already fired
			HandleHealthChanged(playerHealth.HealthPercent);
		}
	}

	private void OnDisable()
	{
		if (playerHealth != null)
		{
			playerHealth.OnHealthChanged -= HandleHealthChanged;
		}
	}

	/// <summary>
	/// PROGRESS BAR UPDATE: Called when health changes.
	/// - Receives health percentage (0-1 range)
	/// - Updates progress bar to match health
	/// - Progress bar is inverted (shows missing health as overlay)
	/// - Also triggers warning system at 50% threshold (handled in ProgressBar.cs)
	/// </summary>
	private void HandleHealthChanged(float percent)
	{
		var clamped = Mathf.Clamp01(percent);
		if (healthBar != null)
		{
			healthBar.SetProgress(clamped);
			return;
		}

		if (sliderBar != null)
		{
			sliderBar.value = clamped;
			return;
		}

		if (fillImage != null)
		{
			fillImage.fillAmount = clamped;
		}
	}
}


