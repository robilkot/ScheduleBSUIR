using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    internal class OnlyIfMondayAndNotNullConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return false;

            if (values[0] is not int week)
                return false;

            if (values[1] is not DateTime day)
                return false;

            return day.DayOfWeek == DayOfWeek.Monday;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
