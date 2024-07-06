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

        private const string StudentGroupTimetableHeaderType = nameof(StudentGroupTimetableHeaderType);
        private const string EmployeeTimetableHeaderType = nameof(EmployeeTimetableHeaderType);

        [Time]
        public async Task<Timetable> GetTimetableAsync(TimetableHeader header, CancellationToken cancellationToken)
        {
            Timetable? timetable;

            var cachedTimetable = await _dbService.GetAsync<Timetable>(header.PrimaryKey);

            bool offline = Connectivity.NetworkAccess is NetworkAccess.None;

            if (offline)
            {
                if (cachedTimetable is null)
                {
                    throw new FileNotFoundException($"Timetable NOT cached yet for {header}");
                }

                timetable = cachedTimetable;
            }
            else
            {
                var lastUpdateResponse = await _webService.GetTimetableLastUpdateAsync(header, cancellationToken);

                // If timetable is not yet cached and NOT expired
                if (cachedTimetable is not null
                    && lastUpdateResponse?.LastUpdateDate <= cachedTimetable.UpdatedAt)
                {
                    timetable = cachedTimetable;
                }
                // Else obtain from api
                else
                {
                    timetable = await _webService.GetTimetableAsync(header, cancellationToken);

                    if (timetable is null)
                    {
                        throw new ArgumentException($"Error obtaining timetable for {header}");
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
        public async Task<TimetableState> GetTimetableStateAsync(TimetableHeader? header)
        {
            var result =
                header is null
                ? TimetableState.Default
                : await IsPinnedAsync(header)
                ? TimetableState.Pinned
                : await IsFavoritedAsync(header)
                ? TimetableState.Favorite
                : TimetableState.Default;

            _loggingService.LogInfo($"Id {header} GetStateAsync {result}", displayCaller: false);

            return result;
        }

        public async Task SetStateAsync(TimetableHeader header, TimetableState state)
        {
            var previousState = await GetTimetableStateAsync(header);

            if (state == previousState)
                return;

            switch (state)
            {
                case TimetableState.Default:
                    {
                        await RemoveFromFavoritesAsync(header);

                        if (previousState == TimetableState.Pinned)
                        {
                            SetPinnedTimetable(null);
                        }

                        break;
                    }
                case TimetableState.Favorite:
                    {
                        await AddToFavoritesAsync(header);

                        if (previousState == TimetableState.Pinned)
                        {
                            SetPinnedTimetable(null);
                        }

                        break;
                    }
                case TimetableState.Pinned:
                    {
                        await AddToFavoritesAsync(header);

                        var previousPinnedId = await GetPinnedTimetableAsync();

                        if (previousPinnedId is not null)
                        {
                            await SetStateAsync(previousPinnedId, TimetableState.Favorite);
                        }

                        SetPinnedTimetable(header);
                        break;
                    }
            }

            WeakReferenceMessenger.Default.Send(new TimetableStateChangedMessage((header, state)));

            _loggingService.LogInfo($"Id {header} SetStateAsync {state}", displayCaller: false);
        }

        #endregion

        #region Pinned timetables
        public async Task<TimetableHeader?> GetPinnedTimetableAsync()
        {
            string preference = _preferencesService.GetPinnedIdPreference();

            if (string.IsNullOrEmpty(preference))
                return null;

            string[] preferenceParts = preference.Split(' ');

            TimetableHeader? result = preferenceParts[0] switch
            {
                StudentGroupTimetableHeaderType => await _dbService.GetAsync<StudentGroupTimetableHeader>(preferenceParts[1]),
                EmployeeTimetableHeaderType => await _dbService.GetAsync<EmployeeTimetableHeader>(preferenceParts[1]),
                _ => throw new UnreachableException(),
            };

            _loggingService.LogInfo($"GetPinnedTimetableId {result}", displayCaller: false);

            return result;
        }
        public async Task<bool> IsPinnedAsync(TimetableHeader? header) => (await GetPinnedTimetableAsync())?.Equals(header) ?? false;
        private void SetPinnedTimetable(TimetableHeader? header)
        {
            string preference = header switch
            {
                StudentGroupTimetableHeader studentGroupId => $"{StudentGroupTimetableHeaderType} {studentGroupId.PrimaryKey}",
                EmployeeTimetableHeader employeeId => $"{EmployeeTimetableHeaderType} {employeeId.PrimaryKey}",
                null => string.Empty,
                _ => throw new UnreachableException(),
            };

            _preferencesService.SetPinnedIdPreference(preference);

            _loggingService.LogInfo($"SetPinnedTimetableId {header} ", displayCaller: false);
        }

        #endregion

        #region Favorite timetables
        private async Task AddToFavoritesAsync<T>(T timetableId) where T : TimetableHeader
        {
            switch (timetableId)
            {
                case StudentGroupTimetableHeader studentGroupId:
                    {
                        await _dbService.AddOrUpdateAsync(studentGroupId);
                        break;
                    }
                case EmployeeTimetableHeader employeeId:
                    {
                        await _dbService.AddOrUpdateAsync(employeeId);
                        break;
                    }
                default: throw new UnreachableException();
            }
        }

        private async Task RemoveFromFavoritesAsync<T>(T timetableId) where T : TimetableHeader
        {
            switch (timetableId)
            {
                case StudentGroupTimetableHeader studentGroupId:
                    {
                        await _dbService.RemoveAsync(studentGroupId);
                        break;
                    }
                case EmployeeTimetableHeader employeeId:
                    {
                        await _dbService.RemoveAsync(employeeId);
                        break;
                    }
                default: throw new UnreachableException();
            }

            _loggingService.LogInfo($"Id {timetableId} removed from favorites", displayCaller: false);
        }

        public async Task<bool> IsFavoritedAsync<T>(T timetableId) where T : TimetableHeader => timetableId switch
        {
            StudentGroupTimetableHeader studentGroupId => await _dbService.GetAsync<StudentGroupTimetableHeader>(studentGroupId.PrimaryKey) is not null,
            EmployeeTimetableHeader employeeId => await _dbService.GetAsync<EmployeeTimetableHeader>(employeeId.PrimaryKey) is not null,
            _ => throw new UnreachableException(),
        };

        public async Task<List<StudentGroupTimetableHeader>> GetFavoriteGroupsTimetablesHeadersAsync() => await _dbService.GetAllAsync<StudentGroupTimetableHeader>();

        public async Task<List<EmployeeTimetableHeader>> GetFavoriteEmployeesTimetablesHeadersAsync() => await _dbService.GetAllAsync<EmployeeTimetableHeader>();

        #endregion
    }
}
