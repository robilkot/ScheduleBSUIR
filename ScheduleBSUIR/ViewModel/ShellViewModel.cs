using System.Collections.ObjectModel;

namespace ScheduleBSUIR.ViewModel
{
    public partial class ShellViewModel : BaseViewModel
    {
        public ObservableCollection<FlyoutItem> FlyoutItems { get; } = [];

        public ShellViewModel() { }
    }
}
