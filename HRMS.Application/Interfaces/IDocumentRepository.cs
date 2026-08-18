using HRMS.Application.Common;
using HRMS.Application.DTOs.Document;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Data access contract for employee document management.
/// All methods are scoped to OrganizationId for strict multi-tenant isolation.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>Get all active (non-deleted) documents for an employee within the organization.</summary>
    Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(Guid employeeId, Guid organizationId);

    /// <summary>Get a single document by its ID, scoped to the organization.</summary>
    Task<EmployeeDocument?> GetByIdAsync(Guid documentId, Guid organizationId);

    /// <summary>Persist a new document metadata record to the database.</summary>
    Task<EmployeeDocument> AddAsync(EmployeeDocument document);

    /// <summary>Soft-delete a document by setting IsDeleted = true.</summary>
    Task<bool> SoftDeleteAsync(Guid documentId, Guid organizationId);
}
