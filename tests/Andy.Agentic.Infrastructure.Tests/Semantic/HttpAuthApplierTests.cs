using System.Text;
using Andy.Agentic.Infrastructure.Semantic.Http;
using FluentAssertions;
using Xunit;

namespace Andy.Agentic.Infrastructure.Tests.Semantic;

/// <summary>Unit tests for <see cref="HttpAuthApplier"/>.</summary>
public class HttpAuthApplierTests
{
    /// <summary>None / empty auth type leaves the client untouched.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("none")]
    public async Task ApplyAsync_WithNoAuth_DoesNotSetHeaders(string? authType)
    {
        using var client = new HttpClient();
        await HttpAuthApplier.ApplyAsync(client, authType, null);
        client.DefaultRequestHeaders.Authorization.Should().BeNull();
    }

    /// <summary>Bearer auth sets the Authorization header.</summary>
    [Fact]
    public async Task ApplyAsync_WithBearer_SetsAuthorizationHeader()
    {
        using var client = new HttpClient();
        await HttpAuthApplier.ApplyAsync(client, "bearer", "{\"token\":\"abc123\"}");

        client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be("abc123");
    }

    /// <summary>API-key auth adds the configured header (default X-API-Key).</summary>
    [Fact]
    public async Task ApplyAsync_WithApiKey_AddsDefaultHeader()
    {
        using var client = new HttpClient();
        await HttpAuthApplier.ApplyAsync(client, "api_key", "{\"apiKey\":\"secret\"}");

        client.DefaultRequestHeaders.GetValues("X-API-Key").Should().ContainSingle().Which.Should().Be("secret");
    }

    /// <summary>API-key auth honors a custom header name.</summary>
    [Fact]
    public async Task ApplyAsync_WithApiKeyCustomHeader_UsesProvidedHeaderName()
    {
        using var client = new HttpClient();
        await HttpAuthApplier.ApplyAsync(client, "api_key", "{\"apiKey\":\"secret\",\"header\":\"X-Registry-Key\"}");

        client.DefaultRequestHeaders.GetValues("X-Registry-Key").Should().ContainSingle().Which.Should().Be("secret");
    }

    /// <summary>Basic auth sets a base64 Authorization header.</summary>
    [Fact]
    public async Task ApplyAsync_WithBasic_SetsBase64Header()
    {
        using var client = new HttpClient();
        await HttpAuthApplier.ApplyAsync(client, "basic", "{\"username\":\"u\",\"password\":\"p\"}");

        var expected = Convert.ToBase64String(Encoding.ASCII.GetBytes("u:p"));
        client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Basic");
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be(expected);
    }

    /// <summary>OAuth2 with a preset token uses it as a bearer token.</summary>
    [Fact]
    public async Task ApplyAsync_WithOAuth2PresetToken_SetsBearer()
    {
        using var client = new HttpClient();
        await HttpAuthApplier.ApplyAsync(client, "oauth2", "{\"accessToken\":\"tok\"}");

        client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be("tok");
    }

    /// <summary>Malformed JSON config is tolerated and produces no header.</summary>
    [Fact]
    public async Task ApplyAsync_WithMalformedJson_DoesNotThrow()
    {
        using var client = new HttpClient();
        await HttpAuthApplier.ApplyAsync(client, "bearer", "not-json");
        client.DefaultRequestHeaders.Authorization.Should().BeNull();
    }
}
