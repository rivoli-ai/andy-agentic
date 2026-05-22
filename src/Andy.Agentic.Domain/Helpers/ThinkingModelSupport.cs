using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Helpers;

/// <summary>
/// Hybrid thinking models (Kimi, Qwen 3.x, etc.) stream reasoning in OpenAI-compatible
/// <c>delta.reasoning_content</c>. Semantic Kernel drops that field, so use raw HTTP streaming.
/// Qwen additionally requires <c>enable_thinking: true</c> on the request.
/// </summary>
public static class ThinkingModelSupport
{
    public static bool IsKimiThinkingModel(string? model) =>
        !string.IsNullOrWhiteSpace(model)
        && model.Contains("kimi", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Moonshot API (api.moonshot.cn) enforces Moonshot-flavored JSON Schema on tool parameters.
    /// </summary>
    public static bool IsMoonshotEndpoint(string? baseUrl) =>
        !string.IsNullOrWhiteSpace(baseUrl)
        && baseUrl.Contains("moonshot", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Tool parameter schemas must be sanitized before sending to Moonshot/Kimi providers.
    /// </summary>
    public static bool RequiresMoonshotToolSchema(string? model, string? baseUrl) =>
        IsKimiThinkingModel(model) || IsMoonshotEndpoint(baseUrl);

    public static bool IsQwenThinkingModel(string? model) =>
        !string.IsNullOrWhiteSpace(model)
        && model.Contains("qwen", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Models known to expose <c>reasoning_content</c> in streaming chat completion deltas.
    /// </summary>
    public static bool SupportsReasoningContentStream(string? model) =>
        IsKimiThinkingModel(model) || IsQwenThinkingModel(model);

    public static bool ShouldUseRawReasoningStream(LlmConfig config, LlmRequest request) =>
        SupportsReasoningContentStream(config.Model);

    /// <summary>
    /// Hybrid thinking models require <c>reasoning_content</c> on every assistant tool-call turn in
    /// request history. Follow-up tool rounds disable streaming thinking via provider overrides.
    /// </summary>
    public static bool MustDisableThinkingForTools(LlmConfig config) =>
        SupportsReasoningContentStream(config.Model);

    /// <summary>
    /// Provider-specific request fields that turn off hybrid thinking for tool-enabled chat paths.
    /// </summary>
    public static IReadOnlyDictionary<string, object>? GetToolPathThinkingDisableOverrides(LlmConfig config)
    {
        if (!MustDisableThinkingForTools(config))
        {
            return null;
        }

        if (IsKimiThinkingModel(config.Model))
        {
            return new Dictionary<string, object>
            {
                ["thinking"] = new Dictionary<string, object> { ["type"] = "disabled" },
            };
        }

        if (IsQwenThinkingModel(config.Model))
        {
            return new Dictionary<string, object> { ["enable_thinking"] = false };
        }

        return null;
    }

    /// <summary>
    /// Qwen hybrid models (3, 3.5, 3.6, …) only emit <c>reasoning_content</c> when thinking is enabled.
    /// </summary>
    public static void ApplyThinkingRequestOptions(LlmConfig config, IDictionary<string, object> request, bool disableThinking = false)
    {
        if (disableThinking)
        {
            var overrides = GetToolPathThinkingDisableOverrides(config);
            if (overrides != null)
            {
                foreach (var (key, value) in overrides)
                {
                    request[key] = value;
                }
            }

            return;
        }

        if (IsQwenThinkingModel(config.Model))
        {
            request["enable_thinking"] = true;
            return;
        }

        if (IsKimiThinkingModel(config.Model))
        {
            request["thinking"] = new Dictionary<string, object> { ["type"] = "enabled" };
        }
    }

    /// <summary>
    /// Kimi hybrid models (e.g. kimi-k2.6) require different sampling params per thinking mode.
    /// Thinking enabled: temperature 1.0. Tool follow-up (thinking disabled): temperature 0.6.
    /// </summary>
    public static bool RequiresKimiHybridSampling(LlmConfig config) =>
        IsKimiThinkingModel(config.Model) && IsMoonshotEndpoint(config.BaseUrl);

    public static double ResolveKimiThinkingRoundTemperature(LlmConfig _) => 1.0;

    public static double ResolveKimiToolRoundTemperature(LlmConfig _) => 0.6;

    public static double ResolveKimiThinkingRoundTopP(LlmConfig config) =>
        config.TopP ?? 0.95;

    public static double ResolveKimiToolRoundTopP(LlmConfig config) =>
        config.TopP ?? 0.95;

    /// <summary>
    /// Uses the LLM config temperature when set. For Moonshot/Kimi hybrid models, temperature
    /// is chosen per request mode — see <see cref="ApplySamplingParameters"/>.
    /// </summary>
    public static bool TryResolveTemperature(LlmConfig config, out double temperature)
    {
        if (config.Temperature.HasValue)
        {
            temperature = config.Temperature.Value;
            return true;
        }

        temperature = 0.7;
        return false;
    }

    /// <summary>
    /// Uses the LLM config top_p when set; otherwise a sensible default for non-Moonshot providers.
    /// </summary>
    public static bool TryResolveTopP(LlmConfig config, out double topP)
    {
        if (config.TopP.HasValue)
        {
            topP = config.TopP.Value;
            return true;
        }

        topP = 1.0;
        return false;
    }

    /// <summary>
    /// Returns true when the conversation already contains an assistant tool-call turn
    /// (follow-up request after tool execution).
    /// </summary>
    public static bool ConversationHasToolCallHistory(IEnumerable<IDictionary<string, object>> messages) =>
        messages.Any(message =>
            message.TryGetValue("role", out var role)
            && string.Equals(role?.ToString(), "assistant", StringComparison.OrdinalIgnoreCase)
            && message.ContainsKey("tool_calls"));

    /// <summary>
    /// Applies sampling parameters to a chat-completions request body from LLM config.
    /// Kimi/Moonshot hybrid models use mode-specific temperature (1.0 thinking, 0.6 tool rounds).
    /// </summary>
    public static void ApplySamplingParameters(
        LlmConfig config,
        IDictionary<string, object> request,
        bool disableThinking = false)
    {
        if (RequiresKimiHybridSampling(config))
        {
            request["temperature"] = disableThinking
                ? ResolveKimiToolRoundTemperature(config)
                : ResolveKimiThinkingRoundTemperature(config);
            request["top_p"] = disableThinking
                ? ResolveKimiToolRoundTopP(config)
                : ResolveKimiThinkingRoundTopP(config);
            return;
        }

        var isMoonshot = IsKimiThinkingModel(config.Model) || IsMoonshotEndpoint(config.BaseUrl);

        if (TryResolveTemperature(config, out var temperature))
        {
            request["temperature"] = temperature;
        }
        else if (!isMoonshot)
        {
            request["temperature"] = temperature;
        }

        if (TryResolveTopP(config, out var topP))
        {
            request["top_p"] = topP;
        }
        else if (!isMoonshot)
        {
            request["top_p"] = topP;
        }

        if (isMoonshot)
        {
            return;
        }

        request["frequency_penalty"] = config.FrequencyPenalty ?? 0.0;
        request["presence_penalty"] = config.PresencePenalty ?? 0.0;
    }
}
