using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class ScheduleToColorPlankStyleConverter : IValueConverter
    {
        private readonly Dictionary<object, string?> _cachedResults = [];
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return null;

            if (!_cachedResults.TryGetValue(value, out string? key))
            {
                if (value is not Schedule schedule)
                    return null;

                var lessonType = schedule.LessonTypeAbbrev is null
                    ? LessonTypesHelper.Announcement
                    : LessonTypesHelper.GetByAbbreviation(schedule.LessonTypeAbbrev);

                key = $"{lessonType.ColorPreferenceKey}Style";

                _cachedResults.Add(value, key);
            }

            return App.Current.Resources[key];
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
