namespace ScheduleBSUIR.Interfaces
{
    public interface IStudentGroup : ITimetableOwner
    {
        public string Name { get; }
        public string? SpecialityAbbreviation { get; }
    }
}
