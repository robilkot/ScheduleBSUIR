using ScheduleBSUIR.Models;

namespace ScheduleBSUIR.Helpers
{
    public static class UrlGenerator
    {
        public static string BaseApiUrl() => "https://iis.bsuir.by/api/v1";
        public static string EmployeeTimetable(EmployeeId id) => $"{BaseApiUrl()}/employees/schedule/{id}";
        public static string EmployeeTimetableLastUpdate(EmployeeId id) => $"{BaseApiUrl()}/last-update-date/employee?id={id}";
        public static string StudentGroupTimetable(StudentGroupId id) => $"{BaseApiUrl()}/schedule?studentGroup={id}";
        public static string StudentGroupTimetableLastUpdate(StudentGroupId id) => $"{BaseApiUrl()}/last-update-date/student-group?groupNumber={id}";
        public static string Groups() => $"{BaseApiUrl()}/student-groups";
        public static string GroupsHeaders(string name) => $"{Groups()}/filters?name={name}";
        public static string Employees() => $"{BaseApiUrl()}/employees/all";
        public static string CurrentWeek() => $"{BaseApiUrl()}/schedule/current-week";
    }
}
