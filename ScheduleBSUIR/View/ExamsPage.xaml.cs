using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class ExamsPage : ContentPage
{
	private readonly ExamsPageViewModel _viewmodel;
	public ExamsPage(ExamsPageViewModel viewmodel)
	{
		InitializeComponent();

        _viewmodel = viewmodel;
		BindingContext = viewmodel;
    }
}