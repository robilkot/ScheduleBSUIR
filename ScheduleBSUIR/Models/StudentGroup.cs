namespace ScheduleBSUIR.Models
{
    public class StudentGroup
    {
        public required string Name { get; set; }
        public int FacultyId { get; set; }
        public string? FacultyAbbrev { get; set; }
        public int SpecialityDepartmentEducationFormId { get; set; }
        public required string SpecialityName { get; set; }
        public string? SpecialityAbbrev { get; set; }
        public int? Course { get; set; }
        public int Id { get; set; }
        public string? CalendarId { get; set; }
        public int EducationDegree { get; set; }
    }
}
