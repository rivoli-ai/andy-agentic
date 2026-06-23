using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces.Database;

namespace Andy.Agentic.Domain.Interfaces;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository : IBaseRepository<UserEntity>
{
    /// <summary>
    /// Get user by Azure AD ID
    /// </summary>
    Task<UserEntity?> GetByAzureAdIdAsync(string azureAdId);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<UserEntity?> GetByEmailAsync(string email);

    /// <summary>
    /// Check if user exists by Azure AD ID
    /// </summary>
    Task<bool> ExistsByAzureAdIdAsync(string azureAdId);

    /// <summary>
    /// Check if user exists by email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);
}




