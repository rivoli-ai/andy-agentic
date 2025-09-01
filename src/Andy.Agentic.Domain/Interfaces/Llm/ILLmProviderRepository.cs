using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Interfaces.Llm;

/// <summary>
///     Abstraction for sending messages to and interacting with a specific LLM provider.
/// </summary>
public interface ILLmProviderRepository
{
    /// <summary>
    ///     Gets the provider name (e.g., "openai", "ollama")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    ///     Sends a message to the LLM provider and returns a streaming response
    /// </summary>
    /// <param name="llmConfig">LLM configuration</param>
    /// <param name="message">Message to send</param>
    /// <param name="tools">Optional tools to include</param>
    /// <param name="toolCalls">Optional tool calls to track</param>
    /// <returns>Streaming response from the LLM provider</returns>
    IAsyncEnumerable<string> SendMessageStreamAsync(
        LlmConfig llmConfig,
        string message,
        List<OpenAiTool>? tools = null,
        List<ToolCall>? toolCalls = null);

    /// <summary>
    ///     Checks if this repository can handle the given provider
    /// </summary>
    /// <param name="provider">Provider name to check</param>
    /// <returns>True if this repository can handle the provider</returns>
    bool CanHandleProvider(string provider);
}
