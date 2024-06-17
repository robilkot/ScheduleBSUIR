using ScheduleBSUIR.Models;

namespace ScheduleBSUIR.Services
{
    public class TimetableService(WebService webService, DbService dbService)
    {
        private readonly WebService _webService = webService;
        private readonly DbService _dbService = dbService;

        // todo: caching?
        public async Task<Timetable> GetTimetableAsync(TypedId id, CancellationToken cancellationToken)
        {
            var lastUpdate = await _webService.GetTimetableLastUpdateAsync(id, cancellationToken);



            var timetable = await _webService.GetTimetableAsync(id, cancellationToken);

            return timetable ?? throw new ArgumentException("Timetable with given id not found", nameof(id));
        }
    }
}
