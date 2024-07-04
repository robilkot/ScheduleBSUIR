using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Models
{
    public class TimetableWeek
    {
        [JsonPropertyName("Понедельник")]
        public List<Schedule>? Monday { get; set; }

        [JsonPropertyName("Вторник")]
        public List<Schedule>? Tuesday { get; set; }

        [JsonPropertyName("Среда")]
        public List<Schedule>? Wednesday { get; set; }

        [JsonPropertyName("Четверг")]
        public List<Schedule>? Thursday { get; set; }

        [JsonPropertyName("Пятница")]
        public List<Schedule>? Friday { get; set; }

        [JsonPropertyName("Суббота")]
        public List<Schedule>? Saturday { get; set; }

        [JsonPropertyName("Воскресенье")]
        public List<Schedule>? Sunday { get; set; }

        public List<Schedule> GetByDayOfWeek(DayOfWeek dayOfWeek) => dayOfWeek switch
        {
            DayOfWeek.Monday => Monday,
            DayOfWeek.Tuesday => Tuesday,
            DayOfWeek.Wednesday => Wednesday,
            DayOfWeek.Thursday => Thursday,
            DayOfWeek.Friday => Friday,
            DayOfWeek.Saturday => Saturday,
            DayOfWeek.Sunday => Sunday,
        } ?? [];
    }
}
