namespace ScheduleBSUIR.Models
{
    public class Schedule
    {
        public required List<int> WeekNumber { get; set; }
        public required List<StudentGroup> StudentGroups { get; set; }
        public int NumSubgroup { get; set; }
        public List<string>? Auditories { get; set; }
        public required string StartLessonTime { get; set; }
        public required string EndLessonTime { get; set; }
        public required string Subject { get; set; }
        public required string SubjectFullName { get; set; }
        public string? Note { get; set; }
        public required string LessonTypeAbbrev { get; set; }
        public string? DateLesson { get; set; }
        public required string StartLessonDate { get; set; }
        public required string EndLessonDate { get; set; }
        public bool Announcement { get; set; }
        public bool Split { get; set; }
        public List<Employee>? Employees { get; set; }
    }
}
