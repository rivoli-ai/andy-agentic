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
            var headerArray = JsonSerializer.Deserialize<JsonElement[]>(headersJson);
            if (headerArray == null)
                return result;

            foreach (var item in headerArray)
            {
                if (item.TryGetProperty("name", out var nameElement) &&
                    item.TryGetProperty("value", out var valueElement))
                {
                    var name = nameElement.GetString();
                    if (!string.IsNullOrEmpty(name))
                        result[name] = valueElement.GetString() ?? string.Empty;
                }
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON in tool headers", nameof(headersJson), ex);
        }
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
        var merged = Parse(headersJson);
        var authHeaders = await ToolAuthHeaderBuilder
            .BuildHeadersAsync(auth, httpClient, cancellationToken)
            .ConfigureAwait(false);

        foreach (var (key, value) in authHeaders)
            merged[key] = value;

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
