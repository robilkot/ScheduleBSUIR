using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Services;

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
        private TimetableTabs _selectedTab = TimetableTabs.Exams;

        [ObservableProperty]
        private bool _favorited = false;

        [ObservableProperty]
        private Timetable? _timetable;

        [ObservableProperty]
        private List<Schedule>? _exams;

        // This property exists to display group number/employee name before timetable is loaded
        [ObservableProperty]
        private string _timetableHeader = string.Empty;

        public int? GetNearestScheduleIndex()
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

                Favorited = Timetable.Favorited;

                Exams = Timetable.Exams;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"GetTimetable threw: {ex.Message}", displayCaller: false);
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

            Favorited = Timetable.Favorited;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(NavigationKeys.TimetableId, out var id)
                && query.TryGetValue(NavigationKeys.TimetableHeader, out var timetableHeader))
            {
                // todo: use TimetableToHeaderTextConverter to unify header formatting?
                TimetableHeader = (string)timetableHeader;

                // Exceptions must be handled inside of command
                _ = Task.Run(async () =>
                {
                    await GetTimetable((TypedId)id);

                    int? nearestScheduleIndex = GetNearestScheduleIndex();

                    if (nearestScheduleIndex is not null)
                    {
                        ScrollToIndex message = new(nearestScheduleIndex.Value);

                        WeakReferenceMessenger.Default.Send(message);
                    }
                });
            }
        }
    }
}
