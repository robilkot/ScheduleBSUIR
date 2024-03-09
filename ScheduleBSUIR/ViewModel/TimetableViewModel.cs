using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
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
        private TypedId _timetableOwnerId = null!;

        public TimetableViewModel(TimetableService timetableService)
        {
            _timetableService = timetableService;
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
