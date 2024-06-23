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
                string firstNameLetter = string.Empty;

                if(!string.IsNullOrEmpty(timetable.EmployeeDto.FirstName))
                {
                    firstNameLetter = $"{timetable.EmployeeDto.FirstName[0]}.";
                }

                string middleNameLetter = string.Empty;

                if (!string.IsNullOrEmpty(timetable.EmployeeDto.MiddleName))
                {
                    middleNameLetter = $"{timetable.EmployeeDto.MiddleName[0]}.";
                }

                header = $"{timetable.EmployeeDto.LastName} {firstNameLetter} {middleNameLetter}";
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
