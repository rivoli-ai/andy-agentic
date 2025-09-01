using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Andy.Agentic.Domain.Interfaces.Llm;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Repository implementation for Ollama LLM provider integration.
/// Provides functionality for communicating with Ollama's local API, including
/// streaming responses and comprehensive error handling for local model inference.
/// Implements the ILLmProviderRepository interface to support the LLM provider factory pattern.
/// </summary>
public class OllamaRepository(HttpClient httpClient) : ILLmProviderRepository
{
    private static readonly JsonSerializerOptions StreamSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string ProviderName => "ollama";

    public bool CanHandleProvider(string provider)
    {
        return provider.Equals("ollama", StringComparison.OrdinalIgnoreCase);
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(
        LlmConfig llmConfig,
        string message,
        List<OpenAiTool>? tools = null,
        List<ToolCall>? toolCalls = null)
    {
        var endpoint = $"{llmConfig.BaseUrl}/api/chat";
        var requestBody = CreateOllamaRequest(llmConfig, message);

        using var content = CreateHttpContent(requestBody);
        ConfigureHttpClientHeaders(llmConfig);

        HttpResponseMessage? response = null;
        string? errorMessage = null;
        
        try
        {
            response = await httpClient.PostAsync(endpoint, content);
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: Failed to communicate with Ollama: {ex.Message}";
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            yield return errorMessage;
            yield break;
        }

        if (response == null)
        {
            yield return "Error: No response from Ollama";
            yield break;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            yield return $"Error: Ollama API error: {response.StatusCode} - {errorContent}";
            yield break;
        }

        await foreach (var chunk in ProcessOllamaStreamingResponseAsync(response))
            yield return chunk;
    }

    private static object CreateOllamaRequest(LlmConfig llmConfig, string message)
    {
        return new
        {
            model = llmConfig.Model,
            messages = new[]
            {
                new { role = "user", content = message }
            },
            stream = true,
            options = new
            {
                num_predict = llmConfig.MaxTokens ?? 4000,
                temperature = llmConfig.Temperature ?? 0.7,
                top_p = llmConfig.TopP ?? 1.0,
                repeat_penalty = 1.0 + (llmConfig.FrequencyPenalty ?? 0.0),
                top_k = 40
            }
        };
    }

    private static StringContent CreateHttpContent(object requestBody)
    {
        var json = JsonSerializer.Serialize(requestBody, StreamSerializerOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private void ConfigureHttpClientHeaders(LlmConfig llmConfig)
    {
        httpClient.DefaultRequestHeaders.Clear();
        // Ollama doesn't require API keys, so no authorization header needed
    }

    private static async IAsyncEnumerable<string> ProcessOllamaStreamingResponseAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8, true);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var jsonContent = ExtractJsonFromStreamLine(line);
            if (string.IsNullOrEmpty(jsonContent)) continue;

            var content = ParseOllamaStreamingResponse(jsonContent);
            if (!string.IsNullOrEmpty(content))
                yield return content;
        }
    }

    private static string ExtractJsonFromStreamLine(string line)
    {
        if (line.StartsWith("data: "))
        {
            var jsonContent = line[6..];
            return jsonContent.Trim() == "[DONE]" ? string.Empty : jsonContent;
        }

        return line;
    }

    private static string ParseOllamaStreamingResponse(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            return string.Empty;

        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonContent);

            if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement) &&
                messageElement.TryGetProperty("content", out var contentElement))
                return contentElement.GetString() ?? string.Empty;

            if (jsonDoc.RootElement.TryGetProperty("response", out var responseElement))
                return responseElement.GetString() ?? string.Empty;

            return string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }
}

