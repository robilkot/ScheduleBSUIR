using ScheduleBSUIR.Helpers.Constants;
using System.Diagnostics;
using System.Windows.Input;

namespace ScheduleBSUIR.View.Controls;

public partial class CustomTabbar : ContentView
{
    public static readonly BindableProperty TabProperty = BindableProperty.Create(nameof(Tab), typeof(TimetableTabs), typeof(CustomTabbar), TimetableTabs.Schedule);
    public TimetableTabs Tab
    {
        get => (TimetableTabs)GetValue(TabProperty);
        set
        {
            SetValue(TabProperty, value);

            if (value == TimetableTabs.Schedule)
            {
                secondOption.FadeTo(0.5, easing: Easing.CubicIn);
                firstOption.FadeTo(1, easing: Easing.CubicIn);
            }
            else
            {
                firstOption.FadeTo(0.5, easing: Easing.CubicIn);
                secondOption.FadeTo(1, easing: Easing.CubicIn);
            }

            MoveSlider(value);
        }
    }

    public ICommand ToggleTabCommand => new Command(() =>
    {
        if (Tab == TimetableTabs.Schedule)
            Tab = TimetableTabs.Exams;
        else
            Tab = TimetableTabs.Schedule;
    });

    public CustomTabbar()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        Dispatcher.Dispatch(async () =>
        {
            // Hack to slide to initial tab
            await Task.Delay(100);
            Tab = Tab;
        });
    }

    private void MoveSlider(TimetableTabs activeTab)
    {
        selector.WidthRequest = selectorGrid.ColumnSpacing + optionLabel.Width + activeTab switch
        {
            TimetableTabs.Schedule => firstOption.Width,
            TimetableTabs.Exams => secondOption.Width,
            _ => throw new UnreachableException(),
        };

        var selector_dX = -selectorGrid.Padding.Left + activeTab switch
        {
            TimetableTabs.Schedule => firstOption.X - selector.TranslationX,
            TimetableTabs.Exams => optionLabel.X - selector.TranslationX,
            _ => throw new UnreachableException(),
        };

        selector.TranslateTo(selector.TranslationX + selector_dX, selector.TranslationY, length: 150, easing: Easing.CubicIn);
    }
}