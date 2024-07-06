using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Maui.CollectionView;
using DevExpress.Maui.Controls;
using DevExpress.Maui.Core.Internal;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class TimetablePage : ContentPage
{
    private readonly TimetablePageViewModel _viewmodel;
    public TimetablePage(TimetablePageViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
        _viewmodel = vm;

        WeakReferenceMessenger.Default.Register<ScrollToIndex>(this, (sender, message) =>
        {
            Dispatcher.Dispatch(() =>
            {
                if (dayScheduleCollectionView is null)
                    return;

                // Decrement value for visual convenience
                var indexToScrollTo = Math.Clamp(message.Value - 2, 0, dayScheduleCollectionView.ScrollItemCount - 1);

                var handle = dayScheduleCollectionView.GetItemHandle(indexToScrollTo);

                dayScheduleCollectionView.ScrollTo(handle, DevExpress.Maui.Core.DXScrollToPosition.Start);
            });
        });
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
        if (e.LastVisibleItemIndex > dayScheduleCollectionView.ScrollItemCount - 10
            || dayScheduleCollectionView.ScrollItemCount == 0)
        {
            await _viewmodel.LoadMoreSchedule();
        }
    }
}