using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class ExamsPageViewModel : BaseViewModel
    {
        private readonly TimetableService _timetableService;

        [ObservableProperty]
        private Timetable _timetable = null!;

        [ObservableProperty]
        private List<ScheduleGroup> _exams = null!;

        public ExamsPageViewModel(TimetableService timetableService)
        {
            _timetableService = timetableService;

            GetTimetableCommand.Execute(null);
        }

        [RelayCommand]
        public async Task GetTimetable()
        {
            Timetable = await _timetableService.GetTimetableAsync(new StudentGroupId("221701"), CancellationToken.None);

            Dictionary<string, List<Schedule>> daysExams = [];

            Timetable.Exams?.ForEach(schedule => {
                if (daysExams.TryGetValue(schedule.DateLesson ?? string.Empty, out var list)) 
                {
                    list.Add(schedule);
                }
                else
                {
                    daysExams.Add(schedule.DateLesson ?? string.Empty, [ schedule ]);
                }
                });

            Exams = daysExams
                .Select(kvp => new ScheduleGroup(kvp.Key, kvp.Value))
                .ToList();
        }
    }
}
