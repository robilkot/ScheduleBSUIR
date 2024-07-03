using ScheduleBSUIR.Models;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters.Common
{
    internal class IEnumerableCutoffConverter : IValueConverter
    {
        public int TakeAmount { get; set; } = 3;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                IEnumerable<EmployeeDto> employeeEnumerable => employeeEnumerable.Take(3).ToList(),
                IEnumerable<StudentGroupDto> groupsEnumerable => groupsEnumerable.Take(3).ToList(),
                _ => null
            };
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
