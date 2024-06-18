using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class ExamsPage : ContentView
{
    private readonly TimetablePageViewModel _viewmodel;
    public ExamsPage()
    {
        var viewmodel = App.Current.Handler.MauiContext.Services.GetRequiredService<TimetablePageViewModel>();

        InitializeComponent();

        _viewmodel = viewmodel;
        BindingContext = viewmodel;
    }
}