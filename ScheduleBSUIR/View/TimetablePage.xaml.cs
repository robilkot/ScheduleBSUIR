using DevExpress.DirectX.Common.Direct2D;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class TimetablePage : ContentPage
{
    private readonly TimetablePageViewModel _viewModel;
    public TimetablePage(TimetablePageViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
        _viewModel = vm;
    }

    // for demo
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.GetTimetableCommand.Execute(new StudentGroupId("210271"));
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (_viewModel.SelectedTab == Helpers.Constants.TimetableTabs.Schedule)
            {
                _viewModel.SelectedTab = Helpers.Constants.TimetableTabs.Exams;
            } 
            else
            {
                _viewModel.SelectedTab = Helpers.Constants.TimetableTabs.Schedule;
            }
        }
        finally { }
    }
}