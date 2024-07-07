using LiteDB;
using ScheduleBSUIR.Interfaces;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class Timetable : ICacheable, IUpdateDateAware
    {
        public EmployeeDto? EmployeeDto { get; set; }
        public StudentGroupDto? StudentGroupDto { get; set; }
        public TimetableWeek? Schedules { get; set; }
        public List<Schedule>? Exams { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartExamsDate { get; set; }
        public DateTime? EndExamsDate { get; set; }

        [BsonId]
        [JsonIgnore]
        public string PrimaryKey => EmployeeDto?.TimetableId ?? StudentGroupDto?.TimetableId ?? String.Empty;
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public DateTime AccessedAt { get; set; }
    }
}
