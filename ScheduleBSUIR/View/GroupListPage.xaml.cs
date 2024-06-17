using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class GroupListPage : ContentPage
{
	private readonly GroupListPageViewModel _viewmodel;
    public GroupListPage(GroupListPageViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
		_viewmodel = viewModel;
	}

    private void SearchEntry_textChanged(object sender, TextChangedEventArgs e)
    {
        _viewmodel.FilterGroupsCommand.Execute(e.NewTextValue);
    }
}