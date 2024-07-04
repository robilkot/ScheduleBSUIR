namespace ScheduleBSUIR.Interfaces
{
    public interface ILoggingService
    {
        void LogError(object? message, bool displayCaller = false);
        void LogInfo(object? message, bool displayCaller = false);
        string GetLocalLog();
        void ClearLocalLog();
    }
}