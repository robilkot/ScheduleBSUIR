using CommunityToolkit.Mvvm.ComponentModel;
using ScheduleBSUIR.Model;
using ScheduleBSUIR.ViewModel;
using System.Collections.ObjectModel;

namespace ScheduleBSUIR.View;

public partial class AddGroupPage : ContentPage
{
	public AddGroupViewModel ViewModel;

    public AddGroupPage(AddGroupViewModel viewModel)
	{
		InitializeComponent();

		ViewModel = viewModel;
		BindingContext = viewModel;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.UpdateGroups();
    }
}