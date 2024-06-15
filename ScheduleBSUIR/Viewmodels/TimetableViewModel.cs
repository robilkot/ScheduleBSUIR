using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class TimetableViewModel(TimetableService timetableService) : BaseViewModel
    {
        private readonly TimetableService _timetableService = timetableService;

        [ObservableProperty]
        private Timetable? _timetable;

        [RelayCommand]
        public async Task GetTimetable()
        {
            Timetable = await _timetableService.GetTimetableAsync(new StudentGroupId("221701"), CancellationToken.None);
        }
    }
}
