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
            Dispatcher.Dispatch(() =>
            {
                if (examsCollectionView is null)
                    return;

                // Decrement by two to make schedule visible
                var indexToScrollTo = Math.Clamp(message.Value - 2, 0, int.MaxValue);

                var handle = examsCollectionView.GetItemHandle(indexToScrollTo);

                examsCollectionView.ScrollTo(handle);
            });
        });
    }

    private void examsCollectionView_SelectionChanged(object sender, DevExpress.Maui.CollectionView.CollectionViewSelectionChangedEventArgs e)
    {
        if (examsCollectionView.SelectedItem is null)
        {
            return;
        }

        sheetContent.BindingContext = (Schedule)examsCollectionView.SelectedItem;

        examDetailSheet.State = BottomSheetState.HalfExpanded;
        examDetailSheet.UpdateSizeToContent = true;

        examsCollectionView.SelectedItem = null;
    }

    private void examDetailSheet_StateChanged(object sender, DevExpress.Maui.Core.ValueChangedEventArgs<BottomSheetState> e)
    {
        if (e.NewValue == BottomSheetState.Hidden)
        {
            sheetContent.BindingContext = null;
        }
    }
}