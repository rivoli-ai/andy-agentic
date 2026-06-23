using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Provides operations for maintaining prompts associated with an agent.
/// </summary>
public interface IPromptRepository
{
    /// <summary>
    ///     Replaces the prompts associated with an agent with the provided collection.
    /// </summary>
    /// <param name="existingAgent">Agent to update.</param>
    /// <param name="prompts">New prompts collection.</param>
    Task UpdatePromptsAsync(AgentEntity existingAgent, List<PromptEntity> prompts);
}
