using ScheduleBSUIR.Models;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class TimetableToHeaderTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Timetable timetable)
            {
                return null;
            }

            string header = string.Empty;

            if (timetable.EmployeeDto is not null)
            {
                header = $"{timetable.EmployeeDto.LastName} {timetable.EmployeeDto.FirstName[0]}. {timetable.EmployeeDto.MiddleName[0]}.";
            }
            else if (timetable.StudentGroupDto is not null)
            {
                header = $"{timetable.StudentGroupDto.Name}";
            }

            return header;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
