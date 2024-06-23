namespace ScheduleBSUIR.Models
{
    public abstract class TypedId(string Id)
    {
        public override string ToString() => Id;
    }
    public sealed class StudentGroupId(string Id) : TypedId(Id)
    {
        public StudentGroupId(StudentGroupDto group) : this(group.Name) { }
        public StudentGroupId(StudentGroupHeader group) : this(group.Name) { }
    }
    public sealed class EmployeeId(string urlId) : TypedId(urlId)
    {
        public EmployeeId(EmployeeDto employeeDto) : this(employeeDto.UrlId) { }
        public EmployeeId(Employee employee) : this(employee.UrlId) { }
    }
}
