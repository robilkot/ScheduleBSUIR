using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class DebugLogPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string _log = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing = false;

        public DebugLogPageViewModel(ILoggingService loggingService)
            : base(loggingService)
        {
            GetLogCommand.Execute(null);
        }

        [RelayCommand]
        public void Refresh()
        {
            GetLogCommand.Execute(null);

            IsRefreshing = false;
        }

        [RelayCommand]
        public void GetLog()
        {
            Log = _loggingService.GetLocalLog();
        }

        [RelayCommand]
        public void ClearLog()
        {
            Log = string.Empty;

            _loggingService.ClearLocalLog();
        }
    }
}
