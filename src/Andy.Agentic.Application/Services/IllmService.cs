using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Helpers;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Interfaces.Llm;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Domain.Models;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Andy.Agentic.Application.Services;

/// <summary>
///     Service for managing Large Language Model (LLM) configurations, providers, and interactions.
///     Handles CRUD operations for LLM configs, provider information, and message processing.
/// </summary>
public class LlmService(
    ILlmRepository llmRepository,
    ILlmProviderFactory providerFactory,
    IMapper mapper,
    ISemanticKernelBuilder semenSemanticKernelBuilder,
    IToolExecutionService toolExecutionService,
    ISkillRegistryClient skillRegistryClient,
    ILogger<LlmService> logger) : ILlmService
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
        config.CreatedByUser = null;
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
        config.CreatedByUser = null;
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
                Id = LLMProviderType.OpenAi.ToString().ToLowerInvariant(),
                Name = "OpenAI",
                BaseUrl = "https://api.openai.com/v1",
                Models = new List<string> { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo", "gpt-3.5-turbo-16k" },
                IsOpenAiCompatible = true
            },
            new()
            {
                Id = LLMProviderType.Anthropic.ToString().ToLowerInvariant(),
                Name = "Anthropic",
                BaseUrl = "https://api.anthropic.com",
                Models = new List<string> { "claude-3-opus", "claude-3-sonnet", "claude-3-haiku" },
                IsOpenAiCompatible = false
            },
            new()
            {
                Id = LLMProviderType.Google.ToString().ToLowerInvariant(),
                Name = "Google",
                BaseUrl = "https://generativelanguage.googleapis.com",
                Models = new List<string> { "gemini-pro", "gemini-pro-vision" },
                IsOpenAiCompatible = false
            },
            new()
            {
                Id = LLMProviderType.Custom.ToString().ToLowerInvariant(),
                Name = "Custom LLM",
                BaseUrl = "",
                Models = new List<string>(),
                IsOpenAiCompatible = true
            },
            new()
            {
                Id = LLMProviderType.Ollama.ToString().ToLowerInvariant(),
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
            },
            new()
            {
                Id = LLMProviderType.AzureOpenAi.ToString().ToLowerInvariant(),
                Name = "Azure OpenAI",
                BaseUrl = "https://your-resource.openai.azure.com/",
                Models = new List<string> { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" },
                IsOpenAiCompatible = true
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
    public async Task<LlmRequest> PrepareLlmMessageAsync(
        Agent agent,
        Prompt prompt,
        string userMessage,
        string sessionId,
        List<ChatHistory> recentMessages,
        List<ChatImage>? images = null)
    {
        var allTools = BuilolsFromAgent(agent);

        var tools = agent.Tools.Select(at => at.Tool).ToList();

        logger.LogInformation(
            "PrepareLlmMessage: sessionId={SessionId}, historyCount={HistoryCount}, allTools={AllToolsCount}, agentTools={AgentToolsCount}, images={ImageCount}",
            sessionId,
            recentMessages.Count,
            allTools.Count,
            tools.Count,
            images?.Count ?? 0);

        // Exclude empty assistant/user text from history — Kimi and other providers reject them (HTTP 400).
        var messages = recentMessages
            .Where(m =>
                (m.Role == "user" && (!string.IsNullOrWhiteSpace(m.Content) || m.Images?.Any() == true))
                || (m.Role == "assistant" && !string.IsNullOrWhiteSpace(m.Content)))
            .ToList();

        var skippedEmpty = recentMessages.Count - messages.Count;
        if (skippedEmpty > 0)
        {
            logger.LogWarning(
                "PrepareLlmMessage: skipped {SkippedCount} empty history message(s) for sessionId={SessionId}",
                skippedEmpty,
                sessionId);
        }

        // User message was already persisted before streaming; avoid duplicating it in the LLM payload.
        var lastHistory = messages.LastOrDefault();
        var userAlreadyInHistory = lastHistory?.Role == "user"
            && string.Equals(lastHistory.Content, userMessage, StringComparison.Ordinal)
            && (images == null || !images.Any());

        if (images != null && images.Any())
        {
            if (!userAlreadyInHistory)
            {
                messages.Add(new ChatHistory
                {
                    Content = userMessage,
                    Role = "user",
                    AgentId = agent.Id,
                    SessionId = sessionId,
                    Timestamp = DateTime.UtcNow,
                    Images = images
                });
            }
        }
        else if (!string.IsNullOrEmpty(userMessage) && !userAlreadyInHistory)
        {
            messages.Add(new ChatHistory
            {
                Content = userMessage,
                Role = "user",
                AgentId = agent.Id,
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow
            });
        }

        return new LlmRequest { Messages = messages, Tools = tools!, Images = images };
    }

    /// <summary>
    ///     Sends a message to an LLM provider and returns a streaming response.
    /// </summary>
    /// <param name="llmConfig">The LLM configuration containing provider and model details.</param>
    /// <param name="message">The message to send to the LLM.</param>
    /// <param name="tools">Optional list of tools available to the LLM.</param>
    /// <param name="toolCalls">Optional list of previous tool calls for context.</param>
    /// <returns>An async enumerable of response chunks from the LLM provider.</returns>
    public async IAsyncEnumerable<StreamingResult> SendToLlmProviderStreamAsync(Agent agent,
        LlmRequest request,
        string session,
        ToolExecutionRecorder toolExecutionRecorder,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "SendToLlmProviderStream starting: agentId={AgentId}, sessionId={SessionId}, model={Model}, baseUrl={BaseUrl}, temperature={Temperature}, topP={TopP}",
            agent.Id,
            session,
            agent.LlmConfig.Model,
            agent.LlmConfig.BaseUrl,
            agent.LlmConfig.Temperature,
            agent.LlmConfig.TopP);

        var chunkCount = 0;
        var contentChunkCount = 0;
        var thinkingChunkCount = 0;

        if (ThinkingModelSupport.ShouldUseRawReasoningStream(agent.LlmConfig, request))
        {
            logger.LogInformation(
                "Using raw HTTP streaming for thinking model {Model} (sessionId={SessionId}, tools={ToolCount})",
                agent.LlmConfig.Model,
                session,
                request.Tools?.Count ?? 0);

            var provider = ResolveOpenAiCompatibleRepository(agent.LlmConfig);
            var openAiTools = BuilolsFromAgent(agent);
            var systemInstruction = AgentSystemInstructionBuilder.Build(agent);

            // Progressive disclosure for thinking models: only the skills' name+description go in the
            // prompt; the model loads full instructions and bundled files on demand via load_skill /
            // read_skill_file (advertised as tools, executed in the callback below).
            var skills = agent.Skills.Where(s => s.IsActive && s.Registry != null).ToList();
            if (skills.Count > 0)
            {
                openAiTools.AddRange(SkillToolCalling.BuildToolSchemas());

                var catalog = SkillToolCalling.BuildCatalogPrompt(skills);
                if (!string.IsNullOrEmpty(catalog))
                {
                    systemInstruction = string.IsNullOrWhiteSpace(systemInstruction)
                        ? catalog
                        : $"{systemInstruction}\n\n{catalog}";
                }
            }

            var loadedSkillLabels = new List<string>();

            await foreach (var chunk in provider.StreamThinkingChatAsync(
                               agent.LlmConfig,
                               request.Messages,
                               openAiTools.Count > 0 ? openAiTools : null,
                               async (toolCalls, ct) =>
                               {
                                   var results = new List<ToolExecutionLog>(toolCalls.Count);
                                   var regular = new List<ToolCall>();
                                   var slots = new int[toolCalls.Count];

                                   // Split skill calls (handled here, on demand) from regular tool calls.
                                   for (var i = 0; i < toolCalls.Count; i++)
                                   {
                                       results.Add(null!);
                                       if (SkillToolCalling.IsSkillTool(toolCalls[i].Function.Name))
                                       {
                                           var (log, label) = await SkillToolCalling.ExecuteAsync(
                                               toolCalls[i], skills, skillRegistryClient, agent.Id, session, ct);
                                           results[i] = log;
                                           toolExecutionRecorder.Add(log);
                                           if (label != null && !loadedSkillLabels.Contains(label))
                                           {
                                               loadedSkillLabels.Add(label);
                                           }
                                       }
                                       else
                                       {
                                           slots[regular.Count] = i;
                                           regular.Add(toolCalls[i]);
                                       }
                                   }

                                   if (regular.Count > 0)
                                   {
                                       var logs = (await toolExecutionService.ExecuteToolCallsAsync(regular, agent, session)).ToList();
                                       for (var j = 0; j < regular.Count; j++)
                                       {
                                           var log = j < logs.Count ? logs[j] : new ToolExecutionLog
                                           {
                                               ToolName = regular[j].Function.Name,
                                               Success = false,
                                               Result = "Tool execution failed",
                                           };
                                           results[slots[j]] = log;
                                           toolExecutionRecorder.Add(log);
                                       }
                                   }

                                   return results;
                               },
                               systemInstruction,
                               cancellationToken))
            {
                chunkCount++;

                if (!string.IsNullOrEmpty(chunk.Thinking))
                {
                    thinkingChunkCount++;
                    yield return new StreamingResult { Thinking = chunk.Thinking };
                    continue;
                }

                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    contentChunkCount++;
                    yield return new StreamingResult { Content = chunk.Content };
                }
            }

            // Skills actually used = the ones the model loaded via load_skill (accurate by construction).
            if (loadedSkillLabels.Count > 0)
            {
                yield return new StreamingResult { SkillsUsed = loadedSkillLabels };
            }

            logger.LogInformation(
                "SendToLlmProviderStream completed (raw reasoning): sessionId={SessionId}, totalChunks={TotalChunks}, contentChunks={ContentChunks}, thinkingChunks={ThinkingChunks}, vllmStyleQwen={VllmStyleQwen}",
                session,
                chunkCount,
                contentChunkCount,
                thinkingChunkCount,
                ThinkingModelSupport.UsesVllmStyleQwenThinkingRequest(agent.LlmConfig));
            yield break;
        }

        var kernel = semenSemanticKernelBuilder.BuildKernelAsync(agent, session, agent.LlmConfig, request, toolExecutionRecorder);
        var isThinking = false;

        await foreach (var chunk in semenSemanticKernelBuilder.CallAgentAsync(kernel, cancellationToken))
        {
            chunkCount++;
            var content = chunk.Content ?? string.Empty;

            if (content.Contains("\u003cthink\u003e") || content.Contains("<|begin_of_box|>"))
            {
                isThinking = true;
                logger.LogDebug("SendToLlmProviderStream chunk #{ChunkIndex}: entering thinking mode", chunkCount);
                continue;
            }

            if (isThinking)
            {
                if (content.Contains("\u003c/think\u003e") || content.Contains("<|end_of_box|>"))
                {
                    isThinking = false;
                    logger.LogDebug("SendToLlmProviderStream chunk #{ChunkIndex}: exiting thinking mode", chunkCount);
                    continue;
                }

                thinkingChunkCount++;
                yield return new StreamingResult { Thinking = content };
            }
            else
            {
                contentChunkCount++;
                if (contentChunkCount == 1)
                {
                    logger.LogInformation(
                        "SendToLlmProviderStream first content chunk #{ChunkIndex}, length={Length}",
                        chunkCount,
                        content.Length);
                }

                yield return new StreamingResult { Content = content };
            }
        }

        logger.LogInformation(
            "SendToLlmProviderStream completed: sessionId={SessionId}, totalSkChunks={TotalChunks}, contentChunks={ContentChunks}, thinkingChunks={ThinkingChunks}",
            session,
            chunkCount,
            contentChunkCount,
            thinkingChunkCount);
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

            if (testConnection.Provider == LLMProviderType.OpenAi ||
                testConnection.Provider == LLMProviderType.Custom ||
                testConnection.Provider == LLMProviderType.AzureOpenAi)
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
                        Success = true,
                        Message = "Connection successful",
                        Latency = latency
                    };
                }

                return new TestConnectionResult
                {
                    Success = false,
                    Message = $"Connection failed with status: {response.StatusCode}",
                    Latency = latency
                };
            }

            if (testConnection.Provider == LLMProviderType.Ollama)
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
                        Success = true,
                        Message = "Connection successful",
                        Latency = latency
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
                        Success = true,
                        Message = "Endpoint is reachable",
                        Latency = latency
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
    ///     Filters out tools that were executed and succeeded in the last message of chat history.
    /// </summary>
    /// <param name="allTools">All available tools from the agent.</param>
    /// <param name="recentMessages">Recent conversation history.</param>
    /// <returns>Filtered list of tools excluding those executed and succeeded in the last message.</returns>
    private List<OpenAiTool> FilterToolsBasedOnLastExecution(List<OpenAiTool> allTools, List<ChatHistory> recentMessages)
    {
        // If no messages or no tools, return all tools
        if (!recentMessages.Any() || !allTools.Any())
        {
            return allTools;
        }

        // Get the last message from chat history
        var lastMessage = recentMessages.LastOrDefault();
        if (lastMessage == null)
        {
            return allTools;
        }

        // Get tool names that were executed and succeeded in the last message
        var executedAndSucceededTools = new HashSet<string>();

        // Check ToolResults array (new format)
        if (lastMessage.ToolResults != null && lastMessage.ToolResults.Any())
        {
            foreach (var toolResult in lastMessage.ToolResults)
            {
                if (toolResult.Success && !string.IsNullOrEmpty(toolResult.ToolName))
                {
                    executedAndSucceededTools.Add(toolResult.ToolName);
                }
            }
        }
        // Note: Legacy single tool execution format is not supported for filtering
        // as ChatHistory doesn't have a Success property

        // If no tools were executed and succeeded, return all tools
        if (!executedAndSucceededTools.Any())
        {
            return allTools;
        }

        // Filter out tools that were executed and succeeded
        var filteredTools = allTools.Where(tool =>
            tool.Function != null &&
            !executedAndSucceededTools.Contains(tool.Function.Name)
        ).ToList();

        Console.WriteLine($"Tools executed and succeeded in last message: {string.Join(", ", executedAndSucceededTools)}");
        Console.WriteLine($"Filtered out {allTools.Count - filteredTools.Count} tools");

        return filteredTools;
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
                    Parameters = OpenAiToolSchemaBuilder.BuildMoonshotSafeParameters(tool?.Tool?.Parameters),
                },
            };

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
        {
            return;
        }

        try
        {
            var array = JsonSerializer.Deserialize<JsonElement[]>(tool.Tool.Parameters);
            if (array == null)
            {
                return;
            }

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

    private ILLmProviderRepository ResolveOpenAiCompatibleRepository(LlmConfig config) =>
        config.Provider == LLMProviderType.Ollama
            ? providerFactory.GetProvider("ollama")
            : providerFactory.GetProvider("openai");
}
