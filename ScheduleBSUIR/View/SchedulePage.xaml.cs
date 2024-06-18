using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class SchedulePage : ContentView
{
	private readonly SchedulePageViewModel _viewmodel;
	public SchedulePage()
	{
        var viewmodel = App.Current.Handler.MauiContext.Services.GetRequiredService<SchedulePageViewModel>();
		
		InitializeComponent();

        _viewmodel = viewmodel;
		BindingContext = viewmodel;
    }
}