using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using System.Diagnostics;

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

                if (cachedTimetable is not null)
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

        public DateTime? GetLastScheduleDate(Timetable? timetable,
            TimetableTabs timetableTabs = TimetableTabs.Schedule,
            SubgroupType subgroupType = SubgroupType.All)
        {
            if (timetable is null)
                return null;

            DateTime? result = null;

            if (timetableTabs is TimetableTabs.Exams)
            {
                var lastSchedule = subgroupType switch
                {
                    SubgroupType.All => timetable.Exams?
                        .LastOrDefault(),

                    SubgroupType.FirstSubgroup => timetable.Exams?
                        .LastOrDefault(schedule => schedule is { NumSubgroup: not SubgroupType.SecondSubgroup }),

                    SubgroupType.SecondSubgroup => timetable.Exams?
                        .LastOrDefault(schedule => schedule is { NumSubgroup: not SubgroupType.FirstSubgroup }),

                    _ => throw new UnreachableException(),
                };

                result = lastSchedule?.DateLesson;
            }

            // todo for schedule tab           
            _loggingService.LogInfo($"GetLastScheduleDate returned {result?.ToString("dd.MM")}.", displayCaller: false);

            return result;
        }

        public DateTime? GetFirstScheduleDate(Timetable? timetable,
            TimetableTabs timetableTabs = TimetableTabs.Schedule,
            SubgroupType subgroupType = SubgroupType.All)
        {
            if (timetable is null)
                return null;

            DateTime? result = null;

            if (timetableTabs is TimetableTabs.Exams)
            {
                var firstSchedule = subgroupType switch
                {
                    SubgroupType.All => timetable.Exams?
                        .FirstOrDefault(),

                    SubgroupType.FirstSubgroup => timetable.Exams?
                        .FirstOrDefault(schedule => schedule is { NumSubgroup: not SubgroupType.SecondSubgroup }),

                    SubgroupType.SecondSubgroup => timetable.Exams?
                        .FirstOrDefault(schedule => schedule is { NumSubgroup: not SubgroupType.FirstSubgroup }),

                    _ => throw new UnreachableException(),
                };

                result = firstSchedule?.DateLesson;
            }

            // todo for schedule tab

            return result;
        }

        public Task<List<DaySchedule>?> GetDaySchedulesAsync(Timetable? timetable,
            DateTime? startDate,
            DateTime? endDate,
            TimetableTabs timetableTabs = TimetableTabs.Schedule,
            SubgroupType subgroupType = SubgroupType.All)
        {
            if (timetable is null)
                return Task.FromResult<List<DaySchedule>?>(null);

            IEnumerable<DaySchedule>? result = null;

            if (timetableTabs is TimetableTabs.Exams)
            {
                var schedules = subgroupType switch
                {
                    SubgroupType.All => timetable.Exams?
                        .Where(schedule => schedule.DateLesson >= startDate && schedule.DateLesson <= endDate),

                    SubgroupType.FirstSubgroup => timetable.Exams?
                        .Where(schedule => schedule.DateLesson >= startDate && schedule.DateLesson <= endDate)
                        .Where(schedule => schedule is { NumSubgroup: not SubgroupType.SecondSubgroup }),

                    SubgroupType.SecondSubgroup => timetable.Exams?
                        .Where(schedule => schedule.DateLesson >= startDate && schedule.DateLesson <= endDate)
                        .Where(schedule => schedule is { NumSubgroup: not SubgroupType.FirstSubgroup }),

                    _ => throw new UnreachableException(),
                };

                result = schedules?
                        .GroupBy(schedule => schedule.DateLesson)
                        .Select(grouping => new DaySchedule(grouping));
            }

            TaskCompletionSource<List<DaySchedule>?> tcs = new();

            _ = Task.Run(() =>
            {
                // Deferred LINQ
                var resultList = result?.ToList();

                tcs.SetResult(resultList);
            });

            return tcs.Task;
        }
    }
}
