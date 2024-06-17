using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using System.Diagnostics;

namespace ScheduleBSUIR.Services
{
    public class TimetableService(WebService webService, DbService dbService, IDateTimeProvider dateTimeProvider)
    {
        private readonly WebService _webService = webService;
        private readonly DbService _dbService = dbService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<Timetable> GetTimetableAsync(TypedId id, CancellationToken cancellationToken)
        {
            Timetable? timetable = default;

            var lastUpdateResponse = await _webService.GetTimetableLastUpdateAsync(id, cancellationToken);

            var cachedTimetable = _dbService.Get<Timetable>(id.ToString());

            // If timetable is not yet cached and NOT expired
            if(cachedTimetable is not null
                && lastUpdateResponse.LastUpdateDate <= cachedTimetable.UpdatedAt)
            {
                Debug.WriteLine("Cached timetable found!");
                timetable = cachedTimetable;
            }
            // Else obtain from web
            else
            {
                Debug.WriteLine("Cached timetable NOT found!");
                timetable = await _webService.GetTimetableAsync(id, cancellationToken);

                if(timetable is null)
                {
                    throw new ArgumentException("Couldn't obtain timetable with given id from web service", nameof(id));
                }

                Debug.WriteLine("Updated timetable in DB");
                timetable.UpdatedAt = _dateTimeProvider.Now;
                _dbService.AddOrUpdate(timetable);
            }

            return timetable ?? throw new ArgumentException("Timetable with given id not found", nameof(id));
        }
    }
}
