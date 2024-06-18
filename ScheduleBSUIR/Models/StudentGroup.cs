using LiteDB;
using ScheduleBSUIR.Models.DB;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class StudentGroup : ICacheable
    {
        public string Name { get; set; } = string.Empty;
        public int FacultyId { get; set; }
        public string? FacultyAbbrev { get; set; }
        public int SpecialityDepartmentEducationFormId { get; set; }
        public string SpecialityName { get; set; } = string.Empty;
        public string? SpecialityAbbrev { get; set; }
        public int? Course { get; set; }
        public int Id { get; set; }
        public string? CalendarId { get; set; }
        public int EducationDegree { get; set; }

        [JsonIgnore]
        [BsonId]
        public string PrimaryKey => Name.ToString();
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public DateTime AccessedAt { get; set; }
    }
}
