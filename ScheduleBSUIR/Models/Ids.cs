using System.Diagnostics;

namespace ScheduleBSUIR.Models
{
    public abstract class TypedId(string Id)
    {
        public static TypedId Create(object dto) => dto switch
        {
            StudentGroupDto studentGroupDto => new StudentGroupId(studentGroupDto.Name),
            StudentGroupHeader studentGroupHeader => new StudentGroupId(studentGroupHeader.Name),
            EmployeeDto employeeDto => new EmployeeId(employeeDto.UrlId),
            _ => throw new UnreachableException()
        };
        public override string ToString() => Id;
    }
    public sealed class StudentGroupId(string name) : TypedId(name)
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
