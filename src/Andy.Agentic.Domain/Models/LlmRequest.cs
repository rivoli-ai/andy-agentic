namespace Andy.Agentic.Domain.Models;

public class LlmRequest
{
    public List<ChatHistory> Messages { get; set; } = new List<ChatHistory>();

    public List<Tool>? Tools { get; set; }
}
