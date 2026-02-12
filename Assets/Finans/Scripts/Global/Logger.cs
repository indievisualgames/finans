using System;
using UnityEngine;

public static class Logger
{
    public enum LogLevel { Debug, Info, Warning, Error }

    private static readonly System.Collections.Generic.Dictionary<string, float> lastLogTimeByKey = new System.Collections.Generic.Dictionary<string, float>();
    private static readonly float defaultThrottleSeconds = 1.0f;

    public static void Log(LogLevel level, string message, string context = "", Exception ex = null)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] [{level}] [{context}] {message}";

        switch (level)
        {
            case LogLevel.Debug:
                Debug.Log(logMessage);
                break;
            case LogLevel.Info:
                Debug.Log(logMessage);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(logMessage);
                break;
            case LogLevel.Error:
                Debug.LogError(logMessage);
                if (ex != null)
                    Debug.LogError($"Exception: {ex}");
                break;
        }
    }

    // Convenience methods
    public static void LogDebug(string message, string context = "") => Log(LogLevel.Debug, message, context);
    public static void LogInfo(string message, string context = "") => Log(LogLevel.Info, message, context);
    public static void LogWarning(string message, string context = "") => Log(LogLevel.Warning, message, context);
    public static void LogError(string message, string context = "", Exception ex = null) => Log(LogLevel.Error, message, context, ex);

    public static void LogThrottled(LogLevel level, string message, string context = "", float throttleSeconds = -1f)
    {
        try
        {
            float window = throttleSeconds > 0 ? throttleSeconds : defaultThrottleSeconds;
            string key = context + "|" + message;
            float now = Time.time;
            if (lastLogTimeByKey.TryGetValue(key, out float last))
            {
                if (now - last < window) return;
            }
            lastLogTimeByKey[key] = now;
            Log(level, message, context);
        }
        catch
        {
            Log(level, message, context);
        }
    }



}