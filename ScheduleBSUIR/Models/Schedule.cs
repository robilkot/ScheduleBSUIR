﻿using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Models
{
    public class Schedule : ITimetableItem
    {
        public required List<int> WeekNumber { get; set; }
        public required List<StudentGroupDto> StudentGroups { get; set; }
        public SubgroupType NumSubgroup { get; set; }
        public List<string>? Auditories { get; set; }
        public required DateTime StartLessonTime { get; set; }
        public required DateTime EndLessonTime { get; set; }
        public string? Subject { get; set; }
        public string? SubjectFullName { get; set; }
        public string? Note { get; set; }
        public string? LessonTypeAbbrev { get; set; }
        public DateTime? DateLesson { get; set; }
        public required DateTime StartLessonDate { get; set; }
        public required DateTime EndLessonDate { get; set; }
        public bool Announcement { get; set; }
        public bool Split { get; set; }
        public List<EmployeeDto>? Employees { get; set; }
    }
}
