using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelBtnStatus : MonoBehaviour
{
    private Dictionary<string, object> gameLevelBtnStatusData = new Dictionary<string, object>();
    // Start is called before the first frame update

    public Dictionary<string, object> GameLevelBtnStatusData
    {
        get { return gameLevelBtnStatusData; }
        set { gameLevelBtnStatusData = value; }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
