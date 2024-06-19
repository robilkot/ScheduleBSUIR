using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class StringListToStringConverter : IValueConverter
    {
        public string Separator { get; set; } = ", ";
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is not IEnumerable<string> list)
                return string.Empty;

            return string.Join(Separator, list);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
