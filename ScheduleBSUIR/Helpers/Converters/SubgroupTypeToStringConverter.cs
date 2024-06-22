using ScheduleBSUIR.Helpers.Constants;
using System.Diagnostics;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    // todo: localize
    class SubgroupTypeToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not SubgroupType subgroupType)
            {
                return null;
            }

            return subgroupType switch
            {
                SubgroupType.All => "Вся группа",
                SubgroupType.FirstSubgroup => "Первая подгруппа",
                SubgroupType.SecondSubgroup => "Вторая подгруппа",
                _ => throw new UnreachableException(),
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
