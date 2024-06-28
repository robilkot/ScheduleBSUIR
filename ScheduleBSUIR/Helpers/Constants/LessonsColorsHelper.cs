using System.Collections.Immutable;

namespace ScheduleBSUIR.Helpers.Constants
{
    public static class LessonsColorsHelper
    {
        // Sserve as default values
        public static readonly ImmutableList<Color> AvailableColors = [
            Color.FromArgb("#31ce57"), // Green
            Color.FromArgb("#ff463a"), // Red
            Color.FromArgb("#fdd70a"), // Yellow
            Color.FromArgb("#ac8e6a"), // Brown
            Color.FromArgb("#be58ff"), // Violet
            Color.FromArgb("#3660bf"), // Blue
            Color.FromArgb("#ff6523"), // Orange
            Color.FromArgb("#ff3961"), // Pink
            Color.FromArgb("#8cafff"), // Lightblue
            Color.FromArgb("#8f8e94"), // Gray
        ];
    }
}
