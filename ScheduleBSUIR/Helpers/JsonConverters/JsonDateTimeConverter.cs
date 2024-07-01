using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScheduleBSUIR.Helpers.JsonConverters
{
    class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DateTime result = DateTime.MinValue;

            if (!DateTime.TryParseExact(
                reader.GetString(),
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result))
            {
                if (!DateTime.TryParseExact(
                reader.GetString(),
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result))
                {

                }
            }

            DateTime.SpecifyKind(result, DateTimeKind.Utc);

            return result;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
