using LiteDB;
using ScheduleBSUIR.Interfaces;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class StudentGroupDto : IAvatarDisplaying, IStudentGroup
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
        [BsonIgnore]
        [JsonIgnore]
        public string AvatarText => $"{Name[0..3]}\n{Name[3..6]}";
        [BsonIgnore]
        [JsonIgnore]
        public string? AvatarUrl => null;
        [BsonIgnore]
        [JsonIgnore]
        public string? SpecialityAbbreviation => SpecialityAbbrev;
        [BsonIgnore]
        [JsonIgnore]
        public string TimetableId => Name;
    }
}
