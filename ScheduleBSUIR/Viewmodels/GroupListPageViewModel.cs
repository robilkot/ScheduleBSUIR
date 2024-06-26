using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevExpress.Data.Helpers;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class GroupListPageViewModel : BaseViewModel
    {
        private GroupsService _groupsService;
        private TimetableService _timetableService;

        private List<StudentGroupHeader> _allGroupsHeaders = [];

        private string _currentGroupFilter = string.Empty;

        [ObservableProperty]
        private List<StudentGroupHeader> _filteredGroups = [];

        // todo: consume message about (un)favoriting
        private List<StudentGroupId> _favoriteGroupsIds = [];

        [ObservableProperty]
        private ObservableCollection<StudentGroupId> _filteredFavoriteGroupsIds = [];

        [ObservableProperty]
        private string _groupName = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing = false;

        public GroupListPageViewModel(GroupsService groupsService, ILoggingService loggingService, TimetableService timetableService)
            : base(loggingService)
        {
            _groupsService = groupsService;
            _timetableService = timetableService;

            RefreshCommand.Execute(string.Empty);
        }

        [RelayCommand]
        public async Task SelectGroup(object selection)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            TypedId groupId = selection switch 
            {
                TypedId id => id,
                StudentGroupHeader studentGroupHeader => TypedId.Create(studentGroupHeader),
                _ => throw new UnreachableException(),
            };

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, groupId },
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
                var groupHeaders = await _groupsService.GetGroupHeadersAsync(cancellationToken);
                _allGroupsHeaders = groupHeaders.ToList();
                FilteredGroups = _allGroupsHeaders;

                _favoriteGroupsIds = await _timetableService.GetFavoriteTimetablesIdsAsync<StudentGroupId>();
                FilteredFavoriteGroupsIds = _favoriteGroupsIds.ToObservableCollection();

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
                FilteredFavoriteGroupsIds = FilteredFavoriteGroupsIds.Where(id => id.DisplayName.StartsWith(groupNameFilter)).ToObservableCollection();
            }
            // Else we have to search in all headers :(
            else
            {
                FilteredGroups = _allGroupsHeaders.Where(header => header.Name.StartsWith(groupNameFilter)).ToList();
                FilteredFavoriteGroupsIds = _favoriteGroupsIds.Where(id => id.DisplayName.StartsWith(groupNameFilter)).ToObservableCollection();
            }

            _currentGroupFilter = groupNameFilter;
        }
    }
}
