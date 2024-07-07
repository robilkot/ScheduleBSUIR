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
using static ScheduleBSUIR.Viewmodels.StudentGroupTimetableHeaderExtensions;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class GroupListPageViewModel : BaseViewModel, IRecipient<TimetableStateChangedMessage>
    {
        private readonly GroupsService _groupsService;
        private readonly TimetableService _timetableService;

        private List<StudentGroupTimetableHeader> _allHeaders = [];

        private string _currentGroupFilter = string.Empty;

        [ObservableProperty]
        private List<StudentGroupTimetableHeader> _filteredHeaders = [];

        private List<StudentGroupTimetableHeader> _favoriteHeaders = [];

        [ObservableProperty]
        private ObservableCollection<StudentGroupTimetableHeader> _filteredFavoriteHeaders = [];

        [ObservableProperty]
        private string _groupName = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing = false;

        public GroupListPageViewModel(GroupsService groupsService, ILoggingService loggingService, TimetableService timetableService)
            : base(loggingService)
        {
            _groupsService = groupsService;
            _timetableService = timetableService;

            WeakReferenceMessenger.Default.Register(this);

            RefreshCommand.Execute(string.Empty);
        }

        [RelayCommand]
        public async Task SelectGroup(StudentGroupTimetableHeader selectedHeader)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableHeader, selectedHeader },
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

                _allHeaders = groupHeaders.ToTimetableHeaders().Cast<StudentGroupTimetableHeader>().ToList();
                FilteredHeaders = _allHeaders;

                _favoriteHeaders = await _timetableService.GetFavoriteGroupsTimetablesHeadersAsync();
                FilteredFavoriteHeaders = _favoriteHeaders.ToObservableCollection();

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
                FilteredHeaders = FilteredHeaders.FilteredBy(groupNameFilter).ToList();
                FilteredFavoriteHeaders = FilteredFavoriteHeaders.FilteredBy(groupNameFilter).ToObservableCollection();
            }
            // Else we have to search in all headers :(
            else
            {
                FilteredHeaders = _allHeaders.FilteredBy(groupNameFilter).ToList();
                FilteredFavoriteHeaders = _favoriteHeaders.FilteredBy(groupNameFilter).ToObservableCollection();
            }

            _currentGroupFilter = groupNameFilter;
        }

        public void Receive(TimetableStateChangedMessage message)
        {
            if (message.Value.Item1 is not StudentGroupTimetableHeader studentGroupTimetableHeader)
                return;

            if (message.Value.Item2 == TimetableState.Default)
            {
                _favoriteHeaders.Remove(studentGroupTimetableHeader);

                if (GroupFilterPredicate(studentGroupTimetableHeader, _currentGroupFilter))
                {
                    FilteredFavoriteHeaders.Remove(studentGroupTimetableHeader);
                }
            }
            else
            {
                if (_favoriteHeaders.Contains(studentGroupTimetableHeader))
                    return;

                _favoriteHeaders.Add(studentGroupTimetableHeader);

                // Not to perform filtering for whole collection
                if (GroupFilterPredicate(studentGroupTimetableHeader, _currentGroupFilter))
                {
                    FilteredFavoriteHeaders.Add(studentGroupTimetableHeader);
                }
            }

            OnPropertyChanged(nameof(FilteredFavoriteHeaders)); // Otherwise converter won't catch up
        }
    }

    public static class StudentGroupTimetableHeaderExtensions
    {
        public static bool GroupFilterPredicate(StudentGroupTimetableHeader header, string filter) => header.HeaderText.StartsWith(filter);
        public static IEnumerable<StudentGroupTimetableHeader> FilteredBy(this IEnumerable<StudentGroupTimetableHeader> headers, string groupNameFilter) =>
            headers.Where(header => GroupFilterPredicate(header, groupNameFilter));
    }
}
