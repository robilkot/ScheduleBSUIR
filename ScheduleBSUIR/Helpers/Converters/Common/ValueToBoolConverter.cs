using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters.Common
{
    class ValueToBoolConverter : IValueConverter
    {
        public object? TrueValue { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.Equals(TrueValue) ?? false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
