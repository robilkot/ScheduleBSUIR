using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;

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

        [ObservableProperty]
        private bool _isRefreshing = false;

        public GroupListPageViewModel(GroupsService groupsService, ILoggingService loggingService)
            : base(loggingService)
        {
            _groupsService = groupsService;

            RefreshCommand.Execute(string.Empty);
        }

        [RelayCommand]
        public async Task SelectGroup(StudentGroupHeader selectedGroup)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            Preferences.Set(PreferencesKeys.SelectedGroupName, selectedGroup.Name);

            StudentGroupId groupId = new(selectedGroup.Name);

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, groupId },
                { NavigationKeys.TimetableHeader, selectedGroup.Name },
            };

            await Shell.Current.GoToAsync(nameof(TimetablePage), true, navigationParameters);

            IsBusy = false;
        }

        [RelayCommand]
        public async Task Refresh(string groupNameFilter = "", CancellationToken cancellationToken = default)
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
                IsRefreshing = false;
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
