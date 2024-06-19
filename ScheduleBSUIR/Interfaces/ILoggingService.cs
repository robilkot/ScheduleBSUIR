namespace ScheduleBSUIR.Interfaces
{
    public interface ILoggingService
    {
        void LogError(object? message);
        void LogInfo(object? message);
    }
}