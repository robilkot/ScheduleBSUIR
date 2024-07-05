using CommunityToolkit.Mvvm.Messaging;
using MethodTimer;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using System.Diagnostics;

namespace ScheduleBSUIR.Services
{
    public class TimetableService(IWebService webService, DbService dbService, IDateTimeProvider dateTimeProvider, ILoggingService loggingService, PreferencesService preferencesService)
    {
        private readonly IWebService _webService = webService;
        private readonly DbService _dbService = dbService;
        private readonly ILoggingService _loggingService = loggingService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
        private readonly PreferencesService _preferencesService = preferencesService;

        private const string StudentGroupIdType = nameof(StudentGroupIdType);
        private const string EmployeeIdType = nameof(EmployeeIdType);

        [Time]
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
                    timetable.UpdatedAt = _dateTimeProvider.Now;
                }

                // Update property for IUpdateAware interface
                timetable.AccessedAt = _dateTimeProvider.Now;

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
        public async Task<TimetableState> GetStateAsync(TypedId? timetableId)
        {
            var result =
                timetableId is null
                ? TimetableState.Default
                : await IsPinnedAsync(timetableId)
                ? TimetableState.Pinned
                : await IsFavoritedAsync(timetableId)
                ? TimetableState.Favorite
                : TimetableState.Default;

            _loggingService.LogInfo($"Id {timetableId} GetStateAsync {result}", displayCaller: false);

            return result;
        }

        public async Task SetStateAsync(TypedId timetableId, TimetableState state)
        {
            var previousState = await GetStateAsync(timetableId);

            if (state == previousState)
                return;

            switch (state)
            {
                case TimetableState.Default:
                    {
                        await RemoveFromFavoritesAsync(timetableId);

                        if (previousState == TimetableState.Pinned)
                        {
                            SetPinnedId(null);
                        }

                        break;
                    }
                case TimetableState.Favorite:
                    {
                        await AddToFavoritesAsync(timetableId);

                        if (previousState == TimetableState.Pinned)
                        {
                            SetPinnedId(null);
                        }

                        break;
                    }
                case TimetableState.Pinned:
                    {
                        await AddToFavoritesAsync(timetableId);

                        var previousPinnedId = await GetPinnedIdAsync();

                        if (previousPinnedId is not null)
                        {
                            await SetStateAsync(previousPinnedId, TimetableState.Favorite);
                        }

                        SetPinnedId(timetableId);
                        break;
                    }
            }

            WeakReferenceMessenger.Default.Send(new TimetableStateChangedMessage((timetableId, state)));

            _loggingService.LogInfo($"Id {timetableId} SetStateAsync {state}", displayCaller: false);
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
        public async Task<bool> IsPinnedAsync(TypedId? id) => (await GetPinnedIdAsync())?.Equals(id) ?? false;
        private void SetPinnedId(TypedId? id)
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
        }

        #endregion

        #region Favorite timetables
        private async Task AddToFavoritesAsync<T>(T timetableId) where T : TypedId
        {
            switch (timetableId)
            {
                case StudentGroupId studentGroupId:
                    {
                        await _dbService.AddOrUpdateAsync(studentGroupId);
                        break;
                    }
                case EmployeeId employeeId:
                    {
                        await _dbService.AddOrUpdateAsync(employeeId);
                        break;
                    }
                default: throw new UnreachableException();
            }
        }

        private async Task RemoveFromFavoritesAsync<T>(T timetableId) where T : TypedId
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
    }
}
