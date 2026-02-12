using UnityEngine;
using UnityEngine.UI;
using System;

public class Coin : MonoBehaviour
{
    public float value; // Set in Inspector (e.g., 0.25, 1.00)
    public Action<float> OnCoinSelected;

    void Start()
    {
        // If using a Button component
        GetComponent<Button>().onClick.AddListener(() => OnCoinSelected?.Invoke(value));
    }
}