using System.Text;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Ensures Kimi/Qwen chat-completion requests include required fields when tools are used.
/// Semantic Kernel and the OpenAI SDK omit <c>reasoning_content</c> on tool-call turns.
/// </summary>
internal sealed class ThinkingModelCompatibilityHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (HttpMethod.Post.Equals(request.Method)
            && ThinkingModelRequestPatcher.ShouldPatch(request.RequestUri)
            && request.Content is not null)
        {
            var originalJson = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var patchedJson = ThinkingModelRequestPatcher.PatchChatCompletionsRequest(originalJson);

            if (!string.Equals(originalJson, patchedJson, StringComparison.Ordinal))
            {
                request.Content = new StringContent(patchedJson, Encoding.UTF8, "application/json");
            }
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
