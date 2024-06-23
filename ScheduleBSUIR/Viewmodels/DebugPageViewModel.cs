using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class DebugPageViewModel : BaseViewModel
    {
        private readonly DbService _dbService;

        [ObservableProperty]
        private string _log = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing = false;

        public DebugPageViewModel(ILoggingService loggingService, DbService dbService)
            : base(loggingService)
        {
            _dbService = dbService;

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

        [RelayCommand]
        public async Task ClearCache()
        {
            await _dbService.ClearDatabase();
        }
    }
}
