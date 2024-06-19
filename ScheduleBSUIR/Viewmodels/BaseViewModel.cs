using CommunityToolkit.Mvvm.ComponentModel;
using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class BaseViewModel(ILoggingService loggingService) : ObservableObject
    {
        protected readonly ILoggingService _loggingService = loggingService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy = false;
        [ObservableProperty]
        private string _title = string.Empty;
        public bool IsNotBusy => !IsBusy;
    }
}
