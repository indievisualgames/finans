using UnityEngine;
using static IFirestoreEnums;
using Ricimi;
using System;
using System.Collections;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif
public class FirebaseAuthenticationCheck : Elevator
{
    [Header("Firebase")]
    public Firebase.Auth.FirebaseAuth auth;
    public Firebase.Auth.FirebaseUser user;


    [Header("UI References")]
    [SerializeField] private GameObject firebaseAuthLoading;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject authenticationScene;
    [SerializeField] private GameObject messageBoxPopupPrefab;

    [Header("Private Fields")]
    //private  canvas;
    private GameObject _popup;
    // private InternetConnectivityCheck _internetConnectivityCheck;
    private IFirestoreOperator FirestoreClient;
    private string LogContext = "FirebaseAuthenticationCheck";
    private bool _isFirstLogin = false;
    private bool _isInitialized = false;

    [Header("Public Fields")]
    public bool FirstLoginCheck
    {
        get { return _isFirstLogin; }
        set { _isFirstLogin = value; }
    }

    void Awake()
    {
        try
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            InitializeFirebase();
        }
        catch (Exception ex)
        {
            Logger.LogError("Firebase initialization failed", LogContext, ex);
        }
    }
    async void Start()
    {
        try
        {
            FirestoreClient = new FirestoreDataOperationManager();
            //   _internetConnectivityCheck = GetComponent<InternetConnectivityCheck>();
            if (canvas == null)
            {
                var found = FindFirstObjectByType<Canvas>();
                if (found != null) { canvas = found; }
                else { Logger.LogError("Canvas not found in scene during start", LogContext); }

            }

            StartCoroutine(LoadTransitionImage());
            /*  if (!await InternetConnectivityChecker.CheckInternetConnectivityAsync())
              {
                  _popup = ShowNoConnectivityPopup(canvas, _internetConnectivityCheck, messageBoxPopupPrefab);
                  firebaseAuthLoading.SetActive(false);
                  Logger.LogError($"No connectivity found", LogContext);
              }*/
        }
        catch (Exception ex)
        {
            Logger.LogError("Something went wrong during start", LogContext, ex);
        }
    }
    private void InitializeFirebase()
    {
        try
        {
            Logger.Log(Logger.LogLevel.Info, "Initializing Firebase", LogContext);

            auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            if (auth == null)
            {
                Logger.LogError("Firebase Auth instance is null", LogContext);
                return;
            }

            Params.__auth = auth;
            auth.StateChanged += AuthStateChanged;

            _isInitialized = true;
            Logger.LogInfo("Firebase initialized successfully", LogContext);

            // Trigger initial state check
            AuthStateChanged(this, null);
        }
        catch (Exception ex)
        {
            Logger.LogError("Firebase initialization failed", LogContext, ex);
        }
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (!_isInitialized) return;
        try
        {
            if (auth == null)
            {
                Logger.LogWarning("AuthStateChanged invoked but auth instance is null", LogContext);
                return;
            }
            var currentUser = auth.CurrentUser;
            var userChanged = currentUser != user;

            if (userChanged)
            {
                var wasSignedIn = user != null && user.IsValid();
                var isSignedIn = currentUser != null && currentUser.IsValid();

                if (wasSignedIn && !isSignedIn)
                {
                    // User signed out
                    PlayerInfo.AuthenticatedID = string.Empty;
                    PlayerInfo.IsAppAuthenticated = false;
                    Logger.LogInfo("User signed out", LogContext);
                }
                else if (!wasSignedIn && isSignedIn)
                {
                    // User signed in
                    PlayerInfo.AuthenticatedID = currentUser.UserId;
                    PlayerInfo.IsAppAuthenticated = true;
                    Logger.LogInfo($"User signed in: {currentUser.UserId}", LogContext);
                }

                user = currentUser;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(Logger.LogLevel.Error, "Auth state change handling failed", LogContext, ex);
            if (firebaseAuthLoading != null) firebaseAuthLoading.SetActive(false);
        }
    }

    //it does not directly log the user out but invalidates the auth
    void OnDestroy()
    {
        try
        {
            if (auth != null)
            {
                auth.StateChanged -= AuthStateChanged;
            }
            auth = null;
        }
        catch (Exception ex)
        {
            Logger.LogError("OnDestroy encountered an issue while cleaning up", LogContext, ex);
        }
    }
    public async void LetsPlay()
    {
        Logger.LogInfo("LetsPlay triggered", LogContext);
        if (firebaseAuthLoading != null) firebaseAuthLoading.SetActive(true);
        try
        {
            if (await HaveConnectivity(canvas, messageBoxPopupPrefab, firebaseAuthLoading))
            {
                Logger.LogInfo("Connectivity check passed", LogContext);
                if (PlayerInfo.IsAppAuthenticated)
                {
                    Logger.LogInfo($"Authenticated user: {PlayerInfo.AuthenticatedID}", LogContext);
                    if (FirestoreClient != null && await FirestoreClient.ValidateTheFirstLogin(PlayerInfo.AuthenticatedID))
                    {
                        Logger.LogInfo("First login detected", LogContext);
                        Transition.LoadLevel(SceneName.Agreement.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
                    }
                    else
                    {
                        Logger.LogInfo("Not first login", LogContext);
                        StartCoroutine(OnSuccess(SceneName.AppAuthentication.ToString()));
                    }
                }
                else
                {
                    Logger.LogInfo($"User not authenticated. Showing auth screen.", LogContext);

                    canvas.gameObject.SetActive(false);
                    if (authenticationScene != null) authenticationScene.SetActive(true);
                    if (firebaseAuthLoading != null) firebaseAuthLoading.SetActive(false);
                }
            }
            else
            {
                Logger.LogWarning("Connectivity check failed in LetsPlay", LogContext);
                if (firebaseAuthLoading != null) firebaseAuthLoading.SetActive(false);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"LetsPlay failed", LogContext, ex);
            if (firebaseAuthLoading != null) firebaseAuthLoading.SetActive(false);
        }
    }


    IEnumerator OnSuccess(string _scenename)
    {
        yield return new WaitForSeconds(0.3f);
        Logger.LogInfo("OnSuccess transition", LogContext);
        if (firebaseAuthLoading != null) firebaseAuthLoading.SetActive(false);
        Transition.LoadLevel(_scenename, Params.SceneTransitionDuration, Params.SceneTransitionColor);

    }

    IEnumerator LoadTransitionImage()
    {
        string url_image = $"{Application.streamingAssetsPath}/transitionBG.png";
        using (UnityWebRequest uwr = new UnityWebRequest(url_image))
        {
            uwr.disposeDownloadHandlerOnDispose = true;
            uwr.disposeUploadHandlerOnDispose = true;
            uwr.disposeCertificateHandlerOnDispose = true;
            DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
            uwr.downloadHandler = texDl;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Params.TransitionBG = texDl.texture;
                if (firebaseAuthLoading != null) firebaseAuthLoading.SetActive(false);
                Logger.LogInfo("Transition BG image loaded", LogContext);
            }
            else
            {
                Logger.LogWarning($"Failed to load Transition BG: {uwr.error}", LogContext);
            }
        }
    }
}


