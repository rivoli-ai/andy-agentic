using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Andy.Agentic.Application.Services;

/// <summary>
/// Service for handling authentication and user management
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the AuthService class
    /// </summary>
    /// <param name="userRepository">The user repository</param>
    /// <param name="httpContextAccessor">The HTTP context accessor</param>
    public AuthService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get the current authenticated user
    /// </summary>
    /// <returns>The current user DTO or null if not authenticated</returns>
    public async Task<UserDto?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var azureAdId = httpContext.User.FindFirst("oid")?.Value ??
                        httpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        if (string.IsNullOrEmpty(azureAdId))
        {
            return null;
        }

        var user = await _userRepository.GetByAzureAdIdAsync(azureAdId);
        return user != null ? MapToDto(user) : null;
    }

    /// <summary>
    /// Create or update user from Microsoft Entra claims
    /// </summary>
    /// <param name="azureAdId">The Azure AD object ID</param>
    /// <param name="email">The user's email address</param>
    /// <param name="displayName">The user's display name</param>
    /// <param name="firstName">The user's first name (optional)</param>
    /// <param name="lastName">The user's last name (optional)</param>
    /// <returns>The user DTO</returns>
    public async Task<UserDto> CreateOrUpdateUserAsync(string azureAdId, string email, string displayName, string? firstName = null, string? lastName = null)
    {
        var existingUser = await _userRepository.GetByAzureAdIdAsync(azureAdId);

        if (existingUser != null)
        {
            // Update existing user
            existingUser.Email = email;
            existingUser.DisplayName = displayName;
            existingUser.FirstName = firstName;
            existingUser.LastName = lastName;
            existingUser.LastLogin = DateTime.UtcNow;

            await _userRepository.UpdateAsync(existingUser);
            return MapToDto(existingUser);
        }

        // Create new user
        var newUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            AzureAdId = azureAdId,
            Email = email,
            DisplayName = displayName,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(newUser);
        return MapToDto(newUser);
    }

    /// <summary>
    /// Check if a user exists by Azure AD ID
    /// </summary>
    /// <param name="azureAdId">The Azure AD object ID</param>
    /// <returns>True if user exists, false otherwise</returns>
    public async Task<bool> UserExistsAsync(string azureAdId)
    {
        return await _userRepository.ExistsByAzureAdIdAsync(azureAdId);
    }

    /// <summary>
    /// Get user by Azure AD ID
    /// </summary>
    /// <param name="azureAdId">The Azure AD object ID</param>
    /// <returns>The user DTO or null if not found</returns>
    public async Task<UserDto?> GetUserByAzureAdIdAsync(string azureAdId)
    {
        var user = await _userRepository.GetByAzureAdIdAsync(azureAdId);
        return user != null ? MapToDto(user) : null;
    }

    private static UserDto MapToDto(UserEntity user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            JobTitle = user.JobTitle,
            Department = user.Department,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            IsActive = user.IsActive
        };
    }
}
