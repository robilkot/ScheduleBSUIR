using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Services
{
    class RealDateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.Now;
    }
}
