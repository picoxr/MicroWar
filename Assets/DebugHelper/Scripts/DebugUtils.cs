using System.Diagnostics;
using UnityEngine;
using System.IO;
using System;

namespace MicroWar
{
    internal enum LogLevel
    {
        LOG,
        WARNING,
        ERROR
    }
    public static class DebugUtils
    {
        internal static Action<LogLevel, string> PrintLog;
        internal static bool SaveLog
        {
            get { return isSaveLog; }
            set { isSaveLog = value; }
        }

        private static string FullLogPath = $"{Application.persistentDataPath}/PICO_log_full.log";
        private static bool isSaveLog = false;

        public static void Log(string context, string message)
        {
            string logMsg = $"[{context}]: {message}";
            UnityEngine.Debug.Log(logMsg);
            SaveLogFile(message);
            PrintLog?.Invoke(LogLevel.LOG, logMsg);
        }

        public static void LogWarning(string context, string message)
        {
            string logMsg = $"[{context}]: {message}";
            UnityEngine.Debug.LogWarning(logMsg);
            SaveLogFile(message);
            PrintLog?.Invoke(LogLevel.WARNING, logMsg);
        }
        public static void LogError(string context, string message)
        {
            string logMsg = $"[{context}]: {message}";
            UnityEngine.Debug.LogError(logMsg);
            SaveLogFile(message);
            PrintLog?.Invoke(LogLevel.ERROR, logMsg);
        }

        private static void SaveLogFile(string message)
        {
            if (!isSaveLog)
                return;

            if (!File.Exists(FullLogPath))
            {
                using (var f = File.Create(FullLogPath)) { }
            }
            using (var file = File.Open(FullLogPath, FileMode.Append))
            {
                StreamWriter sw = new StreamWriter(file);
                StackTrace st = new StackTrace(1);
                sw.WriteLine($"[{DateTime.Now:dd/MM/yyyy/ HH:mm:ss.fff}] {message}");
                sw.WriteLine(st.ToString());
                sw.WriteLine();
                sw.Flush();
            }
        }
    }

}

