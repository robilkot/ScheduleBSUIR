namespace ScheduleBSUIR.Models
{
    public class Timetable
    {
        public Employee? EmployeeDto { get; set; }
        public StudentGroup? StudentGroupDto { get; set; }
        public Dictionary<string, List<Schedule>>? Schedules { get; set; }
        public List<Schedule>? Exams { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartExamsDate { get; set; }
        public DateTime? EndExamsDate { get; set; }
    }
}
