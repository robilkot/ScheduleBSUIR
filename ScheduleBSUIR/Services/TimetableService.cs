using ScheduleBSUIR.Models;

namespace ScheduleBSUIR.Services
{
    public class TimetableService(WebService webService)
    {
        private readonly WebService _webService = webService;

        // todo: caching?
        public async Task<Timetable> GetTimetableAsync(TypedId id, CancellationToken cancellationToken)
        {
            var timetable = await _webService.GetTimetableAsync(id, cancellationToken);

            return timetable ?? throw new ArgumentException("Timetable with given id not found", nameof(id));
        }
    }
}
