using System.Globalization;

namespace ScheduleBSUIR.Helpers
{
    public static class ApiDateTimeParser
    {
        public static DateTime Parse(string apiDateString)
        {
            return DateTime.ParseExact(apiDateString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
    }
}
