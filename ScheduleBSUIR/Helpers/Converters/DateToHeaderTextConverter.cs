using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Services;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters
{
    class DateToHeaderTextConverter : IValueConverter
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        public DateToHeaderTextConverter()
        {
            // todo: DI
            _dateTimeProvider = new DateTimeProviderService();
        }
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is not string dateString)
            {
                return null;
            }

            var dateTime = ApiDateTimeParser.Parse(dateString);

            string resultString = string.Empty;

            // todo: translate extension
            if (_dateTimeProvider.Now.Date == dateTime.Date)
            {
                resultString += "Сегодня, ";
            }
            else if (DateTime.Now.AddDays(1).Date == dateTime.Date)
            {
                resultString += "Завтра, ";
            }
            else if (DateTime.Now.AddDays(2).Date == dateTime.Date)
            {
                resultString += "Послезавтра, ";
            }

            resultString += dateTime.ToString("dd MMMM");

            return resultString;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
