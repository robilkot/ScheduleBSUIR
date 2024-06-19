using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters.Common
{
    class BoolToValueConverter : IValueConverter
    {
        public object? TrueValue { get; set; }
        public object? FalseValue { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)value! ? TrueValue : FalseValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
