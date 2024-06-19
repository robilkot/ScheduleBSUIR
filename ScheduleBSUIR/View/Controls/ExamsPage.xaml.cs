using CommunityToolkit.Mvvm.Messaging;
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
                if(examsCollectionView is null)
                    return;

                // Decrement by two to make schedule visible
                var indexToScrollTo = Math.Clamp(message.Value - 2, 0, int.MaxValue);

                var handle = examsCollectionView.GetItemHandle(indexToScrollTo);

                examsCollectionView.ScrollTo(handle);
            });
        });
    }
}