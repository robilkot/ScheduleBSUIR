using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class TimetablePageViewModel(
        ExamsPageViewModel examsPageViewModel,
        SchedulePageViewModel scheduleViewModel,
        TimetableService timetableService)
        : BaseViewModel, IQueryAttributable
    {
        private readonly ExamsPageViewModel _examsViewModel = examsPageViewModel;
        private readonly SchedulePageViewModel _scheduleViewModel = scheduleViewModel;
        private readonly TimetableService _timetableService = timetableService;

        [ObservableProperty]
        private TimetableTabs _selectedTab = TimetableTabs.Schedule;

        [ObservableProperty]
        private Timetable? _timetable;

        [RelayCommand]
        public async Task GetTimetable(TypedId? id)
        {
            if (id is null)
                return;

            IsBusy = true;
            _examsViewModel.IsBusy = true;
            _scheduleViewModel.IsBusy = true;

            try
            {
                Timetable = await _timetableService.GetTimetableAsync(id, CancellationToken.None);

                _examsViewModel.SetExamsCommand.Execute(Timetable);
                _scheduleViewModel.SetScheduleCommand.Execute(Timetable);
            }
            catch
            {
                // todo: log
            }
            finally
            {
                IsBusy = false;
                _examsViewModel.IsBusy = false;
                _scheduleViewModel.IsBusy = false;
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(NavigationKeys.TimetableId, out var id)
                && query.TryGetValue(NavigationKeys.TimetableHeader, out var timetableHeader))
            {
                _examsViewModel.TimetableHeader = (string)timetableHeader;

                // Let transition smoothly finish before doing heavy-lifting
                await Task.Delay(50);

                // Exceptions must be handled inside of command
                GetTimetableCommand.Execute((TypedId)id);
            }
        }
    }
}
