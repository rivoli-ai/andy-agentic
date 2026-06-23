namespace Andy.Agentic.Application.DTOs;

/// <summary>
/// Data Transfer Object for chat images.
/// Represents an image attached to a chat message for multimodal support.
/// </summary>
public class ChatImageDto
{
    /// <summary>
    /// Base64 encoded image data (with data URI prefix if applicable).
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the image (e.g., "image/png", "image/jpeg").
    /// </summary>
    public string MimeType { get; set; } = "image/jpeg";

    /// <summary>
    /// Optional image name or description.
    /// </summary>
    public string? Name { get; set; }
}

