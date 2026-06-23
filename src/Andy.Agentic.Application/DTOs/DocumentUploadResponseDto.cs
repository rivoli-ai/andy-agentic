namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Response DTO for document upload operations.
/// </summary>
public class DocumentUploadResponseDto
{
    /// <summary>
    ///     Gets or sets whether the upload was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     Gets or sets the response message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the created document (if successful).
    /// </summary>
    public DocumentDto? Document { get; set; }

    /// <summary>
    ///     Gets or sets the error message (if failed).
    /// </summary>
    public string? Error { get; set; }
}
