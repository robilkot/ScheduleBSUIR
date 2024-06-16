namespace ScheduleBSUIR.Models
{
    public class Schedule
    {
        public required List<int> WeekNumber { get; set; }
        public required List<StudentGroup> StudentGroups { get; set; }
        public int NumSubgroup { get; set; }
        public List<string>? Auditories { get; set; }
        public required DateTime StartLessonTime { get; set; }
        public required DateTime EndLessonTime { get; set; }
        public required string Subject { get; set; }
        public required string SubjectFullName { get; set; }
        public string? Note { get; set; }
        public required string LessonTypeAbbrev { get; set; }
        public DateTime? DateLesson { get; set; }
        public required DateTime StartLessonDate { get; set; }
        public required DateTime EndLessonDate { get; set; }
        public bool Announcement { get; set; }
        public bool Split { get; set; }
        public List<Employee>? Employees { get; set; }
    }
}
