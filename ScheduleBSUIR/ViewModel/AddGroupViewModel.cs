using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Model;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.ViewModel
{
    public partial class AddGroupViewModel : BaseViewModel
    {
        private TimetablePage _timetablePage;
        private ExamsPage _examsPage;
        private GroupsService _groupsService;

        public ObservableCollection<StudentGroup> FilteredGroupList = [];
        public List<StudentGroup> GroupList = [];
        [ObservableProperty]
        private string _groupName = string.Empty;

        [RelayCommand]
        public async Task AddGroup()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                if(Shell.Current.Items.FirstOrDefault(i => i.Route == GroupName) is null)
                {
                    _timetablePage.ViewModel.TimetableOwnerId = new StudentGroupId(GroupName);

                    FlyoutItem content = new()
                    {
                        Title = GroupName,
                        Route = GroupName,
                        Items = {
                            _timetablePage,
                            _examsPage
                        }
                    };

                    Shell.Current.Items.Add(content);
                }

                await Shell.Current.GoToAsync($"//{GroupName}");
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

        public async Task UpdateGroups()
        {
            GroupList = await _groupsService.GetGroups();
        }

        public AddGroupViewModel(TimetablePage timetablePage, ExamsPage examsPage, GroupsService groupsService)
        {
            _timetablePage = timetablePage;
            _examsPage = examsPage;
            _groupsService = groupsService;
        }
    }
}
