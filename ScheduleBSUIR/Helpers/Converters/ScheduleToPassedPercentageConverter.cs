using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class ScheduleToPassedPercentageConverter : IMultiValueConverter
    {
        private readonly Lazy<IDateTimeProvider> _lazyDateTimeProvider = new(() => App.Current.Handler?.MauiContext.Services.GetRequiredService<IDateTimeProvider>());
        private readonly Lazy<ILoggingService> _loggingService = new(() => App.Current.Handler?.MauiContext.Services.GetRequiredService<ILoggingService>());
        private readonly Dictionary<Schedule, double> _cachedResults = [];

        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return null;

            if (values[0] is not Schedule schedule)
                return null;

            if (!_cachedResults.TryGetValue(schedule, out double height))
            {
                if (values[1] is not double initialHeight)
                    return null;

                var now = _lazyDateTimeProvider.Value.UtcNow;

                if (now.Date > schedule.DateLesson)
                {
                    height = initialHeight;
                }
                else if(now.Date < schedule.DateLesson)
                {
                    height = 0;
                } 
                else
                {
                    DateTime lessonDate = schedule.DateLesson!.Value;

                    TimeSpan scheduleLenght = schedule.EndLessonTime - schedule.StartLessonTime;

                    DateTime lessonDateTime = DateTime.MinValue
                        .AddDays(lessonDate.Day - 1).AddMonths(lessonDate.Month - 1).AddYears(lessonDate.Year - 1)
                        .AddHours(schedule.StartLessonTime.Hour).AddMinutes(schedule.StartLessonTime.Minute);

                    TimeSpan passedTime = now - lessonDateTime;

                    height = Math.Clamp(passedTime / scheduleLenght, 0d, 1d) * initialHeight;
                }

                _cachedResults.Add(schedule, height);
            }

            return height;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
