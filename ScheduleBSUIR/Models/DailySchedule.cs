namespace ScheduleBSUIR.Models
{
    // Used to group schedules by day
    public class DailySchedule(IEnumerable<Schedule> schedules) : List<Schedule>(schedules)
    {
        private DateTime _day = schedules.FirstOrDefault()?.DateLesson ?? DateTime.MinValue;
        public DateTime Day => _day;
    }
}
