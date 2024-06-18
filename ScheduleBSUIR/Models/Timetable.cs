﻿using LiteDB;
using ScheduleBSUIR.Models.DB;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class Timetable : ICacheable
    {
        public Employee? EmployeeDto { get; set; }
        public StudentGroup? StudentGroupDto { get; set; }
        public Dictionary<string, List<Schedule>>? Schedules { get; set; }
        public List<Schedule>? Exams { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartExamsDate { get; set; }
        public DateTime? EndExamsDate { get; set; }

        [BsonId]
        [JsonIgnore]
        public string PrimaryKey => EmployeeDto is not null 
            ? EmployeeDto.Id.ToString()
            : StudentGroupDto is not null 
            ? StudentGroupDto.Name 
            : String.Empty;
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public DateTime AccessedAt { get; set; }
    }
}
