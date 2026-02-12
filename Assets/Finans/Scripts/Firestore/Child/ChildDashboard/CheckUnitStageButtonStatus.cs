using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static IFirestoreEnums;

public class CheckUnitStageButtonStatus : MonoBehaviour
{
    public GameObject unitStatusDataScript;
    public Dictionary<string, bool> unitButtonStatusForClick = new Dictionary<string, bool>();
    
    [Header("Private Fields")]
    private string forUnitNumber;
    private string context = "UnitBtnStatus";
    async void Start()
    {
        forUnitNumber = transform.name.ToLower();
        DateTime currentDT;
        try
        {
            var clock = new TimeProvider();
            currentDT = clock.NowInTimeZone(FirestoreDatabase.GetChildTimeZoneOrDefault());
        }
        catch
        {
            currentDT = ServerDateTime.GetFastestNISTDate();
        }
        Dictionary<string, bool> unitButtonStatusByDate = new Dictionary<string, bool>();

        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            Dictionary<string, object> unitStatusFSData = GetUnitStatusData();
            try
            {
                if (unitStatusFSData != null && unitStatusFSData.ContainsKey(forUnitNumber))
                {
                    foreach (KeyValuePair<string, object> item in unitStatusFSData[forUnitNumber] as Dictionary<string, object>)
                    {
                        Logger.LogInfo($"Checking status of {item.Key} for  {forUnitNumber}", context);

                        unitButtonStatusByDate.Add(item.Key, Convert.ToBoolean(item.Value));
                        string _status = Convert.ToBoolean(item.Value) ? "Unlocked" : "Locked";
                        Logger.LogInfo($"Found button {item.Key} as {_status}", context);

                    }
                }
                else
                {
                    Logger.LogWarning($"No status map found for {forUnitNumber}; defaulting to locked.", context);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error while evaluating unit button status", context, ex);
            }
        }

        LogUnitButtonStatus(unitButtonStatusByDate);

        foreach (Transform buttonParent in GetComponentInChildren<Transform>())
        {
            foreach (Transform button in buttonParent.GetComponentInChildren<Transform>())
            {
                if (button.name == "Button")
                {
                    if (unitButtonStatusByDate.Count > 0)
                    {
                        if (unitButtonStatusByDate.ContainsKey(buttonParent.name.ToLower()))
                        {
                            if (unitButtonStatusByDate[buttonParent.name.ToLower()])
                            {
                                foreach (Transform lockbutton in button.GetComponentInChildren<Transform>())
                                {
                                    if (lockbutton.name == "Lock")
                                    {
                                        lockbutton.gameObject.SetActive(false);
                                    }
                                }
                                unitButtonStatusForClick.Add(buttonParent.name.ToLower(), false);
                            }
                            else
                            {
                                unitButtonStatusForClick.Add(buttonParent.name.ToLower(), true);

                            }
                        }
                    }
                    else
                    {
                        unitButtonStatusForClick.Add(buttonParent.name.ToLower(), true);
                    }
                }

            }
        }
    }
    private void LogUnitButtonStatus(Dictionary<string, bool> statuses)
    {
        try
        {
            if (statuses == null || statuses.Count == 0)
            {
                Logger.LogInfo($"Unit {forUnitNumber} has no computed button status", context);
                return;
            }
            string summary = string.Join(", ", statuses.Select(kv => $"{kv.Key}:{kv.Value}"));
            Logger.LogInfo($"Unit {forUnitNumber} button statuses => {summary}", context);
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Failed to log unit button statuses", context, ex);
        }
    }
    Dictionary<string, object> GetUnitStatusData()
    {
        Dictionary<string, object> __unitStatusFSData;
        if (SceneManager.GetActiveScene().name == SceneName.Level.ToString())
        {
            __unitStatusFSData = unitStatusDataScript.GetComponent<GameLevels>().unitStatusFSData;
            Logger.LogInfo($"Resolving status from scene {SceneManager.GetActiveScene().name}", context);
        }
        else
        {
            __unitStatusFSData = unitStatusDataScript.GetComponent<ChildDashboard>().unitStatusFSData;
        }
        return __unitStatusFSData;
    }

}