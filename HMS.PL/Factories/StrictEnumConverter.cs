using System.Text.Json;
using System.Text.Json.Serialization;

namespace HMS.PL.Factories
{

    public class StrictEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsEnum;

        public override JsonConverter CreateConverter(
            Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(StrictEnumConverter<>)
                .MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    public class StrictEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString() ?? string.Empty;

                if (Enum.TryParse<T>(raw, ignoreCase: true, out var parsed)
                    && Enum.IsDefined(typeof(T), parsed))
                    return parsed;

                InvalidEnumTracker.AddInvalid(typeof(T).Name, raw,
                    string.Join(", ", Enum.GetNames(typeof(T))));

                return default;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                var intVal = reader.GetInt32();
                if (Enum.IsDefined(typeof(T), intVal))
                    return (T)(object)intVal;

                InvalidEnumTracker.AddInvalid(typeof(T).Name, intVal.ToString(),
                    string.Join(", ", Enum.GetNames(typeof(T))));

                return default;
            }

            return default;
        }

        public override void Write(
            Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }

    public static class InvalidEnumTracker
    {
        private static readonly ThreadLocal<List<InvalidEnumEntry>?> _entries = new();

        public static void AddInvalid(string fieldName, string value, string validValues)
        {
            _entries.Value ??= new List<InvalidEnumEntry>();
            _entries.Value.Add(new InvalidEnumEntry(fieldName, value, validValues));
        }

        public static List<InvalidEnumEntry>? PopInvalid()
        {
            var result = _entries.Value;
            _entries.Value = null;
            return result;
        }
    }

    public record InvalidEnumEntry(string FieldName, string ProvidedValue, string ValidValues);
}