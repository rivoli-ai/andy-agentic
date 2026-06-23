namespace Andy.Agentic.Domain.Models;

public class TestConnection
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public LLMProviderType Provider { get; set; }
}
