using CommunityToolkit.Mvvm.ComponentModel;

namespace ScheduleBSUIR.ViewModel
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy = false;
        [ObservableProperty]
        private string _title = string.Empty;
        public bool IsNotBusy => !IsBusy;
    }
}
