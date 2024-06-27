using ScheduleBSUIR.Interfaces;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class DateToHeaderTextConverter : IValueConverter
    {
        // Converter is being created before app initializtion, thus DI is not possible in ctor
        private readonly Lazy<IDateTimeProvider> _lazyDateTimeProvider = new(() => App.Current.Handler?.MauiContext.Services.GetRequiredService<IDateTimeProvider>());

        private readonly Dictionary<object, string> _cachedResults = [];
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return null;

            if (_cachedResults.TryGetValue(value, out string? result))
                return result;

            if (value is not DateTime dateTime)
                return null;

            string resultString = string.Empty;

            // todo: translate extension
            if (_lazyDateTimeProvider.Value.UtcNow.Date == dateTime.Date)
            {
                resultString += "Сегодня, ";
            }
            else if (_lazyDateTimeProvider.Value.UtcNow.AddDays(1).Date == dateTime.Date)
            {
                resultString += "Завтра, ";
            }
            else if (_lazyDateTimeProvider.Value.UtcNow.AddDays(2).Date == dateTime.Date)
            {
                resultString += "Послезавтра, ";
            }

            resultString += dateTime.ToString("ddd, dd MMMM");

            _cachedResults.Add(value, resultString);

            return resultString;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
