public class ChatMessagePreview
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsToolExecution { get; set; }
    public string? ToolName { get; set; }
}
