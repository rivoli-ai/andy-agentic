using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Represents a document that can be associated with agents.
/// </summary>
public class Document
{
    /// <summary>
    ///     Gets or sets the unique identifier for the document.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the document.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the document.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the type of the document.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the content of the document (for text-based documents).
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    ///     Gets or sets the binary content of the document (for file-based documents).
    /// </summary>
    [Column(TypeName = "bytea")]
    public byte[]? BinaryContent { get; set; }

    /// <summary>
    ///     Gets or sets the file path for file-based documents (deprecated - use BinaryContent instead).
    /// </summary>
    [MaxLength(500)]
    public string? FilePath { get; set; }

    /// <summary>
    ///     Gets or sets the size of the document in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    ///     Gets or sets whether the document is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether this document is public and visible to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;


    /// <summary>
    ///     Gets or sets whether this document is public and visible to all users.
    /// </summary>
    public bool IsRagProcessed { get; set; } = false;

    /// <summary>
    ///     Gets or sets when the document was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets when the document was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets the ID of the user who created this document.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }
}
