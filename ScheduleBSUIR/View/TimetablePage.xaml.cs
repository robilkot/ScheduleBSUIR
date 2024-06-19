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