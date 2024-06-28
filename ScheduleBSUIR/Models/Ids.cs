using LiteDB;
using ScheduleBSUIR.Interfaces;
using System.Diagnostics;

namespace ScheduleBSUIR.Models
{
    public abstract class TypedId
    {
        public TypedId(string id, string? displayName = null)
        {
            PrimaryKey = id;
            DisplayName = displayName ?? id;
        }
        [BsonId]
        public string PrimaryKey { get; init; }
        public string DisplayName { get; init; }

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
    public sealed class StudentGroupId : TypedId, ICacheable
    {
        public StudentGroupId() : base(string.Empty) { }
        public StudentGroupId(StudentGroupDto group) : base(group.Name) { }
        public StudentGroupId(StudentGroupHeader group) : base(group.Name) { }
    }
    public sealed class EmployeeId : TypedId, ICacheable
    {
        public EmployeeId() : base(string.Empty) { }
        public EmployeeId(EmployeeDto employeeDto) : base(employeeDto.UrlId, employeeDto.ToString()) { }
        public EmployeeId(Employee employee) : base(employee.UrlId, employee.ToString()) { }
    }
}
