namespace Andy.Agentic.Domain.Models;

public class LlmRequest
{
    public List<ChatHistory> Messages { get; set; } = new List<ChatHistory>();

    public List<Tool>? Tools { get; set; }

    /// <summary>
    /// Images attached to the current user message for multimodal support.
    /// </summary>
    public List<ChatImage>? Images { get; set; }
}
