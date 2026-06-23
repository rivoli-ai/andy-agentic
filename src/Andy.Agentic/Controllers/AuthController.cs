using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Andy.Agentic.Application.Auth;
using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Infrastructure.Auth;
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
    private readonly AuthProviderRegistry _providerRegistry;
    private readonly GatewayAuthenticationService _gatewayAuthenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        AuthProviderRegistry providerRegistry,
        GatewayAuthenticationService gatewayAuthenticationService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _providerRegistry = providerRegistry;
        _gatewayAuthenticationService = gatewayAuthenticationService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("config")]
    public IActionResult GetAuthConfig()
    {
        var options = _providerRegistry.Options;
        var providers = new List<object>();

        foreach (var (name, config) in options.GetEnabledProviders())
        {
            var type = config.Type ?? "FrontendOidc";
            if (type.Equals("FrontendOidc", StringComparison.OrdinalIgnoreCase))
            {
                var frontendClientId = !string.IsNullOrEmpty(config.SpaClientId) ? config.SpaClientId : config.ClientId;
                providers.Add(new
                {
                    name,
                    type = "FrontendOidc",
                    authority = config.Authority,
                    clientId = frontendClientId,
                    scopes = config.Scopes,
                    tenantId = config.TenantId
                });
            }
        }

        return Ok(new { providers });
    }

    public sealed record TokenRequest(string? IdToken, string? AccessToken);

    public sealed record AuthTokenResponse(string Token, object User);

    [AllowAnonymous]
    [HttpPost("{provider}/token")]
    public async Task<IActionResult> OidcTokenLogin(string provider, [FromBody] TokenRequest request, CancellationToken cancellationToken)
    {
        if (!_providerRegistry.TryGetProvider(provider, out var authProvider) || authProvider == null)
            return NotFound(new { message = $"Provider '{provider}' is not enabled" });

        var tokenToValidate = !string.IsNullOrWhiteSpace(request.IdToken) ? request.IdToken : request.AccessToken;
        if (string.IsNullOrWhiteSpace(tokenToValidate))
            return BadRequest(new { message = "ID token or access token is required" });

        try
        {
            var principal = await authProvider.ValidateTokenAsync(tokenToValidate!, cancellationToken);

            var sub = principal.FindFirst("oid")?.Value
                      ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? Guid.NewGuid().ToString("N");

            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                        ?? principal.FindFirst("preferred_username")?.Value
                        ?? principal.FindFirst("upn")?.Value
                        ?? principal.FindFirst("name")?.Value
                        ?? sub;

            var name = principal.FindFirst("name")?.Value ?? email;
            var firstName = principal.FindFirst("given_name")?.Value;
            var lastName = principal.FindFirst("family_name")?.Value;

            var roles = principal.Claims
                .Where(c => c.Type is "roles" or "role" or ClaimTypes.Role)
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "[Auth] {Provider} principal validated. sub={Sub} email={Email} roles=[{Roles}]",
                    provider, sub, email, string.Join(",", roles));
            }

            await _authService.CreateOrUpdateUserAsync(sub, email, name, firstName, lastName);

            var appJwt = _gatewayAuthenticationService.GenerateToken(sub, email, name, roles);

            return Ok(new AuthTokenResponse(appJwt, new { email, name, roles }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {Provider} OIDC token login", provider);
            return BadRequest(new { message = ex is ArgumentException ? ex.Message : $"Invalid or expired token: {ex.Message}" });
        }
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
    /// Create or update user from gateway JWT claims
    /// </summary>
    [HttpPost("sync")]
    [Authorize]
    public async Task<ActionResult<AuthDto>> SyncUser()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Unauthorized();

        var subjectId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                        ?? User.FindFirst("sub")?.Value
                        ?? User.FindFirst("oid")?.Value
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                    ?? User.FindFirst("email")?.Value
                    ?? User.FindFirst("preferred_username")?.Value
                    ?? User.FindFirst("upn")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value;
        var displayName = User.FindFirst("name")?.Value ?? "Unknown User";
        var firstName = User.FindFirst("given_name")?.Value;
        var lastName = User.FindFirst("family_name")?.Value;

        if (string.IsNullOrEmpty(subjectId) || string.IsNullOrEmpty(email))
            return BadRequest("Required claims not found");

        var user = await _authService.CreateOrUpdateUserAsync(subjectId, email, displayName, firstName, lastName);

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
