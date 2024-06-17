using ScheduleBSUIR.View;

namespace ScheduleBSUIR
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(GroupListPage), typeof(GroupListPage));
            Routing.RegisterRoute(nameof(ExamsPage), typeof(ExamsPage));
            Routing.RegisterRoute(nameof(TimetablePage), typeof(TimetablePage));
        }
    }
}
