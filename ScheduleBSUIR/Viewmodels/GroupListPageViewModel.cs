using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class GroupListPageViewModel : BaseViewModel
    {
        private GroupsService _groupsService;

        private List<StudentGroupHeader> _allGroupsHeaders = [];

        private string _currentGroupFilter = string.Empty;

        [ObservableProperty]
        private List<StudentGroupHeader> _filteredGroups = [];

        [ObservableProperty]
        private string _groupName = string.Empty;

        public GroupListPageViewModel(GroupsService groupsService)
        {
            _groupsService = groupsService;

            UpdateAllGroupsCommand.Execute(string.Empty);
        }

        [RelayCommand]
        public void SelectGroup(StudentGroupHeader selectedGroup)
        {
            Preferences.Set(PreferencesKeys.SelectedGroupName, selectedGroup.Name);

            StudentGroupId groupId = new(selectedGroup.Name);

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, groupId },
                { NavigationKeys.TimetableHeader, selectedGroup.Name },
            };

            Shell.Current.GoToAsync(nameof(TimetablePage), true, navigationParameters);
        }

        [RelayCommand]
        public async Task UpdateAllGroups(string groupNameFilter = "", CancellationToken cancellationToken = default)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var groupHeaders = await _groupsService.GetGroupHeadersAsync(groupNameFilter, cancellationToken);

                _allGroupsHeaders = groupHeaders.ToList();
                FilteredGroups = _allGroupsHeaders;

                GroupName = string.Empty;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void FilterGroups(string groupNameFilter = "")
        {
            // Filter narrowed => can search in already filtered collection
            if (groupNameFilter.Length > _currentGroupFilter.Length)
            {
                FilteredGroups = FilteredGroups.Where(header => header.Name.StartsWith(groupNameFilter)).ToList();
            }
            // Else we have to search in all headers :(
            else
            {
                FilteredGroups = _allGroupsHeaders.Where(header => header.Name.StartsWith(groupNameFilter)).ToList();
            }

            _currentGroupFilter = groupNameFilter;
        }
    }
}
