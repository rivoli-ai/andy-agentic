namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a summarized chat history entry linking a user message and an agent response.
/// </summary>
public class ChatHistoryEntity
{
    /// <summary>
    ///     Unique identifier for the chat history record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Identifier of the related agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    ///     The user's message text.
    /// </summary>
    public string UserMessage { get; set; } = string.Empty;

    /// <summary>
    ///     The agent's response text.
    /// </summary>
    public string AgentResponse { get; set; } = string.Empty;

    /// <summary>
    ///     Timestamp of when the interaction occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     Indicates whether the interaction was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     Number of tokens used for the interaction.
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    ///     Response time in seconds.
    /// </summary>
    public double ResponseTime { get; set; }

    /// <summary>
    ///     Navigation property to the related agent.
    /// </summary>
    public AgentEntity Agent { get; set; } = null!;
}
