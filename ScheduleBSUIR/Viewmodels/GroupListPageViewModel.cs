using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class GroupListPageViewModel : BaseViewModel
    {
        private GroupsService _groupsService;

        private IEnumerable<StudentGroupHeader> _allGroupsHeaders = [];
        private IEnumerable<StudentGroupHeader> _filteredGroupHeaders = [];

        private string _currentGroupFilter = string.Empty;

        [ObservableProperty]
        private ObservableCollection<StudentGroupHeaderGroup> _filteredGroups = [];

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
                _filteredGroupHeaders = _allGroupsHeaders;

                GroupName = string.Empty;

                UpdateGrouping();
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
                _filteredGroupHeaders = _filteredGroupHeaders.Where(header => header.Name.StartsWith(groupNameFilter));
            }
            // Else we have to search in all headers :(
            else
            {
                _filteredGroupHeaders = _allGroupsHeaders.Where(header => header.Name.StartsWith(groupNameFilter));
            }

            _currentGroupFilter = groupNameFilter;

            UpdateGrouping();
        }

        private void UpdateGrouping()
        {
            var groupedHeaders = _filteredGroupHeaders
                .GroupBy(header => header.Name[..3])
                .Select(grouping => new StudentGroupHeaderGroup(grouping.Key, grouping));

            FilteredGroups.Clear();

            foreach (var group in groupedHeaders)
            {
                // await Task.Delay(50);
                FilteredGroups.Add(group);
            }
        }
    }
}
