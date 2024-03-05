using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;

namespace ScheduleBSUIR.ViewModel
{
    public partial class AddGroupViewModel : BaseViewModel
    {
        private string _groupName = "221701";

        [RelayCommand]
        public async Task AddGroup(TimetableViewModel timetableViewmodel)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                timetableViewmodel.Id = new StudentGroupId(_groupName);

                //((ShellViewModel)Shell.Current.BindingContext).FlyoutItems.Add(
                //    new FlyoutItem
                //    {
                //        Title = _groupName,
                //        Items = {
                //            new TimetablePage(timetableViewmodel),
                //            new ExamsPage()
                //        }
                //    }
                //);

                await Shell.Current.DisplayAlert("Error!", "Addded", "Ok");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
