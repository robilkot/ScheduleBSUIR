using ScheduleBSUIR.Interfaces;
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
        public void LogInfo(object? message, bool displayCaller = true)
        {
            var caller = displayCaller ?
                new StackTrace().GetFrame(2)?.GetMethod()?.ReflectedType!.Name
                : "info";
            
            string log = $"[{caller}] {message}";

            _localLog += log += '\n';

            lock (locker)
            {
                Debug.WriteLine(log);
                SaveLog();
            }
        }

        public void LogError(object? message, bool displayCaller = true)
        {
            LogInfo(message, displayCaller);
        }

        public string GetLocalLog() => _localLog;

        public void ClearLocalLog()
        {
            _localLog = string.Empty;

            try
            {
                File.Delete(LogFilePath);
            }
            catch (Exception ex)
            {
                LogError($"Error deleting log: {ex.Message}", displayCaller: false);
            }
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
                LogError($"Error saving log: {ex.Message}", displayCaller: false);
            }
        }
        private void OpenLog()
        {
            try
            {
                using var stream = File.OpenText(LogFilePath);

                string? line;

                while ((line = stream.ReadLine()) != null)
                {
                    _localLog += line + '\n';
                }
            }
            catch(FileNotFoundException)
            {
                // idc
            }
            catch (Exception ex)
            {
                LogError($"Error opening log: {ex.Message}", displayCaller: false);
            }
        }
    }
}
