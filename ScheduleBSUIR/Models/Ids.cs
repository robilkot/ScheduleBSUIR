using ScheduleBSUIR.Interfaces;
using System.Diagnostics;

namespace ScheduleBSUIR.Models
{
    public abstract class TypedId(string Id, string? displayName = null) : ICacheable
    {
        public string PrimaryKey => Id;
        public string DisplayName => displayName ?? Id;

        public static TypedId Create(object dto) => dto switch
        {
            StudentGroupDto studentGroupDto => new StudentGroupId(studentGroupDto),
            StudentGroupHeader studentGroupHeader => new StudentGroupId(studentGroupHeader),
            EmployeeDto employeeDto => new EmployeeId(employeeDto),
            Employee employee => new EmployeeId(employee),
            Timetable timetable when timetable.EmployeeDto is not null => new EmployeeId(timetable.EmployeeDto),
            Timetable timetable when timetable.StudentGroupDto is not null => new StudentGroupId(timetable.StudentGroupDto),
            _ => throw new UnreachableException()
        };
        public override string ToString() => PrimaryKey;
    }
    public sealed class StudentGroupId : TypedId
    {
        public StudentGroupId() : base(string.Empty) { }
        public StudentGroupId(StudentGroupDto group) : base(group.Name) { }
        public StudentGroupId(StudentGroupHeader group) : base(group.Name) { }
    }
    public sealed class EmployeeId : TypedId
    {
        public EmployeeId() : base(string.Empty) { }
        public EmployeeId(EmployeeDto employeeDto) : base(employeeDto.UrlId, employeeDto.ToString()) { }
        public EmployeeId(Employee employee) : base(employee.UrlId, employee.ToString()) { }
    }
}
