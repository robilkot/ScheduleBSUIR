using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Services.Mock
{
    public class MockDateTimeProvider : IDateTimeProvider
    {
        public DateTime FixedDate { get; set; } = DateTime.ParseExact("11.06.2024 12:00", "dd.MM.yyyy HH:mm", null);
        public DateTime Now => FixedDate;

        public DateTime UtcNow => FixedDate;
    }
}
