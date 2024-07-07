using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Maui.CollectionView;
using DevExpress.Maui.Controls;
using DevExpress.Maui.Core.Internal;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class TimetablePage : ContentPage, IRecipient<TimetableStateChangedMessage>, IRecipient<ScrollToIndex>
{
    protected readonly TimetablePageViewModel _viewmodel;
    protected readonly ILoggingService _loggingService;
    public TimetablePage()
    {
        _loggingService = App.Current.Handler.MauiContext.Services.GetRequiredService<ILoggingService>();

        InitializeComponent();

        var vm = App.Current.Handler.MauiContext.Services.GetRequiredService<TimetablePageViewModel>();
        BindingContext = vm;
        _viewmodel = vm;

        WeakReferenceMessenger.Default.Register<ScrollToIndex>(this);
        WeakReferenceMessenger.Default.Register<TimetableStateChangedMessage>(this);
    }

    private void scheduleDetailSheet_StateChanged(object sender, DevExpress.Maui.Core.ValueChangedEventArgs<BottomSheetState> e)
    {
        if (e.NewValue == BottomSheetState.Hidden)
        {
            scheduleDetailSheet.BindingContext = null;
        }
    }

    private void employee_tapped(object sender, TappedEventArgs e)
    {
        scheduleDetailSheet.State = BottomSheetState.Hidden;
    }

    private void dayScheduleCollectionView_Tap(object sender, CollectionViewGestureEventArgs e)
    {
        if (e.Item is not Schedule schedule)
            return;

        scheduleDetailSheet.BindingContext = schedule;

        scheduleDetailSheet.State = BottomSheetState.HalfExpanded;
    }

    private async void dayScheduleCollectionView_Scrolled(object sender, DXCollectionViewScrolledEventArgs e)
    {
        if (e.LastVisibleItemIndex > dayScheduleCollectionView.ScrollItemCount - 12
            || dayScheduleCollectionView.ScrollItemCount == 0)
        {
            await _viewmodel.LoadMoreSchedule();
        }
    }
    public void Receive(ScrollToIndex message)
    {
        Dispatcher.Dispatch(() =>
        {
            if (dayScheduleCollectionView is null)
                return;

            // Decrement value for visual convenience
            var indexToScrollTo = Math.Clamp(message.Value - 1, 0, dayScheduleCollectionView.ScrollItemCount - 1);

            var handle = dayScheduleCollectionView.GetItemHandle(indexToScrollTo);

            dayScheduleCollectionView.ScrollTo(handle, DevExpress.Maui.Core.DXScrollToPosition.Start);
        });
    }

    // If we pin some timetable, pages down the stack should update their state. As well as pinned timetable on separate tab
    public virtual void Receive(TimetableStateChangedMessage message)
    {
        var msgId = message.Value.Item1;
        var msgState = message.Value.Item2;

        if (msgId.Equals(_viewmodel.TimetableId))
        {
            _loggingService.LogInfo($"Regular page rec StateChanged");

            _viewmodel.TimetableState = msgState;
        }
    }
}