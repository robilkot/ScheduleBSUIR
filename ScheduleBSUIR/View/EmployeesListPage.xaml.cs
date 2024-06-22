using ScheduleBSUIR.Models;
using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class EmployeesListPage : ContentPage
{
	private readonly EmployeesListPageViewModel _viewmodel;
    public EmployeesListPage(EmployeesListPageViewModel viewModel)
	{
		BindingContext = viewModel;
		_viewmodel = viewModel;

		InitializeComponent();
	}

    private void SearchEntry_textChanged(object sender, TextChangedEventArgs e)
    {
        _viewmodel.FilterEmployeesCommand.Execute(e.NewTextValue);
    }
}