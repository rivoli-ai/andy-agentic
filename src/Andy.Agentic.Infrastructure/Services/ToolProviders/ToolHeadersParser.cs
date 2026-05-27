using System.Text.Json;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

/// <summary>
/// Parses tool <c>headers</c> JSON and merges them with authentication-derived HTTP headers.
/// </summary>
public static class ToolHeadersParser
{
    public static Dictionary<string, string> Parse(string? headersJson)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(headersJson))
            return result;

        try
        {
            using var doc = JsonDocument.Parse(headersJson);
            return ParseRootElement(doc.RootElement);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON in tool headers", nameof(headersJson), ex);
        }
    }

    private static Dictionary<string, string> ParseRootElement(JsonElement root)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        switch (root.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var item in root.EnumerateArray())
                {
                    if (item.TryGetProperty("name", out var nameElement) &&
                        item.TryGetProperty("value", out var valueElement))
                    {
                        var name = nameElement.GetString();
                        if (!string.IsNullOrEmpty(name))
                            result[name] = valueElement.GetString() ?? string.Empty;
                    }
                }

                break;

            case JsonValueKind.Object:
                foreach (var property in root.EnumerateObject())
                    result[property.Name] = property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString() ?? string.Empty
                        : property.Value.ToString();

                break;
        }

        return result;
    }

    public static async Task<Dictionary<string, string>> BuildMergedHttpHeadersAsync(
        string? headersJson,
        string? authenticationJson,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        var auth = ToolAuthHeaderBuilder.ParseAuthenticationJson(authenticationJson);
        return await BuildMergedHttpHeadersAsync(headersJson, auth, httpClient, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<Dictionary<string, string>> BuildMergedHttpHeadersAsync(
        string? headersJson,
        Dictionary<string, object>? auth,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        // tool.Headers first; auth fills in only missing header names.
        var merged = Parse(headersJson);
        var authHeaders = await ToolAuthHeaderBuilder
            .BuildHeadersAsync(auth, httpClient, cancellationToken)
            .ConfigureAwait(false);

        foreach (var (key, value) in authHeaders)
            merged.TryAdd(key, value);

        return merged;
    }

    public static void ApplyToEnvironmentVariables(
        IReadOnlyDictionary<string, string> headers,
        Dictionary<string, string> environmentVariables)
    {
        foreach (var (key, value) in headers)
        {
            var envKey = key.Replace('-', '_').ToUpperInvariant();
            environmentVariables[envKey] = value;
        }
    }
}
