using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.Viewmodels
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

        //[RelayCommand]
        //public async Task AddGroup()
        //{
        //    if (IsBusy)
        //        return;

            
        //}

        //public async Task UpdateGroups(string? groupNameFilter = null)
        //{
        //    GroupHeadersList = await _groupsService.GetGroupHeaders(groupNameFilter);
        //}
    }
}
