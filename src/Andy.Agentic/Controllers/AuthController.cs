using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andy.Agentic.Controllers;

/// <summary>
/// Controller for authentication endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthDto>> GetCurrentUser()
    {
        var user = await _authService.GetCurrentUserAsync();
        
        if (user == null)
            return Unauthorized();

        return Ok(new AuthDto
        {
            IsAuthenticated = true,
            User = user
        });
    }

    /// <summary>
    /// Create or update user from Microsoft Entra claims
    /// </summary>
    [HttpPost("sync")]
    [Authorize]
    public async Task<ActionResult<AuthDto>> SyncUser()
    {
        Console.WriteLine($"AuthController: SyncUser called. User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"AuthController: User.Identity.Name: {User.Identity?.Name}");
        
        if (!User.Identity?.IsAuthenticated == true)
        {
            Console.WriteLine("AuthController: User not authenticated");
            return Unauthorized();
        }

        // Log all claims for debugging
        Console.WriteLine("AuthController: Available claims:");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"  {claim.Type}: {claim.Value}");
        }

        var azureAdId = User.FindFirst("oid")?.Value ??
                        User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        var email = User.FindFirst("preferred_username")?.Value ??
                    User.FindFirst("email")?.Value ??
                    User.FindFirst("upn")?.Value;
        var displayName = User.FindFirst("name")?.Value ?? "Unknown User";
        var firstName = User.FindFirst("given_name")?.Value;
        var lastName = User.FindFirst("family_name")?.Value;

        Console.WriteLine($"AuthController: Extracted claims - AzureAdId: {azureAdId}, Email: {email}, DisplayName: {displayName}");

        if (string.IsNullOrEmpty(azureAdId) || string.IsNullOrEmpty(email))
        {
            Console.WriteLine("AuthController: Required claims not found");
            return BadRequest("Required claims not found");
        }

        var user = await _authService.CreateOrUpdateUserAsync(azureAdId, email, displayName, firstName, lastName);
        Console.WriteLine($"AuthController: User created/updated: {user?.Id}");

        return Ok(new AuthDto
        {
            IsAuthenticated = true,
            User = user
        });
    }

    /// <summary>
    /// Check authentication status
    /// </summary>
    [HttpGet("status")]
    public ActionResult<AuthDto> GetAuthStatus()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        
        return Ok(new AuthDto
        {
            IsAuthenticated = isAuthenticated
        });
    }
}
