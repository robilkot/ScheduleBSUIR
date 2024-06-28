using DevExpress.Maui.Core.Internal;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.View;

namespace ScheduleBSUIR
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(GroupListPage), typeof(GroupListPage));
            Routing.RegisterRoute(nameof(TimetablePage), typeof(TimetablePage));
        }
    }
}
