using ScheduleBSUIR.Helpers.Constants;
using System.Diagnostics;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    // todo: localize
    class TimetableTabsToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not TimetableTabs timetable)
            {
                return null;
            }

            return timetable switch
            {
                TimetableTabs.Exams => "Сессия",
                TimetableTabs.Schedule => "Занятия",
                _ => throw new UnreachableException(),
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
