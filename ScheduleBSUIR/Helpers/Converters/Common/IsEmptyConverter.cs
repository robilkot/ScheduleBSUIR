using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters.Common
{
    class IsEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object> enumerableValue)
                return enumerableValue.Count() == 0;

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
