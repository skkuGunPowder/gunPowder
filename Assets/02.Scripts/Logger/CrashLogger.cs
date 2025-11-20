using System;
using System.IO;
using UnityEngine;

public class CrashLogger : MonoBehaviour
{
    private static string logPath;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (string.IsNullOrEmpty(logPath))
        {
            logPath = Path.Combine(
                Application.persistentDataPath,
                $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.log"
            );
        }

        Application.logMessageReceived += OnLog;
        Application.logMessageReceivedThreaded += OnLogThreaded;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
        Application.logMessageReceivedThreaded -= OnLogThreaded;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    void OnLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
            Write($"[MAIN] {type}\n{condition}\n{stackTrace}");
    }

    void OnLogThreaded(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
            Write($"[THREAD] {type}\n{condition}\n{stackTrace}");
    }

    void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Write($"[UNHANDLED]\n{e.ExceptionObject}");
    }

    static void Write(string msg)
    {
        try
        {
            File.AppendAllText(
                logPath,
                $"\n=============================\n{DateTime.Now:HH:mm:ss.fff}\n{msg}\n"
            );
        }
        catch { }
    }
}
