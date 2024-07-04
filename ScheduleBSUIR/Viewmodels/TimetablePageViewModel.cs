using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Maui.Core.Internal;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Models.UI;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class TimetablePageViewModel : BaseViewModel, IQueryAttributable, IRecipient<TimetablePinnedMessage>
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

            WeakReferenceMessenger.Default.Register<TimetablePinnedMessage>(this);
        }

        [ObservableProperty]
        private bool _isPinnedTimetable = false;

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
            Schedule = [];

            await LoadMoreSchedule();
            await ScrollToActiveSchedule();
        }

        [ObservableProperty]
        private SubgroupType _selectedMode;
        async partial void OnSelectedModeChanged(SubgroupType value)
        {
            _preferencesService.SetSubgroupTypePreference(value);

            Schedule = [];

            await LoadMoreSchedule();
            await ScrollToActiveSchedule();
        }

        [ObservableProperty]
        private Timetable? _timetable;
        async partial void OnTimetableChanged(Timetable? value)
        {
            Schedule = [];

            await LoadMoreSchedule();
            await ScrollToActiveSchedule();
        }

        [ObservableProperty]
        private TimetableState _timetableState = TimetableState.Default;

        [ObservableProperty]
        private ObservableCollection<ITimetableItem> _schedule = [];

        [ObservableProperty]
        private TypedId? _timetableId = default;
        async partial void OnTimetableIdChanged(TypedId? value)
        {
            TimetableState = value is null
                ? TimetableState.Default
                : await _timetableService.GetState(value);

            _loggingService.LogInfo($"Timetable {value} state: {TimetableState}", displayCaller: false);

            await GetTimetable(forceReload: true);
        }

        [ObservableProperty]
        private TypedId? _previousTimetableId;

        [ObservableProperty]
        private bool _isBackButtonVisible = false;

        public async Task LoadMoreSchedule()
        {
            if (Timetable is null)
                return;

            List<ITimetableItem> newItems = await _timetableItemsGenerator.GenerateMoreItems(Timetable, SelectedTab, SelectedMode);

            _loggingService.LogInfo($"GenerateMoreItems returned {newItems.Count} objects", displayCaller: false);

            foreach (ITimetableItem item in newItems)
            {
                Schedule.Add(item);
            }

            IsLoadingMoreSchedule = false;
        }

        public async Task GetTimetable(bool forceReload)
        {
            if (IsBusy)
                return;

            if (!forceReload && Timetable is not null)
                return;

            IsBusy = true;

            if (TimetableId is null)
            {
                IsPinnedTimetable = true;

                _loggingService.LogInfo($"GetTimetable getting pinned id", displayCaller: false);

                TimetableId = await _timetableService.GetPinnedIdAsync();
            }

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
            if (TimetableId is null)
                return;

            if (newState == TimetableState)
                return;

            TimetableState = newState;

            IsTimetableStatePopupOpen = false;

            await _timetableService.ApplyState(TimetableId, newState);
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
        }

        [RelayCommand]
        public void ToggleSubgroupTypePopup()
        {
            IsTimetableStatePopupOpen = false;
            IsSubgroupTypePopupOpen = !IsSubgroupTypePopupOpen;
        }

        // Accepts studentgroup dto or employeedto
        [RelayCommand]
        public async Task NavigateToTimetable(object dto)
        {
            var timetableId = TypedId.Create(dto);

            if (timetableId.Equals(PreviousTimetableId))
            {
                await NavigateBack();
                return;
            }

            Dictionary<string, object> navigationParameters = new()
            {
                { NavigationKeys.TimetableId, timetableId },
                { NavigationKeys.PreviousTimetableId, TimetableId! },
                { NavigationKeys.IsBackButtonVisible, true },
            };

            // Let bottomsheet close smoothly
            await Task.Delay(150);

            await Shell.Current.GoToAsync(nameof(TimetablePage), true, navigationParameters);
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(NavigationKeys.TimetableId, out var id))
            {
                TimetableId = (TypedId)id;
            }
            if (query.TryGetValue(NavigationKeys.PreviousTimetableId, out var prevId))
            {
                PreviousTimetableId = (TypedId?)prevId;
            }
            if (query.TryGetValue(NavigationKeys.IsBackButtonVisible, out var isBackButtonVisible))
            {
                IsBackButtonVisible = (bool)isBackButtonVisible;
            }
        }

        private async Task ScrollToActiveSchedule()
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

        // If we pin some timetable, pages down the stack should update their state. As well as pinned timetable on separate tab
        public void Receive(TimetablePinnedMessage message)
        {
            if (TimetableState == TimetableState.Pinned && !(message.Value?.Equals(TimetableId) ?? false))
            {
                TimetableState = TimetableState.Favorite;
            }

            if (IsPinnedTimetable)
            {
                TimetableId = message.Value;
            }
        }
    }
}
