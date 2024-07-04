using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.API;

namespace ScheduleBSUIR.Interfaces
{
    public interface IWebService
    {
        Task<IEnumerable<Employee>?> GetEmployeesAsync(CancellationToken cancellationToken);
        Task<IEnumerable<StudentGroupHeader>?> GetGroupHeadersAsync(string groupNameFilter, CancellationToken cancellationToken);
        Task<Timetable?> GetTimetableAsync(TypedId id, CancellationToken cancellationToken);
        Task<LastUpdateResponse?> GetTimetableLastUpdateAsync(TypedId id, CancellationToken cancellationToken);
        Task<int?> GetCurrentWeekAsync(CancellationToken cancellationToken);
    }
}