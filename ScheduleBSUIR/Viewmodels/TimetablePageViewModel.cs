using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class TimetablePageViewModel(TimetableService timetableService, ILoggingService loggingService)
        : BaseViewModel(loggingService), IQueryAttributable
    {
        private readonly TimetableService _timetableService = timetableService;

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
            if(Timetable is null)
                return;

            if(Favorited)
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
                _ = GetTimetable((TypedId)id);
            }
        }
    }
}
