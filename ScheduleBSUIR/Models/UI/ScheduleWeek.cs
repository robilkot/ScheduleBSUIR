using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Models.UI
{
    class ScheduleWeek(int weekNumber) : ITimetableItem
    {
        public int WeekNumber { get; init; } = weekNumber;
    }
}
