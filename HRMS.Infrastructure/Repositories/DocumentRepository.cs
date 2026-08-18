using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(Guid employeeId, Guid organizationId)
    {
        return await _context.Set<EmployeeDocument>()
            .Include(d => d.UploadedByUser)
            .Where(d => d.EmployeeId == employeeId &&
                        d.OrganizationId == organizationId &&
                        !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<EmployeeDocument?> GetByIdAsync(Guid documentId, Guid organizationId)
    {
        return await _context.Set<EmployeeDocument>()
            .Include(d => d.UploadedByUser)
            .FirstOrDefaultAsync(d => d.Id == documentId &&
                                      d.OrganizationId == organizationId &&
                                      !d.IsDeleted);
    }

    public async Task<EmployeeDocument> AddAsync(EmployeeDocument document)
    {
        await _context.Set<EmployeeDocument>().AddAsync(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<bool> SoftDeleteAsync(Guid documentId, Guid organizationId)
    {
        var doc = await GetByIdAsync(documentId, organizationId);
        if (doc == null) return false;

        doc.IsDeleted = true;
        doc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
