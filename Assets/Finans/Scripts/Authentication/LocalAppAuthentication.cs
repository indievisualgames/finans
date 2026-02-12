using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ricimi;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static IFirestoreEnums;

[RequireComponent(typeof(InternetConnectivityCheck))]
public class LocalAppAuthentication : Elevator
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField parentPin;
    [SerializeField] private TMP_InputField childPin;
    [SerializeField] private TMP_Dropdown childIds;
    [SerializeField] private TMP_InputField setNewParentPin;
    [SerializeField] private TMP_InputField confirmPin;
    [SerializeField] private Button setPinButton;
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject loginOptions;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject messageBoxPopupPrefab;

    [Header("Error Messages")]
    [SerializeField] private TMP_Text parentErrorText;
    [SerializeField] private TMP_Text childErrorText;
    [SerializeField] private TMP_Text setPinErrorText;
    [SerializeField] private TMP_Text parentPinHint;
    [SerializeField] private TMP_Text childPinHint;
    [SerializeField] private TMP_Text setNewParentPinHint;

    [Header("Private")]
    private IFirestoreOperator FirestoreClient;
    private Dictionary<string, object> _parentData = new Dictionary<string, object>();
    private List<ChildItem> _childItems = new();
    private bool _isParentLogin = false;
    /* private GameObject popup;
     private bool _autoRetryDone = false;
     private InternetConnectivityCheck _internetConnectivityCheck;*/
    private string _enteredPin = "";
    private bool _onceEntered = false;
    private string context = "LocalAppAuthentication";
    [Serializable]
    public class ChildItem
    {
        public string ID;
        public string Name;
    }

    void Awake()
    {
        try
        {
            if (!PlayerInfo.IsAppAuthenticated)
            {
                Logger.LogInfo("User not authenticated, redirecting to HomeScene", context);
                Transition.LoadLevel(SceneName.HomeScene.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
                return;
            }

            if (canvas == null)
            {
                var found = FindFirstObjectByType<Canvas>();
                if (found != null) { canvas = found; }
                else { Logger.LogError("Canvas not found in scene during Start", context); }
            }

            if (parentPin != null) parentPin.characterLimit = Params.pinCharLimit;
            if (childPin != null) childPin.characterLimit = Params.pinCharLimit;

            Logger.LogInfo("LocalAppAuthentication initialized successfully", context);
        }
        catch (Exception ex)
        {
            Logger.LogError("Something went wrong during awake", context, ex);
        }
    }


    async void Start()
    {
        try
        {
            FirestoreClient = new FirestoreDataOperationManager();
            await InitializeAuthentication();
        }
        catch (Exception ex)
        {
            Logger.LogError("Something went wrong while initializing authentication system", context, ex);
        }
    }

    private async Task InitializeAuthentication()
    {
        try
        {
            //  _internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
            /*  if (_internetConnectivityCheck == null)
              {
                  Logger.LogError("InternetConnectivityCheck component not found", context);
                  return;
              }*/

            // Subscribe to connectivity change events so we can auto-retry when internet returns
            // _internetConnectivityCheck.ConnectivityChanged += OnConnectivityRestored;
            await ActionOnParentDataLoad();
            /*  if (await InternetConnectivityChecker.CheckInternetConnectivityAsync())
              {
                  await ActionOnParentDataLoad();
              }
              else
              {
                  Logger.LogWarning("No internet connectivity found", context);
                  retryAction += async () => await ActionOnParentDataLoad();
                  popup = ShowNoConnectivityPopup(canvas, _internetConnectivityCheck, messageBoxPopupPrefab, false, false, true);
              }*/
        }
        catch (Exception ex)
        {
            Logger.LogError("Authentication initialization failed", context, ex);
        }
        finally
        {
            if (loading != null)
            {
                loading.SetActive(false);
            }
        }
    }

    public void OnParentButtonSelected()
    {
        try
        {
            _isParentLogin = true;
            Logger.LogInfo("Parent login mode selected", context);
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to set parent login mode", context, ex);
        }
    }

    public void OnChildButtonSelected()
    {
        try
        {
            if (_parentData != null && _parentData.ContainsKey(ParentProfile.children.ToString()))
            {
                _isParentLogin = false;
                _childItems.Clear();

                var childrenData = _parentData[ParentProfile.children.ToString()] as Dictionary<string, object>;
                if (childrenData != null)
                {
                    foreach (var item in childrenData)
                    {
                        _childItems.Add(new ChildItem() { Name = (string)item.Value, ID = (string)item.Key });
                    }
                }

                if (childIds != null)
                {
                    childIds.ClearOptions();
                    for (int i = 0; i < _childItems.Count; i++)
                    {
                        childIds.AddOptions(new List<string>() { _childItems[i].Name });
                    }

                }



                // Clear previous error/hint texts for a fresh attempt
                if (childErrorText != null) childErrorText.text = string.Empty;
                if (childPinHint != null) childPinHint.text = string.Empty;



                if (!_childItems.Any())
                {
                    Logger.LogWarning("No child accounts found", context);
                    NoChildAccountFound();
                }
                else
                {
                    Logger.LogInfo($"Found {_childItems.Count} child accounts", context);
                }
            }
            else
            {
                Logger.LogWarning("No children data found in parent profile", context);
                NoChildAccountFound();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to process child button selection", context, ex);
            NoChildAccountFound();
        }
    }



    public async void OnNextForLogin()
    {
        try
        {
            if (!ValidateInput()) return;

            if (loading != null)
            {
                loading.SetActive(true);
            }

            await OnLoginAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError("Login process failed.. Please try again", context, ex);
        }
    }

    private bool ValidateInput()
    {
        try
        {
            if (_isParentLogin)
            {
                if (parentPin == null || string.IsNullOrEmpty(parentPin.text))
                {
                    if (parentErrorText != null)
                    {
                        parentErrorText.text = Message.EmptyPin;
                    }
                    return false;
                }
            }
            else if (!_isParentLogin)
            {
                if (_childItems.Any())
                {
                    if (childPin == null || string.IsNullOrEmpty(childPin.text))
                    {
                        if (childErrorText != null)
                        {
                            childErrorText.text = Message.EmptyPin;
                        }
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError("Input validation failed", context, ex);
            return false;
        }
    }

    private async Task OnLoginAsync()
    {
        try
        {
            if (_isParentLogin)
            {
                await HandleParentLogin();
            }
            else
            {
                await HandleChildLogin();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Login failed. Please try again", context, ex);
        }
        finally
        {
            if (loading != null)
            {
                loading.SetActive(false);
            }

        }
    }

    private async Task HandleParentLogin()
    {
        try
        {
            if (parentPin == null || _parentData == null)
            {
                Logger.LogError("Required data not available for parent login", context);
                return;
            }

            if (!_parentData.TryGetValue(ParentProfile.pin.ToString(), out object storedPinObj) || storedPinObj is not string storedPin)
            {
                Logger.LogError("No PIN found in parent authentication data", context);
                return;
            }

            if (parentPin.text == storedPin)
            {
                Logger.LogInfo($"Parent login successful for user: {PlayerInfo.AuthenticatedID}", context);
                FirestoreDatabase.ParentData = _parentData;
                Transition.LoadLevel(SceneName.ParentAccount.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
            }
            else
            {
                Logger.LogWarning("Incorrect parent PIN entered", context);
                if (parentErrorText != null)
                {
                    parentErrorText.text = Message.IncorrectPinError;
                }
                if (parentPinHint != null)
                {
                    _parentData.TryGetValue(ParentProfile.pinhint.ToString(), out object pinHintObj);
                    var pinHint = pinHintObj as string;
                    parentPinHint.text = $"Pin hint: {pinHint ?? "No hint available"}";
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Parent login handling failed. Please try again.", context, ex);
        }
    }

    private async Task HandleChildLogin()
    {
        try
        {
            if (childIds == null || _childItems == null || _childItems.Count == 0)
            {
                Logger.LogError("Child selection data not available", context);
                return;
            }

            var selectedChildIndex = childIds.value;
            if (selectedChildIndex < 0 || selectedChildIndex >= _childItems.Count)
            {
                Logger.LogError($"Invalid child selection index: {selectedChildIndex}", context);
                return;
            }

            if (await HaveConnectivity(canvas, messageBoxPopupPrefab, loading))
            {

                var selectedChild = _childItems[selectedChildIndex];
                Logger.LogInfo($"Attempting child login for: {selectedChild.Name} using pin {childPin.text}", context);


                Dictionary<string, object> childData = await FirestoreClient.GetFirestoreDocument(
                    FSCollection.parent.ToString(),
                    PlayerInfo.AuthenticatedID,
                    FSCollection.children.ToString(),
                    selectedChild.ID);



                if (childData == null || !childData.ContainsKey(FSMapField.profile.ToString()))
                {
                    Logger.LogError("Child profile data not found", context);
                    return;
                }

                Dictionary<string, object> childProfileData = (Dictionary<string, object>)childData[FSMapField.profile.ToString()];
                var childPinValue = childProfileData[ChildProfile.pin.ToString()] as string;

                if (string.IsNullOrEmpty(childPinValue))
                {
                    Logger.LogError("Child PIN not found in profile", context);
                    return;
                }

                if (childPin != null && childPin.text == childPinValue)
                {
                    PlayerInfo.AuthenticatedChildID = selectedChild.ID;
                    FirestoreDatabase.ChildData = childData;

                    if (childProfileData.ContainsKey(ChildProfile.avatar.ToString()))
                    {
                        PlayerInfo.AvatarURL = (string)childProfileData[ChildProfile.avatar.ToString()];
                    }

                    if (childProfileData.ContainsKey(ChildProfile.firstname.ToString()))
                    {
                        PlayerInfo.ChildName = childProfileData[ChildProfile.firstname.ToString()] as string;
                    }

                    Logger.LogInfo($"Child login successful for: {selectedChild.Name}", context);
                    Transition.LoadLevel(SceneName.Level.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
                }
                else
                {
                    Logger.LogWarning("Incorrect child PIN entered", context);
                    if (childErrorText != null)
                    {
                        childErrorText.text = Message.IncorrectPinError;
                    }
                    if (childPinHint != null)
                    {
                        childProfileData.TryGetValue(ChildProfile.pinhint.ToString(), out object pinHintObj);
                        var pinHint = pinHintObj as string;
                        childPinHint.text = $"Pin hint: {pinHint ?? "No hint available"}";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Child login handling failed. Please try again.", context, ex);
        }
    }

    void NoChildAccountFound()
    {
        try
        {
            if (childPin != null) childPin.readOnly = true;
            if (childIds != null) childIds.interactable = false;
            if (childErrorText != null) childErrorText.text = Message.ChildAccountNotFound;

            Logger.LogWarning("No child account found for user", context);
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to handle no child account scenario", context, ex);
        }
    }

    public void OnPasswordEnter()
    {
        try
        {
            if (parentErrorText != null) parentErrorText.text = "";
            if (childErrorText != null) childErrorText.text = "";
            if (parentPinHint != null) parentPinHint.text = "";
            if (childPinHint != null) childPinHint.text = "";
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to clear error messages", context, ex);
        }
    }

    public void OnClose()
    {
        OnPasswordEnter();
    }
    /*  private void OnConnectivityRestored(bool isConnected)
      {
          try
          {
              if (!isConnected) return;
              if (_autoRetryDone) return;
              _autoRetryDone = true;
              Logger.LogInfo("Internet connectivity restored (event), retrying action", context);
              RetryTheAction(popup);
          }
          catch (Exception ex)
          {
              Logger.LogError("OnConnectivityRestored handler failed", context, ex);
          }
      }
  */
    /*   void OnDestroy()
       {
           try
           {
               if (_internetConnectivityCheck != null)
               {
                   _internetConnectivityCheck.ConnectivityChanged -= OnConnectivityRestored;
               }
           }
           catch { }
       }
   */
    public async void SetNewParentPin()
    {
        try
        {
            await SettingNewParentPin();
            if (setNewParentPinHint != null)
            {
                setNewParentPinHint.text = "New pin updated successfully";
            }
            Logger.LogInfo("Parent PIN updated successfully", context);
            Transition.LoadLevel(SceneName.Level.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to set new parent PIN: {ex.Message}", context, ex);
            if (setNewParentPinHint != null)
            {
                setNewParentPinHint.text = "Failed to update PIN. Please try again.";
            }
        }
    }

    async Task SettingNewParentPin()
    {
        try
        {
            if (string.IsNullOrEmpty(setNewParentPin?.text))
            {
                throw new InvalidOperationException("New PIN text is empty");
            }
            loading.SetActive(true);
            Dictionary<string, object> updatePinData = new Dictionary<string, object>(){
                {FSMapField.profile.ToString(), new Dictionary<string, object>(){
                    {ParentProfile.pin.ToString(), setNewParentPin.text},
                    {ParentProfile.pinhint.ToString(), setNewParentPinHint?.text ?? ""}
                }}
            };

            Logger.LogInfo("Updating parent PIN in Firestore", context);
            bool success = false;
            if (await HaveConnectivity(canvas, messageBoxPopupPrefab, loading))
            {
                success = await FirestoreClient.FirestoreDataSave(
                    FSCollection.parent.ToString(),
                    PlayerInfo.AuthenticatedID,
                    updatePinData);

                if (!success)
                {
                    throw new Exception("Failed to save PIN to Firestore");
                }
                loading.SetActive(false);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to update parent PIN", context, ex);
            throw;
        }
    }

    public void OnNewParentPinEnterComplete(TMP_InputField _inputField)
    {
        try
        {
            if (_inputField == null) return;

            if (_inputField.text.Length == 4 && _onceEntered)
            {
                if (_enteredPin == _inputField.text)
                {
                    if (setPinErrorText != null) setPinErrorText.text = string.Empty;
                    if (setNewParentPinHint != null && setNewParentPinHint.transform.parent?.parent?.parent != null)
                    {
                        setNewParentPinHint.transform.parent.parent.parent.gameObject.SetActive(true);
                    }
                    Logger.LogInfo("PIN confirmation successful", context);
                }
                else
                {
                    if (setPinErrorText != null) setPinErrorText.text = "Pin does not match";
                    Logger.LogWarning("PIN confirmation failed - pins don't match", context);
                }
            }
            else if (_inputField.text.Length == 4 && !_onceEntered)
            {
                _enteredPin = _inputField.text;
                _inputField.text = string.Empty;
                if (_inputField.placeholder != null)
                {
                    var placeholderText = _inputField.placeholder.GetComponent<TextMeshProUGUI>();
                    if (placeholderText != null)
                    {
                        placeholderText.text = "Confirm your pin";
                    }
                }
                _onceEntered = true;
                Logger.LogInfo("First PIN entered, waiting for confirmation", context);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to process PIN entry", context, ex);
        }
    }

    public void OnNewParentConfirmPHEnterComplete(TMP_InputField _inputField)
    {
        try
        {
            if (_inputField != null && _inputField.text.Length >= 3)
            {
                if (setPinButton != null)
                {
                    setPinButton.GetComponent<Button>().enabled = true;
                }
                Logger.LogInfo("PIN confirmation hint entered, enabling set button", context);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to process PIN confirmation hint", context, ex);
        }
    }

    public void OnForgotParentPinClose(TMP_InputField _inputField)
    {
        try
        {
            if (parentPin != null) parentPin.text = string.Empty;
            if (setPinButton != null) setPinButton.GetComponent<Button>().enabled = false;
            if (setNewParentPin != null) setNewParentPin.text = string.Empty;
            if (confirmPin != null) confirmPin.text = string.Empty;
            if (setNewParentPinHint != null) setNewParentPinHint.text = string.Empty;

            _onceEntered = false;

            if (_inputField != null && _inputField.placeholder != null)
            {
                var placeholderText = _inputField.placeholder.GetComponent<TextMeshProUGUI>();
                if (placeholderText != null)
                {
                    placeholderText.text = "Enter New Pin";
                }
            }

            if (setNewParentPinHint != null && setNewParentPinHint.transform.parent?.parent?.parent != null)
            {
                setNewParentPinHint.transform.parent.parent.parent.gameObject.SetActive(false);
            }

            Logger.LogInfo("PIN reset form cleared", context);
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to clear PIN reset form", context, ex);
        }
    }
    /********************Actions on connection found*******************************************/
    async Task ActionOnParentDataLoad()
    {
        Logger.LogInfo("Internet connectivity confirmed, loading parent data", context);
        _parentData = await FirestoreClient.GetFirestoreDataField(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSMapField.profile.ToString());
        if (_parentData == null || _parentData.Count == 0)
        {
            Logger.LogWarning("Parent profile data missing or empty", context);
        }

        if (loginOptions != null)
        {
            loginOptions.SetActive(true);
        }
        else
        {
            Logger.LogWarning("Login options UI element not assigned", context);
        }
    }
}
