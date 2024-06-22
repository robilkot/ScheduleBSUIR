using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models;
using System.Diagnostics;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    // todo localize
    class TimetableToDaterangeTextConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values.Length != 2)
            {
                return null;
            }

            if (values[0] is not Timetable timetable)
            {
                return null;
            }

            string? header = values[1] switch
            {
                TimetableTabs.Exams
                    when timetable is { StartExamsDate: not null, EndExamsDate: not null }
                    => $"Сессия: {timetable.StartExamsDate?.ToString("dd MMMM")} - {timetable.EndExamsDate?.ToString("dd MMMM yyyy")}",

                TimetableTabs.Schedule
                    when timetable is { StartDate: not null, EndDate: not null }
                    => $"Занятия: {timetable.StartDate?.ToString("dd MMMM")} - {timetable.EndDate?.ToString("dd MMMM yyyy")}",

                // Some employees may not have dates specified. No text to show then
                _ => null,
            };

            return header;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
