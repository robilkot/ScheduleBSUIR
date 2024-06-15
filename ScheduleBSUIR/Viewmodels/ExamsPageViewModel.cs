using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class ExamsPageViewModel : BaseViewModel
    {
        private readonly TimetableService _timetableService;

        [ObservableProperty]
        private Timetable? _timetable;

        public ExamsPageViewModel(TimetableService timetableService)
        {
            _timetableService = timetableService;

            GetTimetableCommand.Execute(null);
        }

        [RelayCommand]
        public async Task GetTimetable()
        {
            Timetable = await _timetableService.GetTimetableAsync(new StudentGroupId("221701"), CancellationToken.None);
        }
    }
}
