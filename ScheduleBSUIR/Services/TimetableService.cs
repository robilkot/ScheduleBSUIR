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
            var lastUpdateResponse = await _webService.GetTimetableLastUpdateAsync(id, cancellationToken);

            var cachedTimetable = _dbService.Get<Timetable>(id.ToString());

            Timetable? timetable;

            // If timetable is not yet cached and NOT expired
            if (cachedTimetable is not null
                && lastUpdateResponse?.LastUpdateDate <= cachedTimetable.UpdatedAt)
            {
                Debug.WriteLine("Cached timetable found!");
                timetable = cachedTimetable;
            }
            // Else obtain from api
            else
            {
                Debug.WriteLine("Cached timetable NOT found!");
                timetable = await _webService.GetTimetableAsync(id, cancellationToken);

                if (timetable is null)
                {
                    throw new ArgumentException("Couldn't obtain timetable with given id from web service", nameof(id));
                }

                // Update property for ICacheable interface
                timetable.UpdatedAt = _dateTimeProvider.Now;
            }

            // Update property for ICacheable interface
            timetable.AccessedAt = _dateTimeProvider.Now;

            _dbService.AddOrUpdate(timetable);
            Debug.WriteLine("Updated timetable in DB");

            return timetable;
        }
    }
}
