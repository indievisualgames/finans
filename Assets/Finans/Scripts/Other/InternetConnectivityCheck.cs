using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class InternetConnectivityCheck : MonoBehaviour
{
    public float waitTime = 10f;
    bool startTimer = false;
    float timer;
    bool connection_status = false;
    bool inFlight = false;

    public event System.Action<bool> ConnectivityChanged;

    public bool ConnectionStatus
    {
        get { return connection_status; }
        set { connection_status = value; }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    public void CheckNow(bool status)
    {
        startTimer = status;
        timer = 0f;
    }
    // Update is called once per frame
    async void Update()
    {
        if (!startTimer) return;
        timer += Time.deltaTime;
        if (timer <= waitTime) return;
        if (inFlight) return;
        inFlight = true;
        timer = 0f;
        bool isOnline = await InternetConnectivityChecker.CheckInternetConnectivityAsync();
        inFlight = false;
        if (isOnline)
        {
            startTimer = false;
            ConnectionStatus = true;
            try { ConnectivityChanged?.Invoke(true); } catch { }
        }
    }

    void OnDisable()
    {
        // Prevent continuing async/polling after disable to avoid leaks
        startTimer = false;
        inFlight = false;
    }

    void OnApplicationQuit()
    {
        startTimer = false;
        inFlight = false;
    }

    /*private async Task<bool> CheckInternetConnectivity() {
        return await InternetConnectivityChecker.CheckInternetConnectivityAsync();
    }*/
}
