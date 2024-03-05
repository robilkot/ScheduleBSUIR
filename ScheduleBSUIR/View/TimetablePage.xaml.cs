using ScheduleBSUIR.Services;
using ScheduleBSUIR.ViewModel;

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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.Id = new StudentGroupId("221701");
        await _viewModel.GetTimetableAsync();
    }
}