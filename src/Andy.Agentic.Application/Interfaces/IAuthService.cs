using Andy.Agentic.Application.DTOs;

namespace Andy.Agentic.Application.Interfaces;

/// <summary>
/// Interface for authentication services
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Get the current authenticated user
    /// </summary>
    Task<UserDto?> GetCurrentUserAsync();

    /// <summary>
    /// Create or update user from Microsoft Entra claims
    /// </summary>
    Task<UserDto> CreateOrUpdateUserAsync(string azureAdId, string email, string displayName, string? firstName = null, string? lastName = null);

    /// <summary>
    /// Check if a user exists by Azure AD ID
    /// </summary>
    Task<bool> UserExistsAsync(string azureAdId);

    /// <summary>
    /// Get user by Azure AD ID
    /// </summary>
    Task<UserDto?> GetUserByAzureAdIdAsync(string azureAdId);
}

