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
        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = base.CreateWindow(activationState);

            window.Created += (s, e) =>
            {
                //InitPreferences();
            };

            return window;
        }
    }
}
