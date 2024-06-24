using System.Collections;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters.Common
{
    class IsEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable enumerableValue)
                return !enumerableValue.GetEnumerator().MoveNext();

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
