using System.Security.Claims;

namespace Andy.Agentic.Application.Auth;

public interface IAuthProvider
{
    string Name { get; }
    string Type { get; }

    Task<ClaimsPrincipal> ValidateTokenAsync(string token, CancellationToken ct);
}
