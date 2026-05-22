using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Interfaces.Llm;

/// <summary>
/// Executes tool calls during a raw thinking-model chat loop.
/// </summary>
public delegate Task<IReadOnlyList<ToolExecutionLog>> ToolCallExecutor(
    IReadOnlyList<ToolCall> toolCalls,
    CancellationToken cancellationToken);
