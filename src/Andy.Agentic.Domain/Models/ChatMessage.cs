using System.ComponentModel.DataAnnotations;
using Andy.Agentic.Domain.Models;

public class ChatMessage
{
    [Required] public string Content { get; set; } = string.Empty;

    [Required] public Guid? AgentId { get; set; }

    public string? AgentName { get; set; }

    public string? SessionId { get; set; } = string.Empty;

    public string Role { get; set; } = "user";

    public int? TokenCount { get; set; }

    public bool IsToolExecution { get; set; }

    public Guid? UserId { get; set; }

    public List<ToolExecutionLog> ToolResults { get; set; } = new();
}
