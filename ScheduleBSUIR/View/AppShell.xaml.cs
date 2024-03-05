using ScheduleBSUIR.ViewModel;

namespace ScheduleBSUIR
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            BindingContext = new ShellViewModel();
        }
    }
}
