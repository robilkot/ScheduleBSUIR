using LiteDB;
using ScheduleBSUIR.Interfaces;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class EmployeeDto : IEmployee
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Degree { get; set; } = string.Empty;
        public string DegreeAbbrev { get; set; } = string.Empty;
        public string? Rank { get; set; }
        public string? PhotoLink { get; set; }
        public string CalendarId { get; set; } = string.Empty;
        public int Id { get; set; }
        public string UrlId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public List<string>? JobPositions { get; set; }
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
