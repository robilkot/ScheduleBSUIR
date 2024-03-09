using ScheduleBSUIR.Services;
using ScheduleBSUIR.ViewModel;

namespace ScheduleBSUIR.View;

public partial class TimetablePage : ContentPage
{
    public readonly TimetableViewModel ViewModel;
    public TimetablePage(TimetableViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
		ViewModel = vm;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.GetTimetableAsync();
    }
}