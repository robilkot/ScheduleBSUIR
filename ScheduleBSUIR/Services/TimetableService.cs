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
            Timetable? timetable;

            var cachedTimetable = await _dbService.GetAsync<Timetable>(id.ToString());
            
            bool offline = Connectivity.NetworkAccess is NetworkAccess.None;

            if (offline)
            {
                if (cachedTimetable is null)
                {
                    throw new FileNotFoundException($"No cached timetable found for {id}");
                }

                timetable = cachedTimetable;
                _loggingService.LogInfo($"Cached timetable found for {id}", displayCaller: false);
            }
            else 
            {
                bool cachedTimetableIsFavorite = false;

                if(cachedTimetable is not null)
                {
                    cachedTimetableIsFavorite = cachedTimetable.Favorited;
                }

                var lastUpdateResponse = await _webService.GetTimetableLastUpdateAsync(id, cancellationToken);

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
                        throw new ArgumentException($"Couldn't obtain timetable for {id} from web service");
                    }

                    // Update property for ICacheable interface
                    timetable.UpdatedAt = _dateTimeProvider.Now;
                }

                // Update property for ICacheable interface
                timetable.AccessedAt = _dateTimeProvider.Now;

                // If we obtained timetable from web api, make sure favorited flag remains from locally stored timetable
                timetable.Favorited = cachedTimetableIsFavorite;

                await _dbService.AddOrUpdateAsync(timetable);
            }

            return timetable;
        }

        public async Task AddToFavorites(Timetable timetable)
        {
            timetable.Favorited = true;

            await _dbService.AddOrUpdateAsync(timetable);
        }

        public async Task RemoveFromFavorites(Timetable timetable)
        {
            timetable.Favorited = false;

            await _dbService.AddOrUpdateAsync(timetable);
        }
    }
}
