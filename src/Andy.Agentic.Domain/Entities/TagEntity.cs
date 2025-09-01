using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a tag that can be assigned to agents for organization and search.
/// </summary>
public class TagEntity
{
    /// <summary>
    ///     Unique identifier for the tag.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Tag name.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Optional description of the tag.
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    ///     Optional color associated with the tag (e.g., hex code).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    ///     UTC timestamp when the tag was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Agents associated with this tag.
    /// </summary>
    public virtual ICollection<AgentTagEntity> AgentTags { get; set; } = new List<AgentTagEntity>();
}
