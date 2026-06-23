using System.Net;
using System.Text;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Services.SkillRegistry;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Andy.Agentic.Infrastructure.Tests.Semantic;

/// <summary>Unit tests for <see cref="SkillRegistryClient"/> URL construction and parsing.</summary>
public class SkillRegistryClientTests
{
    private static SkillRegistry Registry => new() { Id = Guid.NewGuid(), BaseUrl = "http://localhost:8080", AuthType = "none" };

    private static (SkillRegistryClient client, List<HttpRequestMessage> requests) BuildClient(
        Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var requests = new List<HttpRequestMessage>();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns((HttpRequestMessage req, CancellationToken _) =>
            {
                requests.Add(req);
                return Task.FromResult(responder(req));
            });

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handler.Object));

        return (new SkillRegistryClient(factory.Object), requests);
    }

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    /// <summary>Search resolves each hit's latest version and maps fields.</summary>
    [Fact]
    public async Task SearchAsync_MapsHitsAndResolvesLatestVersion()
    {
        var (client, requests) = BuildClient(req =>
            req.RequestUri!.AbsolutePath.EndsWith("/versions")
                ? Json("[{\"version\":\"0.9.0\",\"isLatest\":false},{\"version\":\"1.2.0\",\"isLatest\":true}]")
                : Json("[{\"namespaceSlug\":\"acme\",\"skillSlug\":\"pdf-filler\",\"title\":\"PDF Filler\",\"description\":\"fills pdfs\"}]"));

        var results = await client.SearchAsync(Registry, "pdf");

        results.Should().ContainSingle();
        var hit = results[0];
        hit.Namespace.Should().Be("acme");
        hit.SkillSlug.Should().Be("pdf-filler");
        hit.DisplayName.Should().Be("PDF Filler");
        hit.Version.Should().Be("1.2.0");
        requests.Should().Contain(r => r.RequestUri!.Query.Contains("q=pdf"));
    }

    /// <summary>GetSkillMarkdown returns the raw markdown body.</summary>
    [Fact]
    public async Task GetSkillMarkdownAsync_ReturnsBody()
    {
        var (client, requests) = BuildClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("# Title", Encoding.UTF8, "text/markdown") });

        var md = await client.GetSkillMarkdownAsync(Registry, "acme", "pdf-filler", "1.2.0");

        md.Should().Be("# Title");
        requests[0].RequestUri!.AbsolutePath.Should().Be("/api/namespaces/acme/packages/pdf-filler/versions/1.2.0/SKILL.md");
    }

    /// <summary>ReadFile returns text content and renders binary files as a placeholder.</summary>
    [Fact]
    public async Task ReadFileAsync_TextAndBinary()
    {
        var (textClient, _) = BuildClient(_ => Json("{\"path\":\"a.txt\",\"content\":\"hello\",\"isBinary\":false,\"sizeBytes\":5}"));
        (await textClient.ReadFileAsync(Registry, "acme", "s", "1.0.0", "a.txt")).Should().Be("hello");

        var (binClient, _) = BuildClient(_ => Json("{\"path\":\"a.bin\",\"isBinary\":true,\"sizeBytes\":99}"));
        (await binClient.ReadFileAsync(Registry, "acme", "s", "1.0.0", "a.bin")).Should().Contain("binary");
    }

    /// <summary>TestConnection probes the health endpoint.</summary>
    [Fact]
    public async Task TestConnectionAsync_ReturnsTrueOnHealthOk()
    {
        var (client, requests) = BuildClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        (await client.TestConnectionAsync(Registry)).Should().BeTrue();
        requests[0].RequestUri!.AbsolutePath.Should().Be("/api/health");
    }

    /// <summary>TestConnection returns false when the registry is unreachable.</summary>
    [Fact]
    public async Task TestConnectionAsync_ReturnsFalseOnError()
    {
        var (client, _) = BuildClient(_ => throw new HttpRequestException("down"));
        (await client.TestConnectionAsync(Registry)).Should().BeFalse();
    }
}
