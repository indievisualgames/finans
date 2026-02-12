using UnityEngine;

// Quits the finans app when the user hits escape

public class ApplicationQuit : MonoBehaviour
{
    GameObject script;
    ApplicationQuitPopup applicationQuitPopup;
    void Awake()
    {
        script = GameObject.Find("Scriptable");
    }
    void Start()
    {

        if (script)
        {
            applicationQuitPopup = script.GetComponent<ApplicationQuitPopup>();

            Logger.LogInfo($"Found script named {script.name} on start", "ApplicationQuit");

        }
    }
    public void ConfirmedQuit()
    {
        Application.Quit();
    }

    public void CancelQuit()
    {

        if (applicationQuitPopup) { applicationQuitPopup.backButtonPressed = false; }

        var animator = GetComponent<Animator>();
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
            animator.Play("Close");

        Destroy(gameObject);
    }
}