using Andy.Agentic.Domain.Helpers;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Tests;

public class ThinkingModelSupportTests
{
    [Fact]
    public void GetToolPathThinkingDisableOverrides_Kimi_ReturnsDisabledThinking()
    {
        var config = new LlmConfig { Model = "kimi-k2.6" };

        var overrides = ThinkingModelSupport.GetToolPathThinkingDisableOverrides(config);

        Assert.NotNull(overrides);
        Assert.True(overrides.ContainsKey("thinking"));
        var thinking = Assert.IsType<Dictionary<string, object>>(overrides["thinking"]);
        Assert.Equal("disabled", thinking["type"]);
    }

    [Fact]
    public void GetToolPathThinkingDisableOverrides_Qwen_ReturnsEnableThinkingFalse()
    {
        var config = new LlmConfig { Model = "qwen3.5-plus" };

        var overrides = ThinkingModelSupport.GetToolPathThinkingDisableOverrides(config);

        Assert.NotNull(overrides);
        Assert.False(Assert.IsType<bool>(overrides["enable_thinking"]));
    }

    [Fact]
    public void ShouldUseRawReasoningStream_WithTools_ReturnsTrue()
    {
        var config = new LlmConfig { Model = "kimi-k2.6" };
        var request = new LlmRequest { Tools = [new Tool()] };

        Assert.True(ThinkingModelSupport.ShouldUseRawReasoningStream(config, request));
    }

    [Fact]
    public void ApplySamplingParameters_KimiHybridThinkingRound_AlwaysUsesTemperature1()
    {
        var config = new LlmConfig
        {
            Model = "kimi-k2.6",
            BaseUrl = "https://api.moonshot.ai/v1",
            Temperature = 0.6,
        };
        var request = new Dictionary<string, object>();

        ThinkingModelSupport.ApplySamplingParameters(config, request, disableThinking: false);

        Assert.Equal(1.0, request["temperature"]);
        Assert.Equal(0.95, request["top_p"]);
    }

    [Fact]
    public void ApplySamplingParameters_KimiHybridToolRound_UsesTemperature06()
    {
        var config = new LlmConfig
        {
            Model = "kimi-k2.6",
            BaseUrl = "https://api.moonshot.ai/v1",
            Temperature = 1.0,
        };
        var request = new Dictionary<string, object>();

        ThinkingModelSupport.ApplySamplingParameters(config, request, disableThinking: true);

        Assert.Equal(0.6, request["temperature"]);
    }

    [Fact]
    public void ApplySamplingParameters_KimiHybridDefaults_WhenTemperatureUnset()
    {
        var config = new LlmConfig
        {
            Model = "kimi-k2.6",
            BaseUrl = "https://api.moonshot.ai/v1",
        };
        var thinkingRequest = new Dictionary<string, object>();
        var toolRequest = new Dictionary<string, object>();

        ThinkingModelSupport.ApplySamplingParameters(config, thinkingRequest, disableThinking: false);
        ThinkingModelSupport.ApplySamplingParameters(config, toolRequest, disableThinking: true);

        Assert.Equal(1.0, thinkingRequest["temperature"]);
        Assert.Equal(0.6, toolRequest["temperature"]);
    }

    [Fact]
    public void ConversationHasToolCallHistory_DetectsAssistantToolCalls()
    {
        var messages = new List<Dictionary<string, object>>
        {
            new()
            {
                ["role"] = "assistant",
                ["content"] = string.Empty,
                ["tool_calls"] = Array.Empty<object>(),
            },
        };

        Assert.True(ThinkingModelSupport.ConversationHasToolCallHistory(messages));
    }

    [Fact]
    public void ApplyThinkingRequestOptions_WhenDisableThinking_SkipsEnableThinkingForQwen()
    {
        var config = new LlmConfig { Model = "qwen3.5-plus" };
        var request = new Dictionary<string, object>();

        ThinkingModelSupport.ApplyThinkingRequestOptions(config, request, disableThinking: true);

        Assert.False(Assert.IsType<bool>(request["enable_thinking"]));
    }
}
