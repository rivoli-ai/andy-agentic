namespace Andy.Agentic.Domain.Models;

public class TestConnectionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? Latency { get; set; }
}
