using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Calculator scene helper: when the user clicks "Question_button", show an auto-hiding instruction popup.
/// This is wired at runtime to avoid fragile prefab-instance scene YAML edits.
/// </summary>
public static class CalculatorQuestionButtonInstructions
{
	private const string CalculatorSceneNameLower = "calculator";
	private const string QuestionButtonObjectName = "Question_button";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Init()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (!IsCalculatorScene(scene)) return;

		var questionGo = GameObject.Find(QuestionButtonObjectName);
		if (questionGo == null) return;

		var btn = questionGo.GetComponent<Button>() ?? questionGo.GetComponentInChildren<Button>(true);
		if (btn == null) return;

		var handler = questionGo.GetComponent<CalculatorQuestionButtonInstructionsHandler>();
		if (handler == null)
		{
			handler = questionGo.AddComponent<CalculatorQuestionButtonInstructionsHandler>();
		}

		handler.Bind(btn);
	}

	private static bool IsCalculatorScene(Scene scene)
	{
		if (!scene.IsValid()) return false;
		if (!scene.isLoaded) return false;
		return string.Equals(scene.name, "Calculator", System.StringComparison.OrdinalIgnoreCase)
		       || string.Equals(scene.name, CalculatorSceneNameLower, System.StringComparison.OrdinalIgnoreCase);
	}
}

public sealed class CalculatorQuestionButtonInstructionsHandler : MonoBehaviour
{
	[SerializeField] private float autoCloseSeconds = 6f;

	private const string PopupRootName = "Calculator_InstructionsPopup";
	private const string PopupTextName = "InstructionsText";

	private Button boundButton;
	private GameObject popupRoot;
	private TMP_Text popupText;
	private Coroutine autoCloseRoutine;

	private static readonly string Message =
		"Find the flashing bubble and click the number on calculator keypad\n" +
		"The bubble can hide behind the calculator, you can move calculator grabbing the move icon or calculator panel";

	public void Bind(Button button)
	{
		if (button == null) return;

		if (boundButton == button) return;

		if (boundButton != null)
		{
			boundButton.onClick.RemoveListener(Show);
		}

		boundButton = button;
		boundButton.onClick.AddListener(Show);
	}

	private void OnDisable()
	{
		if (boundButton != null)
		{
			boundButton.onClick.RemoveListener(Show);
		}
	}

	private void Show()
	{
		EnsurePopupUI();
		if (popupRoot == null || popupText == null) return;

		popupText.text = Message;
		popupRoot.SetActive(true);

		if (autoCloseRoutine != null) StopCoroutine(autoCloseRoutine);
		autoCloseRoutine = StartCoroutine(AutoCloseAfterDelay());
	}

	private IEnumerator AutoCloseAfterDelay()
	{
		var seconds = Mathf.Max(0.5f, autoCloseSeconds);
		yield return new WaitForSecondsRealtime(seconds);

		if (popupRoot != null)
		{
			popupRoot.SetActive(false);
		}
		autoCloseRoutine = null;
	}

	private void EnsurePopupUI()
	{
		if (popupRoot != null && popupText != null) return;

		// Reuse existing popup if a designer already created one.
		var existing = GameObject.Find(PopupRootName);
		if (existing != null)
		{
			popupRoot = existing;
			popupText = popupRoot.GetComponentInChildren<TMP_Text>(true);
			return;
		}

		var canvas = FindFirstObjectByType<Canvas>();
		if (canvas == null) return;

		// Root panel
		popupRoot = new GameObject(PopupRootName, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
		popupRoot.transform.SetParent(canvas.transform, false);

		var cg = popupRoot.GetComponent<CanvasGroup>();
		cg.alpha = 1f;
		cg.interactable = false;
		cg.blocksRaycasts = false;

		var bg = popupRoot.GetComponent<Image>();
		bg.color = new Color(0f, 0f, 0f, 0.75f);

		var rt = (RectTransform)popupRoot.transform;
		rt.anchorMin = new Vector2(0.5f, 0.5f);
		rt.anchorMax = new Vector2(0.5f, 0.5f);
		rt.pivot = new Vector2(0.5f, 0.5f);
		rt.anchoredPosition = Vector2.zero;
		rt.sizeDelta = new Vector2(900f, 220f);

		// Text
		var textGo = new GameObject(PopupTextName, typeof(RectTransform), typeof(TextMeshProUGUI));
		textGo.transform.SetParent(popupRoot.transform, false);

		var textRt = (RectTransform)textGo.transform;
		textRt.anchorMin = Vector2.zero;
		textRt.anchorMax = Vector2.one;
		textRt.pivot = new Vector2(0.5f, 0.5f);
		textRt.offsetMin = new Vector2(24f, 18f);
		textRt.offsetMax = new Vector2(-24f, -18f);

		popupText = textGo.GetComponent<TextMeshProUGUI>();
		popupText.text = Message;
		popupText.color = Color.white;
		popupText.fontSize = 32f;
		popupText.enableWordWrapping = true;
		popupText.alignment = TextAlignmentOptions.Center;
		popupText.raycastTarget = false;

		popupRoot.SetActive(false);
	}
}


