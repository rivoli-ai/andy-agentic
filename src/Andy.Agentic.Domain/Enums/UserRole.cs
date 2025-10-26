namespace Andy.Agentic.Domain.Enums;

/// <summary>
/// User roles for role-based access control
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Read-only access - can view agents, tools, and use chatbot
    /// </summary>
    Read = 0,
    
    /// <summary>
    /// Full access - can create, edit, delete agents and tools
    /// </summary>
    Write = 1
}


