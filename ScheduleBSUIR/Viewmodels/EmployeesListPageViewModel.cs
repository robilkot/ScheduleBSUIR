using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class EmployeesListPageViewModel : BaseViewModel
    {
        private EmployeesService _employeesService;

        private List<Employee> _allEmployees = [];

        private string _currentEmployeeFilter = string.Empty;

        [ObservableProperty]
        private List<Employee> _filteredEmployees = [];

        [ObservableProperty]
        private string _employeeFilter = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing = false;

        public EmployeesListPageViewModel(EmployeesService employeesService, ILoggingService loggingService)
            : base(loggingService)
        {
            _employeesService = employeesService;

            RefreshCommand.Execute(string.Empty);
        }

        [RelayCommand]
        public async Task SelectEmployee(Employee selectedEmployee)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            // todo check comment in prefs
            //Preferences.Set(PreferencesKeys.SelectedGroupName, selectedEmployee.Name);

            TypedId emplyeeId = TypedId.Create(selectedEmployee);

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, emplyeeId },
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

                _allEmployees = employees.ToList();
                FilteredEmployees = _allEmployees;

                EmployeeFilter = string.Empty;
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        // todo: make better + optimise?
        private static Func<Employee, bool> SearchPredicate(string filter) => new((employee) =>
        {
            return employee.FullName.Contains(filter, StringComparison.InvariantCultureIgnoreCase);
        });

        [RelayCommand]
        public void FilterEmployees(string employeeNameFilter = "")
        {
            // Filter narrowed => can search in already filtered collection
            if (employeeNameFilter.Length > _currentEmployeeFilter.Length)
            {
                FilteredEmployees = FilteredEmployees.Where(SearchPredicate(employeeNameFilter)).ToList();
            }
            // Else we have to search in all list :(
            else
            {
                FilteredEmployees = _allEmployees.Where(SearchPredicate(employeeNameFilter)).ToList();
            }

            _currentEmployeeFilter = employeeNameFilter;
        }
    }
}
