namespace Andy.Agentic.Domain.Models.Semantic;

public class KernelConfiguration
{
    public AiProvider? Provider { get; set; }
    public string? ModelId { get; set; }
    public string? OpenAIApiKey { get; set; }
    public string? AzureEndpoint { get; set; }
    public string? AzureApiKey { get; set; }
    public string? AzureDeploymentName { get; set; }
    public string? AnthropicApiKey { get; set; }
    public string? HuggingFaceApiKey { get; set; }
    public List<ToolConfiguration>? Tools { get; set; }
    public Dictionary<string, object>? AdditionalSettings { get; set; }
}
