using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Data.XtraReports.Native;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

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
        private DateTime? _lastScheduleDate = null;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Schedule))]
        private bool _isLoadingMoreSchedule = false;

        [ObservableProperty]
        private bool _isTimetableModePopupOpen = false;

        [ObservableProperty]
        string currentState = ViewStates.Loading;
        partial void OnCurrentStateChanged(string? oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                SetStateMessage setStateMessage = new(newValue);

                WeakReferenceMessenger.Default.Send(setStateMessage);
            }
        }

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private TimetableTabs _selectedTab = TimetableTabs.Exams;
        partial void OnSelectedTabChanged(TimetableTabs value)
        {
            LoadMoreScheduleCommand.Execute(true);
        }

        [ObservableProperty]
        private SubgroupType _selectedMode = (SubgroupType)Preferences.Get(PreferencesKeys.SelectedSubgroupType, (int)SubgroupType.All);
        partial void OnSelectedModeChanged(SubgroupType value)
        {
            Preferences.Set(PreferencesKeys.SelectedSubgroupType, (int)value);

            LoadMoreScheduleCommand.Execute(true);
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Favorited))]
        private Timetable? _timetable;
        partial void OnTimetableChanged(Timetable? value)
        {
            LoadMoreScheduleCommand.Execute(true);
        }

        [ObservableProperty]
        private bool _favorited = false;

        [ObservableProperty]
        private ObservableRangeCollection<DaySchedule>? _schedule = null;

        [ObservableProperty]
        private TypedId? _timetableId;
        async partial void OnTimetableIdChanged(TypedId? value)
        {
            Favorited = await _timetableService.IsFavoritedAsync(value);

            _loggingService.LogInfo($"Timetable {value} is favorited: {Favorited}", displayCaller: false);
        }

        [RelayCommand]
        public async Task LoadMoreSchedule(bool? reloadAll = false)
        {
            if (IsLoadingMoreSchedule)
                return;

            IsLoadingMoreSchedule = true;

            if (reloadAll ?? false)
            {
                CurrentState = ViewStates.Loading;

                _loadedToDate = null;
                _lastScheduleDate = null;

                Schedule = null;
            }

            // Initial case
            _lastScheduleDate ??= _timetableService.GetLastScheduleDate(Timetable, SelectedTab, SelectedMode);

            _loadedToDate ??= (_timetableService.GetFirstScheduleDate(Timetable, SelectedTab, SelectedMode) ?? _dateTimeProvider.UtcNow)
                - TimeSpan.FromDays(1); // -One additional day to account for adding extra day down below

            // Guard case for overflow if no schedules found or already loaded all possible schedules
            if (_lastScheduleDate is null || _loadedToDate >= _lastScheduleDate)
            {
                CurrentState = ViewStates.Loaded;

                IsLoadingMoreSchedule = false;
                return;
            }

            // Add extra day since GetDaySchedulesAsync accepts [begin, end] dates range
            _loadedToDate += TimeSpan.FromDays(1);

            // Common case
            var newSchedules = await _timetableService.GetDaySchedulesAsync(Timetable, _loadedToDate, _loadedToDate + _loadingStep, SelectedTab, SelectedMode);

            _loadedToDate += _loadingStep;

            _loggingService.LogInfo($"GetDaySchedules got {newSchedules?.Count} objects ({_loadedToDate?.ToString("dd.MM")} - {(_loadedToDate + _loadingStep)?.ToString("dd.MM")})", displayCaller: false);

            Schedule ??= [];

            Schedule.AddRange(newSchedules ?? []);

            CurrentState = ViewStates.Loaded;

            IsLoadingMoreSchedule = false;
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
        public async Task NavigateBack()
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

        private void ScrollToActiveSchedule()
        {
            int? nearestScheduleIndex = GetNearestScheduleIndex();

            if (nearestScheduleIndex is not null)
            {
                ScrollToIndex message = new(nearestScheduleIndex.Value);

                WeakReferenceMessenger.Default.Send(message);
            }
        }

        // Accepts studentgroup dto or employeedto
        [RelayCommand]
        public async Task NavigateToTimetable(object dto)
        {
            var timetableId = TypedId.Create(dto);

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, timetableId },
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

                GetTimetableCommand.Execute(null);
            }
        }
        private int? GetNearestScheduleIndex()
        {
            if (Timetable is null)
                return null;

            var foundSchedule = Schedule?.FirstOrDefault(e => e.FirstOrDefault()?.DateLesson >= _dateTimeProvider.UtcNow.Date);

            return foundSchedule is null ? null : Schedule?.IndexOf(foundSchedule);
        }
    }
}
