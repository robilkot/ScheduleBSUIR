using CommunityToolkit.Mvvm.Messaging;
using DevExpress.Maui.Controls;
using DevExpress.Maui.Core.Internal;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.Messaging;

namespace ScheduleBSUIR.View.Controls;

public partial class ExamsPage : ContentView
{
    public ExamsPage()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<ScrollToIndex>(this, (sender, message) =>
        {
            // todo: does NOT scrolling properly
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
            {
                if (examsCollectionView is null)
                    return;

                // Decrement by two to make schedule visible
                //var indexToScrollTo = Math.Clamp(message.Value - 2, 0, int.MaxValue);

                var handle = examsCollectionView.GetItemHandle(message.Value);

                examsCollectionView.ScrollTo(handle, DevExpress.Maui.Core.DXScrollToPosition.Start);
            });
        });
    }

    private void examsCollectionView_SelectionChanged(object sender, DevExpress.Maui.CollectionView.CollectionViewSelectionChangedEventArgs e)
    {
        if (examsCollectionView.SelectedItem is null)
        {
            return;
        }

        // todooo replace
        sheetContent.BindingContext = (Schedule)examsCollectionView.SelectedItem;

        examDetailSheet.State = BottomSheetState.HalfExpanded;

        examsCollectionView.SelectedItem = null;
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