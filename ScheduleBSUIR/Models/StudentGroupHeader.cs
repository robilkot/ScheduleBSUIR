﻿using LiteDB;
using ScheduleBSUIR.Models.DB;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class StudentGroupHeader : ICacheable
    {
        public string Name { get; set; } = string.Empty;
        public string? SpecialityAbbrev { get; set; }
        public int Id { get; set; }

        [JsonIgnore]
        [BsonId]
        public string PrimaryKey => Name;

        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public DateTime AccessedAt { get; set; }
    }
}
