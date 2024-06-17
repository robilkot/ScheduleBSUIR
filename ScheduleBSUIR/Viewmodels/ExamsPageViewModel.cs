using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Helpers;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class ExamsPageViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly TimetableService _timetableService;

        [ObservableProperty]
        private Timetable _timetable = null!;

        [ObservableProperty]
        private List<ScheduleGroup> _exams = null!;


        [ObservableProperty]
        private TypedId _testId = new StudentGroupId("221701");

        public ExamsPageViewModel(TimetableService timetableService)
        {
            _timetableService = timetableService;

            Exams = [];
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(NavigationKeys.TimetableId, out var id))
            {
                GetTimetableCommand.Execute((TypedId)id);
            }
        }

        [RelayCommand]
        public async Task GetTimetable(TypedId? id)
        {
            if(IsBusy)
                return;

            if(id is null)
                return;

            IsBusy = true;

            try
            {
                Timetable = await _timetableService.GetTimetableAsync(id, CancellationToken.None);

                var daysExams = Timetable.Exams?
                    .GroupBy(schedule => schedule.DateLesson ?? DateTime.MinValue)
                    .ToDictionary(g => g.Key, g => g.ToList());

                Exams = daysExams?
                    .Select(kvp => new ScheduleGroup(kvp.Key, kvp.Value))
                    .ToList()
                    ?? [];
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
    }
}
