
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using static IFirestoreEnums;
using Ricimi;
using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Google.MiniJSON;




#if UNITY_EDITOR
using UnityEditor.Events;
#endif
[RequireComponent(typeof(InternetConnectivityCheck))]
public class ChildProgressDashboard : Elevator
{
    IFirestoreOperator FirestoreClient;
    [SerializeField] GameObject childAccountContainer;

    [SerializeField] GameObject childAccountPrefab;

    [SerializeField] TMP_Text displayName;
    [SerializeField] GameObject noAccount;
    [SerializeField] GameObject loading;
    [SerializeField] GameObject reloading;
    [SerializeField] GameObject screenContent;

    [SerializeField] Image unitImage;
    [SerializeField] TMP_Text unitName;
    [SerializeField] TMP_Text unitNumber;

    [SerializeField] Image stageImage;
    [SerializeField] TMP_Text stageName;
    [SerializeField] TMP_Text level;

    [SerializeField] TMP_Text coins;
    [SerializeField] TMP_Text xp;
    [SerializeField] TMP_Text stars;
    [SerializeField] TMP_Text rank;

    [SerializeField] Image parentPic;
    GameObject popup;
    Canvas m_canvas;
    InternetConnectivityCheck internetConnectivityCheck;
    public GameObject messageBoxPopupPrefab;
    bool autoRetryDone = false;

    void Awake()
    {
        if (PlayerInfo.IsAppAuthenticated)
        {
            FirestoreClient = new FirestoreDataOperationManager();
        }
        else
        {
            Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
    }
    async void Start()
    {

        m_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
        }
        else
        {
            Debug.LogError("InternetConnectivityCheck component not found on ChildProgressDashboard");
        }
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await LoadChildProgressData();
        }
        else
        {
            popup = ShowNoConnectivityPopup(m_canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            loading.SetActive(false);
        }

    }


    async void RetryTheAction()
    {
        loading.SetActive(value: true);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (internetConnectivityCheck.ConnectionStatus) { internetConnectivityCheck.ConnectionStatus = false; }
            if (popup.GetComponent<Popup>() != null) { popup.GetComponent<Popup>().Close(); }
            await LoadChildProgressData();
            if (autoRetryDone) { autoRetryDone = false; }

        }
        loading.SetActive(false);
    }

    void Update()
    {
        // Intentionally left empty; connectivity retries are now driven by InternetConnectivityCheck.ConnectivityChanged
    }


    async Task LoadChildProgressData()
    {
        int children = 1;
        Dictionary<string, object> childAccounts = new Dictionary<string, object>();
        try
        {
            FirestoreDatabase.ParentData = await FirestoreClient.GetFirestoreDataField(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSMapField.profile.ToString());
            if (FirestoreDatabase.ParentData.ContainsKey(ParentProfile.children.ToString()))
            {
                childAccounts = FirestoreDatabase.GetFirestoreParentFieldData(ParentProfile.children.ToString());

            }

            if (childAccounts.Count == 0)
            {
                noAccount.SetActive(true);
            }
            else
            {
                foreach (var child in childAccounts)
                {
                    GameObject ChildAvatarGO = Instantiate(childAccountPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
                    Toggle cToggle = ChildAvatarGO.GetComponent<Toggle>();
                    cToggle.group = childAccountContainer.GetComponent<ToggleGroup>();
                    ChildAvatarGO.GetComponent<ChildAccountAvatar>().accountName.text = Convert.ToString(child.Value);
                    ChildAvatarGO.GetComponent<ChildAccountAvatar>().id = child.Key;
                    cToggle.onValueChanged.AddListener(delegate { OnChildSelected(cToggle); });
                    ChildAvatarGO.transform.SetParent(childAccountContainer.transform, false);
                    cToggle.isOn = children == childAccounts.Count;
                    Debug.Log($"Found child with name {child.Value}");
                }
            }

            LoadProfileAndFinalizeScreen(parentPic, displayName, screenContent, loading);
        }
        catch (Exception ex)
        {
            Debug.Log($"Unable to fetch LoadChildProgressData data {ex.Message}");
        }
    }


    private async void OnChildSelected(Toggle _toggle)
    {
        if (_toggle.isOn)
        {
            reloading.SetActive(true);
            string _id = _toggle.transform.GetComponent<ChildAccountAvatar>().id;
            Dictionary<string, object> childData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), _id);
            Dictionary<string, object> progressData = (Dictionary<string, object>)childData[FSMapField.progress_data.ToString()];
            Dictionary<string, object> scoreData = (Dictionary<string, object>)childData[FSMapField.points_score.ToString()];

            string stage_image_name = UnitStageName[(int)ProgressData.current_unit_name];
            string unit_image_name = (string)progressData[ProgressData.current_unit_name.ToString()];
            int current_unit = Convert.ToInt32(progressData[ProgressData.current_unit.ToString()]);
            string formattedCurrentUnit = current_unit <= 10 ? $"0{current_unit}" : current_unit.ToString();
            string url_unitimage = $"{Application.streamingAssetsPath}/unit/{formattedCurrentUnit}/{unit_image_name.ToLower()}.png";
            Debug.Log($"Unit info image path is {url_unitimage}");

            StartCoroutine(LoadUnitOrStageImage(url_unitimage, unitImage));

            string url_stageimage = $"{Application.streamingAssetsPath}/stage/{stage_image_name.ToLower()}.png";
            Debug.Log($"Unit info image path is {url_stageimage}");
            StartCoroutine(LoadUnitOrStageImage(url_stageimage, stageImage));

            unitName.text = (string)progressData[ProgressData.current_unit_name.ToString()];
            unitNumber.text = current_unit.ToString();
            stageName.text = (string)progressData[ProgressData.current_stage_name.ToString()];
            level.text = progressData[ProgressData.level_completed.ToString()].ToString();
            rank.text = (string)progressData[ProgressData.rank.ToString()];
            coins.text = scoreData[HUD.coins.ToString()].ToString();
            xp.text = scoreData[HUD.xp.ToString()].ToString();
            stars.text = scoreData[HUD.stars.ToString()].ToString();

            Debug.Log($"Selected child with id {_id} and the data for the selected child is {Json.Serialize(childData)}");
            reloading.SetActive(false);
        }
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected || autoRetryDone) return;
        autoRetryDone = true;
        Debug.Log("Connectivity restored in ChildProgressDashboard, retrying action");
        RetryTheAction();
    }

    void OnDestroy()
    {
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
        }
    }
}
