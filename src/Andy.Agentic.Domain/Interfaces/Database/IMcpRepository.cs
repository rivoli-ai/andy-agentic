using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Provides operations for synchronizing MCP server associations for an agent.
/// </summary>
public interface IMcpRepository
{
    /// <summary>
    ///     Updates the set of MCP servers linked to an existing agent.
    /// </summary>
    /// <param name="existingAgent">Agent to update.</param>
    /// <param name="mcpServers">New collection of servers to apply.</param>
    Task UpdateMcpServersAsync(AgentEntity existingAgent, List<AgentMcpServerEntity> mcpServers);
}
