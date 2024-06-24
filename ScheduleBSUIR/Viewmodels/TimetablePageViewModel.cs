using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Maui.Core.Internal;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
        private bool _isBusy;

        [ObservableProperty]
        private TimetableTabs _selectedTab = TimetableTabs.Exams;
        partial void OnSelectedTabChanged(TimetableTabs value)
        {
            ClearLoadedSchedule();

            LoadMoreScheduleCommand.Execute(null);

            ScrollToActiveSchedule();
        }

        [ObservableProperty]
        private SubgroupType _selectedMode = SubgroupType.All;
        partial void OnSelectedModeChanged(SubgroupType value)
        {
            ClearLoadedSchedule();

            LoadMoreScheduleCommand.Execute(null);

            ScrollToActiveSchedule();
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Favorited))]
        private Timetable? _timetable;
        partial void OnTimetableChanged(Timetable? value)
        {
            ClearLoadedSchedule();

            // todo: compare session dates instead of this?
            //SelectedTab = Timetable?.Exams?.Count > 0 ? TimetableTabs.Exams : TimetableTabs.Schedule;

            LoadMoreScheduleCommand.Execute(null);

            ScrollToActiveSchedule();
        }

        public bool Favorited => Timetable?.Favorited ?? false;

        [ObservableProperty]
        private ObservableCollection<DaySchedule>? _schedule = null;

        [ObservableProperty]
        private TypedId? _timetableId;

        // This property exists to display group number/employee name before timetable is loaded
        [ObservableProperty]
        private string _timetableHeader = string.Empty;

        [RelayCommand]
        public async Task LoadMoreSchedule()
        {
            // Initial case
            _lastScheduleDate ??= _timetableService.GetLastScheduleDate(Timetable, SelectedTab, SelectedMode);

            _loadedToDate ??= _timetableService.GetFirstScheduleDate(Timetable, SelectedTab, SelectedMode)
                ?? _dateTimeProvider.Now - TimeSpan.FromDays(1);

            // Guard case for overflow if no schedules found or already loaded all possible schedules
            if (_lastScheduleDate is null || _loadedToDate >= _lastScheduleDate)
            {
                IsLoadingMoreSchedule = false;
                return;
            }

            // Common case
            var newSchedules = await _timetableService.GetDaySchedulesAsync(Timetable, _loadedToDate, _loadedToDate + _loadingStep, SelectedTab, SelectedMode);

            // Add extra day since GetDaySchedulesAsync accepts [begin, end] dates range
            _loadedToDate += _loadingStep + TimeSpan.FromDays(1);

            Schedule ??= [];

            foreach (var schedule in newSchedules ?? [])
            {
                Schedule.Add(schedule);
            }

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

            _loggingService.LogInfo($"Getting timetable with id {TimetableId}", displayCaller: false);

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
            if (Timetable is null)
                return;

            if (Favorited)
            {
                await _timetableService.RemoveFromFavorites(Timetable);

                Vibration.Default.Vibrate(80f);
            }
            else
            {
                await _timetableService.AddToFavorites(Timetable);

                Vibration.Default.Vibrate(80f);
                await Task.Delay(150);
                Vibration.Default.Vibrate(50f);
            }

            OnPropertyChanged(nameof(Favorited));
        }

        [RelayCommand]
        public void ToggleMode(SubgroupType mode)
        {
            SelectedMode = mode;

            Preferences.Set(PreferencesKeys.SelectedSubgroupType, (int)mode);

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
            TypedId timetableId = dto switch
            {
                StudentGroupDto group => new StudentGroupId(group),
                EmployeeDto employee => new EmployeeId(employee),
                _ => throw new UnreachableException(),
            };

            // todo: all this header thing is a hack honestly
            string timetableHeader = dto switch
            {
                StudentGroupDto group => group.Name,
                EmployeeDto employee => employee.LastName,
                _ => throw new UnreachableException(),
            };

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, timetableId },
                { NavigationKeys.TimetableHeader, timetableHeader },
            };

            // Let bottomsheet close smoothly
            await Task.Delay(150);

            await Shell.Current.GoToAsync(nameof(TimetablePage), true, navigationParameters);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            SelectedMode = (SubgroupType)Preferences.Get(PreferencesKeys.SelectedSubgroupType, (int)SubgroupType.All);

            if (query.TryGetValue(NavigationKeys.TimetableId, out var id)
                && query.TryGetValue(NavigationKeys.TimetableHeader, out var header))
            {
                TimetableId = (TypedId)id;
                TimetableHeader = (string)header;

                GetTimetableCommand.Execute(null);
            }
        }
        private int? GetNearestScheduleIndex()
        {
            if (Timetable is null)
                return null;

            var foundSchedule = Schedule?.FirstOrDefault(e => e.FirstOrDefault()?.DateLesson >= _dateTimeProvider.Now.Date);

            return foundSchedule is null ? null : Schedule?.IndexOf(foundSchedule);
        }
        private void ClearLoadedSchedule()
        {
            _loadedToDate = null;
            _lastScheduleDate = null;

            Schedule = null;
        }
    }
}
