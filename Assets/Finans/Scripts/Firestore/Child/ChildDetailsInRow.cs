using Ricimi;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChildDetailsInRow : MonoBehaviour
{
    [SerializeField] TMP_Text srNo;
    [SerializeField] TMP_Text dob;
    [SerializeField] TMP_Text age;
    [SerializeField] TMP_Text firstName;
    [SerializeField] TMP_Text gender;
    [SerializeField] TMP_Text grade;
    [SerializeField] TMP_Text plan;
    [SerializeField] TMP_Text screenTime;
    [SerializeField] Image avatarImage;
    string lastName;
    string pin; string pinHint;
    string fName;
    string childId;
    string avatarName;
    public string SrNo
    {
        get { return srNo.text; }
        set { srNo.text = value; }
    }
    public string FirstName
    {

        get { return fName; }
        set { fName = value; }
    }
    public string LastName
    {
        get { return lastName; }
        set { lastName = value; }
    }

    public string DOB
    {
        get { return dob.text; }
        set { dob.text = value; }
    }
    public string Age
    {
        get { return age.text; }
        set { age.text = value; }
    }

    public string AvatarName
    {
        get { return avatarName; }
        set { avatarName = value; }
    }
    public string Pin
    {
        get { return pin; }
        set { pin = value; }
    }
    public string PinHint
    {
        get { return pinHint; }
        set { pinHint = value; }
    }

    public string Gender
    {
        get { return gender.text; }
        set { gender.text = value; }
    }
    public string Grade
    {
        get { return grade.text; }
        set { grade.text = value; }
    }
    public string Plan
    {
        get { return plan.text; }
        set { plan.text = value; }
    }
    public string ScreenTime
    {
        get { return screenTime.text; }
        set { screenTime.text = value; }
    }
    public string ChildID
    {
        get { return childId; }
        set { childId = value; }
    }
    void Start()
    {
        Debug.Log($"fname and LastName is {fName + " " + LastName}");
        firstName.text = fName + " " + LastName;
        Sprite[] _sprite = Resources.LoadAll<Sprite>(Params.AvatarSSName);
        for (int i = 0; i < _sprite.Length; i++)
        {
            if (_sprite[i].name == AvatarName)
            {
                avatarImage.sprite = _sprite[i];
            }
        }
        Debug.Log($"Avatar name is {AvatarName}");
        Debug.Log($"On child detail row initialize: firstname is {FirstName} lastname is {LastName} age is {Age} DOB is {DOB} gender is {Gender} grade is {Grade} password hint is {PinHint} password is {Pin} and avatar name is {AvatarName}");
    }
    public void EditClick()
    {
        PopupOpener editprofilePopupOpener = GetComponent<PopupOpener>();
        GameObject editprofilePrefab = editprofilePopupOpener.popupPrefab;
        EditChildDetails ecd = editprofilePrefab.GetComponent<EditChildDetails>();
        ecd.EditingChild = transform.gameObject;
        ecd.FirstName = FirstName;
        ecd.LastName = LastName;
        ecd.Age = Age;
        ecd.Gender = Gender;
        ecd.AvatarName = AvatarName;
        ecd.Pin = Pin;
        ecd.PinHint = PinHint;
        ecd.SrNo = SrNo;
        ecd.gradeText = Grade;
        editprofilePopupOpener.OpenPopup();
        Debug.Log($"On edit click :row values sent :: firstname is {FirstName} lastname is {LastName} age is {Age} DOB is {DOB} gender is {Gender} grade is {Grade} password hint is {PinHint} password is {Pin} avatar name is {AvatarName}");
    }
    public void CombineName()
    {
        firstName.text = fName + " " + LastName;
        Sprite[] _sprite = Resources.LoadAll<Sprite>(Params.AvatarSSName);
        for (int i = 0; i < _sprite.Length; i++)
        {
            if (_sprite[i].name == AvatarName)
            {
                avatarImage.sprite = _sprite[i];
            }
        }
    }
}
