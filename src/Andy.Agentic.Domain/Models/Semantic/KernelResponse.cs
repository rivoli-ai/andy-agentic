using Microsoft.SemanticKernel;

namespace Andy.Agentic.Domain.Models.Semantic;

public record KernelResponse(Kernel Kernel, Microsoft.SemanticKernel.ChatCompletion.ChatHistory ChatHistory, Agent? Agent = null);
