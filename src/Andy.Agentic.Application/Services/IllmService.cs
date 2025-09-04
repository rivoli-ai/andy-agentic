using System.Text;
using System.Text.Json;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Interfaces.Llm;
using Andy.Agentic.Domain.Models;
using AutoMapper;

namespace Andy.Agentic.Application.Services;

/// <summary>
///     Service for managing Large Language Model (LLM) configurations, providers, and interactions.
///     Handles CRUD operations for LLM configs, provider information, and message processing.
/// </summary>
public class LlmService(ILlmRepository llmRepository, ILlmProviderFactory providerFactory, IMapper mapper) : ILlmService
{
    /// <summary>
    ///     Retrieves all LLM configurations from the repository.
    /// </summary>
    /// <returns>A collection of all LLM configuration s.</returns>
    public async Task<IEnumerable<LlmConfig>> GetAllLlmConfigsAsync()
    {
        var configs = await llmRepository.GetAllAsync();

        return mapper.Map<IEnumerable<LlmConfig>>(configs);
    }

    /// <summary>
    ///     Retrieves a specific LLM configuration by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the LLM configuration.</param>
    /// <returns>The LLM configuration  if found; otherwise, null.</returns>
    public async Task<LlmConfig?> GetLlmConfigByIdAsync(Guid id)
    {
        var config = await llmRepository.GetByIdAsync(id);

        return config != null ? mapper.Map<LlmConfig>(config) : null;
    }

    /// <summary>
    ///     Creates a new LLM configuration in the repository.
    /// </summary>
    /// <param name="createLlmConfig">The LLM configuration data for creation.</param>
    /// <returns>The created LLM configuration  with generated ID and timestamps.</returns>
    public async Task<LlmConfig> CreateLlmConfigAsync(LlmConfig createLlmConfig)
    {
        var config = mapper.Map<LlmConfigEntity>(createLlmConfig);
        config.Id = Guid.NewGuid();
        config.CreatedAt = DateTime.UtcNow;
        config.UpdatedAt = DateTime.UtcNow;

        await llmRepository.CreateAsync(config);

        return mapper.Map<LlmConfig>(config);
    }

    /// <summary>
    ///     Updates an existing LLM configuration in the repository.
    /// </summary>
    /// <param name="id">The unique identifier of the LLM configuration to update.</param>
    /// <param name="updateLlmConfig">The updated LLM configuration data.</param>
    /// <returns>The updated LLM configuration .</returns>
    /// <exception cref="ArgumentException">Thrown when the LLM configuration is not found.</exception>
    public async Task<LlmConfig> UpdateLlmConfigAsync(LlmConfig updateLlmConfig)
    {
        var config = await llmRepository.GetByIdAsync(updateLlmConfig.Id);
        if (config == null)
        {
            throw new ArgumentException("LLM Config not found");
        }

        mapper.Map(updateLlmConfig, config);
        config.UpdatedAt = DateTime.UtcNow;

        await llmRepository.UpdateAsync(config);

        return mapper.Map<LlmConfig>(config);
    }

    /// <summary>
    ///     Deletes an LLM configuration from the repository.
    /// </summary>
    /// <param name="id">The unique identifier of the LLM configuration to delete.</param>
    /// <returns>True if the configuration was successfully deleted; false if not found.</returns>
    public async Task<bool> DeleteLlmConfigAsync(Guid id)
    {
        var config = await llmRepository.GetByIdAsync(id);
        if (config == null)
        {
            return false;
        }

        await llmRepository.DeleteAsync(id);
        return true;
    }

    /// <summary>
    ///     Retrieves a list of available LLM providers with their supported models and capabilities.
    /// </summary>
    /// <returns>A collection of LLM provider s including OpenAI, Anthropic, Google, Custom, and Ollama.</returns>
    public IEnumerable<LlmProvider> GetProvidersAsync()
    {
        var providers = new List<LlmProvider>
        {
            new()
            {
                Id = "openai",
                Name = "OpenAI",
                BaseUrl = "https://api.openai.com/v1",
                Models = new List<string> { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo", "gpt-3.5-turbo-16k" },
                IsOpenAiCompatible = true
            },
            new()
            {
                Id = "anthropic",
                Name = "Anthropic",
                BaseUrl = "https://api.anthropic.com",
                Models = new List<string> { "claude-3-opus", "claude-3-sonnet", "claude-3-haiku" },
                IsOpenAiCompatible = false
            },
            new()
            {
                Id = "google",
                Name = "Google",
                BaseUrl = "https://generativelanguage.googleapis.com",
                Models = new List<string> { "gemini-pro", "gemini-pro-vision" },
                IsOpenAiCompatible = false
            },
            new()
            {
                Id = "custom",
                Name = "Custom LLM",
                BaseUrl = "",
                Models = new List<string>(),
                IsOpenAiCompatible = true
            },
            new()
            {
                Id = "ollama",
                Name = "Ollama (Local)",
                BaseUrl = "http://localhost:11434",
                Models = new List<string>
                {
                    "llama2",
                    "mistral",
                    "codellama",
                    "llama2:13b",
                    "llama2:70b",
                    "mistral:7b",
                    "codellama:7b",
                    "codellama:13b",
                    "codellama:34b"
                },
                IsOpenAiCompatible = false
            }
        };

        return providers;
    }

    /// <summary>
    ///     Retrieves a specific LLM provider by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the LLM provider.</param>
    /// <returns>The LLM provider  if found; otherwise, null.</returns>
    public LlmProvider? GetProviderByIdAsync(string id)
    {
        var providers = GetProvidersAsync();
        return providers.FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    ///     Prepares a message for LLM processing by combining prompt, conversation context, and user input.
    /// </summary>
    /// <param name="agent">The agent configuration containing available tools.</param>
    /// <param name="prompt">The system prompt template for the agent.</param>
    /// <param name="userMessage">The user's input message.</param>
    /// <param name="sessionId">The chat session identifier for logging.</param>
    /// <param name="recentMessages">Recent conversation history for context building.</param>
    /// <returns>A tuple containing the prepared message and available tools for the LLM.</returns>
    public async Task<(string Message, List<OpenAiTool> Tools)> PrepareLlmMessageAsync(
        Agent agent,
        Prompt prompt,
        string userMessage,
        string sessionId,
        IList<ChatHistory> recentMessages)
    {
        var tools = BuilolsFromAgent(agent);

        var conversationContext = BuildConversationContext(recentMessages);

        Console.WriteLine($"Session ID: {sessionId}");
        Console.WriteLine($"Chat History Count: {recentMessages.Count}");
        Console.WriteLine($"Conversation Context Length: {conversationContext.Length}");

        var fullMessage = $"{prompt.Content}\n\n{conversationContext}\n\nUser: {userMessage}";
        return (fullMessage, tools);
    }

    /// <summary>
    ///     Sends a message to an LLM provider and returns a streaming response.
    /// </summary>
    /// <param name="llmConfig">The LLM configuration containing provider and model details.</param>
    /// <param name="message">The message to send to the LLM.</param>
    /// <param name="tools">Optional list of tools available to the LLM.</param>
    /// <param name="toolCalls">Optional list of previous tool calls for context.</param>
    /// <returns>An async enumerable of response chunks from the LLM provider.</returns>
    public async IAsyncEnumerable<string> SenLlmProviderStreamAsync(
        LlmConfig llmConfig,
        string message,
        List<OpenAiTool>? tools = null,
        List<ToolCall>? toolCalls = null)
    {
        var provider = providerFactory.GetProvider(llmConfig.Provider);

        var config = mapper.Map<LlmConfig>(llmConfig);

        await foreach (var chunk in provider.SendMessageStreamAsync(config, message, tools, toolCalls))
        {
            yield return chunk;
        }
    }

    /// <summary>
    ///     Builds a conversation context string from recent chat history for LLM context.
    /// </summary>
    /// <param name="recentMessages">The list of recent chat messages to include in context.</param>
    /// <returns>A formatted string containing conversation context with truncated message content.</returns>
    public string BuildConversationContext(IList<ChatHistory> recentMessages)
    {
        if (!recentMessages.Any())
        {
            return string.Empty;
        }

        var contextLines = new List<string> { "Previous conversation context:" };

        foreach (var message in recentMessages)
        {
            var role = message.Role == "user" ? "User" : "Assistant";
            var content = message.Content.Length > 200
                ? message.Content.Substring(0, 200) + "..."
                : message.Content;

            contextLines.Add($"{role}: {content}");
        }

        contextLines.Add("---");
        return string.Join("\n", contextLines);
    }

    /// <summary>
    ///     Sends a message to an LLM provider and returns a streaming response.
    /// </summary>
    /// <param name="llmConfig">The LLM configuration containing provider and model details.</param>
    /// <param name="message">The message to send to the LLM.</param>
    /// <param name="tools">Optional list of tools available to the LLM.</param>
    /// <param name="toolCalls">Optional list of previous tool calls for context.</param>
    /// <returns>An async enumerable of response chunks from the LLM provider.</returns>
    public async IAsyncEnumerable<StreamingResult> SendToLlmProviderStreamAsync(
        LlmConfig llmConfig,
        string? message,
        List<OpenAiTool>? tools = null,
        List<ToolCall>? toolCalls = null)
    {
        var provider = providerFactory.GetProvider(llmConfig.Provider);

        var config = mapper.Map<LlmConfig>(llmConfig);

      
        await foreach (var chunk in provider.StreamChatWithTools(config, message, tools))
        {
            yield return chunk!;
        }
    }

    /// <summary>
    ///     Tests the connectivity to an LLM provider using the provided configuration.
    ///     Supports OpenAI-compatible APIs, Ollama, and general endpoint testing.
    /// </summary>
    /// <param name="testConnection">The connection test parameters including provider, API key, and endpoint.</param>
    /// <returns>A test result containing success status, message, and latency information.</returns>
    public async Task<TestConnectionResult> TestConnectionAsync(TestConnection testConnection)
    {
        try
        {
            var startTime = DateTime.UtcNow;

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            if (testConnection.Provider == "openai" || testConnection.Provider == "custom")
            {
                var requestBody = new
                {
                    model = testConnection.Model,
                    messages = new[] { new { role = "user", content = "Hello" } },
                    max_tokens = 10
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(testConnection.ApiKey))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {testConnection.ApiKey}");
                }

                var response = await httpClient.PostAsync($"{testConnection.BaseUrl}/chat/completions", content);
                var endTime = DateTime.UtcNow;
                var latency = (int)(endTime - startTime).TotalMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    return new TestConnectionResult
                    {
                        Success = true, Message = "Connection successful", Latency = latency
                    };
                }

                return new TestConnectionResult
                {
                    Success = false,
                    Message = $"Connection failed with status: {response.StatusCode}",
                    Latency = latency
                };
            }

            if (testConnection.Provider == "ollama")
            {
                var requestBody = new
                {
                    model = testConnection.Model,
                    messages = new[] { new { role = "user", content = "Hello" } },
                    stream = true
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{testConnection.BaseUrl}/api/chat", content);
                var endTime = DateTime.UtcNow;
                var latency = (int)(endTime - startTime).TotalMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    return new TestConnectionResult
                    {
                        Success = true, Message = "Connection successful", Latency = latency
                    };
                }

                return new TestConnectionResult
                {
                    Success = false,
                    Message = $"Connection failed with status: {response.StatusCode}",
                    Latency = latency
                };
            }
            else
            {
                // For non-OpenAI compatible APIs, just test if the endpoint is reachable
                var response = await httpClient.GetAsync(testConnection.BaseUrl);
                var endTime = DateTime.UtcNow;
                var latency = (int)(endTime - startTime).TotalMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    return new TestConnectionResult
                    {
                        Success = true, Message = "Endpoint is reachable", Latency = latency
                    };
                }

                return new TestConnectionResult
                {
                    Success = false,
                    Message = $"Endpoint test failed with status: {response.StatusCode}",
                    Latency = latency
                };
            }
        }
        catch (Exception ex)
        {
            return new TestConnectionResult { Success = false, Message = $"Connection test failed: {ex.Message}" };
        }
    }

    /// <summary>
    ///     Builds a list of OpenAI-compatible tools from an agent's available tools.
    /// </summary>
    /// <param name="agent">The agent containing tool configurations.</param>
    /// <returns>A list of OpenAI tool definitions for the LLM.</returns>
    public List<OpenAiTool> BuilolsFromAgent(Agent agent)
    {
        var availableTools = agent.Tools
            .Where(at => at.Tool.IsActive)
            .Select(at => at)
            .ToList();

        return availableTools.Select(CreateOpenAiTool).OfType<OpenAiTool>().ToList();
    }

    /// <summary>
    ///     Creates an OpenAI-compatible tool definition from an agent tool configuration.
    /// </summary>
    /// <param name="tool">The agent tool configuration to convert.</param>
    /// <returns>An OpenAI tool definition if successful; otherwise, null.</returns>
    public OpenAiTool? CreateOpenAiTool(AgentTool tool)
    {
        try
        {
            var toolFunction = new OpenAiTool
            {
                Type = "function",
                Function = new Function
                {
                    Name = tool?.Tool?.Name,
                    Description = tool?.Tool?.Description,
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, FunctionProperty>(),
                        Required = Array.Empty<string>()
                    }
                }
            };

            ParseAndAdolParameters(tool, toolFunction);
            return toolFunction;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Parses tool parameters from JSON configuration and adds them to an OpenAI tool definition.
    /// </summary>
    /// <param name="tool">The agent tool containing parameter configuration.</param>
    /// <param name="toolFunction">The OpenAI tool to populate with parameters.</param>
    public void ParseAndAdolParameters(AgentTool tool, OpenAiTool toolFunction)
    {
        if (string.IsNullOrEmpty(tool.Tool.Parameters))
            return;

        try
        {
            var array = JsonSerializer.Deserialize<JsonElement[]>(tool.Tool.Parameters);
            if (array == null)
                return;

            var requiredParams = new List<string>();

            foreach (var param in array)
            {
                var paramName = param.GetProperty("name").GetString();
                var paramType = param.GetProperty("type").GetString();
                var paramDescription = param.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "";
                var paramRequired = param.TryGetProperty("required", out var req) && req.GetBoolean();

                if (paramName == null || paramType == null)
                {
                    continue;
                }

                toolFunction.Function.Parameters.Properties[paramName] = new FunctionProperty
                {
                    Type = MapParameterType(paramType),
                    Description = paramDescription
                };

                if (paramRequired)
                {
                    requiredParams.Add(paramName);
                }
            }

            toolFunction.Function.Parameters.Required = requiredParams.ToArray();
        }
        catch
        {
            // Parameter parsing failed, continue with basic tool definition
        }
    }

    /// <summary>
    ///     Maps tool parameter types to OpenAI-compatible JSON schema types.
    /// </summary>
    /// <param name="toolParameterType">The tool parameter type to map.</param>
    /// <returns>The corresponding OpenAI JSON schema type.</returns>
    public string MapParameterType(string toolParameterType) =>
        toolParameterType.ToLowerInvariant() switch
        {
            "string" => "string",
            "number" => "number",
            "integer" => "integer",
            "boolean" => "boolean",
            "array" => "array",
            "object" => "object",
            _ => "string"
        };
}
