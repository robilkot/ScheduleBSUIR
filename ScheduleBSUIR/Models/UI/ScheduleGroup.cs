namespace ScheduleBSUIR.Models.UI
{
    public class ScheduleGroup(DateTime day, List<Schedule> schedules) : List<Schedule>(schedules)
    {
        public DateTime Day { get; private set; } = day;
    }
}
