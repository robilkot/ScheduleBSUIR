using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Services.Mock
{
    public class MockDateTimeProvider : IDateTimeProvider
    {
        public DateTime FixedDate { get; set; } = DateTime.ParseExact("09.02.2024 13:00", "dd.MM.yyyy HH:mm", null); //DateTime.ParseExact("11.06.2024 12:00", "dd.MM.yyyy HH:mm", null);
        public DateTime Now => FixedDate;

        public DateTime UtcNow => FixedDate;

        public Task<int> GetWeekAsync(DateTime date, CancellationToken cancellationToken)
        {
            int obtainedWeekNumber = 1;
            DateTime obtainedWeekUpdateDate = Now;

            DateTime obtainedWeekBegin = obtainedWeekUpdateDate.StartOfWeek(DayOfWeek.Monday);
            DateTime targetWeekBegin = date.StartOfWeek(DayOfWeek.Monday);

            int differenceInWeeks = (int)Math.Floor((targetWeekBegin - obtainedWeekBegin).TotalDays / 7);

            int targetWeekNumber = obtainedWeekNumber + differenceInWeeks % 4;

            return Task.FromResult(targetWeekNumber);
        }
    }
}
