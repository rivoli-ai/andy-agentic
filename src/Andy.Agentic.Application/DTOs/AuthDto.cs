namespace Andy.Agentic.Application.DTOs;

/// <summary>
/// Data Transfer Object for authentication responses
/// </summary>
public class AuthDto
{
    public bool IsAuthenticated { get; set; }
    public UserDto? User { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
}


