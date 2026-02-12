using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine.UI;
using UnityEngine.Events;
using Facebook.Unity;
using Firebase.Auth;
using System.Collections.Generic;
using System;
using Firebase.Extensions;



public class FacebookAuthenticator : Elevator
{
    FirebaseAuthenticationCheck fac;
    public GameObject providerLoading;
    public TMP_Text error;
    public Canvas canvas;
    public GameObject messageBoxPopupPrefab;

    private bool isAuthenticating = false;
    // private bool hasStartedFirebaseSignIn = false;
    private const string LogContext = "FacebookAuthenticator";
    [SerializeField] private bool enableWebViewFallback = true;
    [SerializeField] private float loginTimeoutSeconds = 30f;
    private bool _webFallbackTried = false;
    private readonly System.Collections.Generic.List<string> _permissions = new System.Collections.Generic.List<string> { "public_profile", "email" };
    private Coroutine _watchdogCoroutine;

    void Start()
    {
        try
        {
            fac = transform.GetComponent<FirebaseAuthenticationCheck>();
            if (fac == null)
            {
                Logger.LogError("FirebaseAuthenticationCheck component not found", LogContext);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Firebase Authentication Check Failed", LogContext, ex);
        }
    }

    public async void FacebookAuthenticateAsync()
    {

        SetError(string.Empty);
        if (isAuthenticating)
        {
            Logger.LogInfo("Authentication already in progress; ignoring duplicate request", LogContext);
            return;
        }
        isAuthenticating = true;
        try
        {
            if (providerLoading != null) providerLoading.SetActive(true);

            if (!await HaveConnectivity(canvas, messageBoxPopupPrefab, providerLoading))
            {
                Logger.LogWarning("No internet connectivity for Facebook authentication", LogContext);
                if (providerLoading != null) providerLoading.SetActive(false);
                isAuthenticating = false;
                return;
            }

            if (!FB.IsInitialized)
            {
                Logger.LogInfo("Initializing Facebook SDK", LogContext);
                FB.Init(OnInitComplete, OnHideUnity);
                // Continue in OnInitComplete
                return;
            }

            StartFacebookLogin();
        }
        catch (Exception ex)
        {
            Logger.LogError("Facebook authentication process failed", LogContext, ex);
            SetError(Message.FBAuthenticationFailed);
            if (providerLoading != null) providerLoading.SetActive(false);
            isAuthenticating = false;
        }
    }

    void StartFacebookLogin()
    {
        try
        {
            if (FB.IsLoggedIn) { Logger.LogInfo("Facebook logged in user found...Logging out before new login task", LogContext); FB.LogOut(); }
            _watchdogCoroutine = StartCoroutine(FacebookAuthWatchdog(loginTimeoutSeconds));
            isAuthenticating = true;
            PlayerInfo.CurrentLoginAttempt = "Facebook";
            Logger.LogInfo("Starting Facebook login", LogContext);
            FB.LogInWithReadPermissions(_permissions, HandleLoginResult);
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to start Facebook login", LogContext, ex);
            SetError(Message.AuthenticationError);
            if (providerLoading != null) providerLoading.SetActive(false);
            isAuthenticating = false;
        }
    }

    void HandleLoginResult(ILoginResult result)
    {
        try
        {
            if (result == null)
            {
                Logger.LogWarning("Facebook login returned null result", LogContext);
                SetError(Message.AuthenticationError);
                if (providerLoading != null) providerLoading.SetActive(false);
                if (_watchdogCoroutine != null) { StopCoroutine(_watchdogCoroutine); _watchdogCoroutine = null; }
                isAuthenticating = false;
                return;
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                Logger.LogError($"Facebook login error: {result.Error}", LogContext);
                SetError(Message.AuthenticationError);
                if (providerLoading != null) providerLoading.SetActive(false);
                if (_watchdogCoroutine != null) { StopCoroutine(_watchdogCoroutine); _watchdogCoroutine = null; }
                isAuthenticating = false;
                return;
            }

            if (result.Cancelled)
            {
                Logger.LogWarning("Facebook login cancelled by user", LogContext);
                SetError(Message.AuthenticationCanceled);
                if (providerLoading != null) providerLoading.SetActive(false);
                if (_watchdogCoroutine != null) { StopCoroutine(_watchdogCoroutine); _watchdogCoroutine = null; }
                isAuthenticating = false;
                return;
            }

            Logger.LogInfo("Facebook login successful; will continue with Firebase sign-in", LogContext);
            TryContinueWithFirebaseSignInFromAccessToken();
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed while handling Facebook login result", LogContext, ex);
            SetError(Message.DBAuthenticationError);
            if (providerLoading != null) providerLoading.SetActive(false);
            isAuthenticating = false;
        }
    }

    void TryContinueWithFirebaseSignInFromAccessToken()
    {
        try
        {
            var accessToken = AccessToken.CurrentAccessToken;
            if (accessToken == null || string.IsNullOrEmpty(accessToken.TokenString))
            {
                // Token not ready yet; keep authenticating so focus/watchdog can continue the flow.
                Logger.LogDebug("Access token not yet available; will retry on focus/timeout.", LogContext);
                return;
            }
            Logger.LogInfo($"keyhashes used by the app is:  {accessToken.GetHashCode()}", LogContext); ;
            Credential credential =
                      FacebookAuthProvider.GetCredential(accessToken.TokenString);
            if (credential == null)
            {
                Logger.LogError("Failed to create Firebase credential from Facebook token", LogContext);
                SetError(Message.FailedFCFromFT);
                if (providerLoading != null) providerLoading.SetActive(false);
                isAuthenticating = false;
                return;
            }

            if (fac == null || fac.auth == null)
            {
                Logger.LogError("FirebaseAuthenticationCheck or auth instance is null", LogContext);
                SetError(Message.FirebaseCheckInstanceNull);
                if (providerLoading != null) providerLoading.SetActive(false);
                isAuthenticating = false;
                return;
            }


            fac.auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                try
                {
                    isAuthenticating = false;
                    Logger.LogInfo("Firebase authentication completed successfully", LogContext);
                    if (task.IsCanceled)
                    {
                        if (providerLoading && providerLoading.activeSelf) { providerLoading.SetActive(false); }
                        if (error) { error.text = Message.AuthenticationCanceled; }
                        Logger.LogError(Message.AuthenticationCanceled, "FacebookAuthenticator", task.Exception);
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        if (providerLoading && providerLoading.activeSelf) { providerLoading.SetActive(false); }
                        if (error) { error.text = Message.AuthenticationFaulted; }
                        Logger.LogError(Message.AuthenticationFaulted, "FacebookAuthenticator", task.Exception);
                        return;
                    }

                    if (task.IsCompletedSuccessfully)
                    {
                        var firebaseUser = task.Result.User;
                        if (firebaseUser == null)
                        {
                            Logger.LogError("Firebase returned null user after successful authentication", LogContext);
                            if (providerLoading != null) providerLoading.SetActive(false);
                            return;
                        }

                        Logger.LogInfo($"Debug.Log Google Authenticator completed auth.SignInAndRetrieveDataWithCredentialAsync task sucessfully with credential validation {credential.IsValid()} and provider as {credential} ..", "FacebookAuthenticator");
                        fac.user = firebaseUser;
                        if (_watchdogCoroutine != null) { StopCoroutine(_watchdogCoroutine); _watchdogCoroutine = null; }
                        if (providerLoading != null) providerLoading.SetActive(false);
                        fac.LetsPlay();
                    }
                }

                catch (Exception authEx)
                {
                    Logger.LogError("Firebase authentication failed:", LogContext, authEx);
                    SetError(Message.FirebaseAuthFailed);
                    if (providerLoading != null) providerLoading.SetActive(false);
                    isAuthenticating = false;
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError("TryContinueWithFirebase SignInFromAccessToken failed", LogContext, ex);
            SetError(Message.SignInWithATFailed);
            if (providerLoading != null) providerLoading.SetActive(false);
            isAuthenticating = false;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        try
        {
            if (PlayerInfo.CurrentLoginAttempt == "Facebook" && isAuthenticating && hasFocus)
            {
                Logger.LogInfo("App regained focus; attempting to continue Facebook->Firebase sign-in", LogContext);
                TryContinueWithFirebaseSignInFromAccessToken();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("OnApplicationFocus handling failed", LogContext, ex);
        }
    }

    void OnApplicationPause(bool paused)
    {
        if (PlayerInfo.CurrentLoginAttempt == "Facebook" && isAuthenticating)
        {
            try
            {
                Logger.LogDebug($"OnApplicationPause: paused={paused}", LogContext);
            }
            catch (Exception ex)
            {
                Logger.LogError("OnApplicationPause handling failed", LogContext, ex);
            }
        }
    }

    void OnInitComplete()
    {
        try
        {
            Logger.LogInfo($"Facebook SDK initialized. IsLoggedIn={FB.IsLoggedIn} IsInitialized={FB.IsInitialized}", LogContext);
            FB.ActivateApp();
            StartFacebookLogin();
        }
        catch (Exception ex)
        {
            Logger.LogError("OnInitComplete handling failed", LogContext, ex);
            SetError(Message.AuthenticationError);
            if (providerLoading != null) providerLoading.SetActive(false);
            isAuthenticating = false;
        }
    }

    void OnHideUnity(bool isGameShown)
    {
        try
        {
            Logger.LogDebug($"OnHideUnity called. isGameShown={isGameShown}", LogContext);
        }
        catch (Exception ex)
        {
            Logger.LogError("OnHideUnity handling failed", LogContext, ex);
        }
    }

    void SetError(string message)
    {
        try
        {
            if (error != null) error.text = message;
        }
        catch { /* ignore UI assignment errors */ }
    }
    System.Collections.IEnumerator FacebookAuthWatchdog(float timeoutSeconds = 30f)
    {
        var start = Time.realtimeSinceStartup;
        while (isAuthenticating && Time.realtimeSinceStartup - start < timeoutSeconds)
            yield return null;

        if (isAuthenticating && (AccessToken.CurrentAccessToken == null || string.IsNullOrEmpty(AccessToken.CurrentAccessToken.TokenString)))
        {
            if (enableWebViewFallback && !_webFallbackTried)
            {
                Logger.LogWarning("Facebook login timeout. Retrying with WebView fallback.", LogContext);
                _webFallbackTried = true;
                TryWebViewFallbackLogin();
                yield break;
            }

            Logger.LogWarning("Facebook login timeout. Prompting retry.", LogContext);
            SetError(Message.FBAuthenticationTimeout);
            if (providerLoading != null) providerLoading.SetActive(false);
            isAuthenticating = false;
            // Optionally: show retry popup here
        }
    }

    void TryWebViewFallbackLogin()
    {
        try
        {
            if (providerLoading != null) providerLoading.SetActive(true);
            if (FB.IsLoggedIn)
            {
                FB.LogOut();
            }

            PlayerInfo.CurrentLoginAttempt = "Facebook";
            isAuthenticating = true;

            // Retry using the default SDK behavior (SDK v18 no longer exposes LoginBehavior APIs)
            Logger.LogInfo("Retrying Facebook login using default SDK behavior (fallback attempt)", LogContext);
            FB.LogInWithReadPermissions(_permissions, HandleLoginResult);
            _watchdogCoroutine = StartCoroutine(FacebookAuthWatchdog(loginTimeoutSeconds));
        }
        catch (Exception ex)
        {
            Logger.LogError("WebView fallback login failed", LogContext, ex);
            SetError(Message.AuthenticationError);
            if (providerLoading != null) providerLoading.SetActive(false);
            isAuthenticating = false;
        }
    }

    void OnDisable()
    {
        try
        {
            if (_watchdogCoroutine != null)
            {
                StopCoroutine(_watchdogCoroutine);
                _watchdogCoroutine = null;
            }
        }
        catch { }
    }

    void OnApplicationQuit()
    {
        try
        {
            if (_watchdogCoroutine != null)
            {
                StopCoroutine(_watchdogCoroutine);
                _watchdogCoroutine = null;
            }
            isAuthenticating = false;
        }
        catch { }
    }
}