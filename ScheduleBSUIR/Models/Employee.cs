using LiteDB;
using ScheduleBSUIR.Interfaces;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class Employee : ICacheable, IUpdateDateAware, IAvatarDisplaying
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
        [BsonIgnore]
        [JsonIgnore]
        public string FullName => string.Format("{0} {1} {2}", LastName, FirstName, MiddleName);
        [BsonId]
        [JsonIgnore]
        public string PrimaryKey => UrlId.ToString();
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public DateTime AccessedAt { get; set; }

        [JsonIgnore]
        [BsonIgnore]
        public string AvatarText => $"{FirstName?[0]}{MiddleName?[0]}";
        [JsonIgnore]
        [BsonIgnore]
        public string? AvatarUrl => PhotoLink;

        public override string ToString()
        {
            string? firstNameSymbol = string.IsNullOrEmpty(FirstName) ? null : FirstName[0] + ".";
            string? middleNameSymbol = string.IsNullOrEmpty(MiddleName) ? null : MiddleName[0] + "." ;

            return string.Join(' ', LastName, firstNameSymbol, middleNameSymbol);
        }
    }
}
