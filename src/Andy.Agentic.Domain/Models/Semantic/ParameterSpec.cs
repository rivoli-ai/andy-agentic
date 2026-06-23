using System.Text.Json.Serialization;

namespace Andy.Agentic.Domain.Models.Semantic;

public sealed class ParameterSpec
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;
    // This will be populated from JSON "type": "string|integer|number|boolean|array|object"
    [JsonConverter(typeof(ParameterSpecConverter))]
    [JsonPropertyName("type")]
    public Type? Type { get; init; }

    [JsonPropertyName("required")]
    public bool Required { get; init; }
    [JsonPropertyName("default")]
    public object? Default { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("format")]
    public string? Format { get; init; }       // optional: int32/int64/float/double
}
