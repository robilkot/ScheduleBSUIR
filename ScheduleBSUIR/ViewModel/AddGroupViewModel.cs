using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Model;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.ViewModel
{
    public partial class AddGroupViewModel(TimetablePage timetablePage, ExamsPage examsPage, GroupsService groupsService) : BaseViewModel
    {
        private TimetablePage _timetablePage = timetablePage;
        private ExamsPage _examsPage = examsPage;
        private GroupsService _groupsService = groupsService;

        [ObservableProperty]
        private List<StudentGroupHeader> _groupHeadersList = [];
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

                if(!GroupHeadersList.Any(g => g.Name == GroupName))
                {
                    await Shell.Current.DisplayAlert("Information", "Non-existing group specified", "OK");
                    return;
                };

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

        public async Task UpdateGroups(string? groupNameFilter = null)
        {
            GroupHeadersList = await _groupsService.GetGroupHeaders(groupNameFilter);
        }
    }
}
