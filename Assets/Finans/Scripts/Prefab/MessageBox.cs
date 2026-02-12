using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    [SerializeField] TMP_Text headlineText;
    [SerializeField] TMP_Text userMessage;
    [SerializeField] public GameObject actionButton;
    // [SerializeField] public GameObject acceptButton;
    [SerializeField] public GameObject secondaryButton;
    [SerializeField] public GameObject tertiaryButton;
    public string Headline
    {
        get { return headlineText.text; }
        set { headlineText.text = value; }

    }

    public string Message
    {
        get { return userMessage.text; }
        set { userMessage.text = value; }
    }

    public string ActionText
    {
        get { return actionButton.GetComponentInChildren<TMP_Text>().text; }
        set { actionButton.GetComponentInChildren<TMP_Text>().text = value; }
    }

    public string SecondaryText
    {
        get { return secondaryButton.GetComponentInChildren<TMP_Text>().text; }
        set { secondaryButton.GetComponentInChildren<TMP_Text>().text = value; }
    }
    public string TertiaryText
    {
        get { return tertiaryButton.GetComponentInChildren<TMP_Text>().text; }
        set { tertiaryButton.GetComponentInChildren<TMP_Text>().text = value; }
    }
    public void ResetCanvasScale()
    {
        // Canvas canvas = gameObject.GetComponentInParent<Canvas>();
        CanvasScaler canvasScaler = gameObject.GetComponentInParent<CanvasScaler>();
        //canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasScaler.scaleFactor = 0.9f;
    }

}
