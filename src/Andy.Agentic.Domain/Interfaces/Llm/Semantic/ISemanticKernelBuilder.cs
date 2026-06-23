using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Models.Semantic;
using Microsoft.SemanticKernel;

namespace Andy.Agentic.Domain.Interfaces.Llm.Semantic;

public interface ISemanticKernelBuilder
{
    IAsyncEnumerable<StreamingChatMessageContent> CallAgentAsync(KernelResponse kernel);
    IAsyncEnumerable<StreamingChatMessageContent> CallAgentAsync(KernelResponse kernel, CancellationToken cancellationToken);
    KernelResponse BuildKernelAsync(Agent agent,
        string session,
        LlmConfig config,
        LlmRequest request,
        ToolExecutionRecorder toolExecutionRecorder);
}
