using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static IFirestoreEnums;

public class ChildDashboardStats : Elevator
{
    [Header("UI References")]
    [SerializeField] protected Image unitLabelImage;
    [SerializeField] protected TMP_Text unitLevel;
    [SerializeField] protected TMP_Text unitName;
    [SerializeField] protected TMP_Text stageName;
    [SerializeField] protected Image stageImage;
    [SerializeField] protected TMP_Text rank;
    [SerializeField] protected TMP_Text levelCompleted;
    [SerializeField] protected Image progressOverallImage; //units opened within locked units 
    [SerializeField] protected TMP_Text overallProgressText;
    [SerializeField] protected Image progressQuizImage; //completed quiz out of 6
    [SerializeField] protected TMP_Text quizProgressText;
    [SerializeField] protected Image progressVideoImage; //watched video out of 5
    [SerializeField] protected TMP_Text videoProgressText;
    [SerializeField] protected Image progressReadingImage; //read books out of 4
    [SerializeField] protected TMP_Text readingProgressText;

    [SerializeField] protected Image progressFinUnderstandingImage; //finance understanding      (xp + bonus + quiz) for eg 100
    [SerializeField] protected TMP_Text finUSProgressText;
   [SerializeField] protected  bool loadedStat;
   protected IFirestoreOperator FirestoreClient;
    [Header("Private Fields")]
    private bool open_next_level = false;
    private int currentUnit;
    private string baseContext = "ChildDashboardStats";
    [Header("Protected Fields")]
    protected DateTime currentDate;
     public void LoadAllDataAndFinish(Dictionary<string, object> _progress_data, Image _childPic, TMP_Text _displayName, GameObject _screenContent, GameObject _loading)
    {
        if (LoadProgressData(_progress_data))
        {
            string formattedCurrentUnit = currentUnit <= 10 ? $"0{currentUnit}" : currentUnit.ToString();
            string image_name = (string)_progress_data[ProgressData.current_unit_name.ToString()];
            string url_image = $"{Application.streamingAssetsPath}/unit/{formattedCurrentUnit}/{image_name.ToLower()}.png";
           Logger.LogInfo($"Unit info image path is {url_image}", baseContext);

            StartCoroutine(LoadUnitOrStageImage(url_image, unitLabelImage));
            LoadCProfileAndFinalizeScreen(_childPic, _displayName, _screenContent, _loading);
        }
    }

    private async Task<bool> UpdateProgressData(Dictionary<string, object> __update_progress_data)
    {
        if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
        {
            return await FirestoreClient.FirestoreDataSave(
                FSCollection.parent.ToString(),
                PlayerInfo.AuthenticatedID,
                FSCollection.children.ToString(),
                PlayerInfo.AuthenticatedChildID,
                __update_progress_data);
        }
        else
        {
            Logger.LogError("No internet connectivity while updating child progress data; skipping Firestore save", baseContext);
            return false;
        }
    }
   
    void ParseStageImageAndLoad(string image_name)
    {
        string url_image = $"{Application.streamingAssetsPath}/stage/{image_name.ToLower()}.png";
       Logger.LogInfo($"Unit info image path is {url_image}", baseContext);
     StartCoroutine(LoadUnitOrStageImage(url_image, stageImage));

    }
    public bool LoadProgressData(Dictionary<string, object> _progress_data)
    {
        // Resolve current date in child's timezone for unlock comparisons
        try
        {
            var clock = new TimeProvider();
            currentDate = clock.NowInTimeZone(FirestoreDatabase.GetChildTimeZoneOrDefault());
        }
        catch
        {
            currentDate = ServerDateTime.GetFastestNISTDate();
        }
        foreach (var keyItem in _progress_data)
        {
            Logger.LogInfo($"Keys found in progress data are {keyItem.Key}", baseContext);
            if (keyItem.Key == ProgressData.current_unit.ToString())
            {
                int.TryParse((string)keyItem.Value, out currentUnit);
                unitLevel.text = currentUnit < 10 ? $"0{(string)keyItem.Value}" : keyItem.Value.ToString();
                Logger.LogInfo($"Current unit is {currentUnit}", baseContext );
            }
            if (keyItem.Key == ProgressData.next_unit_date.ToString() && !open_next_level)
            {

                DateTime.TryParse((string)keyItem.Value, out DateTime parsedDate);
                if (DateTime.Compare(currentDate, parsedDate) > 0)
                {
                    open_next_level = true;
                     Logger.LogInfo($"Unit date in FB is {DateTime.Parse((string)keyItem.Value)} and current date time is {currentDate}", baseContext );
                }
            }
            if (keyItem.Key == ProgressData.current_unit_name.ToString())
            {

                unitName.text = keyItem.Value.ToString();
                Logger.LogInfo(message: $"Current unit name {keyItem.Value}", baseContext );
            }
            if (keyItem.Key == ProgressData.current_stage_name.ToString())
            {
                  stageName.text = ToTitleCase(keyItem.Value.ToString());
                ParseStageImageAndLoad(keyItem.Value.ToString());
            }
            if (keyItem.Key == ProgressData.rank.ToString())
            {
                rank.text = keyItem.Value.ToString();
            }
            if (keyItem.Key == ProgressData.level_completed.ToString())
            {
                int o = Convert.ToInt32(keyItem.Value);
                levelCompleted.text = o < 10 ? $"0{o}" : o.ToString();
                Logger.LogInfo(message: $"Level completed {o}", baseContext );
            }
        }
         return true;
    }


}
