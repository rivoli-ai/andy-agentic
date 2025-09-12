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

    Task<LlmRequest> PrepareLlmMessageAsync(Agent agent, Prompt prompt,
        string userMessage, string sessionId, List<ChatHistory> getChatHistoryFunc);

    IAsyncEnumerable<StreamingResult> SendToLlmProviderStreamAsync(
        Agent agent,
        LlmRequest request,
        string session,
        ToolExecutionRecorder toolExecutionRecorder);
}
