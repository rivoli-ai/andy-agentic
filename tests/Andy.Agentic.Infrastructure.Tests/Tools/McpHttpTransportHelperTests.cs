using Andy.Agentic.Infrastructure.Services.ToolProviders;
using FluentAssertions;
using ModelContextProtocol.Client;

namespace Andy.Agentic.Infrastructure.Tests.Tools;

public class McpHttpTransportHelperTests
{
    [Theory]
    [InlineData("http://localhost:5080/adapters/Context7/mcp", HttpTransportMode.StreamableHttp)]
    [InlineData("http://localhost:5080/adapters/Context7/sse", HttpTransportMode.Sse)]
    public void GetModeForDiscovery_InfersFromEndpointPath(string url, HttpTransportMode expected)
    {
        McpHttpTransportHelper.GetModeForDiscovery(null, url).Should().Be(expected);
    }

    [Fact]
    public void ToStorageTransport_PersistsStreamableHttpForMcpPath()
    {
        McpHttpTransportHelper
            .ToStorageTransport(null, "http://localhost:5080/adapters/Context7/mcp")
            .Should()
            .Be("streamable-http");
    }
}
