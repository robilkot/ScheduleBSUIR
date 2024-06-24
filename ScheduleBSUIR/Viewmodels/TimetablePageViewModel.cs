using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

        private readonly TimeSpan _loadingStep = TimeSpan.FromDays(6);
        private DateTime? _loadedFromDate = null;
        private DateTime? _loadedToDate = null;

        [ObservableProperty]
        private bool _isRefreshing = false;

        [ObservableProperty]
        private bool _isLoadingMoreSchedule = false;

        [ObservableProperty]
        private bool _isTimetableModePopupOpen = false;

        // Actually declared in BaseViewModel but we need NotifyPropertyChangedFor here
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSkeletonVisible))]
        private bool _isBusy;

        [ObservableProperty]
        private TimetableTabs _selectedTab = TimetableTabs.Exams;
        partial void OnSelectedTabChanged(TimetableTabs value)
        {
            ClearLoadedSchedule();

            LoadMoreScheduleCommand.Execute(null);
        }

        [ObservableProperty]
        private SubgroupType _selectedMode = SubgroupType.All;
        partial void OnSelectedModeChanged(SubgroupType value)
        {
            ClearLoadedSchedule();

            LoadMoreScheduleCommand.Execute(null);
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Favorited))]
        [NotifyPropertyChangedFor(nameof(IsSkeletonVisible))]
        private Timetable? _timetable;
        partial void OnTimetableChanged(Timetable? value)
        {
            ClearLoadedSchedule();

            // todo: compare session dates instead of this?
            SelectedTab = Timetable?.Exams?.Count > 0 ? TimetableTabs.Exams : TimetableTabs.Schedule;

            LoadMoreScheduleCommand.Execute(null);
        }

        public bool Favorited => Timetable?.Favorited ?? false;
        public bool IsSkeletonVisible => Timetable is null || IsBusy;

        [ObservableProperty]
        private ObservableCollection<DaySchedule> _schedule = [];

        [ObservableProperty]
        private TypedId? _timetableId;

        // This property exists to display group number/employee name before timetable is loaded
        [ObservableProperty]
        private string _timetableHeader = string.Empty;


        [RelayCommand]
        public void LoadMoreSchedule()
        {
            // Guard case for overflow if already loaded all possible schedules
            if (SelectedTab is TimetableTabs.Schedule && _loadedToDate >= Timetable?.EndDate
                ||
                SelectedTab is TimetableTabs.Exams && Schedule.Count > 0)
            {
                IsLoadingMoreSchedule = false;
                return;
            }

            // Initial case
            var yesterday = _dateTimeProvider.Now - TimeSpan.FromDays(1);

            _loadedFromDate ??= SelectedTab switch
            {
                TimetableTabs.Schedule => Timetable?.StartDate,
                TimetableTabs.Exams => Timetable?.StartExamsDate, // Ignored in timetable service, actually
                _ => throw new UnreachableException(),
            } ?? yesterday;

            _loadedToDate ??= _loadedFromDate;

            _loadedToDate += _loadingStep;

            var newSchedules = _timetableService.GetDaySchedules(Timetable, _loadedFromDate, _loadedToDate, SelectedTab, SelectedMode);

            foreach (var schedule in newSchedules ?? [])
            {
                Schedule.Add(schedule);
            }

            IsLoadingMoreSchedule = false;
        }

        [RelayCommand]
        public async Task GetTimetable(TypedId? id)
        {
            if (id is null)
                return;

            if (IsBusy)
                return;

            IsBusy = true;

            _loggingService.LogInfo($"Getting timetable with id {id}", displayCaller: false);

            try
            {
                Timetable = await _timetableService.GetTimetableAsync(id, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Timetable = null;

                // let skeleton appear before popup. maybe will become obosolete
                await Task.Delay(100);

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

        [RelayCommand]
        public async Task Refresh()
        {
            await GetTimetable(TimetableId);

            IsRefreshing = false;

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

                RefreshCommand.Execute(null);
            }
        }
        private int? GetNearestScheduleIndex()
        {
            if (Timetable is null)
                return null;

            var foundSchedule = Schedule.FirstOrDefault(e => e.FirstOrDefault()?.DateLesson >= _dateTimeProvider.Now.Date);

            return foundSchedule is null ? null : Schedule.IndexOf(foundSchedule);
        }
        private void ClearLoadedSchedule()
        {
            Schedule.Clear();

            _loadedFromDate = null;
            _loadedToDate = null;
        }
    }
}
