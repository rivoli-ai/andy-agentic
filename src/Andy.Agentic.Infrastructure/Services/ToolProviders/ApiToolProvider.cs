using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

/// <summary>
/// Tool provider for executing API-based tools
/// </summary>
public class ApiToolProvider(HttpClient httpClient) : IToolProvider
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    /// <summary>
    /// tool type
    /// </summary>
    public string ToolType => "api";

    /// <summary>
    /// ExecuteToolAsync
    /// </summary>
    /// <param name="tool"></param>
    /// <param name="requestParameters"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<object?> ExecuteToolAsync(Tool tool, Dictionary<string, object> requestParameters)
    {
        if (tool == null)
        {
            throw new ArgumentNullException(nameof(tool));
        }

        try
        {
            var configuration = ParseConfiguration(tool.Configuration);
            var auth = ParseAuthentication(tool.Authentication);
            var parameters = requestParameters.Any() ? requestParameters : Parse(tool.Parameters);

            var endpoint = GetRequiredConfigValue<string>(configuration, "endpoint");
            var method = GetConfigValue<string>(configuration, "method", "GET").ToUpper();
            var headers = Parse(tool.Headers);

            using var request = new HttpRequestMessage();
            request.Method = GetHttpMethod(method);

            await ApplyAuthenticationHeadersAsync(request, auth);
            ApplyCustomHeaders(request, headers);

            if (IsGetRequest(method))
            {
                request.RequestUri = BuildGetRequestUri(endpoint, parameters);
            }
            else
            {
                request.RequestUri = new Uri(endpoint);
                var bodyContent = ApiToolRequestBodyBuilder.CreateContent(configuration, parameters);
                if (bodyContent != null)
                {
                    request.Content = bodyContent;
                }
            }

            // Execute the request
            using var response = await _httpClient.SendAsync(request);
            return await ProcessResponse(response);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"Failed to execute API tool '{tool.Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// CanHandleToolType
    /// </summary>
    /// <param name="toolType"></param>
    /// <returns></returns>
    public bool CanHandleToolType(string toolType) => string.Equals(toolType, ToolType, StringComparison.OrdinalIgnoreCase);

    private static Dictionary<string, object> ParseConfiguration(string? configuration)
    {
        if (string.IsNullOrEmpty(configuration))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(configuration)
                   ?? new Dictionary<string, object>();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON in tool configuration", nameof(configuration), ex);
        }
    }

    private static Dictionary<string, object> ParseAuthentication(string? authentication)
    {
        if (string.IsNullOrEmpty(authentication))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(authentication)
                   ?? new Dictionary<string, object>();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON in tool authentication", nameof(authentication), ex);
        }
    }

    private static Dictionary<string, object> Parse(string? parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            var parsedList = JsonSerializer.Deserialize<List<Dictionary<string,object>>>(
                parameters,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? [];

            var result = new Dictionary<string, object>();
            foreach (var param in parsedList)
            {
                if (!string.IsNullOrEmpty(param["name"].ToString()))
                {
                    result["name"] = param["default"];
                }
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON in tool parameters", nameof(parameters), ex);
        }
    }

    private static T GetRequiredConfigValue<T>(Dictionary<string, object> config, string key)
    {
        if (!config.TryGetValue(key, out var value))
        {
            throw new ArgumentException($"Required configuration value '{key}' is missing");
        }

        if (value is JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText())!;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }

    private static T GetConfigValue<T>(Dictionary<string, object> config, string key, T defaultValue)
    {
        if (!config.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        try
        {
            if (value is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText())!;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    private static HttpMethod GetHttpMethod(string method) =>
        method.ToUpper() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            "PATCH" => HttpMethod.Patch,
            "HEAD" => HttpMethod.Head,
            "OPTIONS" => HttpMethod.Options,
            _ => throw new ArgumentException($"Unsupported HTTP method: {method}")
        };

    private static bool IsGetRequest(string method) => string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase);

    private static Uri BuildGetRequestUri(string endpoint, Dictionary<string, object> parameters)
    {
        if (!parameters.Any())
        {
            return new Uri(endpoint);
        }

        var uriBuilder = new UriBuilder(endpoint);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var param in parameters)
        {
            var value = param.Value switch
            {
                JsonElement jsonElement => jsonElement.ToString(),
                null => string.Empty,
                _ => param.Value.ToString()
            };

            query[param.Key] = value;
        }

        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Applies authentication headers to request based on auth type (Bearer, API Key, Basic, OAuth2).
    /// </summary>
    private static async Task ApplyAuthenticationHeadersAsync(HttpRequestMessage request, Dictionary<string, object> auth)
    {
        if (!auth.Any())
        {
            return;
        }

        if (!auth.TryGetValue("type", out var authType))
        {
            return;
        }

        switch (authType.ToString()?.ToLower())
        {
            case "bearer":
                if (auth.TryGetValue("apiKey", out var token) || auth.TryGetValue("token", out token))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.ToString());
                }
                break;

            case "apikey":
            case "api_key":
                // Support both the legacy format (key/value) and the new format (apiKey)
                if (auth.TryGetValue("apiKey", out var apiKey))
                {
                    // API keys can be added as headers (common patterns)
                    // Check for header name, default to "X-API-Key"
                    var headerName = auth.TryGetValue("key", out var keyName) 
                        ? keyName.ToString() 
                        : "X-API-Key";
                    request.Headers.Add(headerName!, apiKey.ToString());
                }
                else if (auth.TryGetValue("key", out var key) && auth.TryGetValue("value", out var value))
                {
                    // Legacy format
                    request.Headers.Add(key.ToString()!, value.ToString()!);
                }
                break;

            case "basic":
                if (auth.TryGetValue("username", out var username) && auth.TryGetValue("password", out var password))
                {
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                }
                break;

            case "oauth2":
                // Check if a pre-obtained token exists
                if (auth.TryGetValue("token", out var oauth2Token) || 
                    auth.TryGetValue("accessToken", out oauth2Token))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", oauth2Token.ToString());
                }
                // Otherwise, obtain token using client credentials flow
                else if (auth.TryGetValue("clientId", out var clientId) && 
                         auth.TryGetValue("clientSecret", out var clientSecret) &&
                         auth.TryGetValue("tokenUrl", out var tokenUrl))
                {
                    var accessToken = await ObtainOAuth2TokenAsync(
                        tokenUrl.ToString()!,
                        clientId.ToString()!,
                        clientSecret.ToString()!,
                        auth.TryGetValue("scopes", out var scopes) ? scopes.ToString() : null
                    );

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        request.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    }
                }
                break;

            case "azure_oauth2":
                // Check if a pre-obtained token exists
                if (auth.TryGetValue("token", out var azureToken) || 
                    auth.TryGetValue("accessToken", out azureToken))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", azureToken.ToString());
                }
                // Otherwise, obtain token using Azure client credentials flow
                else if (auth.TryGetValue("clientId", out var azureClientId) && 
                         auth.TryGetValue("clientSecret", out var azureClientSecret) &&
                         auth.TryGetValue("tenantId", out var tenantId))
                {
                    // Construct Azure token URL from tenant ID
                    var azureTokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                    
                    // Use resource if provided, otherwise use scopes
                    var scope = auth.TryGetValue("resource", out var resource) 
                        ? resource.ToString() 
                        : auth.TryGetValue("scopes", out var azureScopes) 
                            ? azureScopes.ToString() 
                            : null;

                    var accessToken = await ObtainOAuth2TokenAsync(
                        azureTokenUrl,
                        azureClientId.ToString()!,
                        azureClientSecret.ToString()!,
                        scope
                    );

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        request.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Obtains an OAuth2 access token using the client credentials flow.
    /// </summary>
    private static async Task<string?> ObtainOAuth2TokenAsync(
        string tokenUrl, 
        string clientId, 
        string clientSecret, 
        string? scopes)
    {
        try
        {
            using var tokenClient = new HttpClient();
            
            // Prepare the token request
            var requestContent = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            };

            if (!string.IsNullOrWhiteSpace(scopes))
            {
                requestContent["scope"] = scopes;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(requestContent)
            };

            var response = await tokenClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Failed to obtain OAuth2 token. Status: {response.StatusCode}, Error: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

            if (tokenResponse != null && tokenResponse.TryGetValue("access_token", out var accessToken))
            {
                // Handle JsonElement for access_token
                if (accessToken is JsonElement jsonElement)
                {
                    return jsonElement.GetString();
                }
                return accessToken.ToString();
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error obtaining OAuth2 token: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Applies custom headers to the HTTP request.
    /// </summary>
    private static void ApplyCustomHeaders(HttpRequestMessage request, Dictionary<string, object> headers)
    {
        if (!headers.Any())
        {
            return;
        }

        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value.ToString()!);
        }
    }

    /// <summary>
    /// Processes HTTP response, attempting JSON deserialization with string fallback.
    /// </summary>
    private static async Task<object?> ProcessResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseContent))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<object>(responseContent);
            }
            catch (JsonException)
            {
                return responseContent;
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API request failed with status {response.StatusCode}: {errorContent}");
        }
    }
}
