namespace ScheduleBSUIR.Models
{
    public class Employee
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string MiddleName { get; set; }
        public required string Degree { get; set; }
        public required string DegreeAbbrev { get; set; }
        public string? Rank { get; set; }
        public string? PhotoLink { get; set; }
        public required string CalendarId { get; set; }
        public int Id { get; set; }
        public required string UrlId { get; set; }
        public string? Email { get; set; }
        public List<string>? JobPositions { get; set; }
    }
}
