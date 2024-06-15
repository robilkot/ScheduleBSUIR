namespace ScheduleBSUIR.Models.UI
{
    public class ScheduleGroup(string name, List<Schedule> schedules) : List<Schedule>(schedules)
    {
        public string Name { get; private set; } = name;
    }
}
