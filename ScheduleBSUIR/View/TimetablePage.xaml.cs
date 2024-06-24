using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Maui.CollectionView;
using DevExpress.Maui.Controls;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;
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

        // Does not work with pagination
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
    private void scheduleCollectionView_SelectionChanged(object sender, DevExpress.Maui.CollectionView.CollectionViewSelectionChangedEventArgs e)
    {
        if (sender is not DXCollectionView collectionView)
            return;

        if (collectionView.SelectedItem is null)
        {
            return;
        }

        sheetContent.BindingContext = (Schedule)collectionView.SelectedItem;

        examDetailSheet.State = BottomSheetState.HalfExpanded;

        collectionView.SelectedItem = null;
    }

    private void examDetailSheet_StateChanged(object sender, DevExpress.Maui.Core.ValueChangedEventArgs<BottomSheetState> e)
    {
        if (e.NewValue == BottomSheetState.Hidden)
        {
            sheetContent.BindingContext = null;
        }
    }

    private void employee_tapped(object sender, TappedEventArgs e)
    {
        examDetailSheet.State = BottomSheetState.Hidden;
    }
}