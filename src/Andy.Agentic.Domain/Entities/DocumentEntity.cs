using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a document that can be associated with agents.
/// </summary>
public class DocumentEntity
{
    /// <summary>
    ///     Unique identifier for the document.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Name of the document.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Optional description of the document.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    ///     Type of the document (e.g., PDF, TXT, DOCX, etc.).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Content of the document (for text-based documents).
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    ///     Binary content of the document (for file-based documents).
    /// </summary>
    public byte[]? BinaryContent { get; set; }

    /// <summary>
    ///     File path for file-based documents (deprecated - use BinaryContent instead).
    /// </summary>
    [MaxLength(500)]
    public string? FilePath { get; set; }

    /// <summary>
    ///     Size of the document in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    ///     Indicates whether the document is active and can be used.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Indicates whether this document is public and visible to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    ///     Indicates whether this document has been processed for RAG (Retrieval Augmented Generation).
    /// </summary>
    public bool IsRagProcessed { get; set; } = false;

    /// <summary>
    ///     UTC timestamp when the document was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     UTC timestamp when the document was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Foreign key referencing the user who created this document.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    ///     Navigation property to the user who created this document.
    /// </summary>
    public virtual UserEntity? CreatedByUser { get; set; }

    /// <summary>
    ///     Collection of agent-document relationships.
    /// </summary>
    public virtual ICollection<AgentDocumentEntity> AgentDocuments { get; set; } = new List<AgentDocumentEntity>();
}
