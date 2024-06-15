using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class TimetablePage : ContentPage
{
    private readonly TimetableViewModel _viewModel;
    public TimetablePage(TimetableViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
        _viewModel = vm;
    }
}