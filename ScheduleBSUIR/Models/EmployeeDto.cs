namespace ScheduleBSUIR.Models
{
    public class EmployeeDto
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

        public override string ToString()
        {
            string? firstNameSymbol = FirstName is not null ? FirstName[0] + "." : null;
            string? middleNameSymbol = MiddleName is not null ? MiddleName[0] + "." : null;

            return string.Join(' ', LastName, firstNameSymbol, middleNameSymbol);
        }
    }
}
