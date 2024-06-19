using System.Reflection;

namespace ScheduleBSUIR.Helpers
{
    // Prevents strokeshape from leaking
    // Call to init should be removed once fixed in MAUI
    // https://github.com/dotnet/maui/issues/18925
    public static class BorderPerformanceWorkaround
    {
        private static readonly PropertyInfo _bindablePropertyPropertyChangingProperty =
            typeof(BindableProperty).GetProperty("PropertyChanging", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly PropertyInfo _bindablePropertyPropertyChangedProperty =
            typeof(BindableProperty).GetProperty("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Init()
        {
            _bindablePropertyPropertyChangingProperty.SetValue(Border.StrokeShapeProperty, new BindableProperty.BindingPropertyChangingDelegate((bindable, oldValue, newValue) => { }));
            _bindablePropertyPropertyChangedProperty.SetValue(Border.StrokeShapeProperty, new BindableProperty.BindingPropertyChangedDelegate((bindable, oldValue, newValue) => { }));
            _bindablePropertyPropertyChangingProperty.SetValue(Border.StrokeProperty, new BindableProperty.BindingPropertyChangingDelegate((bindable, oldValue, newValue) => { }));
            _bindablePropertyPropertyChangedProperty.SetValue(Border.StrokeProperty, new BindableProperty.BindingPropertyChangedDelegate((bindable, oldValue, newValue) => { }));
        }
    }
}
