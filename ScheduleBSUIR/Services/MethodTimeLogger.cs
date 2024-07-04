using ScheduleBSUIR.Interfaces;
using System.Reflection;

namespace ScheduleBSUIR.Services
{
    public static class MethodTimeLogger
    {
        private static readonly ILoggingService s_loggingService;
        static MethodTimeLogger()
        {
            s_loggingService = App.Current.Handler.MauiContext.Services.GetRequiredService<ILoggingService>();
        }
        public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
        {
            s_loggingService.LogInfo($"{methodBase.Name} took {elapsed:ss\\.FFFFF}");
        }
    }
}
