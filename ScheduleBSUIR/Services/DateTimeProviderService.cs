using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Services
{
    class DateTimeProviderService : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.Now;
    }
}
