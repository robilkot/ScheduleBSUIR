namespace ScheduleBSUIR.Interfaces
{
    public interface IEmployee : ITimetableOwner, IAvatarDisplaying
    {
        public string FirstName { get; }
        public string? MiddleName { get; }
        public string LastName { get; }
        public string AbbreviatedName
        {
            get
            {
                string? firstNameSymbol = string.IsNullOrEmpty(FirstName) ? null : FirstName[0] + ".";
                string? middleNameSymbol = string.IsNullOrEmpty(MiddleName) ? null : MiddleName[0] + ".";

                return string.Join(' ', LastName, firstNameSymbol, middleNameSymbol);
            }
        }
        public string FullName { get => string.Join(' ', LastName, FirstName, MiddleName); }
    }
}
