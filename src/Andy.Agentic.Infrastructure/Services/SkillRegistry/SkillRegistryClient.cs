using System.Text.Json;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Semantic.Http;
using Microsoft.Extensions.Logging;

namespace Andy.Agentic.Infrastructure.Services.SkillRegistry;

/// <summary>
///     HTTP client for an external agent-skill registry (andy-skills). Resolves search hits,
///     SKILL.md instructions, and bundled files. Credentials are applied per request via
///     <see cref="HttpAuthApplier"/> and never returned to callers.
/// </summary>
public class SkillRegistryClient(IHttpClientFactory httpClientFactory, ILogger<SkillRegistryClient>? logger = null)
    : ISkillRegistryClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <inheritdoc />
    public async Task<IReadOnlyList<SkillSearchResult>> SearchAsync(
        Domain.Models.SkillRegistry registry,
        string query,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateClientAsync(registry, cancellationToken);

        var url = $"api/search?q={Uri.EscapeDataString(query ?? string.Empty)}";
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var hits = JsonSerializer.Deserialize<List<PackageSearchHit>>(body, JsonOptions) ?? [];

        var results = new List<SkillSearchResult>(hits.Count);
        foreach (var hit in hits)
        {
            // Search hits carry no version; resolve the latest so attach has a concrete coordinate.
            var version = await TryResolveLatestVersionAsync(client, hit.NamespaceSlug, hit.SkillSlug, cancellationToken);
            results.Add(new SkillSearchResult
            {
                Namespace = hit.NamespaceSlug,
                SkillSlug = hit.SkillSlug,
                Version = version ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(hit.Title) ? hit.SkillSlug : hit.Title,
                Description = hit.Description ?? string.Empty,
            });
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<string> GetSkillMarkdownAsync(
        Domain.Models.SkillRegistry registry,
        string @namespace,
        string skillSlug,
        string version,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateClientAsync(registry, cancellationToken);
        var url = $"api/namespaces/{Esc(@namespace)}/packages/{Esc(skillSlug)}/versions/{Esc(version)}/SKILL.md";
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListFilesAsync(
        Domain.Models.SkillRegistry registry,
        string @namespace,
        string skillSlug,
        string version,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateClientAsync(registry, cancellationToken);
        var url = $"api/namespaces/{Esc(@namespace)}/packages/{Esc(skillSlug)}/versions/{Esc(version)}/zip/tree";
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var tree = JsonSerializer.Deserialize<ZipTreeResponse>(body, JsonOptions);
        return tree?.Paths ?? [];
    }

    /// <inheritdoc />
    public async Task<string> ReadFileAsync(
        Domain.Models.SkillRegistry registry,
        string @namespace,
        string skillSlug,
        string version,
        string path,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateClientAsync(registry, cancellationToken);
        var url = $"api/namespaces/{Esc(@namespace)}/packages/{Esc(skillSlug)}/versions/{Esc(version)}/zip/file?path={Uri.EscapeDataString(path)}";
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var file = JsonSerializer.Deserialize<ZipFileResponse>(body, JsonOptions);
        if (file is null)
        {
            return string.Empty;
        }

        return file.IsBinary
            ? $"[binary file '{path}', {file.SizeBytes} bytes — not shown]"
            : file.Content ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(
        Domain.Models.SkillRegistry registry,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await CreateClientAsync(registry, cancellationToken);
            var response = await client.GetAsync("api/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Skill registry connection test failed for {BaseUrl}", registry.BaseUrl);
            return false;
        }
    }

    private async Task<HttpClient> CreateClientAsync(Domain.Models.SkillRegistry registry, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(registry.BaseUrl.TrimEnd('/') + "/");
        await HttpAuthApplier.ApplyAsync(client, registry.AuthType, registry.AuthConfig, cancellationToken);
        return client;
    }

    private async Task<string?> TryResolveLatestVersionAsync(
        HttpClient client,
        string @namespace,
        string skillSlug,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = $"api/namespaces/{Esc(@namespace)}/packages/{Esc(skillSlug)}/versions";
            var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var versions = JsonSerializer.Deserialize<List<SkillVersion>>(body, JsonOptions) ?? [];
            var latest = versions.FirstOrDefault(v => v.IsLatest) ?? versions.FirstOrDefault();
            return latest?.Version;
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "Failed to resolve latest version for {Namespace}/{Skill}", @namespace, skillSlug);
            return null;
        }
    }

    private static string Esc(string value) => Uri.EscapeDataString(value);

    private sealed record PackageSearchHit(string NamespaceSlug, string SkillSlug, string? Title, string? Description);

    private sealed record SkillVersion(string Version, bool IsLatest);

    private sealed record ZipTreeResponse(List<string> Paths);

    private sealed record ZipFileResponse(string Path, string? Content, bool IsBinary, long SizeBytes);
}
