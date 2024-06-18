using ScheduleBSUIR.Models;
namespace ScheduleBSUIR.View.Controls;

public partial class TimetableHeader : ContentView
{
    public static readonly BindableProperty TimetableProperty = BindableProperty.Create(nameof(Timetable), typeof(Timetable), typeof(TimetableHeader));
    public Timetable Timetable
    {
        get => (Timetable)GetValue(TimetableProperty);
        set => SetValue(TimetableProperty, value);
    }

    public TimetableHeader()
	{
		InitializeComponent();
	}
}