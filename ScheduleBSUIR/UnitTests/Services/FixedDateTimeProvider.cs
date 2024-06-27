using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.UnitTests.Services
{
    public class FixedDateTimeProvider : IDateTimeProvider
    {
        public DateTime FixedDate { get; set; } = DateTime.ParseExact("10.06.2024 12:00", "dd.MM.yyyy HH:mm", null);
        public DateTime Now => FixedDate;

        public DateTime UtcNow => FixedDate;
    }
}
