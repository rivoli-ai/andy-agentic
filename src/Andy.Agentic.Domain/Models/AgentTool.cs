namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Data Transfer Object for AgentTool entities.
///     Represents the association between an agent and a tool with specific configuration.
/// </summary>
public class AgentTool
{
    public bool IsActive { get; set; } = true;

    public Guid? ToolId { get; set; }

    public Guid AgentId { get; set; }

    public Tool? Tool { get; set; } = null!;
}

