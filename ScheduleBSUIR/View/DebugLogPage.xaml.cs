using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class DebugLogPage : ContentPage
{
    private readonly DebugLogPageViewModel _viewmodel;
	public DebugLogPage(DebugLogPageViewModel viewmodel)
	{
		InitializeComponent();
	
        _viewmodel = viewmodel;
        BindingContext = viewmodel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewmodel.GetLogCommand.Execute(null);
    }

    private void CopyButton_Clicked(object sender, EventArgs e)
    {
        Clipboard.SetTextAsync(_viewmodel.Log);
    }

    private void ClearButton_Clicked(object sender, EventArgs e)
    {
        _viewmodel.ClearLogCommand.Execute(null);
    }
}