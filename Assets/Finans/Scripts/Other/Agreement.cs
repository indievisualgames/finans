using System;
using System.Collections;
using Firebase.Extensions;
using Firebase.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static IFirestoreEnums;

public class Agreement : MonoBehaviour
{
    [SerializeField] Image errorCircle;
    [SerializeField] TMP_Text agreementText;
    [SerializeField] GameObject loader;
    bool isSelected = false;
    FirebaseStorage storage;
    StorageReference storageRef;


    public class AgreementText
    {
        public string Title;
        public string SubTitle;
        public string Description;
    }

    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://eparent-finans.appspot.com/");
        GetStorageRefAndLoad();
    }
    public void OnAgreementSelectToggle()
    {
        isSelected = !isSelected;
        if (isSelected)
        {
            if (errorCircle.gameObject.activeSelf)
            {
                errorCircle.gameObject.SetActive(false);
            }
        }
    }
    public void OnClickAgree()
    {
        if (isSelected)
        {
            //SceneManager.LoadScene(SceneName.LanguageRegion.ToString());
            Ricimi.Transition.LoadLevel(SceneName.LanguageRegion.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
        else
        {
            errorCircle.gameObject.SetActive(true);
        }
    }

    public void OnClickDisagree()
    {
        //  SceneManager.LoadScene(SceneName.HomeScene.ToString());
        Ricimi.Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }

    void GetStorageRefAndLoad()
    {
        StorageReference jsonRef = storageRef.Child($"documents/agreement.json");
        _ = jsonRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log($"Debug Log: GetDownloadUrlAsync is on main thread");
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Download URL: " + task.Result);
                StartCoroutine(LoadAgreement(Convert.ToString(task.Result)));
            }
            else
            {
                Debug.Log($"Debug.Log::::::Exception occured {task.Exception}");
            }
        });
    }

    IEnumerator LoadAgreement(string AgreementUrl)
    {
        UnityWebRequest request = UnityWebRequest.Get(AgreementUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Debug log request.downloadHandler {request.downloadHandler.text}");
            AgreementText agreement = JsonUtility.FromJson<AgreementText>(request.downloadHandler.text);
            agreementText.text = agreement.Description;
            loader.SetActive(false);

        }
        else
        {
            Debug.Log(request.error);
        }
    }
}
