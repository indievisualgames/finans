using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CalculatorLearningStage", menuName = "CalculatorLearning/Stage", order = 1)]
public class CalculatorLearningStage : ScriptableObject
{
	public string stageName;
	public ButtonFocusType buttonFocus;
	public StageGroup stageGroup;
	[TextArea] public string gameplayDescription;
	public string financialConcept;
	public string completionReward;
	[Range(1, 5)] public int difficultyRating;
	public Vector2Int targetAgeRange;
	public string unlockCondition;
}

public enum ButtonFocusType {
	Number0, Number1, Number2, Number3, Number4, Number5, Number6, Number7, Number8, Number9,
	Add, Subtract, Multiply, Divide,
	Decimal, Percent,
	Clear, Backspace, MemoryAdd, MemorySubtract, MemoryRecall, MemoryClear
}

public enum StageGroup {
	Numbers, OddEven, Arithmetic, ControlMemory, DecimalPercent, Advanced
}


