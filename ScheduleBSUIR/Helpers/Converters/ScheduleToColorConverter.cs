using ScheduleBSUIR.Models;
using System.Diagnostics;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class ScheduleToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is not Schedule schedule)
            {
                return null;
            }

            var lessonType = LessonTypesHelper.GetByAbbreviation(schedule.LessonTypeAbbrev);

            var key = lessonType.ColorResourceKey;

            return App.Current.Resources[key];
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
