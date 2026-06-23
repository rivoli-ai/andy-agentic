using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

/// <summary>
/// Builds <see cref="HttpClientTransport"/> instances with a dedicated <see cref="HttpClient"/>
/// that buffers JSON-RPC request bodies before send (required for some MCP HTTP gateways).
/// </summary>
internal static class McpHttpTransportFactory
{
    public static HttpClientTransport Create(
        string endpoint,
        HttpTransportMode mode,
        IReadOnlyDictionary<string, string>? authHeaders = null,
        string? clientName = null,
        TimeSpan? connectionTimeout = null)
    {
        var headers = authHeaders?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = new HttpClientTransportOptions
        {
            Endpoint = new Uri(endpoint),
            TransportMode = mode,
            Name = clientName ?? "MCP Client",
            AdditionalHeaders = headers,
            ConnectionTimeout = connectionTimeout ?? TimeSpan.FromSeconds(120),
        };

        var httpClient = new HttpClient(new McpRequestContentBufferingHandler(new HttpClientHandler()))
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };

        return new HttpClientTransport(options, httpClient, NullLoggerFactory.Instance, ownsHttpClient: true);
    }

    /// <summary>
    /// The MCP SDK may attach non-rewindable <see cref="HttpContent"/>; buffering avoids 500s from gateways.
    /// </summary>
    private sealed class McpRequestContentBufferingHandler : DelegatingHandler
    {
        public McpRequestContentBufferingHandler(HttpMessageHandler inner)
            : base(inner)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Content is not null)
            {
                var mediaType = request.Content.Headers.ContentType?.MediaType ?? "application/json";
                var body = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                request.Content = new StringContent(body, Encoding.UTF8, mediaType);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
