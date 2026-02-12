using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine.UI;
using UnityEngine.Events;
using Ricimi;
using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Firebase.Extensions;

public class GoogleAuthenticator : Elevator
{
    [Header("UI References")]
    [SerializeField] private GameObject providerLoading;
    [SerializeField] private TMP_Text error;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject messageBoxPopupPrefab;
    [SerializeField] private bool forceAccountChooser = true;

    [Header("Google Sign-In Settings")]
    [Tooltip("Paste your Web Client ID from Firebase Console here")]
    [SerializeField] private string webClientId = "YOUR_WEB_CLIENT_ID.apps.googleusercontent.com";
    [Tooltip("The name of the GameObject that receives plugin callbacks")]
    [SerializeField] private string unityGameObjectName = "Scriptable"; // must match the GameObject name

    [Header("Private Fields")]
    private FirebaseAuthenticationCheck fac;
    private bool isAuthenticating = false;
    // private AndroidJavaClass unityPlayer;
    //private AndroidJavaObject currentActivity;
    private AndroidJavaClass pluginConfigClass;
    private AndroidJavaClass pluginPickerClass;
    private bool playGamesPlatformActivated = false;
    // private List<string> googleAccounts = new List<string>();

    void Awake()
    {
        if (!string.IsNullOrEmpty(unityGameObjectName))
            gameObject.name = unityGameObjectName;
        // DontDestroyOnLoad(gameObject);


#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            pluginConfigClass = new AndroidJavaClass("com.eparent.googleaccountpickerplugin.GoogleAccountPickerConfig");
             pluginPickerClass = new AndroidJavaClass("com.eparent.googleaccountpickerplugin.GoogleAccountPicker");
            // Optional setters; call only if present in plugin to avoid noisy errors
            var setIdOk = SafeCallStatic("setWebClientId", webClientId);
            var setNameOk = SafeCallStatic("setUnityGameObjectName", unityGameObjectName);
            Logger.LogInfo($"Config initialized: WebClientId={(setIdOk ? webClientId : "(not set)")}, GameObject={(setNameOk ? unityGameObjectName : "(not set)")}", "GoogleAuthenticator");
       }
        catch (AndroidJavaException e)
        {
            Debug.LogError("[GoogleAuthenticator] Failed to load plugin class: " + e);
        }
       
#endif
    }
    void Start()
    {

    }
    private bool GetFirebaseAuthInstance()
    {
        try
        {
            Logger.LogInfo("Initializing GoogleAuthenticator", "GoogleAuthenticator");
            fac = transform.GetComponent<FirebaseAuthenticationCheck>();
            if (fac == null)
            {
                Logger.LogError("FirebaseAuthenticationCheck component not found", "GoogleAuthenticator");
            }
            else
            {
                Logger.LogInfo("FirebaseAuthenticationCheck component found", "GoogleAuthenticator");
            }

        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to initialize GoogleAuthenticator", "GoogleAuthenticator", ex);
        }

        return fac != null;
    }
    public async void GoogleAuthenticateAsync()
    {
        if (error != null)
        {
            error.text = string.Empty;
        }

        if (isAuthenticating)
        {
            Logger.LogInfo("Authentication already in progress; ignoring duplicate request", "GoogleAuthenticator");
            return;
        }
        try
        {
            if (providerLoading) { providerLoading.SetActive(true); }
            isAuthenticating = true;
            if (InitializePlayGamesPlatform())
            {
                if (await HaveConnectivity(canvas, messageBoxPopupPrefab, providerLoading))
                {
                    Logger.LogInfo("Starting Google authentication process", "GoogleAuthenticator");

                    if (forceAccountChooser)
                    {
                        PlayerInfo.CurrentLoginAttempt = "Google";
                        LaunchGoogleAccountPicker();
                    }
                    else
                    { // Proceed with normal authentication flow
                        ProceedWithAuthentication();
                    }
                }
                else
                {
                    HandleAuthenticationError(Message.MBNoInternetMessage);
                    isAuthenticating = false;
                }
            }
            else
            {
                HandleAuthenticationError(Message.AuthenticationError);
                isAuthenticating = false;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Google authentication process failed", "GoogleAuth", ex);
            HandleAuthenticationError(Message.AuthenticationError);
            isAuthenticating = false;
        }
    }



    private void ProceedWithAuthentication()
    {
        try
        {
            isAuthenticating = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            PlayGamesPlatform.Instance.ManuallyAuthenticate(OnGooglePlayGamesSignInResult);
#endif
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to proceed with authentication", "GoogleAuthenticator", ex);

            HandleAuthenticationError(Message.AuthenticationError);
        }
    }
    private void OnGooglePlayGamesSignInResult(SignInStatus signInStatus)
    {
        try
        {
            Logger.LogInfo($"Google Play Games sign-in result: {signInStatus}", "GoogleAuthenticator");

            if (signInStatus == SignInStatus.Success)
            {
                Logger.LogInfo("Google Play Games sign-in successful, requesting server-side access", "GoogleAuthenticator");

#if UNITY_ANDROID && !UNITY_EDITOR
                PlayGamesPlatform.Instance.RequestServerSideAccess(false, authCode =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(authCode))
                        {
                            Logger.LogError("Received empty auth code from Google Play Games", "GoogleAuthenticator");
                            HandleAuthenticationError(Message.DBAuthenticationError);
                            return;
                        }

                        Logger.LogInfo("Received auth code from Google Play Games, trying to get Firebase credential", "GoogleAuthenticator");

                        Firebase.Auth.Credential credential = Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);

                        if (credential == null)
                        {
                            Logger.LogError("Failed to create Firebase credential from auth code", "GoogleAuthenticator");
                            HandleAuthenticationError(Message.DBAuthenticationError);
                            return;
                        }

                        if (!credential.IsValid())
                        {
                            Logger.LogError("Created Firebase credential is invalid", "GoogleAuthenticator");
                            HandleAuthenticationError(Message.DBAuthenticationError);
                            return;
                        }

                        Logger.LogInfo("Firebase credential created successfully, signing in", "GoogleAuthenticator");

                        if (fac?.auth == null)
                        {
                            Logger.LogError("Firebase authentication instance not available", "GoogleAuth");
                            HandleAuthenticationError(Message.DBAuthenticationError);
                            return;
                        }

                        fac.auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                        {
                            try
                            {
                                if (task.IsCanceled)
                                {
                                    Logger.LogWarning("Firebase authentication was canceled", "GoogleAuthenticator");
                                    HandleAuthenticationError(Message.AuthenticationCanceled);
                                    return;
                                }

                                if (task.IsFaulted)
                                {
                                    Logger.LogError($"Firebase authentication failed: {task.Exception}", "GoogleAuthenticator");
                                    HandleAuthenticationError(Message.AuthenticationFaulted);
                                    return;
                                }

                                if (task.IsCompletedSuccessfully)
                                {
                                    Logger.LogInfo("Firebase authentication completed successfully", "GoogleAuthenticator");

                                    if (fac != null)
                                    {
                                        fac.user = task.Result.User;
                                        isAuthenticating = false;
                                        if (providerLoading) { providerLoading.SetActive(false); }
                                        fac.LetsPlay();
                                    }
                                    else
                                    {
                                        Logger.LogError("FirebaseAuthenticationCheck instance not available", "GoogleAuthenticator");
                                        HandleAuthenticationError(Message.DBAuthenticationError);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("Error processing Firebase authentication result", "GoogleAuth", ex);
                                //        isAuthenticating = false;
                                HandleAuthenticationError(Message.DBAuthenticationError);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error in Google Play Games authentication callback: {ex.Message}", "GoogleAuth", ex);
                        // isAuthenticating = false;
                        HandleAuthenticationError(Message.DBAuthenticationError);
                    }
                });
#endif
            }
            else if (signInStatus == SignInStatus.InternalError)
            {
                Logger.LogError("Google Play Games internal error during sign-in", "GoogleAuthenticator");
                isAuthenticating = false;
                if (providerLoading) { providerLoading.SetActive(false); }
                HandleAuthenticationError(Message.AuthenticationInternalError);
            }
            else
            {
                Logger.LogWarning($"Google Play Games sign-in failed with status: {signInStatus}", "GoogleAuthenticator");
                isAuthenticating = false;
                if (providerLoading) { providerLoading.SetActive(false); }
                HandleAuthenticationError(Message.AuthenticationError);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error handling Google Play Games sign-in result", "GoogleAuthenticator", ex);
            isAuthenticating = false;
            if (providerLoading) { providerLoading.SetActive(false); }
            HandleAuthenticationError(Message.AuthenticationError);
        }
    }

    private void HandleAuthenticationError(string errorMessage)
    {
        try
        {
            if (error != null)
            {
                error.text = errorMessage;
            }
            else
            {
                Logger.LogWarning("Error text UI element not assigned", "GoogleAuth");
            }
            isAuthenticating = false;
            if (providerLoading) { providerLoading.SetActive(false); }

            Logger.LogWarning($"Authentication error handled: {errorMessage}", "GoogleAuth");
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to handle authentication error", "GoogleAuth", ex);
        }
    }

    private bool InitializePlayGamesPlatform()
    {
        if (!playGamesPlatformActivated)
        {
            if (!GetFirebaseAuthInstance()) { return false; }
            try
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                PlayGamesPlatform.DebugLogEnabled = true;
                PlayGamesPlatform.Activate();
#endif
                Logger.LogInfo("Google Play Games platform activated successfully", "GoogleAuthenticator");
                playGamesPlatformActivated = true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to initialize Google Play Games platform", "GoogleAuth", ex);
                return false;
            }

        }
        return true;

    }

    /*Google Account Picker*/

    /// <summary>
    /// Launches the Google account picker.
    /// Your Activity is already coded to signOut() first, so the chooser will always show again.
    /// </summary>
    public void LaunchGoogleAccountPicker()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (pluginPickerClass == null)
        {
            Logger.LogError("Plugin picker class not available.",  "GoogleAuthenticator");
            return;
        }

      using (AndroidJavaObject unityActivity = GetUnityActivity())
        {
            try
            {
                Logger.LogInfo("Launching Google Account Picker", "GoogleAuthenticator");
                pluginPickerClass.CallStatic("LaunchAccountPicker", unityActivity);
            }
            catch (AndroidJavaException e)
            {
                Logger.LogError("LaunchAccountPicker failed: ", "GoogleAuthenticator" , e);
            }
        }
#else
        Logger.LogInfo("LaunchGoogleAccountPicker called in Editor (no-op).", "GoogleAuthenticator");
#endif
    }


    /// <summary>
    /// Optional: Sign out without launching the picker.
    /// NOTE: This requires your plugin to expose a static SignOut(Activity) method.
    /// If it doesn't exist, this call safely no-ops and logs a warning.
    /// </summary>
    public void SignOut()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
       
        using (AndroidJavaObject unityActivity = GetUnityActivity())
        {
            if (!SafeCallStatic("SignOut", unityActivity))
            {
                Debug.LogWarning("[GoogleAuthenticator] Plugin SignOut() not found. (This is optional; Play Games v2 doesn’t require in-game sign-out.)");
            }/*else{
                pluginClass.CallStatic("SignOut", unityActivity);
            }*/
        }
#else
        Debug.Log("[GoogleAuthenticator] SignOut called in Editor (no-op).");
#endif
    }

    /// <summary>
    /// Data model matching JSON from plugin.
    /// </summary>
    [Serializable]
    public class GoogleAccountData
    {
        public string email;
        public string idToken;
        public string displayName;
        public string error; // if something went wrong
    }
    /// <summary>
    /// Callback from Android plugin with the JSON result.
    /// Called automatically by UnityPlayer.UnitySendMessage().
    /// </summary>
    public void OnAccountPicked(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Logger.LogError("Empty response from Google Sign-In", "GoogleAuthenticator");
            providerLoading.SetActive(false);
            return;
        }

        if (message.StartsWith("ERROR"))
        {
            providerLoading.SetActive(false);
            Debug.LogError("[GoogleAuthenticator] Google Sign-In failed: " + message);
            return;
        }

        Logger.LogInfo($"Google Sign-In Success. Token received: {message}", "GoogleAuthenticator");
        // ---- Option A: Firebase with ID Token ----
        // AuthenticateWithFirebase(account.idToken);

        // ---- Option B: If using Play Games ----
        // (You already have PlayGamesPlatform.Instance.RequestServerSideAccess working)
        ProceedWithAuthentication();
    }
    // Kept for compatibility: If plugin sends split callbacks, it still funnel them to the single handler for simplicity
    public void OnGoogleSignInSuccess(string payload) => OnAccountPicked(payload);
    public void OnGoogleSignInFailed(string error) => OnAccountPicked("ERROR: " + error);
    // === Helpers ===
    private AndroidJavaObject GetUnityActivity()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }
    /// <summary>
    /// Calls a static method on the plugin class, returns false if method is missing or throws.
    /// Keeps things resilient across different plugin builds.
    /// </summary>
    private bool SafeCallStatic(string methodName, params object[] args)
    {
        try
        {
            if (pluginConfigClass == null) { Logger.LogError("Plugin config class not available.", "GoogleAuthenticator"); return false; }
            pluginConfigClass.CallStatic(methodName, args);
            return true;
        }
        catch (AndroidJavaException e)
        {
            // Method may not exist in your current plugin build — that's fine for optional calls.
            Debug.LogWarning($"[GoogleAuthenticator] CallStatic('{methodName}') failed or missing: {e.Message}");
            return false;
        }
    }
}

