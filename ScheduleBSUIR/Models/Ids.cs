using LiteDB;
using ScheduleBSUIR.Interfaces;
using System.Diagnostics;

namespace ScheduleBSUIR.Models
{
    public abstract class TypedId(string id, string? displayName = null)
    {
        [BsonId]
        public string PrimaryKey { get; init; } = id;
        public string DisplayName { get; init; } = displayName ?? id;

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
        public override bool Equals(object? obj)
        {
            if (obj is not TypedId id)
                return false;

            return PrimaryKey.Equals(id.PrimaryKey);
        }
        public override int GetHashCode() => PrimaryKey.GetHashCode();
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
