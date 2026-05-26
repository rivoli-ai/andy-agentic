using Andy.Agentic.Infrastructure.Services.ToolProviders;
using FluentAssertions;

namespace Andy.Agentic.Infrastructure.Tests.Services;

public class ToolHeadersParserTests
{
    [Fact]
    public void Parse_WithHeaderArray_ReturnsCaseInsensitiveDictionary()
    {
        const string headersJson =
            """[{"name":"X-Custom","value":"abc"},{"name":"Authorization","value":"Bearer custom"}]""";

        var headers = ToolHeadersParser.Parse(headersJson);

        headers.Should().HaveCount(2);
        headers["x-custom"].Should().Be("abc");
        headers["Authorization"].Should().Be("Bearer custom");
    }

    [Fact]
    public async Task BuildMergedHttpHeadersAsync_AuthOverridesCustomHeaderWithSameName()
    {
        const string headersJson = """[{"name":"Authorization","value":"Bearer custom"}]""";
        const string authenticationJson = """{"type":"bearer","apiKey":"from-auth"}""";

        var merged = await ToolHeadersParser.BuildMergedHttpHeadersAsync(headersJson, authenticationJson);

        merged["Authorization"].Should().Be("Bearer from-auth");
    }

    [Fact]
    public void ApplyToEnvironmentVariables_MapsHeaderNamesToUpperSnakeCase()
    {
        var env = new Dictionary<string, string>();
        var headers = new Dictionary<string, string>
        {
            ["X-Api-Key"] = "secret",
            ["Authorization"] = "Bearer token",
        };

        ToolHeadersParser.ApplyToEnvironmentVariables(headers, env);

        env["X_API_KEY"].Should().Be("secret");
        env["AUTHORIZATION"].Should().Be("Bearer token");
    }
}
