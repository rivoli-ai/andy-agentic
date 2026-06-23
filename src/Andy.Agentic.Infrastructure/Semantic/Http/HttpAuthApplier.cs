using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Andy.Agentic.Infrastructure.Semantic.Http;

/// <summary>
///     Applies an authentication scheme to an <see cref="HttpClient"/> from a serialized
///     configuration. Shared by API tools and the skill-registry client so the supported
///     schemes (none / api_key / bearer / basic / oauth2) stay consistent.
/// </summary>
public static class HttpAuthApplier
{
    /// <summary>
    ///     Applies authentication to <paramref name="client"/>.
    /// </summary>
    /// <param name="client">The client whose default headers are configured.</param>
    /// <param name="authType">Scheme: none | api_key | bearer | basic | oauth2.</param>
    /// <param name="authConfigJson">
    ///     JSON object with the scheme-specific fields, e.g.
    ///     <c>{ "apiKey": "...", "key": "X-API-Key" }</c> for api_key,
    ///     <c>{ "token": "..." }</c> for bearer,
    ///     <c>{ "username": "...", "password": "..." }</c> for basic,
    ///     <c>{ "token": "..." }</c> or <c>{ "clientId", "clientSecret", "tokenUrl", "scopes" }</c> for oauth2.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task ApplyAsync(
        HttpClient client,
        string? authType,
        string? authConfigJson,
        CancellationToken cancellationToken = default)
    {
        var type = authType?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(type) || type == "none")
        {
            return;
        }

        var auth = ParseConfig(authConfigJson);

        switch (type)
        {
            case "bearer":
                if (TryGet(auth, out var token, "token", "apiKey", "accessToken"))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                break;

            case "apikey":
            case "api_key":
                if (TryGet(auth, out var apiKey, "apiKey", "key", "value"))
                {
                    var headerName = TryGet(auth, out var name, "header", "headerName") ? name : "X-API-Key";
                    client.DefaultRequestHeaders.Remove(headerName);
                    client.DefaultRequestHeaders.Add(headerName, apiKey);
                }
                break;

            case "basic":
                if (TryGet(auth, out var username, "username") && TryGet(auth, out var password, "password"))
                {
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                }
                break;

            case "oauth2":
                if (TryGet(auth, out var presetToken, "token", "accessToken"))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", presetToken);
                }
                else if (TryGet(auth, out var clientId, "clientId")
                         && TryGet(auth, out var clientSecret, "clientSecret")
                         && TryGet(auth, out var tokenUrl, "tokenUrl"))
                {
                    var accessToken = await ObtainOAuth2TokenAsync(
                        tokenUrl,
                        clientId,
                        clientSecret,
                        TryGet(auth, out var scopes, "scopes", "scope") ? scopes : null,
                        cancellationToken);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    }
                }
                break;
        }
    }

    private static Dictionary<string, string> ParseConfig(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                result[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString() ?? string.Empty
                    : prop.Value.ToString();
            }
            return result;
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static bool TryGet(Dictionary<string, string> auth, out string value, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (auth.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v))
            {
                value = v;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static async Task<string?> ObtainOAuth2TokenAsync(
        string tokenUrl,
        string clientId,
        string clientSecret,
        string? scopes,
        CancellationToken cancellationToken)
    {
        using var tokenClient = new HttpClient();

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
        };

        if (!string.IsNullOrWhiteSpace(scopes))
        {
            form["scope"] = scopes;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(form),
        };

        var response = await tokenClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.TryGetProperty("access_token", out var accessToken)
            ? accessToken.GetString()
            : null;
    }
}
