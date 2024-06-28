using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Data.XtraReports.Native;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Models.UI;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class TimetablePageViewModel(
        TimetableService timetableService,
        ILoggingService loggingService,
        IDateTimeProvider dateTimeProvider)
        : BaseViewModel(loggingService), IQueryAttributable
    {
        private readonly TimetableService _timetableService = timetableService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        private readonly TimeSpan _loadingStep = TimeSpan.FromDays(10);
        private DateTime? _loadedToDate = null;
        private DateTime? _nearestScheduleDate = null;
        private DateTime? _lastScheduleDate = null;

        [ObservableProperty]
        private bool _isLoadingMoreSchedule = false;

        [ObservableProperty]
        private bool _isTimetableModePopupOpen = false;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private TimetableTabs _selectedTab = TimetableTabs.Exams;
        partial void OnSelectedTabChanged(TimetableTabs value)
        {
            ClearLoadedSchedule();

            LoadMoreScheduleCommand.Execute(true);
        }

        [ObservableProperty]
        private SubgroupType _selectedMode = (SubgroupType)Preferences.Get(PreferencesKeys.SelectedSubgroupType, (int)SubgroupType.All);
        partial void OnSelectedModeChanged(SubgroupType value)
        {
            Preferences.Set(PreferencesKeys.SelectedSubgroupType, (int)value);

            ClearLoadedSchedule();

            LoadMoreScheduleCommand.Execute(true);
        }

        [ObservableProperty]
        private Timetable? _timetable;
        partial void OnTimetableChanged(Timetable? value)
        {
            ClearLoadedSchedule();

            LoadMoreScheduleCommand.Execute(true);
        }

        [ObservableProperty]
        private bool _favorited = false;

        [ObservableProperty]
        private ObservableRangeCollection<ITimetableItem> _schedule = [];

        [ObservableProperty]
        private TypedId _timetableId = default!;
        async partial void OnTimetableIdChanged(TypedId value)
        {
            Favorited = await _timetableService.IsFavoritedAsync(value);

            _loggingService.LogInfo($"Timetable {value} is favorited: {Favorited}", displayCaller: false);
        }

        [ObservableProperty]
        private TypedId? _previousTimetableId;

        [ObservableProperty]
        private bool _isBackButtonVisible = false;

        [RelayCommand]
        public async Task LoadMoreSchedule(bool? scrollToNearest = false)
        {
            if (Timetable is null)
                return;

            // Initial case
            _nearestScheduleDate ??= _timetableService.GetNearestScheduleDate(Timetable, SelectedTab, SelectedMode);

            _loadedToDate ??= (_timetableService.GetFirstScheduleDate(Timetable, SelectedTab, SelectedMode) ?? _dateTimeProvider.UtcNow)
                - TimeSpan.FromDays(1); // -One additional day to account for adding extra day down below

            _lastScheduleDate ??= _timetableService.GetLastScheduleDate(Timetable, SelectedTab, SelectedMode);

            // Guard case for overflow if no schedules found or already loaded all possible schedules
            if (_lastScheduleDate is null || _loadedToDate >= _lastScheduleDate)
            {
                IsLoadingMoreSchedule = false;
                return;
            }

            // Add extra day since GetDaySchedulesAsync accepts [begin, end] dates range
            _loadedToDate += TimeSpan.FromDays(1);

            // Common case
            List<DailySchedule> newSchedules = [];

            do
            {
                var daysFromIteration = await _timetableService.GetDaySchedulesAsync(Timetable, _loadedToDate, _loadedToDate + _loadingStep, SelectedTab, SelectedMode);

                newSchedules.AddRange(daysFromIteration ?? []);

                _loadedToDate += _loadingStep;

                _loggingService.LogInfo($"GetDaySchedules got {daysFromIteration?.Count} objects ({_loadedToDate?.ToString("dd.MM")} - {(_loadedToDate + _loadingStep)?.ToString("dd.MM")})", displayCaller: false);
            }
            // Repeat loading if we need to load till nearest schedule. todo: maybe should be single 'if'
            while (_loadedToDate < _nearestScheduleDate && (scrollToNearest ?? false));

            Schedule ??= [];

            // todo: also add ScheduleWeek?
            foreach (var day in newSchedules ?? [])
            {
                Schedule.Add(new ScheduleDay(day.Day));

                Schedule.AddRange(day);
            }

            IsLoadingMoreSchedule = false;

            if (scrollToNearest ?? false)
            {
                ScrollToActiveSchedule();
            }
        }

        [RelayCommand]
        public async Task GetTimetable()
        {
            if (TimetableId is null)
                return;

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Timetable = await _timetableService.GetTimetableAsync(TimetableId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Timetable = null;

                // todo: error popup?
                await Shell.Current.DisplayAlert("Error", "Couldn't get timetable", "OK");

                _loggingService.LogError($"GetTimetable threw: {ex.Message}\n{ex.StackTrace}", displayCaller: false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public static async Task NavigateBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task ToogleBookmark()
        {
            if (TimetableId is null)
                return;

            if (Favorited)
            {
                await _timetableService.RemoveFromFavoritesAsync(TimetableId);

                Favorited = false;

                Vibration.Default.Vibrate(80f);
            }
            else
            {
                await _timetableService.AddToFavoritesAsync(TimetableId);

                Favorited = true;

                Vibration.Default.Vibrate(80f);
                await Task.Delay(150);
                Vibration.Default.Vibrate(50f);
            }
        }

        [RelayCommand]
        public void ToggleMode(SubgroupType mode)
        {
            SelectedMode = mode;

            IsTimetableModePopupOpen = false;
        }

        [RelayCommand]
        public void ToggleTimetableModePopup()
        {
            IsTimetableModePopupOpen = !IsTimetableModePopupOpen;
        }

        // Accepts studentgroup dto or employeedto
        [RelayCommand]
        public async Task NavigateToTimetable(object dto)
        {
            var timetableId = TypedId.Create(dto);

            if (timetableId.Equals(PreviousTimetableId))
            {
                await NavigateBack();
                return;
            }

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, timetableId },
                { NavigationKeys.PreviousTimetableId, TimetableId },
                { NavigationKeys.IsBackButtonVisible, true },
            };

            // Let bottomsheet close smoothly
            await Task.Delay(150);

            await Shell.Current.GoToAsync(nameof(TimetablePage), true, navigationParameters);
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(NavigationKeys.TimetableId, out var id))
            {
                TimetableId = (TypedId)id;
            }
            if (query.TryGetValue(NavigationKeys.PreviousTimetableId, out var prevId))
            {
                PreviousTimetableId = (TypedId?)prevId;
            }
            if (query.TryGetValue(NavigationKeys.IsBackButtonVisible, out var isBackButtonVisible))
            {
                IsBackButtonVisible = (bool)isBackButtonVisible;
            }

            GetTimetableCommand.Execute(null);
        }

        private void ClearLoadedSchedule()
        {
            _loadedToDate = null;
            _lastScheduleDate = null;
            _nearestScheduleDate = null;

            Schedule = [];
        }
        private int? GetNearestScheduleIndex()
        {
            if (Timetable is null)
                return null;

            var foundSchedule = Schedule?.FirstOrDefault(e => e is Schedule schedule && schedule.DateLesson >= _dateTimeProvider.UtcNow.Date);

            return foundSchedule is null ? null : Schedule?.IndexOf(foundSchedule);
        }
        private void ScrollToActiveSchedule()
        {
            int? nearestScheduleIndex = GetNearestScheduleIndex();

            if (nearestScheduleIndex is not null)
            {
                ScrollToIndex message = new(nearestScheduleIndex.Value);

                WeakReferenceMessenger.Default.Send(message);
            }
        }
    }
}
