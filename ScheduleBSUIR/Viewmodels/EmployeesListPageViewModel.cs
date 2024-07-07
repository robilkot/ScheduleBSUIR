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
using static ScheduleBSUIR.Viewmodels.EmployeeTimetableHeaderExtensions;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class EmployeesListPageViewModel : BaseViewModel, IRecipient<TimetableStateChangedMessage>
    {
        private readonly EmployeesService _employeesService;
        private readonly TimetableService _timetableService;

        private List<EmployeeTimetableHeader> _allEmployees = [];

        private string _currentEmployeeFilter = string.Empty;

        [ObservableProperty]
        private List<EmployeeTimetableHeader> _filteredEmployees = [];

        private List<EmployeeTimetableHeader> _favoriteEmployeesIds = [];

        [ObservableProperty]
        private ObservableCollection<EmployeeTimetableHeader> _filteredFavoriteEmployeesIds = [];

        [ObservableProperty]
        private string _employeeFilter = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing = false;

        public EmployeesListPageViewModel(EmployeesService employeesService, ILoggingService loggingService, TimetableService timetableService)
            : base(loggingService)
        {
            _employeesService = employeesService;
            _timetableService = timetableService;

            WeakReferenceMessenger.Default.Register(this);

            RefreshCommand.Execute(string.Empty);
        }

        [RelayCommand]
        public async Task SelectEmployee(EmployeeTimetableHeader selectedHeader)
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
        public async Task Refresh(CancellationToken cancellationToken = default)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var employees = await _employeesService.GetEmployeesAsync(cancellationToken);

                _allEmployees = employees.ToTimetableHeaders().Cast<EmployeeTimetableHeader>().ToList();
                FilteredEmployees = _allEmployees;

                _favoriteEmployeesIds = await _timetableService.GetFavoriteEmployeesTimetablesHeadersAsync();
                FilteredFavoriteEmployeesIds = _favoriteEmployeesIds.ToObservableCollection();

                EmployeeFilter = string.Empty;
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public void FilterEmployees(string employeeNameFilter = "")
        {
            // Filter narrowed => can search in already filtered collection
            if (employeeNameFilter.Length > _currentEmployeeFilter.Length)
            {
                FilteredEmployees = FilteredEmployees
                    .FilteredBy(employeeNameFilter)
                    .ToList();
                FilteredFavoriteEmployeesIds = FilteredFavoriteEmployeesIds
                    .FilteredBy(employeeNameFilter)
                    .ToObservableCollection();
            }
            // Else we have to search in all list :(
            else
            {
                FilteredEmployees = _allEmployees
                    .FilteredBy(employeeNameFilter)
                    .ToList();
                FilteredFavoriteEmployeesIds = _favoriteEmployeesIds
                    .FilteredBy(employeeNameFilter)
                    .ToObservableCollection();
            }

            _currentEmployeeFilter = employeeNameFilter;
        }

        public void Receive(TimetableStateChangedMessage message)
        {
            if (message.Value.Item1 is not EmployeeTimetableHeader employeeTimetableHeader)
                return;

            if (message.Value.Item2 == TimetableState.Default)
            {
                _favoriteEmployeesIds.Remove(employeeTimetableHeader);

                if (EmployeePredicate((IEmployee)employeeTimetableHeader.TimetableOwner, _currentEmployeeFilter))
                {
                    FilteredFavoriteEmployeesIds.Remove(employeeTimetableHeader);
                }
            }
            else
            {
                if (_favoriteEmployeesIds.Contains(employeeTimetableHeader))
                    return;

                _favoriteEmployeesIds.Add(employeeTimetableHeader);

                // Not to perform filtering for whole collection
                if (EmployeePredicate((IEmployee)employeeTimetableHeader.TimetableOwner, _currentEmployeeFilter))
                {
                    FilteredFavoriteEmployeesIds.Add(employeeTimetableHeader);
                }
            }

            OnPropertyChanged(nameof(FilteredFavoriteEmployeesIds)); // Otherwise converter won't catch up
        }
    }
    public static class EmployeeTimetableHeaderExtensions
    {
        public static bool EmployeePredicate(IEmployee employee, string filter) => employee.FullName.Contains(filter, StringComparison.InvariantCultureIgnoreCase);
        public static IEnumerable<EmployeeTimetableHeader> FilteredBy(this IEnumerable<EmployeeTimetableHeader> headers, string employeeFilter) =>
            headers.Where(header => EmployeePredicate((IEmployee)header.TimetableOwner, employeeFilter));
    }
}
