using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using ScheduleBSUIR.Constants;
using ScheduleBSUIR.Model;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.ViewModel
{
    public partial class TimetableViewModel : BaseViewModel
    {
        private readonly TimetableService _timetableService;

        [ObservableProperty]
        private Timetable _timetable = null!;
        [ObservableProperty]
        private List<List<Schedule>> _dailySchedules = [];
        [ObservableProperty]
        private TypedId _timetableOwnerId = null!;

        public TimetableViewModel(TimetableService timetableService)
        {
            _timetableService = timetableService;

            for(int i = 0; i < 7; i++)
            {
                DailySchedules.Add([]);
            }
        }

        [RelayCommand]
        public async Task GetTimetableAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                Timetable = await _timetableService.GetTimetable(TimetableOwnerId);

                List<List<Schedule>> newDailySchedules = [];

                for (int i = 0; i < 7; i++)
                {
                    newDailySchedules.Add([]);
                }

                foreach (var day in Timetable.Schedules!)
                {
                    // todo: display names of weekdays

                    var dayInWeekIndex = (int)Weekdays.WeekdaysStrings[day.Key];

                    newDailySchedules[dayInWeekIndex] = day.Value;
                }

                DailySchedules = newDailySchedules;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
