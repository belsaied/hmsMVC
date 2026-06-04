using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HMS.PL.Factories
{
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly string[] Formats =
        [
            "yyyy-M-d", "yyyy-MM-dd",
            "yyyy-M-dTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss",
            "yyyy-M-dTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ssZ"
        ];

        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var str = reader.GetString();

            if (DateTime.TryParseExact(
                    str,
                    Formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                    out var result))
                return result;

            if (DateTime.TryParse(str, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal, out result))
                return result;

            // Return sentinel — the real error will be surfaced by model validation below
            return DateTime.MinValue;
        }

        public override void Write(
            Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}