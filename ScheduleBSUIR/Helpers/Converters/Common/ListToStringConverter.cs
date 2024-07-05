using CommunityToolkit.Maui.Converters;
using System.Collections;
using System.Globalization;

namespace ScheduleBSUIR.Helpers.Converters.Common
{
    // Taken from MCT but with string.Empty fallback value for null input
    public class ListToStringConverter : BaseConverterOneWay<IEnumerable, string, string?>
    {
        private string separator = string.Empty;

        public override string DefaultConvertReturnValue { get; set; } = string.Empty;
        public string Separator
        {
            get => separator;
            set
            {
                separator = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public override string ConvertFrom(IEnumerable? value, string? parameter = null, CultureInfo? culture = null)
        {
            if(value is null)
                return string.Empty;

            IEnumerable<string> values = from x in value.OfType<object>()
                                         select x.ToString() into x
                                         where !string.IsNullOrWhiteSpace(x)
                                         select x;
            return string.Join(parameter ?? Separator, values);
        }
    }
}
