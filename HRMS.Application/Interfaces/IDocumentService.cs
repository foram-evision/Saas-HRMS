using HRMS.Application.Common;
using HRMS.Application.DTOs.Document;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Application service contract for employee document management.
/// Handles upload validation, metadata persistence, and file lifecycle.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Upload a new document for an employee.
    /// Validates file type, size, and tenant ownership before persisting.
    /// </summary>
    Task<ApiResponse<DocumentResponseDto>> UploadAsync(
        Guid employeeId,
        Guid organizationId,
        Guid uploadedByUserId,
        UploadDocumentDto dto);

    /// <summary>Get all documents for a specific employee (scoped to organization).</summary>
    Task<ApiResponse<List<DocumentResponseDto>>> GetByEmployeeAsync(Guid employeeId, Guid organizationId);

    /// <summary>Get metadata for a single document.</summary>
    Task<ApiResponse<DocumentResponseDto>> GetByIdAsync(Guid documentId, Guid organizationId);

    /// <summary>
    /// Soft-delete a document record and remove the physical file.
    /// </summary>
    Task<ApiResponse<bool>> DeleteAsync(Guid documentId, Guid organizationId, Guid performedByUserId);

    /// <summary>
    /// Resolve the absolute file path for streaming a document download.
    /// Returns null if document does not exist or tenant mismatch.
    /// </summary>
    Task<(EmployeeDocument? Document, string? AbsolutePath)> GetDownloadInfoAsync(Guid documentId, Guid organizationId);
}
