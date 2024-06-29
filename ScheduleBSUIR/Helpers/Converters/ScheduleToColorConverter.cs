using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class ScheduleToColorConverter : IValueConverter
    {
        private readonly Lazy<PreferencesService> _lazyPreferencesService = new(() => App.Current.Handler?.MauiContext.Services.GetRequiredService<PreferencesService>());
        private readonly Dictionary<object, string?> _cachedResults = [];
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return null;

            if (!_cachedResults.TryGetValue(value, out string? key))
            {
                if (value is not Schedule schedule)
                    return null;

                var lessonType = LessonTypesHelper.GetByAbbreviation(schedule.LessonTypeAbbrev ?? LessonTypesHelper.AnnouncementAbbreviation);

                key = lessonType.ColorPreferenceKey;

                _cachedResults.Add(value, key);
            }

            return _lazyPreferencesService.Value.GetColorPreference(key!);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
