using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.MiniJSON;
using Ricimi;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static IFirestoreEnums;
public class AddChildProfile : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField firstName;
    [SerializeField] private TMP_InputField lastName;
    [SerializeField] private TMP_InputField age;
    [SerializeField] private GameObject loading;
    [SerializeField] private TMP_InputField pinHint;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] TMP_InputField pin;
    [SerializeField] private TMP_Dropdown grade;
    [SerializeField] private TMP_Dropdown boyAvatar;
    [SerializeField] private TMP_Dropdown girlAvatar;
    [SerializeField] private ToggleGroup genderToggle;
    [Header("Private Fields")]
    private string gender;
    private string avatar;
    private string _context = "AddChildProfile";
    private IFirestoreOperator FirestoreClient;

    private AddChildDashboard acd;
    private DateTime currentDT;

    void Awake()
    {
        age.characterLimit = Params.ageCharLimit;
        pin.characterLimit = Params.pinCharLimit;
        pinHint.characterLimit = Params.pinHintCharLimit;
    }
    void Start()
    {
        currentDT = ServerDateTime.GetFastestNISTDate();
        acd = GameObject.Find("Scriptable").GetComponent<AddChildDashboard>();
        FirestoreClient = new FirestoreDataOperationManager();
        avatar = girlAvatar.options[girlAvatar.value].image.name.ToString();
        loading.SetActive(false);
    }

    public string GenerateID()
    {
        return Guid.NewGuid().ToString("N");
    }
    public async void Submit()
    {
        if (await InsertData())
        {
            if (await acd.FetchChildDashboardData(true))
            {
                loading.SetActive(false);
                gameObject.GetComponentInParent<Popup>().Close();
            }
        }
    }

    private async Task<bool> InsertData()
    {

        bool status = false;
        if (ValidateFields())
        {
            loading.SetActive(true);
            if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
            {
                errorText.text = string.Empty;
                // loading.SetActive(true);
                string childID = GenerateID();

                foreach (Toggle toggle in genderToggle.ActiveToggles())
                {
                    if (toggle.isOn)
                    {
                        gender = toggle.GetComponentInChildren<TMP_Text>().text.Trim();
                        Logger.LogInfo("toggle value is " + gender, _context);
                    }
                }
                Dictionary<string, object> dataParent = new Dictionary<string, object>() {
            {FSMapField.profile.ToString(), new Dictionary<string, Dictionary<string, object>>{{FSCollection.children.ToString(), new Dictionary<string, object>() {{childID, firstName.text.Trim()} } } }}
        };
                /****
                Create child profile alongwith unit buttons status, poit score, progress data, quiz played status and maingame/penguin map field 
                ****/
                Dictionary<string, object> dataChild = new Dictionary<string, object>() {
            {FSMapField.profile.ToString(), FirestoreData.ChildProfileData(firstName.text.Trim(),lastName.text.Trim(), age.text.Trim(),  avatar.Trim(),pinHint.text, pin.text.Trim(),  grade.options[grade.value].text.Trim(), gender, "free", "Off")},
           {FSMapField.unit_stage_btn_status.ToString(), FirestoreData.UnitStageButtonStatus(Unit.unit01.ToString(), true)},
          {FSMapField.points_score.ToString(), FirestoreData.CreateInitialScorePoints(0,0,0,0,0,0)},
          {FSMapField.progress_data.ToString(), FirestoreData.CreateProgess("1",UnitLevelName[1],UnitStageName[1] ,currentDT, "0", 0 )},
          {FSMapField.unit_stage_data.ToString(),new Dictionary<string, object>() {
            {Unit.unit01.ToString(), new Dictionary<string, object>() {
                {FSMapField.quizzes.ToString(), FirestoreData.DefaultQuizzes() },
                {FSMapField.maingame.ToString(), new Dictionary<string, object>() {
                     {MainGame.date.ToString(), currentDT.ToShortDateString()},
                      {MainGame.game_points.ToString(),0},
                    {MainGame.level.ToString(),0},

                    }}}}}}                 
                    
           /*,{FSMapField.unit_stage_data.ToString(), FirestoreData.CreateFlashcard( Unit.unit01.ToString(), UnitButtonName.flashcard.ToString(), Inference.GenerateRandomNumber(Params.TotalFlashCard,new HashSet<int>(){}), new HashSet<int>(){}, -1, true)}*/
          
          
            };

                Logger.LogInfo(Json.Serialize(dataParent), _context);
                Logger.LogInfo(Json.Serialize(dataChild), _context);


                status = await FirestoreClient.FirestoreDataSave(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), childID, dataParent, dataChild);
                Logger.LogInfo($"Debug log: Datasave done {status}", _context);
            }
            else
            {
                errorText.text = Message.NoInternetErrorText;
                loading.SetActive(false);
            }



        }
        return status;
    }

    public void OnGenderMaleSelection()
    {
        Logger.LogInfo($"Boy avatar is {boyAvatar.isActiveAndEnabled} while girl avatar is {girlAvatar.isActiveAndEnabled}", _context);
        if (girlAvatar.isActiveAndEnabled)
        {
            girlAvatar.gameObject.SetActive(false); boyAvatar.gameObject.SetActive(true);
            avatar = girlAvatar.options[index: girlAvatar.value].image.name.ToString();
            Logger.LogInfo($"Avatar is for boy named {avatar}", _context);
        }
    }
    public void OnGenderFemaleSelection()
    {
        Logger.LogInfo($"Boy avatar is {boyAvatar.isActiveAndEnabled} while girl avatar is {girlAvatar.isActiveAndEnabled}", _context);
        if (boyAvatar.isActiveAndEnabled)
        {
            boyAvatar.gameObject.SetActive(false); girlAvatar.gameObject.SetActive(true);
            avatar = boyAvatar.options[boyAvatar.value].image.name.ToString();
            Logger.LogInfo($"Avatar is for girl named {avatar}", _context);
        }
    }

    public void SelectMaleAvatarName()
    {
        avatar = boyAvatar.options[boyAvatar.value].image.name.ToString();
        Logger.LogInfo($"Avatar is for boy named {avatar}", _context);
    }
    public void SelectFemaleAvatarName()
    {
        avatar = girlAvatar.options[girlAvatar.value].image.name.ToString();
        Logger.LogInfo($"Avatar is for girl named {avatar}", _context);
    }
    bool ValidateFields()
    {
        if (firstName.text.Trim() == string.Empty)
        {
            errorText.text = Message.FirstnameEmpty; return false;
        }
        else if (firstName.text.Trim().Length < 4)
        {
            errorText.text = Message.ShortFirstname; return false;
        }
        if (lastName.text.Trim() == string.Empty)
        {
            errorText.text = Message.LasttnameEmpty; return false;
        }
        else if (lastName.text.Trim().Length < 4)
        {
            errorText.text = Message.ShortLastname; return false;
        }
        else if (age.text.Trim() == string.Empty)
        {
            errorText.text = Message.AgeEmpty; return false;
        }
        else if (pin.text.Trim() == string.Empty)
        {
            errorText.text = Message.FormFillEmptyPin; return false;
        }

        else if (pin.text.Trim().Length < 4)
        {
            errorText.text = Message.PinCharCheck; return false;
        }
        else if (pinHint.text == string.Empty)
        {
            errorText.text = Message.FormFillEmptyPinhint; return false;
        }
        OnTextChange();

        return true;
    }
    public void OnTextChange()
    {
        if (errorText.text.Length > 0)
        {
            errorText.text = string.Empty;
        }
    }
}
