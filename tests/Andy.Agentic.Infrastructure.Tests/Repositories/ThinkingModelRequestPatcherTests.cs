using System.Text.Json;
using Andy.Agentic.Infrastructure.Repositories.Llm;
using FluentAssertions;

namespace Andy.Agentic.Infrastructure.Tests.Repositories;

public class ThinkingModelRequestPatcherTests
{
    [Fact]
    public void PatchChatCompletionsRequest_KimiFirstRequestWithTools_KeepsThinkingEnabled()
    {
        const string input = """
            {
              "model": "kimi-k2.6",
              "messages": [{"role": "user", "content": "hi"}],
              "tools": [{"type": "function", "function": {"name": "search"}}]
            }
            """;

        var patched = ThinkingModelRequestPatcher.PatchChatCompletionsRequest(input);
        using var expected = JsonDocument.Parse(input);
        using var actual = JsonDocument.Parse(patched);

        JsonSerializer.Serialize(expected.RootElement).Should().Be(JsonSerializer.Serialize(actual.RootElement));
    }

    [Fact]
    public void PatchChatCompletionsRequest_KimiWithToolCalls_AddsReasoningContentAndDisablesThinking()
    {
        const string input = """
            {
              "model": "kimi-k2.6",
              "messages": [
                {"role": "user", "content": "hi"},
                {"role": "assistant", "tool_calls": [{"id": "call_1", "type": "function", "function": {"name": "search", "arguments": "{}"}}]},
                {"role": "tool", "tool_call_id": "call_1", "content": "ok"}
              ],
              "tools": [{"type": "function", "function": {"name": "search"}}]
            }
            """;

        var patched = ThinkingModelRequestPatcher.PatchChatCompletionsRequest(input);
        using var doc = JsonDocument.Parse(patched);

        doc.RootElement.GetProperty("thinking").GetProperty("type").GetString().Should().Be("disabled");
        doc.RootElement.GetProperty("messages")[1].GetProperty("reasoning_content").GetString().Should().BeEmpty();
    }

    [Fact]
    public void PatchChatCompletionsRequest_KimiWithScalarToolEnum_ConvertsEnumToArray()
    {
        const string input = """
            {
              "model": "kimi-k2.6",
              "messages": [{"role": "user", "content": "hi"}],
              "tools": [{
                "type": "function",
                "function": {
                  "name": "resolve_library",
                  "parameters": {
                    "type": "object",
                    "properties": {
                      "libraryName": {
                        "type": "string",
                        "enum": "/microsoft/dotnet"
                      }
                    }
                  }
                }
              }]
            }
            """;

        var patched = ThinkingModelRequestPatcher.PatchChatCompletionsRequest(input);
        using var doc = JsonDocument.Parse(patched);

        var enumElement = doc.RootElement
            .GetProperty("tools")[0]
            .GetProperty("function")
            .GetProperty("parameters")
            .GetProperty("properties")
            .GetProperty("libraryName")
            .GetProperty("enum");

        Assert.Equal(JsonValueKind.Array, enumElement.ValueKind);
        Assert.Equal("/microsoft/dotnet", enumElement[0].GetString());
    }

    [Fact]
    public void PatchChatCompletionsRequest_ScalarToolEnum_AlwaysSanitizedEvenWithoutThinkingModel()
    {
        const string input = """
            {
              "model": "gpt-4o",
              "messages": [{"role": "user", "content": "hi"}],
              "tools": [{
                "type": "function",
                "function": {
                  "name": "resolve_library",
                  "parameters": {
                    "type": "object",
                    "properties": {
                      "libraryName": {
                        "type": "string",
                        "enum": "/microsoft/dotnet"
                      }
                    }
                  }
                }
              }]
            }
            """;

        var patched = ThinkingModelRequestPatcher.PatchChatCompletionsRequest(input);
        using var doc = JsonDocument.Parse(patched);

        var enumElement = doc.RootElement
            .GetProperty("tools")[0]
            .GetProperty("function")
            .GetProperty("parameters")
            .GetProperty("properties")
            .GetProperty("libraryName")
            .GetProperty("enum");

        Assert.Equal(JsonValueKind.Array, enumElement.ValueKind);
        Assert.Equal("/microsoft/dotnet", enumElement[0].GetString());
    }

    [Fact]
    public void PatchChatCompletionsRequest_NonThinkingModelWithoutTools_LeavesPayloadUnchanged()
    {
        const string input = """{"model":"gpt-4o","messages":[{"role":"user","content":"hi"}]}""";

        var patched = ThinkingModelRequestPatcher.PatchChatCompletionsRequest(input);

        patched.Should().Be(input);
    }
}
