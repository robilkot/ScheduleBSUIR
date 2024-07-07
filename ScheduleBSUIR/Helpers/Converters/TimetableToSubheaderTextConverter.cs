using ScheduleBSUIR.Models;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class TimetableToSubheaderTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Timetable timetable)
            {
                return null;
            }

            string header = string.Empty;

            if (timetable.StudentGroupDto is not null)
            {
                header = $"{timetable.StudentGroupDto.Course} курс {timetable.StudentGroupDto.SpecialityAbbreviation}";
            }

            return header;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
