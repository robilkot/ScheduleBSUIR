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

            var cachedTimetable = await _dbService.GetAsync<Timetable>(id.PrimaryKey);

            bool offline = Connectivity.NetworkAccess is NetworkAccess.None;

            if (offline)
            {
                if (cachedTimetable is null)
                {
                    throw new FileNotFoundException($"Timetable NOT cached yet for {id}");
                }

                timetable = cachedTimetable;
                _loggingService.LogInfo($"Timetable cached for {id}", displayCaller: false);
            }
            else
            {
                var lastUpdateResponse = await _webService.GetTimetableLastUpdateAsync(id, cancellationToken);

                // If timetable is not yet cached and NOT expired
                if (cachedTimetable is not null
                    && lastUpdateResponse?.LastUpdateDate <= cachedTimetable.UpdatedAt)
                {
                    _loggingService.LogInfo($"Timetable cached for {id}", displayCaller: false);
                    timetable = cachedTimetable;
                }
                // Else obtain from api
                else
                {
                    _loggingService.LogInfo($"Timetable NOT cached yet for {id}", displayCaller: false);
                    timetable = await _webService.GetTimetableAsync(id, cancellationToken);

                    if (timetable is null)
                    {
                        throw new ArgumentException($"Error obtaining timetable for {id}");
                    }

                    // Update property for ICacheable interface
                    timetable.UpdatedAt = _dateTimeProvider.UtcNow;
                }

                // Update property for IUpdateAware interface
                timetable.AccessedAt = _dateTimeProvider.UtcNow;

                // Schedules come unsorted :(
                timetable.Exams = timetable.Exams?
                    .OrderBy(s => s.DateLesson)
                    .ThenBy(s => s.StartLessonTime)
                    .ThenBy(s => s.EndLessonTime)
                    .ToList();

                await _dbService.AddOrUpdateAsync(timetable);
            }

            return timetable;
        }

        public async Task AddToFavoritesAsync<T>(T timetableId) where T : TypedId
        {
            switch (timetableId)
            {
                case StudentGroupId studentGroupId: {
                        await _dbService.AddOrUpdateAsync(studentGroupId);
                        break;
                    }
                case EmployeeId employeeId: {
                        await _dbService.AddOrUpdateAsync(employeeId);
                        break;
                    }
                default: throw new UnreachableException();
            }

            _loggingService.LogInfo($"Id {timetableId} added to favorites", displayCaller: false);
        }

        public async Task RemoveFromFavoritesAsync<T>(T timetableId) where T : TypedId
        {
            switch(timetableId)
            {
                case StudentGroupId studentGroupId: {
                        await _dbService.RemoveAsync(studentGroupId);
                        break;
                    }
                case EmployeeId employeeId: {
                        await _dbService.RemoveAsync(employeeId);
                        break;
                    }
                default: throw new UnreachableException();
            }

            _loggingService.LogInfo($"Id {timetableId} removed from favorites", displayCaller: false);
        }

        public async Task<bool> IsFavoritedAsync<T>(T? timetableId) where T : TypedId
        {
            if (timetableId is null)
                return false;

            TypedId? timetableIdInDb = timetableId switch
            {
                StudentGroupId studentGroupId => await _dbService.GetAsync<StudentGroupId>(studentGroupId.PrimaryKey),
                EmployeeId employeeId => await _dbService.GetAsync<EmployeeId>(employeeId.PrimaryKey),
                _ => throw new UnreachableException(),
            };

            return timetableIdInDb is not null;
        }

        public async Task<List<StudentGroupId>> GetFavoriteGroupsTimetablesIdsAsync()
        {
            // Calling GetAllAsync<TypedId> will not return both StudentGroupIds and EmployeeIds because of how LiteDb works 
            // => using generic method
            List<StudentGroupId> ids = await _dbService.GetAllAsync<StudentGroupId>();

            return ids;
        }
        public async Task<List<EmployeeId>> GetFavoriteEmployeesTimetablesIdsAsync()
        {
            List<EmployeeId> ids = await _dbService.GetAllAsync<EmployeeId>();

            return ids;
        }

        public DateTime? GetLastScheduleDate(Timetable? timetable,
            TimetableTabs timetableTabs = TimetableTabs.Schedule,
            SubgroupType subgroupType = SubgroupType.All)
        {
            if (timetable is null)
                return null;

            DateTime? result = null;

            // Assuming the list is sorted
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
            _loggingService.LogInfo($"GetLastScheduleDate {result?.ToString("dd.MM")} ({timetableTabs}, {subgroupType}).", displayCaller: false);

            return result;
        }

        public DateTime? GetFirstScheduleDate(Timetable? timetable,
            TimetableTabs timetableTabs = TimetableTabs.Schedule,
            SubgroupType subgroupType = SubgroupType.All)
        {
            if (timetable is null)
                return null;

            DateTime? result = null;

            // Assuming the list is sorted
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
            _loggingService.LogInfo($"GetFirstScheduleDate {result?.ToString("dd.MM")} ({timetableTabs}, {subgroupType}).", displayCaller: false);

            return result;
        }

        public DateTime? GetNearestScheduleDate(Timetable? timetable,
            TimetableTabs timetableTabs = TimetableTabs.Schedule,
            SubgroupType subgroupType = SubgroupType.All)
        {
            if (timetable is null)
                return null;

            DateTime? result = null;

            // Assuming the list is sorted
            if (timetableTabs is TimetableTabs.Exams)
            {
                var firstSchedule = subgroupType switch
                {
                    SubgroupType.All => timetable.Exams?
                        .FirstOrDefault(schedule => schedule.DateLesson >= _dateTimeProvider.UtcNow.Date),

                    SubgroupType.FirstSubgroup => timetable.Exams?
                        .FirstOrDefault(schedule => schedule.NumSubgroup != SubgroupType.SecondSubgroup && schedule.DateLesson >= _dateTimeProvider.UtcNow.Date),

                    SubgroupType.SecondSubgroup => timetable.Exams?
                        .FirstOrDefault(schedule => schedule.NumSubgroup != SubgroupType.FirstSubgroup && schedule.DateLesson >= _dateTimeProvider.UtcNow.Date),

                    _ => throw new UnreachableException(),
                };

                result = firstSchedule?.DateLesson;
            }

            // todo for schedule tab
            _loggingService.LogInfo($"GetNearestScheduleDate {result?.ToString("dd.MM")} ({timetableTabs}, {subgroupType}).", displayCaller: false);

            return result;
        }

        public Task<List<DailySchedule>?> GetDaySchedulesAsync(Timetable? timetable,
            DateTime? startDate,
            DateTime? endDate,
            TimetableTabs timetableTabs = TimetableTabs.Schedule,
            SubgroupType subgroupType = SubgroupType.All)
        {
            if (timetable is null)
                return Task.FromResult<List<DailySchedule>?>(null);

            IEnumerable<DailySchedule>? result = null;

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
                        .Select(grouping => new DailySchedule(grouping));
            }

            TaskCompletionSource<List<DailySchedule>?> tcs = new();

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
