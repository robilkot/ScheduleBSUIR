using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models;
namespace ScheduleBSUIR.View.Controls;

public partial class TimetableHeader : ContentView
{
    public static readonly BindableProperty TimetableIdProperty = BindableProperty.Create(nameof(TimetableId), typeof(TypedId), typeof(TimetableHeader), null);
    public TypedId TimetableId
    {
        get => (TypedId)GetValue(TimetableIdProperty);
        set => SetValue(TimetableIdProperty, value);

    }

    public static readonly BindableProperty TimetableProperty = BindableProperty.Create(nameof(Timetable), typeof(Timetable), typeof(TimetableHeader), null);
    public Timetable Timetable
    {
        get => (Timetable)GetValue(TimetableProperty);
        set => SetValue(TimetableProperty, value);

    }

    public static readonly BindableProperty TabProperty = BindableProperty.Create(nameof(Tab), typeof(TimetableTabs), typeof(TimetableHeader), TimetableTabs.Schedule);
    public TimetableTabs Tab
    {
        get => (TimetableTabs)GetValue(TabProperty);
        set
        {
            SetValue(TabProperty, value);
            // Timetable needs to be converted to labels' texts
            OnPropertyChanged(nameof(Timetable));
        }
    }

    public TimetableHeader()
    {
        InitializeComponent();
    }
}