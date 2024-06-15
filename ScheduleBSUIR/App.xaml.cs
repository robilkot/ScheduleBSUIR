using ScheduleBSUIR.View;

namespace ScheduleBSUIR
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var examsPage = serviceProvider.GetService<ExamsPage>();

            MainPage = examsPage;
        }
    }
}
