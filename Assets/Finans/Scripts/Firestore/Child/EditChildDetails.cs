using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ricimi;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static IFirestoreEnums;

public class EditChildDetails : MonoBehaviour
{
    private string srNo;
    [SerializeField] TMP_InputField firstName;
    [SerializeField] TMP_InputField lastName;
    [SerializeField] TMP_InputField age;
    [SerializeField] TMP_InputField pinHint;
    [SerializeField] TMP_InputField pin;
    public TMP_Dropdown boyAvatar;
    public TMP_Dropdown girlAvatar;
    [SerializeField] TMP_Dropdown grade;
    [SerializeField] Toggle boyToggle;
    [SerializeField] Toggle girlToggle;
    private string plan;
    public string genderText;
    private string updatedGender;
    private IFirestoreOperator FirestoreClient;
    public string gradeText;
    public GameObject loading;
    [SerializeField] GameObject editChildGO;
    [SerializeField] TMP_Text errorText;
    public string SrNo
    {
        get { return srNo; }
        set { srNo = value; }
    }
    public string GradeText
    {
        get { return gradeText.Trim(); }
        set { gradeText = value; }
    }

    public string Age
    {
        get { return age.text.Trim(); }
        set { age.text = value; }
    }

    public string AvatarName;

    public string FirstName
    {
        get { return firstName.text.Trim(); }
        set { firstName.text = value; }
    }
    public string LastName
    {
        get { return lastName.text.Trim(); }
        set { lastName.text = value; }
    }

    public string Gender
    {
        get { return genderText; }
        set { genderText = value; }
    }
    public string Pin
    {
        get { return pin.text.Trim(); }
        set { pin.text = value; }
    }

    public string PinHint
    {
        get { return pinHint.text; }
        set { pinHint.text = value; }
    }
    public string Plan
    {
        get { return plan; }
        set { plan = value; }
    }

    [Tooltip("Dont add any gameobject here. For runtime use only!")]
    public GameObject EditingChild
    {
        get { return editChildGO; }
        set { editChildGO = value; }
    }

    string __firstName;
    string __lastName;
    string __age;
    string __pin;
    string __pinHint;
    TMP_Dropdown activeAvatarDropdown;
    void Awake()
    {
        age.characterLimit = Params.ageCharLimit;
        pin.characterLimit = Params.pinCharLimit;
        pinHint.characterLimit = Params.pinHintCharLimit;

        Debug.Log(message: $"Edit child form found avatar with name {AvatarName}");
        Debug.Log(message: $"Edit child form found gender as {Gender}");
        GenderSelectionFound(Gender);


    }
    void Start()
    {
        FirestoreClient = new FirestoreDataOperationManager();
        __firstName = firstName.text = FirstName;
        __lastName = lastName.text = LastName;
        __age = age.text = Age;
        __pin = pin.text = Pin;
        __pinHint = pinHint.text = PinHint;

        grade.value = grade.options.FindIndex(option => option.text == GradeText);
        loading.SetActive(false);
        Debug.Log($"Got go row for {editChildGO.name} FirstName is {FirstName} LastName is {LastName} Age is {Age} pin is {Pin} pinhint is  {PinHint} grade text i got is {GradeText} and the DD selected grade is {grade.options[grade.value].text} gender is {Gender} and avatar name is {AvatarName}");


    }
    public async void Submit()
    {
        if (CheckChanges())
        {
            Dictionary<string, string> updatedData = PreparedUpdatedData();
            if (updatedData.Any())
            {
                if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
                {
                    errorText.text = string.Empty;
                    loading.SetActive(true);
                    ChildDetailsInRow cd = EditingChild.GetComponent<ChildDetailsInRow>();

                    if (await UpdateKidProfile(updatedData))
                    {
                        cd.FirstName = FirstName;
                        cd.LastName = LastName;
                        cd.Age = Age;
                        cd.Grade = grade.options[grade.value].text.Trim();
                        cd.AvatarName = AvatarName;
                        cd.Gender = updatedGender;
                        cd.CombineName();
                        GetComponent<Popup>().Close();
                    }
                    Debug.Log($"On EditChild submit first name is {FirstName} lastname is {LastName} age is {Age} grade is {grade.options[grade.value].text.Trim()} pin hint is {PinHint} pin is {Pin} and avatar is {GetActiveAvatarDropdown().options[GetActiveAvatarDropdown().value].image.name.Trim()}");
                }
                else
                {
                    //no internet
                    errorText.text = Message.NoInternetErrorText;
                }



            }
            else { Debug.Log($"Child data is empty"); }
        }

    }
    async Task<bool> UpdateKidProfile(Dictionary<string, string> _updatedData)
    {
        Dictionary<string, object> _dataParent = new Dictionary<string, object>() {
        {FSMapField.profile.ToString(), new Dictionary<string, Dictionary<string, object>>{{FSCollection.children.ToString(), new Dictionary<string, object>() {{EditingChild.GetComponent<ChildDetailsInRow>().ChildID, FirstName}}}}}};
        Dictionary<string, object> _dataChild = new Dictionary<string, object>() { { FSMapField.profile.ToString(), _updatedData } };
        return await FirestoreClient.FirestoreDataSave(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), EditingChild.GetComponent<ChildDetailsInRow>().ChildID, _dataParent, _dataChild);

    }

    public void OnPinEnter()
    {
        if (pin.text.Length > 4)
        {
            Pin = pin.text.Substring(0, 4);
        }

    }

    Dictionary<string, string> PreparedUpdatedData()
    {
        activeAvatarDropdown = GetActiveAvatarDropdown();
        Dictionary<string, string> childData = new Dictionary<string, string>();
        if (__firstName != FirstName)
        { childData.Add(ChildProfile.firstname.ToString(), FirstName); };
        if (__lastName != LastName)
        { childData.Add(ChildProfile.lastname.ToString(), LastName); }
        if (__age != Age)
        { childData.Add(ChildProfile.age.ToString(), Age); };
        if (__pinHint != PinHint)
        { childData.Add(ChildProfile.pinhint.ToString(), PinHint); };
        if (__pin != Pin)
        { childData.Add(ChildProfile.pin.ToString(), Pin); };
        if (GradeText != grade.options[grade.value].text.Trim())
        {
            childData.Add(ChildProfile.grade.ToString(), grade.options[grade.value].text.Trim());
        }
        if (Gender != updatedGender)
        {
            childData.Add(ChildProfile.gender.ToString(), updatedGender);
        }
        if (AvatarName != activeAvatarDropdown.options[activeAvatarDropdown.value].image.name.Trim())
        {
            AvatarName = activeAvatarDropdown.options[activeAvatarDropdown.value].image.name.Trim();
            childData.Add(ChildProfile.avatar.ToString(), AvatarName);
        }
        return childData;
    }

    TMP_Dropdown GetActiveAvatarDropdown()
    {

        if (boyAvatar.isActiveAndEnabled)
        {
            return boyAvatar;
        }
        return girlAvatar;
    }

    bool CheckChanges()
    {
        if (FirstName != string.Empty && LastName != string.Empty && Age != string.Empty && Pin != string.Empty && PinHint != string.Empty)
        {
            Debug.Log($"Update can be performed");
            return true;
        }
        else
        {
            Debug.Log($"Update can't be performed");
        }
        return false;
    }
    public void OnBoyToggleChange(Toggle _toggle)
    {
        Debug.Log($"Triggered when gender {_toggle.name} changed, it is {_toggle.isOn} now");

        if (_toggle.isOn)
        {
            boyAvatar.gameObject.SetActive(true);
            girlAvatar.gameObject.SetActive(false);
            updatedGender = "Boy";
        }
    }

    public void OnGirlToggleChange(Toggle _toggle)
    {
        if (_toggle.isOn)
        {
            girlAvatar.gameObject.SetActive(true);
            boyAvatar.gameObject.SetActive(false);
            updatedGender = "Girl";
        }
        Debug.Log($"Triggered when gender {_toggle.name} changed, it is {_toggle.isOn} now");
    }
    public void GenderSelectionFound(string _gender)
    {
        Debug.Log($"Avatar name found on start {AvatarName}");
        if (_gender == "Boy")
        {
            boyAvatar.value = boyAvatar.options.Select(option => option.image.name).ToList().IndexOf(AvatarName);
            Debug.Log($"Avatar name from avatar dropdown {boyAvatar.options[index: boyAvatar.value].image.name}");
            boyToggle.isOn = true;
            girlToggle.isOn = false;
        }
        else
        {
            girlAvatar.value = girlAvatar.options.Select(option => option.image.name).ToList().IndexOf(AvatarName);
            Debug.Log($"Avatar name from avatar dropdown {girlAvatar.options[index: girlAvatar.value].image.name}");
            girlToggle.isOn = true;
            boyToggle.isOn = false;
        }
    }


}
