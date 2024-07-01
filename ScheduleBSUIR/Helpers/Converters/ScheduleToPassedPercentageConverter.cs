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

                // todo: review after non-exams schedule is implemented
                var nowUtc = _lazyDateTimeProvider.Value.Now.ToUniversalTime();
                var scheduleDateUtc = schedule.DateLesson.Value.Date;

                if (nowUtc.Date > scheduleDateUtc)
                {
                    height = initialHeight;
                }
                else if(nowUtc.Date < scheduleDateUtc)
                {
                    height = 0;
                } 
                else
                {
                    TimeSpan scheduleLenght = schedule.EndLessonTime - schedule.StartLessonTime;

                    var startLessonTimeUtc = schedule.StartLessonTime.ToUniversalTime();

                    DateTime lessonDateTimeUtc = scheduleDateUtc
                        .AddHours(startLessonTimeUtc.Hour).AddMinutes(startLessonTimeUtc.Minute);

                    TimeSpan passedTime = nowUtc - lessonDateTimeUtc;

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
