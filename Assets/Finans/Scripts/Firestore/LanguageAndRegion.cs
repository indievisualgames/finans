using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ricimi;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static IFirestoreEnums;

public class LanguageAndRegion : MonoBehaviour
{
    private IFirestoreOperator FirestoreClient;
    [SerializeField] Image errorCircle;
    [SerializeField] TMP_InputField parentName;
    [SerializeField] TMP_Dropdown countryDropdown;
    [SerializeField] TMP_Text currency;
    [SerializeField] TMP_Dropdown languageDropdown;
    [SerializeField] TMP_InputField pin;
    [SerializeField] TMP_InputField pinHint;
    [SerializeField] TMP_Text errorText;
    bool isSelected = false;
    public GameObject loading;

    string country = string.Empty;
    string countryCode = string.Empty;

    List<string> countryCurrencyOptions = new List<string>();
    List<LanguageRegionData.CountryData> SortedLanguageRegionData;
    List<string> languageOptions = new List<string>();
    List<string> SortedlanguageOptions = new List<string>();

    void Awake()
    {
        pin.characterLimit = Params.pinCharLimit;
        pinHint.characterLimit = Params.pinHintCharLimit;
        SortedLanguageRegionData = LanguageRegionData.Country.GroupBy(x => x.Country).Select(x => x.First()).ToList().OrderBy(x => x.Country).ToList();
    }
    void Start()
    {
        FirestoreClient = new FirestoreDataOperationManager();
        PopulateCountryDropdown();
    }

    public void OnAgreementSelectToggle()
    {
        isSelected = !isSelected;
        if (isSelected)
        {
            if (errorCircle.gameObject.activeSelf)
            {
                errorCircle.gameObject.SetActive(false);
            }
        }
    }

    public void OnCountrySelected()
    {
        languageOptions.Clear();
        SortedlanguageOptions.Clear();
        for (int i = 0; i < SortedLanguageRegionData.Count; i++)
        {
            if (i == countryDropdown.value)
            {
                foreach (var countries in LanguageRegionData.Country)
                {
                    if (countries.Country == SortedLanguageRegionData[i].Country)
                    {
                        Debug.Log($"Found country language {countries.Language}");
                        languageOptions.Add(countries.Language);
                    }

                }
            }




            if (i == countryDropdown.value)
            {
                country = SortedLanguageRegionData[i].Country;
                countryCode = SortedLanguageRegionData[i].CountryCode;
                currency.text = SortedLanguageRegionData[i].Currency;
            }
        }

        SortedlanguageOptions = languageOptions.Distinct().ToList();
        PopulateLanguageDropdown();
    }

    bool ValidateFields()
    {
        if (parentName.text.Trim() == string.Empty)
        {
            errorText.text = Message.NameEmpty; return false;
        }
        else if (parentName.text.Trim().Length < 6)
        {
            errorText.text = Message.ShortName; return false;
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
    public async void OnSubmit()
    {
        if (ValidateFields())
        {
            if (isSelected)
            {
                loading.SetActive(true);
                if (await OnSubmitAsync())
                {
                    loading.SetActive(false);
                    Transition.LoadLevel(SceneName.ParentAccount.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
                };

            }
            else
            {
                errorCircle.gameObject.SetActive(true);
            }
        }

    }

    async Task<bool> OnSubmitAsync()
    {
        Dictionary<string, object> _data = new Dictionary<string, object>(){
        {FSMapField.profile.ToString(), FirestoreData.ParentProfileData(parentName.text.Trim(), country, currency.text.Trim(), languageDropdown.options[languageDropdown.value].text.Trim(), countryCode, pin.text.Trim(), pinHint.text ) }
        };
        return await FirestoreClient.FirestoreDataSave(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, _data);
    }
    void PopulateCountryDropdown()
    {
        List<string> countryOptions = new List<string>();
        for (int i = 0; i < SortedLanguageRegionData.Count; i++)
        {
            countryOptions.Add(SortedLanguageRegionData[i].Country);
            countryCurrencyOptions.Add(SortedLanguageRegionData[i].Currency);

        }
        countryDropdown.AddOptions(countryOptions);
        currency.text = SortedLanguageRegionData[countryDropdown.value].Currency;
        country = countryOptions[countryDropdown.value];
        countryCode = SortedLanguageRegionData[countryDropdown.value].CountryCode;
        OnCountrySelected();
        OperationFinished();


    }
    void PopulateLanguageDropdown()
    {
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(SortedlanguageOptions);
        Debug.Log(message: $"Language selected is {languageDropdown.options[languageDropdown.value].text}");

    }

    void OperationFinished()
    {
        loading.SetActive(false);
    }
}
