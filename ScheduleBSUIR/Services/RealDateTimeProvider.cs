using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using System.Diagnostics;

namespace ScheduleBSUIR.Services
{
    class RealDateTimeProvider(IWebService _webService, ILoggingService _loggingService) : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.Now;

        public async Task<int> GetWeekAsync(DateTime date, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            int obtainedWeekNumber;
            DateTime obtainedWeekUpdateDate = Now;

            if (Connectivity.NetworkAccess is NetworkAccess.None)
            {
                obtainedWeekNumber = Preferences.Get(PreferencesKeys.Week, 0);
                obtainedWeekUpdateDate = Preferences.Get(PreferencesKeys.WeekUpdateDate, DateTime.MinValue);

                if (obtainedWeekNumber == 0)
                {
                    _loggingService.LogInfo($"GetCurrentWeekAsync could not obtain week from preferences");

                    throw new Exception("GetCurrentWeekAsync could not obtain week from preferences");
                }
            }
            else
            {
                int? currentWeekNumber = await _webService.GetCurrentWeekAsync(cancellationToken);

                if (currentWeekNumber is null)
                {
                    _loggingService.LogInfo($"GetCurrentWeekAsync could not obtain week number from webapi");

                    throw new Exception("GetCurrentWeekAsync could not obtain week number from webapi");
                }

                Preferences.Set(PreferencesKeys.Week, currentWeekNumber.Value);
                Preferences.Set(PreferencesKeys.WeekUpdateDate, Now);

                obtainedWeekNumber = currentWeekNumber.Value;
            }

            DateTime obtainedWeekBegin = obtainedWeekUpdateDate.StartOfWeek(DayOfWeek.Monday);
            DateTime targetWeekBegin = date.StartOfWeek(DayOfWeek.Monday);

            int differenceInWeeks = (int)Math.Floor((targetWeekBegin - obtainedWeekBegin).TotalDays / 7);

            int targetWeekNumber = obtainedWeekNumber + differenceInWeeks % 4;

            _loggingService.LogInfo($"GetCurrentWeekAsync {targetWeekNumber} in {stopwatch.Elapsed:ss\\.FFFFF}");

            return targetWeekNumber;
        }
    }
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
