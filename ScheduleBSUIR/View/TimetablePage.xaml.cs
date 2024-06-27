using DevExpress.Maui.CollectionView;
using DevExpress.Maui.Controls;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class TimetablePage : ContentPage
{
    private readonly TimetablePageViewModel _viewModel;
    public TimetablePage(TimetablePageViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
        _viewModel = vm;

        //WeakReferenceMessenger.Default.Register<ScrollToIndex>(this, (sender, message) =>
        //{
        //    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
        //    {
        //        if (dayScheduleCollectionView is null)
        //            return;

        //        // Decrement by two to make schedule visible
        //        //var indexToScrollTo = Math.Clamp(message.Value - 2, 0, int.MaxValue);

        //        var handle = dayScheduleCollectionView.GetItemHandle(message.Value);

        //        dayScheduleCollectionView.ScrollTo(handle, DevExpress.Maui.Core.DXScrollToPosition.Start);
        //    });
        //});
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
}