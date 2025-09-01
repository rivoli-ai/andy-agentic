using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Andy.Agentic.Domain.Interfaces.Llm;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Repository implementation for OpenAI LLM provider integration.
/// Provides functionality for communicating with OpenAI's chat completion API, including
/// streaming responses, tool integration, and comprehensive error handling.
/// Implements the ILLmProviderRepository interface to support the LLM provider factory pattern.
/// </summary>
public class OpenAiRepository : ILLmProviderRepository
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions StreamSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OpenAiRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string ProviderName => "openai";

    public bool CanHandleProvider(string provider)
    {
        return provider.Equals("openai", StringComparison.OrdinalIgnoreCase);
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(
        LlmConfig llmConfig,
        string message,
        List<OpenAiTool>? tools = null,
        List<ToolCall>? toolCalls = null)
    {
        var endpoint = $"{llmConfig.BaseUrl}/chat/completions";
        var requestBody = CreateOpenAiRequest(llmConfig, message, tools);

        using var content = CreateHttpContent(requestBody);
        ConfigureHttpClientHeaders(llmConfig);

        HttpResponseMessage? response = null;
        string? errorMessage = null;
        
        try
        {
            response = await _httpClient.PostAsync(endpoint, content);
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: Failed to communicate with OpenAI: {ex.Message}";
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            yield return errorMessage;
            yield break;
        }

        if (response == null)
        {
            yield return "Error: No response from OpenAI";
            yield break;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            yield return $"Error: OpenAI API error: {response.StatusCode} - {errorContent}";
            yield break;
        }

        await foreach (var chunk in ProcessOpenAiStreamingResponseAsync(response, toolCalls))
            yield return chunk;
    }

    private static object CreateOpenAiRequest(LlmConfig llmConfig, string message, List<OpenAiTool>? tools)
    {
        var baseRequest = new Dictionary<string, object>
        {
            ["model"] = llmConfig.Model,
            ["messages"] = new[] { new { role = "user", content = message } },
            ["stream"] = true,
            ["max_tokens"] = llmConfig.MaxTokens ?? 4000,
            ["temperature"] = llmConfig.Temperature ?? 0.7,
            ["top_p"] = llmConfig.TopP ?? 1.0,
            ["frequency_penalty"] = llmConfig.FrequencyPenalty ?? 0.0,
            ["presence_penalty"] = llmConfig.PresencePenalty ?? 0.0
        };

        if (tools?.Any() == true)
        {
            baseRequest["tools"] = tools;
            baseRequest["tool_choice"] = "auto";
        }

        return baseRequest;
    }

    private static StringContent CreateHttpContent(object requestBody)
    {
        var json = JsonSerializer.Serialize(requestBody, StreamSerializerOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private void ConfigureHttpClientHeaders(LlmConfig llmConfig)
    {
        _httpClient.DefaultRequestHeaders.Clear();

        if (!string.IsNullOrEmpty(llmConfig.ApiKey))
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {llmConfig.ApiKey}");
    }

    private static async IAsyncEnumerable<string> ProcessOpenAiStreamingResponseAsync(
        HttpResponseMessage response,
        List<ToolCall>? toolCalls)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8, true);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var jsonContent = ExtractJsonFromStreamLine(line);
            if (string.IsNullOrEmpty(jsonContent)) continue;

            var (parsedChunk, hasToolCalls, chunkToolCalls) = ParseOpenAiStreamingResponse(jsonContent);

            if (hasToolCalls && chunkToolCalls.Any()) 
                toolCalls?.AddRange(chunkToolCalls);

            if (!string.IsNullOrEmpty(parsedChunk)) 
                yield return parsedChunk;
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

    private static (string Content, bool HasToolCalls, List<ToolCall> ToolCalls) ParseOpenAiStreamingResponse(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            return (string.Empty, false, new List<ToolCall>());

        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonContent);

            if (!jsonDoc.RootElement.TryGetProperty("choices", out var choicesElement) ||
                choicesElement.GetArrayLength() == 0)
                return (string.Empty, false, new List<ToolCall>());

            var firstChoice = choicesElement.EnumerateArray().First();

            if (!firstChoice.TryGetProperty("delta", out var deltaElement))
                return (string.Empty, false, new List<ToolCall>());

            // Check for tool calls first (priority)
            if (deltaElement.TryGetProperty("tool_calls", out var toolCallsElement) &&
                toolCallsElement.ValueKind != JsonValueKind.Null)
            {
                var toolCalls = ParseToolCallsFromChunk(toolCallsElement);
                return (string.Empty, true, toolCalls);
            }

            // Check for content
            if (deltaElement.TryGetProperty("content", out var deltaContentElement))
                return (deltaContentElement.GetString() ?? string.Empty, false, new List<ToolCall>());

            return (string.Empty, false, new List<ToolCall>());
        }
        catch (JsonException)
        {
            return (jsonContent, false, new List<ToolCall>());
        }
    }

    private static List<ToolCall> ParseToolCallsFromChunk(JsonElement toolCallsElement)
    {
        var toolCalls = new List<ToolCall>();

        try
        {
            foreach (var toolCallElement in toolCallsElement.EnumerateArray())
            {
                var toolCall = new ToolCall
                {
                    Id = toolCallElement.GetProperty("id").GetString() ?? "",
                    Type = toolCallElement.GetProperty("type").GetString() ?? "function",
                    Function = new ToolCallFunction
                    {
                        Name = toolCallElement.GetProperty("function").GetProperty("name").GetString() ?? "",
                        Arguments = toolCallElement.GetProperty("function").GetProperty("arguments").GetString() ?? "{}"
                    }
                };
                toolCalls.Add(toolCall);
            }
        }
        catch
        {
            // If parsing fails, return empty list
        }

        return toolCalls;
    }
}

