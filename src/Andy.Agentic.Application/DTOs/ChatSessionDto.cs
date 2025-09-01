public class ChatSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public int MessageCount { get; set; }
    public int TotalTokens { get; set; }
    public string? SessionTitle { get; set; }
    public string? Description { get; set; } // First user message as session description
    public bool IsActive { get; set; }

    public string Title { get; set; } = string.Empty;
}
