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
                InitPreferences();
            };

            return window;
        }

        // todo: move to serivce?
        private void InitPreferences()
        {
            foreach (var (Key, DefaultColor) in PreferencesKeys.ColorPreferencesKeys.Zip(LessonsColorsHelper.AvailableColors))
            {
                string? hexColor = Preferences.Get(Key, null);

                Color newColor = hexColor is null ? DefaultColor : Color.FromRgba(hexColor);

                App.Current!.Resources[Key] = newColor;
            }

        }
    }
}
