using LiteDB;
using ScheduleBSUIR.Interfaces;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class Employee : IEmployee, ICacheable, IUpdateDateAware
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        // Actually a degree abbrev
        public string Degree { get; set; } = string.Empty;
        public string? Rank { get; set; }
        public string? PhotoLink { get; set; }
        public string CalendarId { get; set; } = string.Empty;
        public int Id { get; set; }
        public string UrlId { get; set; } = string.Empty;
        public List<string>? AcademicDepartment { get; set; }
        public string Fio { get; set; } = string.Empty;

        [BsonId]
        [JsonIgnore]
        public string PrimaryKey => UrlId;
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public DateTime AccessedAt { get; set; }
        [BsonIgnore]
        [JsonIgnore]
        public string AvatarText => $"{FirstName?.FirstOrDefault()}{MiddleName?.FirstOrDefault()}";
        [BsonIgnore]
        [JsonIgnore]
        public string? AvatarUrl => PhotoLink;
        [BsonIgnore]
        [JsonIgnore]
        public string TimetableId => UrlId;
    }
}
