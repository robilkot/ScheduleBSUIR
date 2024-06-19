using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;

namespace ScheduleBSUIR.Services
{
    public class TimetableService(WebService webService, DbService dbService, IDateTimeProvider dateTimeProvider, ILoggingService loggingService)
    {
        private readonly WebService _webService = webService;
        private readonly DbService _dbService = dbService;
        private readonly ILoggingService _loggingService = loggingService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<Timetable> GetTimetableAsync(TypedId id, CancellationToken cancellationToken)
        {
            var lastUpdateResponse = await _webService.GetTimetableLastUpdateAsync(id, cancellationToken);

            _loggingService.LogInfo($"Timetable for {id} last changed at {lastUpdateResponse?.LastUpdateDate}", displayCaller: false);

            var cachedTimetable = _dbService.Get<Timetable>(id.ToString());

            Timetable? timetable;

            // If timetable is not yet cached and NOT expired
            if (cachedTimetable is not null
                && lastUpdateResponse?.LastUpdateDate <= cachedTimetable.UpdatedAt)
            {
                _loggingService.LogInfo($"Cached timetable found for {id}", displayCaller: false);
                timetable = cachedTimetable;
            }
            // Else obtain from api
            else
            {
                _loggingService.LogInfo($"Cached timetable NOT found for {id}", displayCaller: false);
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
            _loggingService.LogInfo($"Updated DB timetable for {id}", displayCaller: false);

            return timetable;
        }

        public async Task AddToFavorites(Timetable timetable)
        {
            timetable.Favorited = true;

            // todo: save to db
        }
        public async Task RemoveFromFavorites(Timetable timetable)
        {
            timetable.Favorited = false;

            // todo: save to db
        }
    }
}
