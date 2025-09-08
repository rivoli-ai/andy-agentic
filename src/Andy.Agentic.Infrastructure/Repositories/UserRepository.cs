using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Infrastructure.Data;
using Andy.Agentic.Infrastructure.Repositories.Database;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories;

/// <summary>
/// Repository for User entity operations
/// </summary>
public class UserRepository : EfRepository<UserEntity>, IUserRepository
{
    private readonly AndyDbContext _context;

    /// <summary>
    /// Initializes a new instance of the UserRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public UserRepository(AndyDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Get user by Azure AD ID
    /// </summary>
    /// <param name="azureAdId">The Azure AD object ID</param>
    /// <returns>The user entity or null if not found</returns>
    public async Task<UserEntity?> GetByAzureAdIdAsync(string azureAdId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.AzureAdId == azureAdId);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <returns>The user entity or null if not found</returns>
    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Check if user exists by Azure AD ID
    /// </summary>
    /// <param name="azureAdId">The Azure AD object ID</param>
    /// <returns>True if user exists, false otherwise</returns>
    public async Task<bool> ExistsByAzureAdIdAsync(string azureAdId)
    {
        return await _context.Users
            .AnyAsync(u => u.AzureAdId == azureAdId);
    }

    /// <summary>
    /// Check if user exists by email
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <returns>True if user exists, false otherwise</returns>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }
}
