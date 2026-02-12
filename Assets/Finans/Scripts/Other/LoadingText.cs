using UnityEngine;
using TMPro;

public class LoadingText : MonoBehaviour
{
    TMP_Text loadingText;
    // Updates once per frame


    void Start()
    {
        loadingText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        //loadingText.text = "Generating Card...";
        loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));
    }

}


