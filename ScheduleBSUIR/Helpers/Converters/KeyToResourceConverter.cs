using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class KeyToResourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string str)
                return null;

            return App.Current!.Resources[str];
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
