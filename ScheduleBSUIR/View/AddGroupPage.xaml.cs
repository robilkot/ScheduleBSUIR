using ScheduleBSUIR.Model;
using ScheduleBSUIR.ViewModel;

namespace ScheduleBSUIR.View;

public partial class AddGroupPage : ContentPage
{
    public AddGroupViewModel ViewModel { get; init; }

    public AddGroupPage(AddGroupViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel.GroupHeadersList.Count == 0)
        {
            await ViewModel.UpdateGroups();
        }
    }

    private void GroupNameEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        Task.Run(() => ViewModel.UpdateGroups(e.NewTextValue));
    }

    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.GroupName = ((StudentGroupHeader)e.CurrentSelection[0]).Name;
    }
}