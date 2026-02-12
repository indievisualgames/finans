using TMPro;
using UnityEngine;

public class OnEnablleEmptyErrorText : MonoBehaviour
{
    [SerializeField] TMP_Text errorText; [SerializeField] TMP_Text hintText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        hintText.text = errorText.text = "";
    }


}
