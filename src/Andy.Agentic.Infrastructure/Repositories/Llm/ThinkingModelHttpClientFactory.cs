namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Creates HTTP clients that patch Kimi/Qwen thinking + tool-call request bodies.
/// </summary>
internal static class ThinkingModelHttpClientFactory
{
    public static HttpClient Create()
    {
        var handler = new ThinkingModelCompatibilityHandler { InnerHandler = new HttpClientHandler() };
        return new HttpClient(handler, disposeHandler: true);
    }
}
