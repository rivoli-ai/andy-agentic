using Microsoft.AspNetCore.SignalR;

namespace Andy.Agentic.Infrastructure.Semantic;

/// <summary>
/// SignalR hub for real-time document RAG processing status updates.
/// </summary>
public class DocumentRagHub : Hub
{
    /// <summary>
    /// Join a group for a specific agent to receive RAG processing updates.
    /// </summary>
    /// <param name="agentId">The agent ID to receive updates for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task JoinAgentGroup(string agentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Agent_{agentId}");
    }

    /// <summary>
    /// Leave a group for a specific agent.
    /// </summary>
    /// <param name="agentId">The agent ID to stop receiving updates for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LeaveAgentGroup(string agentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Agent_{agentId}");
    }
}
