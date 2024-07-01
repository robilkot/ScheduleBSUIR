using ScheduleBSUIR.Helpers.Constants;

namespace ScheduleBSUIR
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
