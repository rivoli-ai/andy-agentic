using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.Interfaces;

public interface IAgentService
{
    Task<IEnumerable<Agent>> GetAllAgentsAsync();
    Task<Agent?> GetAgentByIdAsync(Guid id);
    Task<Agent> CreateAgentAsync(Agent createAgentDto);
    Task<Agent> UpdateAgentAsync(Agent updateAgentDto);
    Task<bool> DeleteAgentAsync(Guid id);
    Task<IEnumerable<Agent>> SearchAgentsAsync(string searchTerm);
    Task<IEnumerable<Agent>> GetAgentsByTypeAsync(string type);
    Task<IEnumerable<Agent>> GetAgentsByTagAsync(string tag);
    
    // New methods for public/private visibility
    Task<IEnumerable<Agent>> GetVisibleAgentsAsync(Guid userId);
    Task<Agent?> GetVisibleAgentByIdAsync(Guid id, Guid userId);
}
