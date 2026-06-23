using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
/// Represents a user in the system, linked to Microsoft Entra (Azure AD)
/// </summary>
public class UserEntity
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD Object ID (unique identifier from Microsoft Entra)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string AzureAdId { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when the user last logged in
    /// </summary>
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional: User's first name
    /// </summary>
    [MaxLength(255)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Optional: User's last name
    /// </summary>
    [MaxLength(255)]
    public string? LastName { get; set; }

    /// <summary>
    /// Optional: User's job title
    /// </summary>
    [MaxLength(255)]
    public string? JobTitle { get; set; }

    /// <summary>
    /// Optional: User's department
    /// </summary>
    [MaxLength(255)]
    public string? Department { get; set; }

    /// <summary>
    /// Collection of agents created by this user
    /// </summary>
    public virtual ICollection<AgentEntity> Agents { get; set; } = new List<AgentEntity>();

    /// <summary>
    /// Collection of chat messages sent by this user
    /// </summary>
    public virtual ICollection<ChatMessageEntity> ChatMessages { get; set; } = new List<ChatMessageEntity>();

    /// <summary>
    /// Collection of tool executions performed by this user
    /// </summary>
    public virtual ICollection<ToolExecutionLogEntity> ToolExecutions { get; set; } = new List<ToolExecutionLogEntity>();

    /// <summary>
    /// Collection of tools created by this user
    /// </summary>
    public virtual ICollection<ToolEntity> Tools { get; set; } = new List<ToolEntity>();

    /// <summary>
    /// Collection of LLM configurations created by this user
    /// </summary>
    public virtual ICollection<LlmConfigEntity> LlmConfigs { get; set; } = new List<LlmConfigEntity>();
}

