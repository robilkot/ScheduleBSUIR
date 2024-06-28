using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class GroupListPageViewModel : BaseViewModel, IRecipient<TimetableFavoritedMessage>, IRecipient<TimetableUnfavoritedMessage>
    {
        private readonly GroupsService _groupsService;
        private readonly TimetableService _timetableService;

        private List<StudentGroupHeader> _allGroupsHeaders = [];

        private string _currentGroupFilter = string.Empty;

        [ObservableProperty]
        private List<StudentGroupHeader> _filteredGroups = [];

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

            WeakReferenceMessenger.Default.Register<TimetableFavoritedMessage>(this);
            WeakReferenceMessenger.Default.Register<TimetableUnfavoritedMessage>(this);

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
                _ => TypedId.Create(selection),
            };

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, groupId },
                { NavigationKeys.IsBackButtonVisible, true },
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

                _favoriteGroupsIds = await _timetableService.GetFavoriteGroupsTimetablesIdsAsync();
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

        public void Receive(TimetableFavoritedMessage message)
        {
            if (message.Value is not StudentGroupId studentGroupId)
                return;

            _favoriteGroupsIds.Add(studentGroupId);

            // Not to perform filtering for whole collection
            if (studentGroupId.DisplayName.StartsWith(_currentGroupFilter))
            {
                FilteredFavoriteGroupsIds.Add(studentGroupId);
                OnPropertyChanged(nameof(FilteredFavoriteGroupsIds)); // Otherwise converter won't catch up
            }
        }

        public void Receive(TimetableUnfavoritedMessage message)
        {
            if (message.Value is not StudentGroupId studentGroupId)
                return;

            _favoriteGroupsIds.Remove(studentGroupId);

            if (studentGroupId.DisplayName.StartsWith(_currentGroupFilter))
            {
                FilteredFavoriteGroupsIds.Remove(studentGroupId);
                OnPropertyChanged(nameof(FilteredFavoriteGroupsIds)); // Otherwise converter won't catch up
            }
        }
    }
}
