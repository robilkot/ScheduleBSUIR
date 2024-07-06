using ScheduleBSUIR.Models;
using System.Diagnostics;

namespace ScheduleBSUIR.Helpers
{
    public static class UrlGenerator
    {
        public static string BaseApiUrl() => "https://iis.bsuir.by/api/v1";
        public static string Timetable(TimetableHeader id) => id switch
        {
            StudentGroupTimetableHeader groupId => $"{BaseApiUrl()}/schedule?studentGroup={groupId}",
            EmployeeTimetableHeader employeeId => $"{BaseApiUrl()}/employees/schedule/{employeeId}",
            _ => throw new UnreachableException(),
        };
        public static string TimetableLastUpdate(TimetableHeader id) => id switch
        {
            StudentGroupTimetableHeader groupId => $"{BaseApiUrl()}/last-update-date/student-group?groupNumber={groupId}",
            EmployeeTimetableHeader employeeId => $"{BaseApiUrl()}/last-update-date/employee?id={employeeId}",
            _ => throw new UnreachableException(),
        };
        public static string Groups() => $"{BaseApiUrl()}/student-groups";
        public static string GroupsHeaders(string name) => $"{Groups()}/filters?name={name}";
        public static string Employees() => $"{BaseApiUrl()}/employees/all";
        public static string CurrentWeek() => $"{BaseApiUrl()}/schedule/current-week";
    }
}
