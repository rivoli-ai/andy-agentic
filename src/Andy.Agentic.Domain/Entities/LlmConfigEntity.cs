using System.ComponentModel.DataAnnotations;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a persisted Large Language Model (LLM) configuration used by agents.
/// </summary>
public class LlmConfigEntity
{
    /// <summary>
    ///     Unique identifier for the LLM configuration.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Human-readable name for this configuration.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Base URL for the LLM provider API.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    ///     API key used to authenticate with the LLM provider.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    ///     Model identifier (e.g., gpt-4o, llama3:latest).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    ///     Provider type.
    /// </summary>
    [Required]
    public LLMProviderType Provider { get; set; }

    /// <summary>
    ///     Flag indicating whether this configuration is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Optional maximum tokens for responses.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    ///     Optional temperature for controlling randomness.
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    ///     Optional nucleus sampling parameter.
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    ///     Optional frequency penalty to reduce token repetition.
    /// </summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    ///     Optional presence penalty to encourage new topic introduction.
    /// </summary>
    public double? PresencePenalty { get; set; }

    /// <summary>
    ///     UTC timestamp when this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     UTC timestamp when this configuration was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Foreign key referencing the user who created this LLM configuration.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    ///     Navigation property to the user who created this LLM configuration.
    /// </summary>
    public virtual UserEntity? CreatedByUser { get; set; }

    /// <summary>
    ///     Indicates whether this LLM configuration is public and visible to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;
}
