using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class IsNotNullOrEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return false;
            if (value is IEnumerable<object> enumerableValue)
                return enumerableValue.Any();
            return !string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
