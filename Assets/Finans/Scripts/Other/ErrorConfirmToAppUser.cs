using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class ErrorConfirmToAppUser : MonoBehaviour
{

    [SerializeField] GameObject popupPrefab;
    public float waitTime = 10f;
    bool startTimer = false;
    float timer;
    /* static GameObject PopUP
     {
         get => popupPrefab;
         set => popupPrefab = value;
     }*/


    static GameObject _popupPrefab;
    // Start is called before the first frame update
    void Start()
    {
        // CheckForInternetConnection();
        /*      StartCoroutine(CheckRoutineForInternetConnection(isConnected =>
          {
              if (isConnected)
              {
                  Debug.Log("Internet Available!");
              }
              else
              {
                  Debug.Log("Internet Not Available");
              }
          }));*/
        _popupPrefab = popupPrefab;
        // Debug.Log($"Internet status {await InternetConnectivityChecker.CheckInternetConnectivityAsync()}");

    }

    // Update is called once per frame
    void Update()
    {
        if (startTimer) { timer += Time.deltaTime; }
        if (timer > waitTime)
        {
            startTimer = false;
            timer = 0f;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("No internet access");
            }
            else
            {
                Debug.Log("internet connection");
                // StartCoroutine(CheckRoutine());
            }

        }
    }
    void CheckForInternetConnection_Other(int timeoutMs = 10000, string url = null)
    {
        try
        {
            url ??= CultureInfo.InstalledUICulture switch
            {
                { Name: var n } when n.StartsWith("fa") => // Iran
                    "http://www.aparat.com",
                { Name: var n } when n.StartsWith("zh") => // China
                    "http://www.baidu.com",
                _ =>
                    "http://www.gstatic.com/generate_204",
            };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.KeepAlive = false;
            request.Timeout = timeoutMs;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                bool htmlLookUpResult = (int)response.StatusCode < 299 && (int)response.StatusCode >= 200;
                if (htmlLookUpResult)
                {
                    Debug.Log($"htmlLookUpResult is {htmlLookUpResult}");
                    // return true;
                }

                Debug.Log($"htmlLookUpResult is {htmlLookUpResult}");
                Debug.Log($"htmlLookUpResult status code is  is {response.StatusCode}");
            }
            // return false;
        }
        catch
        {
            Debug.Log("HTMLWebRequest failed ");
            //  return false;  

        }
    }
    public static IEnumerator CheckRoutine()
    {
        UnityWebRequest request = new UnityWebRequest("https://www.google.com/")
        {
            timeout = 10000
        };

        yield return request.SendWebRequest();
        Debug.Log($"Result after SendWebRequest {request.result}");
        Debug.Log($"Done status {request.isDone}");
        Debug.Log($"Responsecode {request.responseCode}");
        Debug.Log($"DownloadedBytes {request.downloadedBytes}");
        if (string.IsNullOrEmpty(request.error))
        {
            Debug.Log("Request have no error ");
            //////////////////////////////// yield return true;
        }
        else
        {
            Debug.Log("nooooooooooooo");
            //////////////////////////////////  yield return false;
        }
        ///////////////////////////////////  startTimer = true;
    }


    public static bool CheckForInternetConnection()
    {
        UnityWebRequest request = new UnityWebRequest("https://www.google.com/")
        {
            timeout = 10000
        };
        UnityWebRequestAsyncOperation response = request.SendWebRequest();
        if (string.IsNullOrEmpty(request.error))
        {
            Debug.Log($"yesss {request.error}");
            return true;
        }
        else
        {
            Debug.Log("nooooooooooooo");
            return false;
        }
    }


    public void CheckForInternetConnection(bool __status)
    {

        if (__status)
        {

        }
        else
        {

        }

    }
    IEnumerator CheckRoutineForInternetConnection(Action<bool> action)
    {
        UnityWebRequest request = new UnityWebRequest("https://www.google.com/")
        {
            timeout = 10000
        };

        yield return request.SendWebRequest();
        Debug.Log($"Result after SendWebRequest {request.result}");
        Debug.Log($"Done status {request.isDone}");
        Debug.Log($"Responsecode {request.responseCode}");
        Debug.Log($"DownloadedBytes {request.downloadedBytes}");
        if (string.IsNullOrEmpty(request.error))
        {
            Debug.Log("Request have no error ");
            CheckForInternetConnection(true);
            //////////////////////////////// yield return true;
        }
        else
        {
            Debug.Log("nooooooooooooo");
            CheckForInternetConnection(__status: false);
            //////////////////////////////////  yield return false;
        }
        ///////////////////////////////////  startTimer = true;
    }

    public static void ShowMessageBox()
    {
        _popupPrefab.SetActive(true);
    }
}
