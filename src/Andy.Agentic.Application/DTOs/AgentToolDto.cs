namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for AgentTool entities.
///     Represents the association between an agent and a tool with specific configuration.
/// </summary>
public class AgentToolDto
{
    public bool IsActive { get; set; } = true;

    public Guid? ToolId { get; set; }

    public Guid AgentId { get; set; }

    public ToolDto? Tool { get; set; } = null!;
}

