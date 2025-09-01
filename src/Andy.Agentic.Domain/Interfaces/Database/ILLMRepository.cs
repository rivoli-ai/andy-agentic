using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Provides data access operations for <see cref="LlmConfigEntity" />.
/// </summary>
public interface ILlmRepository : IBaseRepository<LlmConfigEntity>
{
    /// <summary>
    ///     Retrieves all LLM configurations.
    /// </summary>
    Task<IEnumerable<LlmConfigEntity>> GetAllAsync();

    /// <summary>
    ///     Retrieves an LLM configuration by identifier.
    /// </summary>
    Task<LlmConfigEntity?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Creates a new LLM configuration.
    /// </summary>
    Task<LlmConfigEntity> CreateAsync(LlmConfigEntity config);

    /// <summary>
    ///     Updates an existing LLM configuration.
    /// </summary>
    Task<LlmConfigEntity> UpdateAsync(LlmConfigEntity config);

    /// <summary>
    ///     Deletes an LLM configuration by identifier.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}
