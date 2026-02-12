using UnityEngine;
using UnityEngine.UI;

public class ScreenTimer : MonoBehaviour
{
    public bool startTimer = false;
    int hour;
    int minutes;
    int seconds;
    int milliseconds;
    string timerFormatted;
    [SerializeField] Text timerScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    float _currentTime;

    void Start()
    {
        // startTimer = false;
        _currentTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (startTimer)
        {
            _currentTime = _currentTime + Time.deltaTime;
            System.TimeSpan time = System.TimeSpan.FromSeconds(_currentTime);
            hour = time.Hours;
            minutes = time.Minutes;
            seconds = time.Seconds;
            milliseconds = time.Milliseconds;
            timerFormatted = string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minutes, seconds);
            // Debug.Log($"Time elasped is {hour:00}:{Mathf.FloorToInt(minutes / 60):00}:{Mathf.FloorToInt(seconds):00}");
            //  Debug.Log($"Formatted elasped time is {timerFormatted}");

            timerScreen.text = timerFormatted;
        }


    }

    void OnDisable()
    {
        Debug.Log($"Timer stopped on disable");
        startTimer = false;

    }
    public void ResetTimer()
    {
        Debug.Log($"Resetting timer");
        _currentTime = 0;
        timerScreen.text = "00:00:00";
    }
}