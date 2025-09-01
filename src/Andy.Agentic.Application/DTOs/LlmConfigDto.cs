using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for LLM configuration entities.
///     Represents configuration settings for Large Language Model providers.
/// </summary>
public class LlmConfigDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the LLM configuration.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the LLM configuration.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the base URL for the LLM provider API.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the API key for authentication with the LLM provider.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the model name to use with the LLM provider.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the provider name (e.g., "openai", "ollama").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether this LLM configuration is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets the maximum number of tokens for responses.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    ///     Gets or sets the temperature setting for response randomness (0.0 to 2.0).
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    ///     Gets or sets the top-p setting for nucleus sampling (0.0 to 1.0).
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    ///     Gets or sets the frequency penalty for reducing repetition (-2.0 to 2.0).
    /// </summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    ///     Gets or sets the presence penalty for encouraging new topics (-2.0 to 2.0).
    /// </summary>
    public double? PresencePenalty { get; set; }

    /// <summary>
    ///     Gets or sets when the LLM configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets when the LLM configuration was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

