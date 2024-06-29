using ScheduleBSUIR.Services;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class KeyToColorPreferenceConverter : IValueConverter
    {
        private readonly Lazy<PreferencesService> _lazyPreferencesService = new(() => App.Current.Handler?.MauiContext.Services.GetRequiredService<PreferencesService>());
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string str)
                return null;

            return _lazyPreferencesService.Value.GetColorPreference(str);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
