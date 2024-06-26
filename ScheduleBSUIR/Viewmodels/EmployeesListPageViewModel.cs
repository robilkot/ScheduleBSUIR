﻿using CommunityToolkit.Maui.Core.Extensions;
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
    public partial class EmployeesListPageViewModel : BaseViewModel, IRecipient<TimetableFavoritedMessage>, IRecipient<TimetableUnfavoritedMessage>
    {
        private readonly EmployeesService _employeesService;
        private readonly TimetableService _timetableService;

        private List<Employee> _allEmployees = [];

        private string _currentEmployeeFilter = string.Empty;

        [ObservableProperty]
        private List<Employee> _filteredEmployees = [];

        private List<EmployeeId> _favoriteEmployeesIds = [];

        [ObservableProperty]
        private ObservableCollection<EmployeeId> _filteredFavoriteEmployeesIds = [];

        [ObservableProperty]
        private string _employeeFilter = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing = false;

        public EmployeesListPageViewModel(EmployeesService employeesService, ILoggingService loggingService, TimetableService timetableService)
            : base(loggingService)
        {
            _employeesService = employeesService;
            _timetableService = timetableService;

            WeakReferenceMessenger.Default.Register<TimetableFavoritedMessage>(this);
            WeakReferenceMessenger.Default.Register<TimetableUnfavoritedMessage>(this);

            RefreshCommand.Execute(string.Empty);
        }

        [RelayCommand]
        public async Task SelectEmployee(object selection)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            TypedId employeeId = selection switch
            {
                TypedId id => id,
                _ => TypedId.Create(selection),
            };

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, employeeId },
                { NavigationKeys.IsBackButtonVisible, true },
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

                _favoriteEmployeesIds = await _timetableService.GetFavoriteEmployeesTimetablesIdsAsync();
                FilteredFavoriteEmployeesIds = _favoriteEmployeesIds.ToObservableCollection();

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
                FilteredFavoriteEmployeesIds = FilteredFavoriteEmployeesIds
                    .Where(id => id.DisplayName.Contains(employeeNameFilter, StringComparison.InvariantCultureIgnoreCase))
                    .ToObservableCollection();
            }
            // Else we have to search in all list :(
            else
            {
                FilteredEmployees = _allEmployees.Where(SearchPredicate(employeeNameFilter)).ToList();
                FilteredFavoriteEmployeesIds = _favoriteEmployeesIds
                    .Where(id => id.DisplayName.Contains(employeeNameFilter, StringComparison.InvariantCultureIgnoreCase))
                    .ToObservableCollection();
            }

            _currentEmployeeFilter = employeeNameFilter;
        }

        public void Receive(TimetableFavoritedMessage message)
        {
            if (message.Value is not EmployeeId employeeId)
                return;

            _favoriteEmployeesIds.Add(employeeId);

            // Not to perform filtering for whole collection
            if (employeeId.DisplayName.Contains(_currentEmployeeFilter, StringComparison.InvariantCultureIgnoreCase))
            {
                FilteredFavoriteEmployeesIds.Add(employeeId);
                OnPropertyChanged(nameof(FilteredFavoriteEmployeesIds)); // Otherwise converter won't catch up
            }
        }

        public void Receive(TimetableUnfavoritedMessage message)
        {
            if (message.Value is not EmployeeId employeeId)
                return;

            _favoriteEmployeesIds.Remove(employeeId);

            if (employeeId.DisplayName.Contains(_currentEmployeeFilter, StringComparison.InvariantCultureIgnoreCase))
            {
                FilteredFavoriteEmployeesIds.Remove(employeeId);
                OnPropertyChanged(nameof(FilteredFavoriteEmployeesIds)); // Otherwise converter won't catch up
            }
        }
    }
}
