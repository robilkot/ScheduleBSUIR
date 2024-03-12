namespace ScheduleBSUIR.Constants
{
    public static class Weekdays
    {
        public static readonly Dictionary<string, WeekdaysEnum> WeekdaysStrings = new()
        {
            { "Понедельник", WeekdaysEnum.Monday },
            { "Вторник", WeekdaysEnum.Tuesday },
            { "Среда", WeekdaysEnum.Wednesday },
            { "Четверг", WeekdaysEnum.Thursday },
            { "Пятница", WeekdaysEnum.Friday },
            { "Суббота", WeekdaysEnum.Saturday },
            { "Воскресенье", WeekdaysEnum.Sunday },
        };
    }
}
