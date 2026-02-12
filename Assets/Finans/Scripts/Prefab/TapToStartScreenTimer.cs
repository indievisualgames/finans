using UnityEngine;

public class TapToStartScreenTimer : MonoBehaviour
{
    public GameObject timerObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void StartScreenTimer()
    {
        if (timerObject != null)
        {
            Debug.Log($"Starting timer............");
            timerObject.GetComponent<ScreenTimer>().startTimer = true;
        }
    }
}
