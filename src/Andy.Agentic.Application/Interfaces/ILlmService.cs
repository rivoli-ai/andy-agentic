using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.Interfaces;

public interface ILlmService
{
    Task<IEnumerable<LlmConfig>> GetAllLlmConfigsAsync();
    Task<LlmConfig?> GetLlmConfigByIdAsync(Guid id);
    Task<LlmConfig> CreateLlmConfigAsync(LlmConfig createLlmConfigDto);
    Task<LlmConfig> UpdateLlmConfigAsync(LlmConfig updateLlmConfigDto);
    Task<bool> DeleteLlmConfigAsync(Guid id);
    IEnumerable<LlmProvider> GetProvidersAsync();
    LlmProvider? GetProviderByIdAsync(string id);
    Task<TestConnectionResult> TestConnectionAsync(TestConnection testConnectionDto);

    Task<(string Message, List<OpenAiTool> Tools)> PrepareLlmMessageAsync(Agent agent, Prompt prompt,
        string userMessage, string sessionId, IList<ChatHistory> getChatHistoryFunc);

    string BuildConversationContext(IList<ChatHistory> recentMessages);

    IAsyncEnumerable<StreamingResult> SendToLlmProviderStreamAsync(LlmConfig llmConfig, string message,
        List<OpenAiTool>? tools = null, List<ToolCall>? toolCalls = null);
}
