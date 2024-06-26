using ScheduleBSUIR.Models;
using System.Diagnostics;

namespace ScheduleBSUIR.Helpers
{
    public static class UrlGenerator
    {
        public static string BaseApiUrl() => "https://iis.bsuir.by/api/v1";
        public static string Timetable(TypedId id) => id switch
        {
            StudentGroupId groupId => $"{BaseApiUrl()}/schedule?studentGroup={groupId}",
            EmployeeId employeeId => $"{BaseApiUrl()}/employees/schedule/{employeeId}",
            _ => throw new UnreachableException(),
        };
        public static string TimetableLastUpdate(TypedId id) => id switch
        {
            StudentGroupId groupId => $"{BaseApiUrl()}/last-update-date/student-group?groupNumber={groupId}",
            EmployeeId employeeId => $"{BaseApiUrl()}/last-update-date/employee?id={employeeId}",
            _ => throw new UnreachableException(),
        };
        public static string Groups() => $"{BaseApiUrl()}/student-groups";
        public static string GroupsHeaders(string name) => $"{Groups()}/filters?name={name}";
        public static string Employees() => $"{BaseApiUrl()}/employees/all";
        public static string CurrentWeek() => $"{BaseApiUrl()}/schedule/current-week";
    }
}
