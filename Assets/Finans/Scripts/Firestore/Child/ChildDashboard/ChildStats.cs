using Ricimi;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static IFirestoreEnums;

public class ChildStats : Elevator
{
    [SerializeField] Image parentPic;
    [SerializeField] TMP_Text displayName;
    public GameObject loading;
    public GameObject screenContent;
    void Awake()
    {
        if (!PlayerInfo.IsAppAuthenticated)
        {
            Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }


    }

    void Start()
    {
        // displayName.text = $"Hello {Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.DisplayName}";
        LoadProfileAndFinalizeScreen(parentPic, displayName, screenContent, loading);

    }


}
