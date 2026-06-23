using System.Text.Json;
using System.Text.Json.Serialization;

namespace Andy.Agentic.Domain.Helpers;

/// <summary>
/// Ensures <c>enum</c> is always serialized as a JSON array for Moonshot/Kimi tool schemas.
/// </summary>
internal sealed class MoonshotEnumArrayJsonConverter : JsonConverter<string[]?>
{
    public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => new[] { reader.GetString() ?? string.Empty },
            JsonTokenType.StartArray => JsonSerializer.Deserialize<string[]>(ref reader, options),
            _ => throw new JsonException($"Unexpected token {reader.TokenType} for enum array."),
        };
    }

    public override void Write(Utf8JsonWriter writer, string[]? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            return;
        }

        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }

        writer.WriteEndArray();
    }
}
