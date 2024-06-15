namespace ScheduleBSUIR.Models
{
    public class Timetable
    {
        public Employee? EmployeeDto { get; set; }
        public StudentGroup? StudentGroupDto { get; set; }
        public Dictionary<string, List<Schedule>>? Schedules { get; set; }
        public List<Schedule>? Exams { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? StartExamsDate { get; set; }
        public string? EndExamsDate { get; set; }
    }
}
