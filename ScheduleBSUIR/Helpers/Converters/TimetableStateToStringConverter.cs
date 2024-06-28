﻿using ScheduleBSUIR.Helpers.Constants;
using System.Diagnostics;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    // todo: localize
    class TimetableStateToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not TimetableState state)
            {
                return null;
            }

            return state switch
            {
                TimetableState.Default => "Не сохранять",
                TimetableState.Favorite => "Избранное",
                TimetableState.Pinned => "Закреплённое",
                _ => throw new UnreachableException(),
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
