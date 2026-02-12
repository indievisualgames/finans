using System;
using System.Globalization;

public enum CalculatorOperator
{
	None,
	Add,
	Subtract,
	Multiply,
	Divide
}

public class CalculatorModel
{
	private string currentInputBuffer = "";
	private double accumulatorValue = 0d;
	private CalculatorOperator pendingOperator = CalculatorOperator.None;
	private bool hasPendingOperator = false;
	private bool justEvaluated = false;
	private bool hasDecimalInBuffer = false;
	private double memoryValue = 0d;
	private const int MaxDigits = 10;

	public event Action<string> OnDisplayChanged;

	public string DisplayText { get; private set; } = "0";

	public double MemoryValue => memoryValue;
	public double CurrentEntryValue
	{
		get
		{
			if (string.IsNullOrEmpty(currentInputBuffer)) return 0d;
			if (double.TryParse(currentInputBuffer, out var v)) return v;
			return 0d;
		}
	}

	public void ResetAll()
	{
		currentInputBuffer = string.Empty;
		accumulatorValue = 0d;
		pendingOperator = CalculatorOperator.None;
		hasPendingOperator = false;
		justEvaluated = false;
		hasDecimalInBuffer = false;
		UpdateDisplay("0");
	}

	public void ClearEntry()
	{
		currentInputBuffer = string.Empty;
		hasDecimalInBuffer = false;
		justEvaluated = false;
		UpdateDisplay("0");
	}

	public void Backspace()
	{
		if (justEvaluated)
		{
			ClearEntry();
			return;
		}

		if (string.IsNullOrEmpty(currentInputBuffer))
		{
			UpdateDisplay("0");
			return;
		}

		if (currentInputBuffer.Length == 1)
		{
			currentInputBuffer = string.Empty;
			UpdateDisplay("0");
			return;
		}

		char lastChar = currentInputBuffer[currentInputBuffer.Length - 1];
		currentInputBuffer = currentInputBuffer.Substring(0, currentInputBuffer.Length - 1);

		if (lastChar == '.')
		{
			hasDecimalInBuffer = false;
		}

		UpdateDisplay(currentInputBuffer);
	}

	public void InputDigit(int digit)
	{
		if (digit < 0 || digit > 9) return;
		if (justEvaluated)
		{
			currentInputBuffer = string.Empty;
			justEvaluated = false;
		}
		// Enforce max of 10 digits (ignoring decimal separator and sign)
		if (CountDigits(currentInputBuffer) >= MaxDigits)
		{
			UpdateDisplay(currentInputBuffer.Length == 0 ? "0" : currentInputBuffer);
			return;
		}
		if (currentInputBuffer == "0")
		{
			currentInputBuffer = digit.ToString();
		}
		else
		{
			currentInputBuffer += digit.ToString();
		}
		UpdateDisplay(currentInputBuffer);
	}

	public void InputDecimal()
	{
		if (justEvaluated)
		{
			currentInputBuffer = string.Empty;
			justEvaluated = false;
			hasDecimalInBuffer = false;
		}
		if (!hasDecimalInBuffer)
		{
			hasDecimalInBuffer = true;
			if (string.IsNullOrEmpty(currentInputBuffer))
			{
				currentInputBuffer = "0.";
			}
			else
			{
				currentInputBuffer += ".";
			}
			UpdateDisplay(currentInputBuffer);
		}
	}

	public void SetOperator(CalculatorOperator op)
	{
		if (!hasPendingOperator)
		{
			accumulatorValue = CurrentEntryValue;
			hasPendingOperator = true;
		}
		else
		{
			if (!string.IsNullOrEmpty(currentInputBuffer) || justEvaluated)
			{
				EvaluatePending();
			}
		}
		pendingOperator = op;
		currentInputBuffer = string.Empty;
		hasDecimalInBuffer = false;
		justEvaluated = false;
		UpdateDisplay(FormatResult(accumulatorValue));
	}

	public void PressPercent()
	{
		if (!hasPendingOperator)
		{
			UpdateDisplay(FormatResult(CurrentEntryValue));
			return;
		}
		var baseValue = accumulatorValue;
		var percentValue = baseValue * (CurrentEntryValue / 100d);
		var formatted = FormatResult(percentValue);
		currentInputBuffer = formatted;
		hasDecimalInBuffer = formatted.Contains(".");
		UpdateDisplay(formatted);
	}

	public void PressEquals()
	{
		if (hasPendingOperator)
		{
			EvaluatePending();
			pendingOperator = CalculatorOperator.None;
			hasPendingOperator = false;
			var formatted = FormatResult(accumulatorValue);
			currentInputBuffer = formatted;
			hasDecimalInBuffer = formatted.Contains(".");
			justEvaluated = true;
			UpdateDisplay(formatted);
		}
	}

	public void MemoryAdd()
	{
		memoryValue += CurrentEntryValue;
	}

	public void MemorySubtract()
	{
		memoryValue -= CurrentEntryValue;
	}

	public void MemoryClear()
	{
		memoryValue = 0d;
	}

	public void MemoryRecall()
	{
		var formatted = FormatResult(memoryValue);
		currentInputBuffer = formatted == "Error" ? string.Empty : formatted;
		hasDecimalInBuffer = formatted.Contains(".");
		justEvaluated = false;
		UpdateDisplay(formatted);
	}

	private void EvaluatePending()
	{
		var right = CurrentEntryValue;
		switch (pendingOperator)
		{
			case CalculatorOperator.Add:
				accumulatorValue = accumulatorValue + right;
				break;
			case CalculatorOperator.Subtract:
				accumulatorValue = accumulatorValue - right;
				break;
			case CalculatorOperator.Multiply:
				accumulatorValue = accumulatorValue * right;
				break;
			case CalculatorOperator.Divide:
				if (right == 0d)
				{
					UpdateDisplay("Error");
					ResetAll();
					return;
				}
				accumulatorValue = accumulatorValue / right;
				break;
		}
	}

	private void UpdateDisplay(string text)
	{
		DisplayText = text;
		OnDisplayChanged?.Invoke(DisplayText);
	}

	private int CountDigits(string text)
	{
		if (string.IsNullOrEmpty(text)) return 0;
		int count = 0;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c >= '0' && c <= '9') count++;
		}
		return count;
	}

	private string FormatResult(double value)
	{
		var sign = value < 0 ? "-" : string.Empty;
		double abs = Math.Abs(value);
		if (abs == 0d)
		{
			return "0";
		}
		string integerPart = Math.Floor(abs).ToString(CultureInfo.InvariantCulture);
		int integerDigits = integerPart.Length;
		if (integerDigits > MaxDigits)
		{
			return "Error";
		}
		int allowedFractionDigits = MaxDigits - Math.Max(1, integerDigits);
		if (allowedFractionDigits <= 0)
		{
			return sign + integerPart;
		}
		double rounded = Math.Round(abs, allowedFractionDigits, MidpointRounding.AwayFromZero);
		string text = rounded.ToString($"F{allowedFractionDigits}", CultureInfo.InvariantCulture);
		text = TrimTrailingZeros(text);
		return sign + text;
	}

	private static string TrimTrailingZeros(string text)
	{
		if (text.IndexOf('.') >= 0)
		{
			int i = text.Length - 1;
			while (i >= 0 && text[i] == '0') i--;
			if (i >= 0 && text[i] == '.') i--;
			return text.Substring(0, i + 1);
		}
		return text;
	}
}


