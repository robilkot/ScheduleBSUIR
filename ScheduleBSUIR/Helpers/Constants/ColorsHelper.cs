using System.Collections.Immutable;

namespace ScheduleBSUIR.Helpers.Constants
{
    public static class ColorsHelper
    {
        public static readonly Color Green = Color.FromArgb("#31ce57");
        public static readonly Color Red = Color.FromArgb("#ff463a");
        public static readonly Color Yellow = Color.FromArgb("#fdd70a");
        public static readonly Color Brown = Color.FromArgb("#ac8e6a");
        public static readonly Color Violet = Color.FromArgb("#be58ff");
        public static readonly Color Blue = Color.FromArgb("#3660bf");
        public static readonly Color Orange = Color.FromArgb("#ff6523");
        public static readonly Color Pink = Color.FromArgb("#ff3961");
        public static readonly Color Lightblue = Color.FromArgb("#8cafff");
        public static readonly Color Gray = Color.FromArgb("#8f8e94");

        public static readonly ImmutableList<Color> AvailableColors = [
            Red,
            Pink,
            Orange,
            Yellow,
            Green,
            Lightblue,
            Blue,
            Violet,
            Brown,
            Gray,
        ];
    }
}
