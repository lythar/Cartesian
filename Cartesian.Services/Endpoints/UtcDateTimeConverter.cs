using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cartesian.Services.Endpoints;

public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        writer.WriteStringValue(utcValue);
    }
}

public class NullableUtcDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var value = reader.GetDateTime();
        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var utcValue = value.Value.Kind == DateTimeKind.Utc ? value.Value : value.Value.ToUniversalTime();
        writer.WriteStringValue(utcValue);
    }
}
