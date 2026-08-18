using HRMS.Domain.Base;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

/// <summary>
/// Represents a document uploaded and associated with an employee.
/// Metadata is stored in the database; the file is stored on the filesystem (V1)
/// or Azure Blob Storage (V2). Swap IStorageService implementation to migrate.
/// </summary>
public class EmployeeDocument : BaseEntity
{
    /// <summary>Employee this document belongs to.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Organization this document belongs to (multi-tenant scoping).</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>HR/Admin user who uploaded this document.</summary>
    public Guid UploadedByUserId { get; set; }

    /// <summary>Classification of the document (Resume, OfferLetter, IdentityProof, etc.).</summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>
    /// Original file name provided by the uploader.
    /// Stored for display purposes only — not used for file access to prevent path traversal.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// GUID-based file name used for actual storage.
    /// Example: "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d.pdf"
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>
    /// Relative path to the stored file (or blob URI in future storage).
    /// Example (V1): "uploads/documents/{orgId}/{year}/{month}/{storedFileName}"
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long FileSize { get; set; }

    /// <summary>MIME content type (e.g., "application/pdf", "image/jpeg").</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Optional human-readable description of the document's content.</summary>
    public string? Description { get; set; }

    /// <summary>Soft-delete flag. Deleted documents remain in DB but are hidden from API responses.</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>UTC timestamp when this record was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when this record was last modified.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ─────────────────────────────────────────────────

    public virtual Employee Employee { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual ApplicationUser UploadedByUser { get; set; } = null!;
}
