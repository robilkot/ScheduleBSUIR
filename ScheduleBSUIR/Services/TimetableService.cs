using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Data.Linq;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using System.Diagnostics;

namespace ScheduleBSUIR.Services
{
    public class TimetableService(WebService webService, DbService dbService, IDateTimeProvider dateTimeProvider, ILoggingService loggingService, PreferencesService preferencesService)
    {
        private readonly WebService _webService = webService;
        private readonly DbService _dbService = dbService;
        private readonly ILoggingService _loggingService = loggingService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
        private readonly PreferencesService _preferencesService = preferencesService;

        private const string StudentGroupIdType = nameof(StudentGroupIdType);
        private const string EmployeeIdType = nameof(EmployeeIdType);
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
            }
            else
            {
                var lastUpdateResponse = await _webService.GetTimetableLastUpdateAsync(id, cancellationToken);

                // If timetable is not yet cached and NOT expired
                if (cachedTimetable is not null
                    && lastUpdateResponse?.LastUpdateDate <= cachedTimetable.UpdatedAt)
                {
                    timetable = cachedTimetable;
                }
                // Else obtain from api
                else
                {
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

        #region Timetable states
        public async Task<TimetableState> GetState(TypedId timetableId) =>
            await IsPinnedAsync(timetableId)
            ? TimetableState.Pinned 
            : await IsFavoritedAsync(timetableId) 
            ? TimetableState.Favorite 
            : TimetableState.Default;
        public async Task ApplyState(TypedId timetableId, TimetableState state)
        {
            var currentPinnedId = await GetPinnedIdAsync();

            switch (state)
            {
                case TimetableState.Default:
                    {
                        await RemoveFromFavoritesAsync(timetableId);

                        if (timetableId.Equals(currentPinnedId))
                        {
                            await SetPinnedIdAsync(null);
                        }
                        break;
                    }
                case TimetableState.Favorite:
                    {
                        await AddToFavoritesAsync(timetableId);

                        if (timetableId.Equals(currentPinnedId))
                        {
                            await SetPinnedIdAsync(null);
                        }
                        break;
                    }
                case TimetableState.Pinned:
                    {
                        await AddToFavoritesAsync(timetableId);
                        await SetPinnedIdAsync(timetableId);
                        break;
                    }
            }
        }

        #endregion

        #region Pinned timetables
        public async Task<TypedId?> GetPinnedIdAsync()
        {
            string preference = _preferencesService.GetPinnedIdPreference();

            if (string.IsNullOrEmpty(preference))
                return null;

            string[] preferenceParts = preference.Split(' ');

            TypedId? result = preferenceParts[0] switch
            {
                StudentGroupIdType => await _dbService.GetAsync<StudentGroupId>(preferenceParts[1]),
                EmployeeIdType => await _dbService.GetAsync<EmployeeId>(preferenceParts[1]),
                _ => throw new UnreachableException(),
            };

            _loggingService.LogInfo($"GetPinnedTimetableId {result}", displayCaller: false);

            return result;
        }
        public Task SetPinnedIdAsync(TypedId? id)
        {
            string preference = id switch
            {
                StudentGroupId studentGroupId => $"{StudentGroupIdType} {studentGroupId.PrimaryKey}",
                EmployeeId employeeId => $"{EmployeeIdType} {employeeId.PrimaryKey}",
                null => string.Empty,
                _ => throw new UnreachableException(),
            };

            _preferencesService.SetPinnedIdPreference(preference);

            _loggingService.LogInfo($"SetPinnedTimetableId {id} ", displayCaller: false);

            WeakReferenceMessenger.Default.Send(new TimetablePinnedMessage(id));

            return Task.FromResult(true);
        }
        public async Task<bool> IsPinnedAsync(TypedId? id) => (await GetPinnedIdAsync())?.Equals(id) ?? false;

        #endregion

        #region Favorite timetables
        public async Task AddToFavoritesAsync<T>(T timetableId) where T : TypedId
        {
            bool alreadyFavorited;

            switch (timetableId)
            {
                case StudentGroupId studentGroupId:
                    {
                        alreadyFavorited = await _dbService.GetAsync<StudentGroupId>(studentGroupId.PrimaryKey) is not null;
                        await _dbService.AddOrUpdateAsync(studentGroupId);
                        break;
                    }
                case EmployeeId employeeId:
                    {
                        alreadyFavorited = await _dbService.GetAsync<EmployeeId>(employeeId.PrimaryKey) is not null;
                        await _dbService.AddOrUpdateAsync(employeeId);
                        break;
                    }
                default: throw new UnreachableException();
            }

            if (!alreadyFavorited)
            {
                WeakReferenceMessenger.Default.Send(new TimetableFavoritedMessage(timetableId));

                _loggingService.LogInfo($"Id {timetableId} added to favorites", displayCaller: false);
            }
        }

        public async Task RemoveFromFavoritesAsync<T>(T timetableId) where T : TypedId
        {
            switch (timetableId)
            {
                case StudentGroupId studentGroupId:
                    {
                        await _dbService.RemoveAsync(studentGroupId);
                        break;
                    }
                case EmployeeId employeeId:
                    {
                        await _dbService.RemoveAsync(employeeId);
                        break;
                    }
                default: throw new UnreachableException();
            }

            WeakReferenceMessenger.Default.Send(new TimetableUnfavoritedMessage(timetableId));

            _loggingService.LogInfo($"Id {timetableId} removed from favorites", displayCaller: false);
        }

        public async Task<bool> IsFavoritedAsync<T>(T timetableId) where T : TypedId => timetableId switch
            {
                StudentGroupId studentGroupId => await _dbService.GetAsync<StudentGroupId>(studentGroupId.PrimaryKey) is not null,
                EmployeeId employeeId => await _dbService.GetAsync<EmployeeId>(employeeId.PrimaryKey) is not null,
                _ => throw new UnreachableException(),
            };

        public async Task<List<StudentGroupId>> GetFavoriteGroupsTimetablesIdsAsync() => await _dbService.GetAllAsync<StudentGroupId>();

        public async Task<List<EmployeeId>> GetFavoriteEmployeesTimetablesIdsAsync() => await _dbService.GetAllAsync<EmployeeId>();

        #endregion

        // todo: move to schedule generator
        public DateTime? GetLastScheduleDate(Timetable timetable,
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

        public DateTime? GetFirstScheduleDate(Timetable timetable,
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

        public DateTime? GetNearestScheduleDate(Timetable timetable,
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

        public Task<List<DailySchedule>?> GetDaySchedulesAsync(Timetable timetable,
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
