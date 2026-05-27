using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Helpers;

/// <summary>
/// Builds the system instruction sent to LLM providers from an agent's prompt templates.
/// Matches Semantic Kernel <c>ChatCompletionAgent</c> behaviour (all prompts joined).
/// </summary>
public static class AgentSystemInstructionBuilder
{
    public static string? Build(Agent agent)
    {
        if (agent.Prompts is not { Count: > 0 })
        {
            return null;
        }

        var parts = agent.Prompts
            .Select(p => p.Content?.Trim())
            .Where(c => !string.IsNullOrEmpty(c))
            .ToList();

        return parts.Count == 0 ? null : string.Join('\n', parts);
    }
}
