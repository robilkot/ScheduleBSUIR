using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageViewModel viewmodel)
    {
        InitializeComponent();

        BindingContext = viewmodel;
    }

    private void ColorPlank_Tapped(object sender, TappedEventArgs e)
    {
        colorPickerPopup.PlacementTarget = (Microsoft.Maui.Controls.View)sender;

        LessonType selectedType = (LessonType)((Microsoft.Maui.Controls.View)sender).BindingContext;
        colorSelector.SelectedColor = (Color)App.Current!.Resources[selectedType.ColorResourceKey];

        colorPickerPopup.IsOpen = !colorPickerPopup.IsOpen;
    }

    private void DXColorSelector_SelectedColorChanged(object sender, DevExpress.Maui.Core.ValueChangedEventArgs<Color> e)
    {
        if (e.NewValue is null)
            return;

        LessonType selectedType = (LessonType)colorPickerPopup.PlacementTarget.BindingContext;

        App.Current!.Resources[selectedType.ColorResourceKey] = e.NewValue;

        Preferences.Set(selectedType.ColorResourceKey, e.NewValue.ToHex());

        if(colorPickerPopup.IsOpen)
        {
            colorPickerPopup.IsOpen = false;
            colorPickerPopup.PlacementTarget.BindingContext = selectedType with { };
        }
    }

}