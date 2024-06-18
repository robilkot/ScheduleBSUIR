using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class TimetablePageViewModel(
        SchedulePageViewModel scheduleViewModel,
        TimetableService timetableService)
        : BaseViewModel, IQueryAttributable
    {
        private readonly SchedulePageViewModel _scheduleViewModel = scheduleViewModel;
        private readonly TimetableService _timetableService = timetableService;

        [ObservableProperty]
        private TimetableTabs _selectedTab = TimetableTabs.Exams;

        [ObservableProperty]
        private Timetable? _timetable;

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

            try
            {
                Timetable = await _timetableService.GetTimetableAsync(id, CancellationToken.None);

                _scheduleViewModel.SetScheduleCommand.Execute(Timetable);
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
