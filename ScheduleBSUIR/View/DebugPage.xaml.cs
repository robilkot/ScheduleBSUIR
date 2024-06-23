using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class DebugPage : ContentPage
{
    private readonly DebugPageViewModel _viewmodel;
	public DebugPage(DebugPageViewModel viewmodel)
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

    private void ClearCacheButton_Clicked(object sender, EventArgs e)
    {
        _viewmodel.ClearCacheCommand.Execute(null);
    }
}