using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CalculatorController : MonoBehaviour
{
	[Header("Display")]
	[SerializeField] private TextMeshProUGUI displayText;
	[SerializeField] private TextMeshProUGUI memoryIndicatorText;

	[Header("Number Buttons")]
	[SerializeField] private Button button0;
	[SerializeField] private Button button1;
	[SerializeField] private Button button2;
	[SerializeField] private Button button3;
	[SerializeField] private Button button4;
	[SerializeField] private Button button5;
	[SerializeField] private Button button6;
	[SerializeField] private Button button7;
	[SerializeField] private Button button8;
	[SerializeField] private Button button9;

	[Header("Operator Buttons")]
	[SerializeField] private Button buttonAdd;
	[SerializeField] private Button buttonSubtract;
	[SerializeField] private Button buttonMultiply;
	[SerializeField] private Button buttonDivide;
	[SerializeField] private Button buttonEquals;
	[SerializeField] private Button buttonDecimal;
	[SerializeField] private Button buttonPercent;

	[Header("Control / Memory Buttons")]
	[SerializeField] private Button buttonClearAll;
	[SerializeField] private Button buttonClearEntry;
	[SerializeField] private Button buttonBackspace;
	[SerializeField] private Button buttonMemoryAdd;
	[SerializeField] private Button buttonMemorySubtract;
	[SerializeField] private Button buttonMemoryRecall;
	[SerializeField] private Button buttonMemoryClear;

	[Header("Highlighting")]
	[SerializeField] private Color highlightColor = new Color(1f, 0.9f, 0.2f);
	[SerializeField] private Color normalColor = Color.white;

	public event Action<CalcButtonKind> OnButtonInvoked;
	public event Action<string> OnDisplayChanged;
	public event Action OnEqualsExecuted;

	public CalculatorModel Model { get; private set; } = new CalculatorModel();

	private readonly Dictionary<ButtonFocusType, Button> focusMap = new Dictionary<ButtonFocusType, Button>();
	private readonly List<Button> allButtons = new List<Button>();

	public string CurrentDisplay => displayText != null ? displayText.text : Model.DisplayText;

	private void Awake()
	{
		EnsureUiInputSubsystem();
		Model.OnDisplayChanged += HandleModelDisplayChanged;
		BuildMaps();
		WireButtons();
		RefreshDisplay();
		UpdateMemoryIndicator();
	}

	private void OnDestroy()
	{
		Model.OnDisplayChanged -= HandleModelDisplayChanged;
	}

	private void BuildMaps()
	{
		focusMap.Clear();
		focusMap[ButtonFocusType.Number0] = button0;
		focusMap[ButtonFocusType.Number1] = button1;
		focusMap[ButtonFocusType.Number2] = button2;
		focusMap[ButtonFocusType.Number3] = button3;
		focusMap[ButtonFocusType.Number4] = button4;
		focusMap[ButtonFocusType.Number5] = button5;
		focusMap[ButtonFocusType.Number6] = button6;
		focusMap[ButtonFocusType.Number7] = button7;
		focusMap[ButtonFocusType.Number8] = button8;
		focusMap[ButtonFocusType.Number9] = button9;
		focusMap[ButtonFocusType.Add] = buttonAdd;
		focusMap[ButtonFocusType.Subtract] = buttonSubtract;
		focusMap[ButtonFocusType.Multiply] = buttonMultiply;
		focusMap[ButtonFocusType.Divide] = buttonDivide;
		focusMap[ButtonFocusType.Decimal] = buttonDecimal;
		focusMap[ButtonFocusType.Percent] = buttonPercent;
		focusMap[ButtonFocusType.Clear] = buttonClearAll != null ? buttonClearAll : buttonClearEntry;
		focusMap[ButtonFocusType.Backspace] = buttonBackspace;
		focusMap[ButtonFocusType.MemoryAdd] = buttonMemoryAdd;
		focusMap[ButtonFocusType.MemorySubtract] = buttonMemorySubtract;
		focusMap[ButtonFocusType.MemoryRecall] = buttonMemoryRecall;
		focusMap[ButtonFocusType.MemoryClear] = buttonMemoryClear;

		allButtons.Clear();
		allButtons.AddRange(new[] {
			button0, button1, button2, button3, button4, button5, button6, button7, button8, button9,
			buttonAdd, buttonSubtract, buttonMultiply, buttonDivide, buttonEquals, buttonDecimal, buttonPercent,
			buttonClearAll, buttonClearEntry, buttonBackspace, buttonMemoryAdd, buttonMemorySubtract, buttonMemoryRecall, buttonMemoryClear
		});
	}

	private void WireButtons()
	{
		if (button0) button0.onClick.AddListener(() => InvokeNumber(0));
		if (button1) button1.onClick.AddListener(() => InvokeNumber(1));
		if (button2) button2.onClick.AddListener(() => InvokeNumber(2));
		if (button3) button3.onClick.AddListener(() => InvokeNumber(3));
		if (button4) button4.onClick.AddListener(() => InvokeNumber(4));
		if (button5) button5.onClick.AddListener(() => InvokeNumber(5));
		if (button6) button6.onClick.AddListener(() => InvokeNumber(6));
		if (button7) button7.onClick.AddListener(() => InvokeNumber(7));
		if (button8) button8.onClick.AddListener(() => InvokeNumber(8));
		if (button9) button9.onClick.AddListener(() => InvokeNumber(9));

		if (buttonDecimal) buttonDecimal.onClick.AddListener(() => { Model.InputDecimal(); OnButtonInvoked?.Invoke(CalcButtonKind.Decimal); });
		if (buttonAdd) buttonAdd.onClick.AddListener(() => { Model.SetOperator(CalculatorOperator.Add); OnButtonInvoked?.Invoke(CalcButtonKind.Add); });
		if (buttonSubtract) buttonSubtract.onClick.AddListener(() => { Model.SetOperator(CalculatorOperator.Subtract); OnButtonInvoked?.Invoke(CalcButtonKind.Subtract); });
		if (buttonMultiply) buttonMultiply.onClick.AddListener(() => { Model.SetOperator(CalculatorOperator.Multiply); OnButtonInvoked?.Invoke(CalcButtonKind.Multiply); });
		if (buttonDivide) buttonDivide.onClick.AddListener(() => { Model.SetOperator(CalculatorOperator.Divide); OnButtonInvoked?.Invoke(CalcButtonKind.Divide); });
		if (buttonPercent) buttonPercent.onClick.AddListener(() => { Model.PressPercent(); OnButtonInvoked?.Invoke(CalcButtonKind.Percent); });
		if (buttonEquals) buttonEquals.onClick.AddListener(() => { Model.PressEquals(); OnButtonInvoked?.Invoke(CalcButtonKind.Equals); OnEqualsExecuted?.Invoke(); });

		if (buttonClearAll) buttonClearAll.onClick.AddListener(() => { Model.ResetAll(); OnButtonInvoked?.Invoke(CalcButtonKind.ClearAll); });
		if (buttonClearEntry) buttonClearEntry.onClick.AddListener(() => { Model.ClearEntry(); OnButtonInvoked?.Invoke(CalcButtonKind.ClearEntry); });
		if (buttonBackspace) buttonBackspace.onClick.AddListener(() => { Model.Backspace(); OnButtonInvoked?.Invoke(CalcButtonKind.Backspace); });
		if (buttonMemoryAdd) buttonMemoryAdd.onClick.AddListener(() => { Model.MemoryAdd(); UpdateMemoryIndicator(); OnButtonInvoked?.Invoke(CalcButtonKind.MemoryAdd); });
		if (buttonMemorySubtract) buttonMemorySubtract.onClick.AddListener(() => { Model.MemorySubtract(); UpdateMemoryIndicator(); OnButtonInvoked?.Invoke(CalcButtonKind.MemorySubtract); });
		if (buttonMemoryClear) buttonMemoryClear.onClick.AddListener(() => { Model.MemoryClear(); UpdateMemoryIndicator(); OnButtonInvoked?.Invoke(CalcButtonKind.MemoryClear); });
		if (buttonMemoryRecall) buttonMemoryRecall.onClick.AddListener(() => { Model.MemoryRecall(); OnButtonInvoked?.Invoke(CalcButtonKind.MemoryRecall); });
	}

	private void InvokeNumber(int n)
	{
		Model.InputDigit(n);
		var kind = (CalcButtonKind)Enum.Parse(typeof(CalcButtonKind), $"Number{n}");
		OnButtonInvoked?.Invoke(kind);
	}

	private void HandleModelDisplayChanged(string text)
	{
		RefreshDisplay();
	}

	private void RefreshDisplay()
	{
		if (displayText != null)
		{
			displayText.text = Model.DisplayText;
		}
		OnDisplayChanged?.Invoke(Model.DisplayText);
	}

	private void UpdateMemoryIndicator()
	{
		if (memoryIndicatorText != null)
		{
			memoryIndicatorText.text = Math.Abs(Model.MemoryValue) > double.Epsilon ? "M" : string.Empty;
		}
	}

	public void ConfigureForStage(CalculatorLearningStage stage)
	{
		ApplyHighlight(stage.buttonFocus);
		ApplyButtonLocking(stage);
	}

	private void ApplyHighlight(ButtonFocusType focus)
	{
		foreach (var btn in allButtons)
		{
			if (btn == null) continue;
			var targetGraphic = btn.targetGraphic as Graphic;
			if (targetGraphic != null)
			{
				targetGraphic.color = normalColor;
			}
		}
		if (focusMap.TryGetValue(focus, out var focused) && focused != null)
		{
			var g = focused.targetGraphic as Graphic;
			if (g != null)
			{
				g.color = highlightColor;
			}
		}
	}

	private void ApplyButtonLocking(CalculatorLearningStage stage)
	{
		bool IsMemoryStage = stage.stageGroup == StageGroup.ControlMemory;
		bool IsArithmeticStage = stage.stageGroup == StageGroup.Arithmetic || IsMemoryStage || stage.stageGroup == StageGroup.Advanced || stage.stageGroup == StageGroup.DecimalPercent;
		foreach (var btn in allButtons)
		{
			if (btn == null) continue;
			btn.interactable = false;
		}
		EnableIfNotNull(button0); EnableIfNotNull(button1); EnableIfNotNull(button2); EnableIfNotNull(button3); EnableIfNotNull(button4);
		EnableIfNotNull(button5); EnableIfNotNull(button6); EnableIfNotNull(button7); EnableIfNotNull(button8); EnableIfNotNull(button9);
		EnableIfNotNull(buttonDecimal);
		EnableIfNotNull(buttonClearAll); EnableIfNotNull(buttonClearEntry); EnableIfNotNull(buttonBackspace);
		EnableIfNotNull(buttonEquals);
		if (IsArithmeticStage)
		{
			EnableIfNotNull(buttonAdd); EnableIfNotNull(buttonSubtract); EnableIfNotNull(buttonMultiply); EnableIfNotNull(buttonDivide);
			EnableIfNotNull(buttonPercent);
		}
		if (IsMemoryStage)
		{
			EnableIfNotNull(buttonMemoryAdd); EnableIfNotNull(buttonMemorySubtract); EnableIfNotNull(buttonMemoryRecall); EnableIfNotNull(buttonMemoryClear);
		}
	}

	private void EnableIfNotNull(Button button)
	{
		if (button != null) button.interactable = true;
	}

	private void EnsureUiInputSubsystem()
	{
		if (FindFirstObjectByType<EventSystem>() == null)
		{
			var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
			es.hideFlags = HideFlags.None;
		}
		var canvas = GetComponentInParent<Canvas>();
		if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
		{
			canvas.gameObject.AddComponent<GraphicRaycaster>();
		}
	}
}


