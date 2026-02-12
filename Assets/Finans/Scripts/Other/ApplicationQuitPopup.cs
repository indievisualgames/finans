using UnityEngine;
using static IFirestoreEnums;


public class ApplicationQuitPopup : MonoBehaviour
{
    [SerializeField] Canvas m_canvas;
    [SerializeField] GameObject quitPopup;
    int timesBackButtonPressed = 0;
    [SerializeField] public bool backButtonPressed = false;
    GameObject noInternetPopoup;
    void Start()
    {
        if (m_canvas == null) { m_canvas = GameObject.Find("Canvas").GetComponent<Canvas>(); }


    }
    void Update()
    {

        if (Input.GetKey(KeyCode.Escape) && !backButtonPressed)
        {
            timesBackButtonPressed++;
            ConfirmQuit();
        }
    }

    void ConfirmQuit()
    {
        if (timesBackButtonPressed == 2)
        {
            noInternetPopoup = GameObject.FindGameObjectWithTag(Tags.MessageBox.ToString());
            backButtonPressed = true;
            timesBackButtonPressed = 0;
            if (quitPopup && noInternetPopoup == null)
            {
                GameObject ChildDetailsGO = Instantiate(quitPopup, new Vector3(0f, 0f, 0f), Quaternion.identity);
                ChildDetailsGO.transform.SetParent(m_canvas.transform, false);
                //  m_canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                //  m_canvas.GetComponent<CanvasScaler>().scaleFactor = 0.9f;

                ChildDetailsGO.SetActive(true);
            }
            else
            {
                Logger.LogInfo("Cannot show quit confirm box as nointernet connection popup is already present... ", "ApplicationQuitPopup");

            }
        }
    }


}