using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database;

/// <summary>
/// Repository implementation for managing MCP (Model Context Protocol) server entities in the database.
/// Provides functionality for updating MCP server collections associated with agents, including
/// creation of new servers, updates to existing ones, and removal of unused servers.
/// Implements synchronization logic to maintain consistency between agent MCP server collections.
/// </summary>
public class McpRepository(AndyDbContext context) : IMcpRepository
{
    public async Task UpdateMcpServersAsync(AgentEntity agent, List<AgentMcpServerEntity> mcpServers)
    {
        try
        {
            var existingServers = agent.McpServers.ToDictionary(s => s.Name, s => s);
            var updatedServers = new List<AgentMcpServerEntity>();

            foreach (var serverDto in mcpServers)
            {
                if (existingServers.TryGetValue(serverDto.Name, out var existingServer))
                {
                    // Update existing server
                    existingServer.IsActive = serverDto.IsActive;
                    existingServer.Capabilities = serverDto.Capabilities;
                    updatedServers.Add(existingServer);

                    // Remove from dictionary so remaining are those to delete
                    existingServers.Remove(serverDto.Name);
                }
                else
                {
                    // Create new server
                    var newServer = new AgentMcpServerEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = serverDto.Name,
                        IsActive = serverDto.IsActive,
                        Capabilities = serverDto.Capabilities,
                        AgentId = agent.Id
                    };
                    context.AgentMcpServers.Add(newServer);
                    updatedServers.Add(newServer);
                }
            }

            // Remove servers not in the update list
            foreach (var removedServer in existingServers.Values)
                context.AgentMcpServers.Remove(removedServer);

            // Replace the agent's server collection
            agent.McpServers.Clear();
            foreach (var server in updatedServers)
                agent.McpServers.Add(server);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            try
            {
                await context.Entry(agent).ReloadAsync();
                throw new InvalidOperationException(
                    "The MCP server data was modified by another operation. Please refresh and try again.", ex);
            }
            catch
            {
                throw new InvalidOperationException(
                    "The MCP server data was modified by another operation. Please refresh and try again.", ex);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update MCP servers: {ex.Message}", ex);
        }
    }
}
