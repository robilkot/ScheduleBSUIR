using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Models.UI
{
    class DayHeader(DateTime day, int? week = null) : ITimetableItem
    {
        public DateTime Day { get; init; } = day;
        public int? Week { get; init; } = week;
    }
}
