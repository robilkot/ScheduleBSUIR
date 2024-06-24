namespace ScheduleBSUIR.Models
{
    // Used to group schedules by day
    public class DaySchedule(IEnumerable<Schedule> schedules) : List<Schedule>(schedules)
    {
        public DateTime Day => this.FirstOrDefault()?.DateLesson ?? DateTime.MinValue;
    }
}
