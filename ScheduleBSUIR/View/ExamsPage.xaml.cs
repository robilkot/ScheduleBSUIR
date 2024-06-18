using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class ExamsPage : ContentView
{
	private readonly ExamsPageViewModel _viewmodel;
	public ExamsPage()
	{
        var viewmodel = App.Current.Handler.MauiContext.Services.GetRequiredService<ExamsPageViewModel>();
		
		InitializeComponent();

        _viewmodel = viewmodel;
		BindingContext = viewmodel;
    }
}