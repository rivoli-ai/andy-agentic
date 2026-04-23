using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

/// <summary>
/// Builds JSON request bodies for API tools: optional static template from configuration merged with runtime parameters.
/// </summary>
internal static class ApiToolRequestBodyBuilder
{
    private const string BodyTemplateKey = "bodyTemplate";
    private const string BodyLegacyKey = "body";

    /// <summary>
    /// Creates JSON <see cref="StringContent"/> for POST/PUT/PATCH/DELETE, or null when no body should be sent.
    /// </summary>
    public static StringContent? CreateContent(
        Dictionary<string, object> configuration,
        IReadOnlyDictionary<string, object> parameters)
    {
        var templateRaw = GetBodyTemplateRaw(configuration);
        var opts = new JsonSerializerOptions { WriteIndented = false };

        if (string.IsNullOrWhiteSpace(templateRaw))
        {
            if (parameters.Count == 0)
            {
                return null;
            }

            var json = JsonSerializer.Serialize(parameters, opts);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(templateRaw);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Tool configuration bodyTemplate must be valid JSON.", ex);
        }

        if (root is JsonObject obj)
        {
            foreach (var kv in parameters)
            {
                obj[kv.Key] = ToJsonNode(kv.Value);
            }

            return new StringContent(obj.ToJsonString(opts), Encoding.UTF8, "application/json");
        }

        if (parameters.Count > 0)
        {
            throw new InvalidOperationException(
                "When the API tool defines parameters, bodyTemplate must be a JSON object so values can be merged into the request body.");
        }

        return new StringContent(root.ToJsonString(opts), Encoding.UTF8, "application/json");
    }

    private static string? GetBodyTemplateRaw(Dictionary<string, object> configuration)
    {
        var raw = GetConfigString(configuration, BodyTemplateKey);
        if (!string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        return GetConfigString(configuration, BodyLegacyKey);
    }

    private static string? GetConfigString(Dictionary<string, object> config, string key)
    {
        if (!config.TryGetValue(key, out var value) || value == null)
        {
            return null;
        }

        if (value is JsonElement je)
        {
            return je.ValueKind == JsonValueKind.String ? je.GetString() : je.GetRawText();
        }

        return value.ToString();
    }

    private static JsonNode? ToJsonNode(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is JsonElement je)
        {
            return JsonNode.Parse(je.GetRawText());
        }

        return JsonSerializer.SerializeToNode(value);
    }
}
