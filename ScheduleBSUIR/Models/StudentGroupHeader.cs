using LiteDB;
using ScheduleBSUIR.Interfaces;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class StudentGroupHeader : IStudentGroup, ICacheable, IUpdateDateAware
    {
        public string Name { get; set; } = string.Empty;
        public string? SpecialityAbbrev { get; set; }
        public int Id { get; set; }
        [BsonId]
        [JsonIgnore]
        public string PrimaryKey => Name;
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public DateTime AccessedAt { get; set; }
        [BsonIgnore]
        [JsonIgnore]
        public string? SpecialityAbbreviation => SpecialityAbbrev;
        [BsonIgnore]
        [JsonIgnore]
        public string TimetableId => Name;
    }
}
