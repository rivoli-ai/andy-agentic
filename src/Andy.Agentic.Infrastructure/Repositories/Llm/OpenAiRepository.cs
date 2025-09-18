using Andy.Agentic.Domain.Interfaces.Llm;
using Andy.Agentic.Domain.Models;
using ModelContextProtocol.Protocol;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Repository implementation for OpenAI LLM provider integration.
/// Provides functionality for communicating with OpenAI's chat completion API, including
/// streaming responses, tool integration, and comprehensive error handling.
/// Implements the ILLmProviderRepository interface to support the LLM provider factory pattern.
/// </summary>
public class OpenAiRepository : ILLmProviderRepository
{
    #region Constants

    private const string ProviderNameValue = "openai";
    private const string UserRole = "user";
    private const string ToolChoiceAuto = "auto";
    private const string DataPrefix = "data: ";
    private const string DoneMarker = "[DONE]";
    private const string FunctionType = "function";
    private const string ContentType = "application/json";
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";
    private const string ChatCompletionsEndpoint = "/chat/completions";
    private const string ChoicesProperty = "choices";
    private const string DeltaProperty = "delta";
    private const string ContentProperty = "content";
    private const string ToolCallsProperty = "tool_calls";
    private const string FunctionProperty = "function";
    private const string IdProperty = "id";
    private const string TypeProperty = "type";
    private const string NameProperty = "name";
    private const string ArgumentsProperty = "arguments";

    #endregion

    #region Private Fields

    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions StreamSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the OpenAiRepository class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for making API requests.</param>
    public OpenAiRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the name of the LLM provider this repository handles.
    /// </summary>
    public string ProviderName => ProviderNameValue;

    #endregion

    #region Public Methods

    /// <summary>
    /// Determines whether this repository can handle the specified provider.
    /// </summary>
    /// <param name="provider">The provider name to check.</param>
    /// <returns>True if this repository can handle the provider; otherwise, false.</returns>
    public bool CanHandleProvider(string provider) => provider.Equals(ProviderNameValue, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Streams chat completions with tool support from the OpenAI API using the official SDK.
    /// </summary>
    /// <param name="config">The LLM configuration containing API settings.</param>
    /// <param name="message">The user message to send to the LLM.</param>
    /// <param name="tools">The list of tools available for the LLM to use.</param>
    /// <returns>An async enumerable of streaming results containing content and tool calls.</returns>
    public async IAsyncEnumerable<StreamingResult> StreamChatWithTools(LlmConfig config,
        List<ChatHistory> message,
        List<OpenAiTool> tools)
    {
        var client = CreateClient(config);
        var chatClient = client.GetChatClient(config.Model);
        var options = CreateChatOptions(config, tools);


        var messages =  message

            .Where(x=> !string.IsNullOrEmpty(x.Content) || x.ToolResults.Any())
            .OrderBy(x=>x.Timestamp)
            .Select<ChatHistory, OpenAI.Chat.ChatMessage>(x =>
        {
            if(x.Role =="user")
            {
                return new UserChatMessage(x.Content );
            }

            return new AssistantChatMessage($"{(!string.IsNullOrEmpty(x.Content) ? $"Message : {x.Content}": string.Empty )  }\n  {string.Join('\n', x.ToolResults.Select(res => $"tool Executed : {res.ToolName} with result : {(res.Success ? res.Result?.ToString() : res.ErrorMessage)}" )) }");
        }).ToList();

        var streamingResult = chatClient.CompleteChatStreamingAsync(messages, options);

        var assistantMessage = "";

        var toolCallsByIndex = new Dictionary<int, AggregatedToolCall>();

        await foreach (var update in streamingResult)
        {
            foreach (var contentPart in update.ContentUpdate)
            {
                assistantMessage += contentPart.Text;
                yield return new StreamingResult { Content = contentPart.Text };
            }

            foreach (var toolCallUpdate in update.ToolCallUpdates)
            {
                if (!toolCallsByIndex.TryGetValue(toolCallUpdate.Index, out var agg))
                {
                    agg = new AggregatedToolCall { Index = toolCallUpdate.Index };
                    toolCallsByIndex[toolCallUpdate.Index] = agg;
                }

                if (!string.IsNullOrEmpty(toolCallUpdate.ToolCallId))
                {
                    agg.Id = toolCallUpdate.ToolCallId;
                }

                if (!string.IsNullOrEmpty(toolCallUpdate.FunctionName))
                {
                    agg.Name = toolCallUpdate.FunctionName;
                }

                var argsDelta = toolCallUpdate.FunctionArgumentsUpdate?.ToString();
                if (!string.IsNullOrEmpty(argsDelta))
                {
                    agg.ArgumentsBuffer.Append(argsDelta);
                }
            }
        }

        if (toolCallsByIndex.Count <= 0)
        {
            yield break;
        }

        var toolCallRequests = toolCallsByIndex
            .OrderBy(kv => kv.Key)
            .Select(kv => new ToolCall
            {
                Id = kv.Value.Id ?? string.Empty,
                Function = new ToolCallFunction
                {
                    Name = kv.Value.Name ?? string.Empty, Arguments = kv.Value.Arguments
                }
            })
            .ToList();

        yield return new StreamingResult
        {
            ToolCalls = toolCallRequests,
            AssistantMessage = assistantMessage,
            Messages = messages.Select(m => m.Content?.ToString()).ToList()
        };
    }

    /// <summary>
    /// Sends a message to the OpenAI API and streams the response using HTTP streaming.
    /// </summary>
    /// <param name="llmConfig">The LLM configuration containing API settings.</param>
    /// <param name="message">The message to send to the LLM.</param>
    /// <param name="tools">Optional tools to include in the request.</param>
    /// <param name="toolCalls">Optional tool calls to track during streaming.</param>
    /// <returns>An async enumerable of response chunks as they are received.</returns>
    public async IAsyncEnumerable<string> SendMessageStreamAsync(
        LlmConfig llmConfig,
        string message,
        List<OpenAiTool>? tools = null,
        List<ToolCall>? toolCalls = null)
    {
        var endpoint = $"{llmConfig.BaseUrl}{ChatCompletionsEndpoint}";
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
        {
            yield return chunk;
        }
    }

    #endregion

    #region Private Helper Methods - Client Creation

    /// <summary>
    /// Creates an OpenAI client with the specified configuration.
    /// </summary>
    /// <param name="config">The LLM configuration containing API settings.</param>
    /// <returns>An initialized OpenAI client.</returns>
    private OpenAIClient CreateClient(LlmConfig config)
    {
        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(config.BaseUrl)
        };

        return new OpenAIClient(new ApiKeyCredential(config.ApiKey), clientOptions);
    }

    

    /// <summary>
    /// Creates chat completion options with the specified configuration and tools.
    /// </summary>
    /// <param name="config">The LLM configuration containing API settings.</param>
    /// <param name="tools">The list of tools to include in the chat completion.</param>
    /// <returns>Configured chat completion options.</returns>
    private ChatCompletionOptions CreateChatOptions(LlmConfig config, List<OpenAiTool> tools)
    {
        var options = new ChatCompletionOptions
        {
            Temperature = (float?)config.Temperature,
            TopP = (float?)config.TopP,
            FrequencyPenalty = (float?)config.FrequencyPenalty,
            PresencePenalty = (float?)config.PresencePenalty
        };

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        foreach (var tool in tools)
        {
            var chatTool = ChatTool.CreateFunctionTool(
                tool.Function.Name,
                tool.Function.Description,
                    BinaryData.FromObjectAsJson(tool.Function.Parameters, jsonOptions)
            );

            options.Tools.Add(chatTool);
        }

        return options;
    }

    #endregion

    #region Private Helper Methods - HTTP Request Creation

    /// <summary>
    /// Creates an OpenAI API request object with the specified configuration and message.
    /// </summary>
    /// <param name="llmConfig">The LLM configuration containing API settings.</param>
    /// <param name="message">The message to send to the LLM.</param>
    /// <param name="tools">Optional tools to include in the request.</param>
    /// <returns>A dictionary representing the OpenAI API request.</returns>
    private static object CreateOpenAiRequest(LlmConfig llmConfig, string message, List<OpenAiTool>? tools)
    {
        var baseRequest = new Dictionary<string, object>
        {
            ["model"] = llmConfig.Model,
            ["messages"] = new[] { new { role = UserRole, content = message } },
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
            baseRequest["tool_choice"] = ToolChoiceAuto;
        }

        return baseRequest;
    }

    /// <summary>
    /// Creates HTTP content from the request object.
    /// </summary>
    /// <param name="requestBody">The request object to serialize.</param>
    /// <returns>StringContent with JSON serialized request body.</returns>
    private static StringContent CreateHttpContent(object requestBody)
    {
        var json = JsonSerializer.Serialize(requestBody, StreamSerializerOptions);
        return new StringContent(json, Encoding.UTF8, ContentType);
    }

    /// <summary>
    /// Configures HTTP client headers with the API key.
    /// </summary>
    /// <param name="llmConfig">The LLM configuration containing the API key.</param>
    private void ConfigureHttpClientHeaders(LlmConfig llmConfig)
    {
        _httpClient.DefaultRequestHeaders.Clear();

        if (!string.IsNullOrEmpty(llmConfig.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add(AuthorizationHeader, $"{BearerPrefix}{llmConfig.ApiKey}");
        }
    }

    #endregion

    #region Private Helper Methods - Stream Processing

    /// <summary>
    /// Processes the streaming response from OpenAI API and yields content chunks.
    /// </summary>
    /// <param name="response">The HTTP response from the OpenAI API.</param>
    /// <param name="toolCalls">Optional list to collect tool calls from the response.</param>
    /// <returns>An async enumerable of content chunks.</returns>
    private static async IAsyncEnumerable<string> ProcessOpenAiStreamingResponseAsync(
        HttpResponseMessage response,
        List<ToolCall>? toolCalls)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8, true);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var jsonContent = ExtractJsonFromStreamLine(line);
            if (string.IsNullOrEmpty(jsonContent))
            {
                continue;
            }

            var (parsedChunk, hasToolCalls, chunkToolCalls) = ParseOpenAiStreamingResponse(jsonContent);

            if (hasToolCalls && chunkToolCalls.Any())
            {
                toolCalls?.AddRange(chunkToolCalls);
            }

            if (!string.IsNullOrEmpty(parsedChunk))
            {
                yield return parsedChunk;
            }
        }
    }

    /// <summary>
    /// Extracts JSON content from a streaming line, handling the data prefix and done marker.
    /// </summary>
    /// <param name="line">The line from the stream.</param>
    /// <returns>Extracted JSON content or empty string if done or invalid.</returns>
    private static string ExtractJsonFromStreamLine(string line)
    {
        if (line.StartsWith(DataPrefix))
        {
            var jsonContent = line[DataPrefix.Length..];
            return jsonContent.Trim() == DoneMarker ? string.Empty : jsonContent;
        }

        return line;
    }

    /// <summary>
    /// Parses a streaming response chunk from OpenAI API.
    /// </summary>
    /// <param name="jsonContent">The JSON content to parse.</param>
    /// <returns>A tuple containing content, tool calls flag, and parsed tool calls.</returns>
    private static (string Content, bool HasToolCalls, List<ToolCall> ToolCalls) ParseOpenAiStreamingResponse(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return (string.Empty, false, new List<ToolCall>());
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonContent);

            if (!jsonDoc.RootElement.TryGetProperty(ChoicesProperty, out var choicesElement) ||
                choicesElement.GetArrayLength() == 0)
            {
                return (string.Empty, false, new List<ToolCall>());
            }

            var firstChoice = choicesElement.EnumerateArray().First();

            if (!firstChoice.TryGetProperty(DeltaProperty, out var deltaElement))
            {
                return (string.Empty, false, new List<ToolCall>());
            }

            // Check for tool calls first (priority)
            if (deltaElement.TryGetProperty(ToolCallsProperty, out var toolCallsElement) &&
                toolCallsElement.ValueKind != JsonValueKind.Null)
            {
                var toolCalls = ParseToolCallsFromChunk(toolCallsElement);
                return (string.Empty, true, toolCalls);
            }

            // Check for content
            if (deltaElement.TryGetProperty(ContentProperty, out var deltaContentElement))
            {
                return (deltaContentElement.GetString() ?? string.Empty, false, new List<ToolCall>());
            }

            return (string.Empty, false, new List<ToolCall>());
        }
        catch (JsonException)
        {
            return (jsonContent, false, new List<ToolCall>());
        }
    }

    /// <summary>
    /// Parses tool calls from a JSON element containing tool call data.
    /// </summary>
    /// <param name="toolCallsElement">The JSON element containing tool calls.</param>
    /// <returns>A list of parsed tool calls.</returns>
    private static List<ToolCall> ParseToolCallsFromChunk(JsonElement toolCallsElement)
    {
        var toolCalls = new List<ToolCall>();

        try
        {
            foreach (var toolCallElement in toolCallsElement.EnumerateArray())
            {
                var toolCall = new ToolCall
                {
                    Id = toolCallElement.GetProperty(IdProperty).GetString() ?? string.Empty,
                    Type = toolCallElement.GetProperty(TypeProperty).GetString() ?? FunctionType,
                    Function = new ToolCallFunction
                    {
                        Name = toolCallElement.GetProperty(FunctionProperty).GetProperty(NameProperty).GetString() ?? string.Empty,
                        Arguments = toolCallElement.GetProperty(FunctionProperty).GetProperty(ArgumentsProperty).GetString() ?? "{}"
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

    #endregion
}

