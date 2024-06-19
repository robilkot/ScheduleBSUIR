namespace ScheduleBSUIR.Interfaces
{
    public interface ILoggingService
    {
        void LogError(object? message, bool displayCaller = true);
        void LogInfo(object? message, bool displayCaller = true);
        string GetLocalLog();
        void ClearLocalLog();
    }
}