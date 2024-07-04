using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Models.UI
{
    class ScheduleDay(DateTime day, int? week = null) : ITimetableItem
    {
        public DateTime Day { get; init; } = day;
        public int? Week { get; init; } = week;
    }
}
