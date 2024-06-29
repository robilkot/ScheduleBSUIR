using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR.View;

public partial class SettingsPage : ContentPage
{
    private readonly PreferencesService _preferencesService;
    public SettingsPage(SettingsPageViewModel viewmodel, PreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
        
        InitializeComponent();

        BindingContext = viewmodel;
    }

    private void ColorPlank_Tapped(object sender, TappedEventArgs e)
    {
        colorPickerPopup.PlacementTarget = (Microsoft.Maui.Controls.View)sender;

        LessonType selectedType = (LessonType)((Microsoft.Maui.Controls.View)sender).BindingContext;
        colorSelector.SelectedColor = _preferencesService.GetColorPreference(selectedType.ColorPreferenceKey);

        colorPickerPopup.IsOpen = !colorPickerPopup.IsOpen;
    }

    private void DXColorSelector_SelectedColorChanged(object sender, DevExpress.Maui.Core.ValueChangedEventArgs<Color> e)
    {
        if (e.NewValue is null)
            return;

        LessonType selectedType = (LessonType)colorPickerPopup.PlacementTarget.BindingContext;
        _preferencesService.SetColorPreference(selectedType.ColorPreferenceKey, e.NewValue);

        if(colorPickerPopup.IsOpen)
        {
            colorPickerPopup.IsOpen = false;
            colorPickerPopup.PlacementTarget.BindingContext = selectedType with { };
        }
    }

    private void RotateColorPlankArrow()
    {
        colorPickerPopup.PlacementTarget
            .GetVisualTreeDescendants()
            .OfType<Image>()
            .FirstOrDefault()
            ?.RotateTo(colorPickerPopup.IsOpen ? -90 : 90, easing: Easing.CubicInOut);
    }

    private void ColorPickerPopup_ChangingState(object sender, EventArgs e)
    {
        RotateColorPlankArrow();
    }
}