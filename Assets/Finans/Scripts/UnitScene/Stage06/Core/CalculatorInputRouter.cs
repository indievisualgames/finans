using UnityEngine;

namespace Game.Core
{
    public sealed class CalculatorInputRouter : MonoBehaviour, ICalculatorInputSource
    {
        public event System.Action<string> OnKey;

        // Wire each calculator UI Button's OnClick(String) → this method with its label (e.g., "7", "+", "=")
        public void OnUIButtonClick(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            OnKey?.Invoke(key);
        }
    }
}


