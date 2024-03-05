using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.ViewModel
{
    public partial class ShellViewModel : BaseViewModel
    {
        public ObservableCollection<FlyoutItem> FlyoutItems { get; } = [];

        public ShellViewModel()
        {
            //var defaultFlyoutItem = new FlyoutItem
            //{
            //    Title = "Add new group",
            //    FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
            //};

            //defaultFlyoutItem.Items.Add(new ShellContent { Title = "Timetable", ContentTemplate = new DataTemplate(typeof(TimetablePage)) });
            //defaultFlyoutItem.Items.Add(new ShellContent { Title = "Exams", ContentTemplate = new DataTemplate(typeof(ExamsPage)) });

            //FlyoutItems.Add(defaultFlyoutItem);
        }
    }
}
