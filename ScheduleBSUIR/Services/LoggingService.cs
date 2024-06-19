using ScheduleBSUIR.Interfaces;
using System.Diagnostics;

namespace ScheduleBSUIR.Services
{
    public class LoggingService : ILoggingService
    {
        public void LogInfo(object? message)
        {
            var className = new StackTrace().GetFrame(2)?.GetMethod()?.ReflectedType!.Name;
            var methodName = new StackTrace().GetFrame(2)?.GetMethod()?.Name;

            string log = $"[{className}] [{methodName}] {message}";

            Debug.WriteLine(log);
        }

        public void LogError(object? message)
        {
            LogInfo(message);
        }
    }
}
