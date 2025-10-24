using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Andy.Agentic.Domain.Models;
using Microsoft.SemanticKernel;

namespace Andy.Agentic.Infrastructure.Semantic.Tools;

/// <summary>
/// Initializes a new instance of the <see cref="ApiToolFactory"/> class.
/// </summary>
public class ApiToolFactory: ToolFactory
{
    /// <summary>
    /// Creates a KernelFunction asynchronously using the provided Tool object. 
    /// The function is created from the DynamicApiCall method, with the tool's name and description.
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="tool">The Tool object containing the name and description for the KernelFunction.</param>
    /// <returns>A KernelFunction created from the DynamicApiCall method.</returns>
    public override KernelFunction CreateToolAsync(Agent agent, Tool tool)
    {
        IEnumerable<KernelParameterMetadata>? parameters = null;

        async Task<string> DynamicApiCall(KernelArguments args)
        {
            using var client = new HttpClient();

            var headers = ParseHeaders(tool.Headers);

            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value.ToString());
            }

            // Parse and apply authentication
            await ApplyAuthenticationAsync(client, tool.Authentication);

            var callArgs = ParseToolCallArguments(args);

            var configuration = ParseConfiguration(tool.Configuration);

            var endpoint = GetRequiredConfigValue<string>(configuration, "endpoint");
            var method = GetConfigValue<string>(configuration, "method", "GET").ToUpper();

            using var request = new HttpRequestMessage();
            request.Method = GetHttpMethod(method);

            if (IsGetRequest(method))
            {
                var query = string.Join("&", callArgs.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                var url = $"{endpoint}?{query}";
                request.RequestUri = new Uri(url);
            }
            else
            {
                request.RequestUri = new Uri(endpoint);
                if (callArgs.Any())
                {
                    var json = JsonSerializer.Serialize(callArgs);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        if (!string.IsNullOrEmpty(tool.Parameters))
        {
            var paramSchema = ConvertParamsToDictionary(tool.Parameters);

            parameters = paramSchema.Select(p =>
            {
                var metadata = new KernelParameterMetadata(p.Name)
                {
                    Description = $"Parameter for {p.Name}",
                    ParameterType = p.Type,
                    IsRequired = true
                };
                return metadata;
            }).ToList();
        }

        return KernelFunctionFactory.CreateFromMethod(
            method: DynamicApiCall,
            functionName: tool.Name,
            description: tool.Description,
            parameters: parameters
        );
    }

    /// <summary>
    /// Parses and applies authentication headers based on the authentication JSON string.
    /// Supports Bearer, API Key, Basic, and OAuth2 authentication types.
    /// </summary>
    private static async Task ApplyAuthenticationAsync(HttpClient client, string? authentication)
    {
        if (string.IsNullOrEmpty(authentication))
        {
            return;
        }

        try
        {
            var auth = JsonSerializer.Deserialize<Dictionary<string, object>>(authentication);
            if (auth == null || !auth.Any())
            {
                return;
            }

            if (!auth.TryGetValue("type", out var authTypeObj))
            {
                return;
            }

            var authType = authTypeObj?.ToString()?.ToLower();

            switch (authType)
            {
                case "bearer":
                    if (auth.TryGetValue("apiKey", out var token) || auth.TryGetValue("token", out token))
                    {
                        client.DefaultRequestHeaders.Authorization = 
                            new AuthenticationHeaderValue("Bearer", token.ToString());
                    }
                    break;

                case "apikey":
                case "api_key":
                    if (auth.TryGetValue("apiKey", out var apiKey))
                    {
                        // API keys can be added as headers (common patterns)
                        // Check for header name, default to "X-API-Key"
                        var headerName = auth.TryGetValue("key", out var keyName) 
                            ? keyName.ToString() 
                            : "X-API-Key";
                        client.DefaultRequestHeaders.Add(headerName!, apiKey.ToString());
                    }
                    break;

                case "basic":
                    if (auth.TryGetValue("username", out var username) && 
                        auth.TryGetValue("password", out var password))
                    {
                        var credentials = Convert.ToBase64String(
                            Encoding.ASCII.GetBytes($"{username}:{password}"));
                        client.DefaultRequestHeaders.Authorization = 
                            new AuthenticationHeaderValue("Basic", credentials);
                    }
                    break;

                case "oauth2":
                    // Check if a pre-obtained token exists
                    if (auth.TryGetValue("token", out var oauth2Token) || 
                        auth.TryGetValue("accessToken", out oauth2Token))
                    {
                        client.DefaultRequestHeaders.Authorization = 
                            new AuthenticationHeaderValue("Bearer", oauth2Token.ToString());
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
                            client.DefaultRequestHeaders.Authorization = 
                                new AuthenticationHeaderValue("Bearer", accessToken);
                        }
                    }
                    break;

                case "azure_oauth2":
                    // Check if a pre-obtained token exists
                    if (auth.TryGetValue("token", out var azureToken) || 
                        auth.TryGetValue("accessToken", out azureToken))
                    {
                        client.DefaultRequestHeaders.Authorization = 
                            new AuthenticationHeaderValue("Bearer", azureToken.ToString());
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
                            client.DefaultRequestHeaders.Authorization = 
                                new AuthenticationHeaderValue("Bearer", accessToken);
                        }
                    }
                    break;
            }
        }
        catch (JsonException)
        {
            // If authentication is not valid JSON, ignore it
            // This maintains backward compatibility with old hardcoded Bearer tokens
            return;
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
    /// Gets the HTTP method from string representation.
    /// </summary>
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

    /// <summary>
    /// Determines if the method is a GET request.
    /// </summary>
    private static bool IsGetRequest(string method) => string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a configuration value with a default fallback.
    /// </summary>
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

}
