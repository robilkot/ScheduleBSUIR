using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Services
{
    internal class DummyLoggingService : ILoggingService
    {
        public string GetLocalLog() => string.Empty;

        public void ClearLocalLog()
        {

        }

        public void LogError(object? message, bool displayCaller = false)
        {

        }

        public void LogInfo(object? message, bool displayCaller = false)
        {

        }
    }
}
