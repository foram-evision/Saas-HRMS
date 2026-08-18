using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.Document;

/// <summary>
/// Response DTO for an employee document.
/// Never exposes the physical file path or stored filename — only metadata safe for API consumers.
/// </summary>
public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid OrganizationId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }

    /// <summary>File size formatted as human-readable string (e.g., "1.2 MB").</summary>
    public string FileSizeFormatted { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
