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

            _loggingService.LogInfo($"getting timetable with id {id}");

            try
            {
                Timetable = await _timetableService.GetTimetableAsync(id, CancellationToken.None);

                Exams = Timetable.Exams;
            }
            catch
            {
                // todo: log
            }
            finally
            {
                IsBusy = false;
            }
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
