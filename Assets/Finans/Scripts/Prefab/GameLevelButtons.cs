using System;
using UnityEngine;

public class GameLevelButtons : MonoBehaviour
{
    [SerializeField] GameObject lockedBTN;
    [SerializeField] GameObject unlockedBTN;
    // Start is called before the first frame update
    void Start()
    {
        if (transform.parent.GetComponent<GameLevelBtnStatus>().GameLevelBtnStatusData.ContainsKey(transform.name))
        {
            bool value = Convert.ToBoolean(transform.parent.GetComponent<GameLevelBtnStatus>().GameLevelBtnStatusData[transform.name]);
            if (!value)
            {
                Logger.LogInfo($"Found locked map field data from fs and the level {transform.name} is played, button unlocked", "GameLevelButtons");
                lockedBTN.SetActive(false);
                unlockedBTN.SetActive(true);
            }
            else
            {
                Logger.LogInfo($"Found locked map field data from fs but the level {transform.name} is not played yet..keeping button locked", "GameLevelButtons");
            }

        }
        else
        {
            Logger.LogInfo($"Does not contain maingame {transform.name} level data, keeping the level locked", "GameLevelButtons");
        }

    }


}
