using LiteDB;
using ScheduleBSUIR.Models.DB;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class EmployeeDto : ICacheable
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
        [BsonId]
        [JsonIgnore]
        public string PrimaryKey => UrlId.ToString();
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public DateTime AccessedAt { get; set; }
    }
}
