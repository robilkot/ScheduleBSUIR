using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DevExpress.XtraEditors.Filtering;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Xml;

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

        [ObservableProperty]
        private bool _isRefreshing = false;

        [ObservableProperty]
        private bool _isTimetableModePopupOpen = false;

        // Actually declared in BaseViewModel but we need NotifyPropertyChangedFor here
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSkeletonVisible))]
        private bool _isBusy;

        [ObservableProperty]
        private TimetableTabs _selectedTab = TimetableTabs.Exams;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Exams))]
        private SubgroupType _selectedMode = SubgroupType.All;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Exams))]
        [NotifyPropertyChangedFor(nameof(Favorited))]
        [NotifyPropertyChangedFor(nameof(IsSkeletonVisible))]
        private Timetable? _timetable;

        public bool Favorited => Timetable?.Favorited ?? false;
        public bool IsSkeletonVisible => Timetable is null || IsBusy;
        public List<Schedule>? Exams => SelectedMode switch 
        { 
            SubgroupType.All => Timetable?.Exams,

            SubgroupType.FirstSubgroup => Timetable?.Exams?
                .Where(schedule => schedule is { NumSubgroup: SubgroupType.All or SubgroupType.FirstSubgroup } )
                .ToList(),

            SubgroupType.SecondSubgroup => Timetable?.Exams?
                .Where(schedule => schedule is { NumSubgroup: SubgroupType.All or SubgroupType.SecondSubgroup })
                .ToList(),

            _ => throw new UnreachableException(),
        };


        [ObservableProperty]
        private TypedId? _timetableId;

        // This property exists to display group number/employee name before timetable is loaded
        [ObservableProperty]
        private string _timetableHeader = string.Empty;


        [RelayCommand]
        public async Task GetTimetable(TypedId? id)
        {
            if (id is null)
                return;

            if (IsBusy)
                return;

            IsBusy = true;

            _loggingService.LogInfo($"Getting timetable with id {id}", displayCaller: false);

            _loggingService.LogInfo($"GetTimetable is on main thread: {MainThread.IsMainThread}", displayCaller: false);

            try
            {
                Timetable = await _timetableService.GetTimetableAsync(id, CancellationToken.None);

                // todo: compare session dates instead of this?
                SelectedTab = Exams?.Count > 0 ? TimetableTabs.Exams : TimetableTabs.Schedule;
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
        public async Task OpenTimetable(object dto)
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
            _loggingService.LogInfo($"ApplyQueryAttributes is on main thread: {MainThread.IsMainThread}", displayCaller: false);

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

            if (SelectedTab == TimetableTabs.Exams)
            {
                if (Exams is null)
                    return null;

                var foundSchedule = Exams.FirstOrDefault(e => e.DateLesson >= _dateTimeProvider.Now.Date);

                return foundSchedule is null ? null : Exams.IndexOf(foundSchedule);
            }

            // todo
            if (SelectedTab == TimetableTabs.Schedule)
            {
                //if (Exams is null)
                //    return null;

                //return Exams.FirstOrDefault(e => e.DateLesson >= _dateTimeProvider.Now.Date);
            }

            return null;
        }
    }
}
