using System.Text;
using System.Text.Json;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

/// <summary>
/// Converts tool authentication JSON into HTTP headers for MCP and API clients.
/// </summary>
public static class ToolAuthHeaderBuilder
{
    public static Dictionary<string, object>? ParseAuthenticationJson(string? authentication)
    {
        if (string.IsNullOrWhiteSpace(authentication))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(authentication);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static async Task<Dictionary<string, string>> BuildHeadersAsync(
        string? authentication,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        var auth = ParseAuthenticationJson(authentication);
        return await BuildHeadersAsync(auth, httpClient, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Dictionary<string, string>> BuildHeadersAsync(
        Dictionary<string, object>? auth,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (auth == null || auth.Count == 0)
            return headers;

        if (!auth.TryGetValue("type", out var authTypeObj))
            return headers;

        var authType = authTypeObj.ToString()?.ToLowerInvariant() ?? "none";
        if (authType is "none")
            return headers;

        switch (authType)
        {
            case "bearer":
                if (TryGetString(auth, "apiKey", out var bearer) || TryGetString(auth, "token", out bearer))
                    headers["Authorization"] = $"Bearer {bearer}";
                break;

            case "apikey":
            case "api_key":
                if (TryGetString(auth, "apiKey", out var apiKey))
                {
                    var headerName = TryGetString(auth, "key", out var keyName) ? keyName : "X-API-Key";
                    headers[headerName] = apiKey;
                }
                else if (TryGetString(auth, "key", out var legacyKey) && TryGetString(auth, "value", out var legacyValue))
                {
                    headers[legacyKey] = legacyValue;
                }
                break;

            case "basic":
                if (TryGetString(auth, "username", out var username) && TryGetString(auth, "password", out var password))
                {
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                    headers["Authorization"] = $"Basic {credentials}";
                }
                break;

            case "oauth2":
                if (TryGetString(auth, "token", out var oauthToken) || TryGetString(auth, "accessToken", out oauthToken))
                {
                    headers["Authorization"] = $"Bearer {oauthToken}";
                }
                else if (httpClient != null
                         && TryGetString(auth, "clientId", out var clientId)
                         && TryGetString(auth, "clientSecret", out var clientSecret)
                         && TryGetString(auth, "tokenUrl", out var tokenUrl))
                {
                    var scope = TryGetString(auth, "scopes", out var scopes) ? scopes : null;
                    var accessToken = await ObtainOAuth2TokenAsync(httpClient, tokenUrl, clientId, clientSecret, scope, cancellationToken)
                        .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(accessToken))
                        headers["Authorization"] = $"Bearer {accessToken}";
                }
                break;

            case "azure_oauth2":
                if (TryGetString(auth, "token", out var azureToken) || TryGetString(auth, "accessToken", out azureToken))
                {
                    headers["Authorization"] = $"Bearer {azureToken}";
                }
                else if (httpClient != null
                         && TryGetString(auth, "clientId", out var azureClientId)
                         && TryGetString(auth, "clientSecret", out var azureClientSecret)
                         && TryGetString(auth, "tenantId", out var tenantId))
                {
                    var azureTokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                    var scope = TryGetString(auth, "resource", out var resource)
                        ? resource
                        : TryGetString(auth, "scopes", out var azureScopes) ? azureScopes : null;
                    var accessToken = await ObtainOAuth2TokenAsync(httpClient, azureTokenUrl, azureClientId, azureClientSecret, scope, cancellationToken)
                        .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(accessToken))
                        headers["Authorization"] = $"Bearer {accessToken}";
                }
                break;
        }

        return headers;
    }

    private static bool TryGetString(Dictionary<string, object> auth, string key, out string value)
    {
        value = string.Empty;
        if (!auth.TryGetValue(key, out var raw) || raw == null)
            return false;

        if (raw is JsonElement element)
        {
            value = element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : element.ToString();
            return !string.IsNullOrWhiteSpace(value);
        }

        value = raw.ToString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static async Task<string?> ObtainOAuth2TokenAsync(
        HttpClient httpClient,
        string tokenUrl,
        string clientId,
        string clientSecret,
        string? scopes,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestContent = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            };

            if (!string.IsNullOrWhiteSpace(scopes))
                requestContent["scope"] = scopes;

            using var response = await httpClient
                .PostAsync(tokenUrl, new FormUrlEncodedContent(requestContent), cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("access_token", out var tokenElement))
                return tokenElement.GetString();

            return null;
        }
        catch
        {
            return null;
        }
    }
}
