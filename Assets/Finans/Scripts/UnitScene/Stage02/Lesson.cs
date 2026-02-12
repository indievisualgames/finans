using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using static IFirestoreEnums;
using Ricimi;
using System.Threading.Tasks;
using TMPro;
using System;



#if UNITY_EDITOR
using UnityEditor.Events;
#endif
[RequireComponent(typeof(InternetConnectivityCheck))]
//[RequireComponent(typeof(CurrentSceneName))]
public class Lesson : Elevator
{
     [Header("UI References")]
    
    [SerializeField] private GameObject loading;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject screenContent;
    [SerializeField] private Image childPic;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private GameObject messageBoxPopupPrefab;
    [SerializeField]  private Button[] proceedButton = new Button[5];
     [SerializeField]  private Button[] disabledButton = new Button[5];

    [Header("Private Fields")]
    private IFirestoreOperator FirestoreClient;
    private GameObject popup;
    private InternetConnectivityCheck internetConnectivityCheck;
    private string unitLevel = "";
    private string buttonName = "";
    private Dictionary<string, object> currentUnitStatusData = new Dictionary<string, object>();
    private string context = "Lesson";
    void Awake()
    {
        proceedButton[0].name = UnitStageButtonStatus.flashcard.ToString();
        proceedButton[1].name = UnitStageButtonStatus.minigames.ToString();
        proceedButton[2].name = UnitStageButtonStatus.vocabs.ToString();
        proceedButton[3].name = UnitStageButtonStatus.calculator.ToString();
        proceedButton[4].name = UnitStageButtonStatus.video.ToString();

        if (!PlayerInfo.IsAppAuthenticated)
        {
            Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
        
       
            
            foreach (KeyValuePair<string, string> button in PlayerInfo.UnitButtonInfo)
            {
                unitLevel = button.Key;
                buttonName = button.Value;
            }

       
       

    }
    async void Start()
    {
        GameObject content = GameObject.FindWithTag("Level");
        if (content)
        {
            content.GetComponent<UnloadSceneAdditiveAsync>().toggleContent.SetActive(false);
            Logger.LogInfo($"gameobject {content.name} found....", context);
        }
        if (canvas == null)
        {
            var found = FindFirstObjectByType<Canvas>();
            if (found != null) { canvas = found; }
            else { Logger.LogError("Canvas not found in scene during start", context); }

        }
        FirestoreClient = new FirestoreDataOperationManager();
        internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
        }
        else
        {
            Logger.LogError("InternetConnectivityCheck component not found on Lesson", context);
        }
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            await LoadLessonData();
            if (currentUnitStatusData != null && currentUnitStatusData.Count > 0)
            {
                foreach (KeyValuePair<string, object> data in currentUnitStatusData)
                {
                    for (int i = 0; i < proceedButton.Length; i++)
                    {
                        if (data.Key == proceedButton[i].gameObject.name)
                        {
                            bool enabled = Convert.ToBoolean(data.Value);
                            proceedButton[i].gameObject.SetActive(enabled);
                            disabledButton[i].gameObject.SetActive(!enabled);
                            Logger.LogInfo($"Data found for button {data.Key} or {proceedButton[i].gameObject.name} is: {data.Value}", context);
                        }

                    }
                }
            }
            else
            {
                Logger.LogWarning("No status data found for Lesson buttons; leaving defaults.", context);
            }
        }
        else
        {
            popup = ShowNoConnectivityPopup(canvas, internetConnectivityCheck, messageBoxPopupPrefab);
            loading.SetActive(false);

        }


    }
    async Task LoadLessonData()
    {
        if (!Params.ChildDataloaded)
        {
            FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);
            Logger.LogInfo($"Child data loading done.....", context);
        }
        Dictionary<string, object> unitStatusFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_btn_status.ToString());
        currentUnitStatusData = (Dictionary<string, object>)unitStatusFSData[$"unit{unitLevel}"];
        /*   Dictionary<string, object> unitStageStatus = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_btn_status.ToString());
          Dictionary<string, object> currentUnitData = (Dictionary<string, object>)unitStageStatus[$"unit{unitLevel}"];
       Logger.LogInfo($"Current unit for lesson is {$"unit{unitLevel}"}");

         if (currentUnitData.ContainsKey(FSMapField.lesson.ToString()))
              {

              } */


        //   Dictionary<string, object> unitStageData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_data.ToString());
        //  Dictionary<string, object> currentUnitData = (Dictionary<string, object>)unitStageData[$"unit{unitLevel}"];
        //   Dictionary<string, object> maData = (Dictionary<string, object>)unitStageData[$"unit{unitLevel}"];







        /*  if (!Convert.ToBoolean(currentUnitData[UnitStageButtonStatus.lesson.ToString()]))
          {
              Logger.LogInfo($"Logger.LogInfo:::::Lesson status is {Convert.ToBoolean(currentUnitData[UnitStageButtonStatus.lesson.ToString()])}");
              Dictionary<string, object> data = new Dictionary<string, object>() {
                  {FSMapField.unit_stage_btn_status.ToString(), new Dictionary<string, object>(){
                      {$"unit{unitLevel}", new Dictionary<string, object>(){
                      {UnitStageButtonStatus.lesson.ToString(), true}
                      }}}}};

              _ = await FirestoreClient.FirestoreDataSave(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID, data);


          }*/




        LoadCProfileAndFinalizeScreen(childPic, displayName, screenContent, loading);

    }
    async void RetryTheAction()
    {
        loading.SetActive(value: true);
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            if (internetConnectivityCheck.ConnectionStatus) { internetConnectivityCheck.ConnectionStatus = false; }
            if (popup.GetComponent<Popup>() != null) { popup.GetComponent<Popup>().Close(); }
            await LoadLessonData();

        }
        loading.SetActive(false);
    }
    void Update()
    {
        // Intentionally left empty; connectivity retries are now event-driven via InternetConnectivityCheck.ConnectivityChanged
    }

    private void OnConnectivityRestored(bool isConnected)
    {
        if (!isConnected) return;
        Logger.LogInfo("Connectivity restored in Lesson, retrying action", context);
        RetryTheAction();
    }

    void OnDestroy()
    {
        if (internetConnectivityCheck != null)
        {
            internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
        }
    }

    public void OnProceed(Button btn)
    {
        PlayerInfo.UnitButtonInfo.Clear();
        PlayerInfo.UnitButtonInfo.Add(unitLevel, btn.name.ToLower());

        Transition.LoadLevel(btn.name.ToLower(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
    }
}
