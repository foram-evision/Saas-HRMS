using HRMS.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HRMS.Application.DTOs.Document;

/// <summary>
/// Request DTO for uploading a new employee document.
/// Uses IFormFile so the file content streams directly from the HTTP multipart request.
/// </summary>
public class UploadDocumentDto
{
    /// <summary>Classification of the document being uploaded.</summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>The file being uploaded.</summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>Optional human-readable description of the document.</summary>
    public string? Description { get; set; }
}
