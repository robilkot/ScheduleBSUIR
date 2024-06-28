using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Data.XtraReports.Native;
using DevExpress.Maui.Core.Internal;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Models.UI;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class TimetablePageViewModel : BaseViewModel, IQueryAttributable, IRecipient<TimetablePinnedMessage>
    {
        private readonly TimetableService _timetableService;
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly TimeSpan _loadingStep = TimeSpan.FromDays(10);
        private DateTime? _loadedToDate = null;
        private DateTime? _nearestScheduleDate = null;
        private DateTime? _lastScheduleDate = null;

        public TimetablePageViewModel(TimetableService timetableService, ILoggingService loggingService, IDateTimeProvider dateTimeProvider) : base(loggingService)
        {
            _timetableService = timetableService;
            _dateTimeProvider = dateTimeProvider;

            WeakReferenceMessenger.Default.Register(this);
        }

        [ObservableProperty]
        private bool _isPinnedTimetable = false;

        [ObservableProperty]
        private bool _isLoadingMoreSchedule = false;

        [ObservableProperty]
        private bool _isSubgroupTypePopupOpen = false;

        [ObservableProperty]
        private bool _isTimetableStatePopupOpen = false;

        [ObservableProperty]
        private TimetableTabs _selectedTab = TimetableTabs.Exams;
        partial void OnSelectedTabChanged(TimetableTabs value)
        {
            ClearLoadedSchedule();

            LoadMoreScheduleCommand.Execute(true);
        }

        [ObservableProperty]
        private SubgroupType _selectedMode = (SubgroupType)Preferences.Get(PreferencesKeys.SelectedSubgroupType, (int)SubgroupType.All);
        partial void OnSelectedModeChanged(SubgroupType oldValue, SubgroupType newValue)
        {
            Preferences.Set(PreferencesKeys.SelectedSubgroupType, (int)newValue);

            //// We may reduce existing collection if moving to subgroup mode from group mode
            //if (oldValue == SubgroupType.All && newValue != SubgroupType.All)
            //{
            //    List<Schedule> itemsToRemove =
            //        Schedule
            //        .OfType<Schedule>()
            //        .Where(item => item.NumSubgroup != newValue && item.NumSubgroup != SubgroupType.All)
            //        .ToList();

            //    Schedule.RemoveRange(itemsToRemove);

            //    List<ScheduleDay> headersToRemove = 
            //        Schedule
            //        .OfType<ScheduleDay>()
            //        .Where(day => itemsToRemove.Any(schedule => schedule.DateLesson == day.Day))
            //        .Where((day) =>
            //        {
            //            return !Schedule
            //                .OfType<Schedule>()
            //                .Any(schedule => schedule.DateLesson == day.Day);
            //        })
            //        .ToList();

            //    Schedule.RemoveRange(headersToRemove);

            //    ScrollToActiveSchedule();
            //}
            //else
            //{
            //}

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
        private TimetableState _timetableState = TimetableState.Default;

        // todo: caching
        [ObservableProperty]
        private ObservableRangeCollection<ITimetableItem> _schedule = [];

        [ObservableProperty]
        private TypedId? _timetableId = default;
        async partial void OnTimetableIdChanged(TypedId? value)
        {
            TimetableState = value is null
                ? TimetableState.Default
                : await _timetableService.GetState(value);

            _loggingService.LogInfo($"Timetable {value} state: {TimetableState}", displayCaller: false);

            GetTimetableCommand.Execute(null);
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
            DateTime targetDate = _loadedToDate.Value + _loadingStep;

            // Adjust target date if loading to the nearest schedule
            if (scrollToNearest ?? false)
            {
                while(targetDate < _nearestScheduleDate)
                {
                    targetDate += _loadingStep;
                }
            }

            var newDays = await _timetableService.GetDaySchedulesAsync(Timetable, _loadedToDate, targetDate, SelectedTab, SelectedMode);

            _loadedToDate = targetDate;

            _loggingService.LogInfo($"GetDaySchedules got {newDays?.Count} objects ({_loadedToDate?.ToString("dd.MM")} - {(_loadedToDate + _loadingStep)?.ToString("dd.MM")})", displayCaller: false);
            
            Schedule ??= [];

            // todo: also add ScheduleWeek?
            foreach (var day in newDays ?? [])
            {
                Schedule.Add(new ScheduleDay(day.Day));

                Schedule.AddRange(day);
            }

            IsLoadingMoreSchedule = false;

            if (scrollToNearest ?? false)
                ScrollToActiveSchedule();
        }

        [RelayCommand]
        public async Task GetTimetable()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (TimetableId is null)
            {
                IsPinnedTimetable = true;

                _loggingService.LogInfo($"GetTimetable getting pinned id", displayCaller: false);

                TimetableId = await _timetableService.GetPinnedIdAsync();
            }

            try
            {
                Timetable = TimetableId is null
                    ? null
                    : await _timetableService.GetTimetableAsync(TimetableId, CancellationToken.None);
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
        public async Task SetState(TimetableState newState)
        {
            if (TimetableId is null)
                return;

            if (newState == TimetableState)
                return;

            TimetableState = newState;

            IsTimetableStatePopupOpen = false;

            await _timetableService.ApplyState(TimetableId, newState);
        }

        [RelayCommand]
        public void ToggleStatePopup()
        {
            IsSubgroupTypePopupOpen = false;
            IsTimetableStatePopupOpen = !IsTimetableStatePopupOpen;
        }

        [RelayCommand]
        public void SetSubgroupType(SubgroupType type)
        {
            SelectedMode = type;
            IsSubgroupTypePopupOpen = false;
        }

        [RelayCommand]
        public void ToggleSubgroupTypePopup()
        {
            IsTimetableStatePopupOpen = false;
            IsSubgroupTypePopupOpen = !IsSubgroupTypePopupOpen;
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
                { NavigationKeys.PreviousTimetableId, TimetableId! },
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

        // If we pin some timetable, pages down the stack should update their state. As well as pinned timetable on separate tab
        public void Receive(TimetablePinnedMessage message)
        {
            if (TimetableState == TimetableState.Pinned && !(message.Value?.Equals(TimetableId) ?? false))
            {
                TimetableState = TimetableState.Favorite;
            }

            if(IsPinnedTimetable)
            {
                TimetableId = message.Value;
            }
        }
    }
}
