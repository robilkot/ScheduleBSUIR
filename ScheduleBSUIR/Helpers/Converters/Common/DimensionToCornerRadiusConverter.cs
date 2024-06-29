using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters.Common
{
    // There is no way to bind double to CornerRadius property, so...
    class DimensionToCornerRadiusConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not double number)
                return value;

            return new CornerRadius(number);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
