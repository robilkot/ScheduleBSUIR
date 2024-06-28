using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Maui.CollectionView;
using DevExpress.Maui.Controls;
using DevExpress.Maui.Core.Internal;
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

                // Increment value for visual convenience
                var indexToScrollTo = Math.Clamp(message.Value + 8, 0, dayScheduleCollectionView.ScrollItemCount - 1);

                var handle = dayScheduleCollectionView.GetItemHandle(indexToScrollTo);

                dayScheduleCollectionView.ScrollTo(handle, DevExpress.Maui.Core.DXScrollToPosition.End);
            });
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewmodel.GetTimetableCommand.Execute(null);
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