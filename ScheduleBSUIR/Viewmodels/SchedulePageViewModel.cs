using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Models;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class SchedulePageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private Timetable _timetable = null!;

        [ObservableProperty]
        private ObservableCollection<Schedule> _schedule = [];

        [RelayCommand]
        public void SetSchedule(Timetable? timetable)
        {
            if (timetable is null)
                return;

            IsBusy = true;

            Timetable = timetable;

            try
            {
                // todo: schedules by day
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
