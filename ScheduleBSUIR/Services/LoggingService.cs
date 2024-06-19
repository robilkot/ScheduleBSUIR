﻿using ScheduleBSUIR.Interfaces;
using System.Diagnostics;

namespace ScheduleBSUIR.Services
{
    // Sucks
    public class LoggingService : ILoggingService
    {
        private string _localLog = string.Empty;
        public const string LogFileName = "ScheduleBSUIRlog.txt";
        private static string LogFilePath => System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, LogFileName);

        private readonly object locker = new();
        public LoggingService()
        {
            OpenLog();
        }
        public void LogInfo(object? message)
        {
            var className = new StackTrace().GetFrame(2)?.GetMethod()?.ReflectedType!.Name;
            //var methodName = new StackTrace().GetFrame(2)?.GetMethod()?.Name;

            string log = $"[{className}] {message}";

            _localLog += log += '\n';

            lock (locker)
            {
                Debug.WriteLine(log);
                SaveLog();
            }
        }

        public void LogError(object? message)
        {
            LogInfo(message);
        }

        public string GetLocalLog() => _localLog;

        public void ClearLocalLog()
        {
            _localLog = string.Empty;

            SaveLog();
        }

        private void SaveLog()
        {
            try
            {
                using FileStream outputStream = File.OpenWrite(LogFilePath);
                using StreamWriter streamWriter = new StreamWriter(outputStream);

                streamWriter.Write(_localLog);
            }
            catch (Exception ex)
            {
                LogError($"Error saving log: {ex.Message}");
            }
        }
        private void OpenLog()
        {
            try
            {
                using var stream = File.OpenText(LogFilePath);

                string line;

                while ((line = stream.ReadLine()) != null)
                {
                    _localLog += line + '\n';
                }
            }
            catch (Exception ex)
            {
                LogError($"Error opening log: {ex.Message}");
            }
        }
    }
}
