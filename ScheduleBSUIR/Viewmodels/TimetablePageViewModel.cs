﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Maui.Core.Internal;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class TimetablePageViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly TimetableService _timetableService;
        private readonly TimetableItemsGenerator _timetableItemsGenerator;
        private readonly PreferencesService _preferencesService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public TimetablePageViewModel(
            TimetableService timetableService,
            ILoggingService loggingService,
            IDateTimeProvider dateTimeProvider,
            PreferencesService preferencesService,
            TimetableItemsGenerator timetableItemsGenerator) : base(loggingService)
        {
            _timetableService = timetableService;
            _dateTimeProvider = dateTimeProvider;
            _preferencesService = preferencesService;
            _timetableItemsGenerator = timetableItemsGenerator;

            SelectedMode = _preferencesService.GetSubgroupTypePreference();
        }

        private bool _scheduleLoaded = false;

        [ObservableProperty]
        private bool _isPinnedTimetable = false;
        async partial void OnIsPinnedTimetableChanged(bool value)
        {
            if (value)
            {
                TimetableId = await _timetableService.GetPinnedTimetableAsync();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        [ObservableProperty]
        private bool _isLoadingMoreSchedule = false;

        [ObservableProperty]
        private bool _isSubgroupTypePopupOpen = false;

        [ObservableProperty]
        private bool _isTimetableStatePopupOpen = false;

        [ObservableProperty]
        private TimetableTabs _selectedTab = TimetableTabs.Exams;
        async partial void OnSelectedTabChanged(TimetableTabs value)
        {
            await ReloadSchedule();
        }

        [ObservableProperty]
        private SubgroupType _selectedMode;
        async partial void OnSelectedModeChanged(SubgroupType value)
        {
            await ReloadSchedule();
        }

        [ObservableProperty]
        private Timetable? _timetable;
        async partial void OnTimetableChanged(Timetable? value)
        {
            await ReloadSchedule();
        }

        [ObservableProperty]
        private TimetableState _timetableState = TimetableState.Default;

        [ObservableProperty]
        private ObservableCollection<ITimetableItem> _schedule = [];

        [ObservableProperty]
        private TimetableHeader? _timetableId = default;
        async partial void OnTimetableIdChanged(TimetableHeader? value)
        {
            TimetableState = await _timetableService.GetTimetableStateAsync(value);

            await GetTimetable(forceReload: true);
        }

        [ObservableProperty]
        private TimetableHeader? _previousTimetableId;

        public async Task LoadMoreSchedule()
        {
            if (_scheduleLoaded || IsLoadingMoreSchedule || Timetable is null)
                return;

            IsLoadingMoreSchedule = true;

            List<ITimetableItem> newItems = await _timetableItemsGenerator.GenerateMoreItems(Timetable, SelectedTab, SelectedMode);

            foreach (ITimetableItem item in newItems)
            {
                Schedule.Add(item);
            }

            if (newItems.Count == 0)
            {
                _scheduleLoaded = true;
            }

            IsLoadingMoreSchedule = false;
        }

        [RelayCommand]
        public async Task GetTimetable(bool forceReload)
        {
            if (IsBusy)
                return;

            if (!forceReload && Timetable is not null)
                return;

            IsBusy = true;

            try
            {
                Timetable = TimetableId is null
                    ? null
                    : await _timetableService.GetTimetableAsync(TimetableId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Timetable = null;

                // todo: error popup?
                await Shell.Current.DisplayAlert("Error", "Couldn't get timetable", "OK");

                _loggingService.LogError($"GetTimetable threw: {ex.Message}\n{ex.StackTrace}", displayCaller: false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public static async Task NavigateBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task SetState(TimetableState newState)
        {
            TimetableState = newState;
            IsTimetableStatePopupOpen = false;

            await _timetableService.SetStateAsync(TimetableId!, newState);
        }

        [RelayCommand]
        public void ToggleStatePopup()
        {
            IsSubgroupTypePopupOpen = false;
            IsTimetableStatePopupOpen = !IsTimetableStatePopupOpen;
        }

        [RelayCommand]
        public void SetSubgroupType(SubgroupType type)
        {
            SelectedMode = type;
            IsSubgroupTypePopupOpen = false;

            _preferencesService.SetSubgroupTypePreference(type);
        }

        [RelayCommand]
        public void ToggleSubgroupTypePopup()
        {
            IsTimetableStatePopupOpen = false;
            IsSubgroupTypePopupOpen = !IsSubgroupTypePopupOpen;
        }

        [RelayCommand]
        public async Task NavigateToTimetable(ITimetableOwner owner)
        {
            var header = TimetableHeader.FromOwner(owner);

            if (header.Equals(PreviousTimetableId))
            {
                await NavigateBack();
                return;
            }

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableHeader, header },
                { NavigationKeys.PreviousTimetableHeader, TimetableId! },
            };

            // Let bottomsheet close smoothly
            await Task.Delay(150);

            await Shell.Current.GoToAsync(nameof(TimetablePage), true, navigationParameters);
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(NavigationKeys.TimetableHeader, out var id))
            {
                TimetableId = id as TimetableHeader;
            }
            if (query.TryGetValue(NavigationKeys.PreviousTimetableHeader, out var prevId))
            {
                PreviousTimetableId = prevId as TimetableHeader;
            }
        }

        private async Task ReloadSchedule()
        {
            Schedule = [];
            _scheduleLoaded = false;

            if (Timetable is null)
            {
                _loggingService.LogInfo($"ReloadSchedule timetable was NULL", displayCaller: false);
                return;
            }

            await LoadMoreSchedule();
            await ScrollToActiveSchedule();
        }

        public async Task ScrollToActiveSchedule()
        {
            int? nearestScheduleIndex = _timetableItemsGenerator.GetNearestScheduleIndex();

            if (nearestScheduleIndex is not null)
            {
                while (Schedule.Count < (nearestScheduleIndex + 1))
                {
                    await LoadMoreSchedule();
                }

                ScrollToIndex message = new(nearestScheduleIndex.Value);

                WeakReferenceMessenger.Default.Send(message);
            }
        }
    }
}
