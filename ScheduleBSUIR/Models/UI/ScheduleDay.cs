using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Models.UI
{
    class ScheduleDay(DateTime day) : ITimetableItem
    {
        public DateTime Day { get; init; } = day;
    }
}
