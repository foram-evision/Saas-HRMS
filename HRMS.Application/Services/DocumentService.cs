using HRMS.Application.Common;
using HRMS.Application.DTOs.Document;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IStorageService _storageService;
    private readonly IAuditLogService _auditLogService;

    public DocumentService(
        IDocumentRepository documentRepository,
        IEmployeeRepository employeeRepository,
        IStorageService storageService,
        IAuditLogService auditLogService)
    {
        _documentRepository = documentRepository;
        _employeeRepository = employeeRepository;
        _storageService = storageService;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<DocumentResponseDto>> UploadAsync(
        Guid employeeId,
        Guid organizationId,
        Guid uploadedByUserId,
        UploadDocumentDto dto)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null || employee.OrganizationId != organizationId)
        {
            return ApiResponse<DocumentResponseDto>.Failure(AppConstants.Messages.EmployeeNotFound);
        }

        var file = dto.File;
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        
        var date = DateTime.UtcNow;
        var subFolder = Path.Combine("documents", organizationId.ToString(), date.Year.ToString(), date.Month.ToString("D2")).Replace("\\", "/");

        using var stream = file.OpenReadStream();
        var relativePath = await _storageService.UploadAsync(stream, storedFileName, subFolder);

        var document = new EmployeeDocument
        {
            EmployeeId = employeeId,
            OrganizationId = organizationId,
            UploadedByUserId = uploadedByUserId,
            DocumentType = dto.DocumentType,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            FilePath = relativePath,
            FileSize = file.Length,
            ContentType = file.ContentType,
            Description = dto.Description?.Trim()
        };

        var saved = await _documentRepository.AddAsync(document);

        await _auditLogService.LogAsync(
            uploadedByUserId, organizationId,
            AppConstants.AuditEvents.DocumentUploaded, "EmployeeDocument", saved.Id,
            newValues: $"{{\"FileName\":\"{saved.FileName}\",\"Type\":\"{saved.DocumentType}\"}}");

        return ApiResponse<DocumentResponseDto>.SuccessResult(MapToDto(saved), "Document uploaded successfully.");
    }

    public async Task<ApiResponse<List<DocumentResponseDto>>> GetByEmployeeAsync(Guid employeeId, Guid organizationId)
    {
        var docs = await _documentRepository.GetByEmployeeAsync(employeeId, organizationId);
        var dtos = docs.Select(MapToDto).ToList();
        return ApiResponse<List<DocumentResponseDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<DocumentResponseDto>> GetByIdAsync(Guid documentId, Guid organizationId)
    {
        var doc = await _documentRepository.GetByIdAsync(documentId, organizationId);
        if (doc == null)
            return ApiResponse<DocumentResponseDto>.Failure(AppConstants.DocumentMessages.DocumentNotFound);

        return ApiResponse<DocumentResponseDto>.SuccessResult(MapToDto(doc));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid documentId, Guid organizationId, Guid performedByUserId)
    {
        var doc = await _documentRepository.GetByIdAsync(documentId, organizationId);
        if (doc == null)
            return ApiResponse<bool>.Failure(AppConstants.DocumentMessages.DocumentNotFound);

        // Soft delete in DB
        var result = await _documentRepository.SoftDeleteAsync(documentId, organizationId);
        if (!result) return ApiResponse<bool>.Failure(AppConstants.DocumentMessages.DocumentNotFound);

        // Remove physical file
        await _storageService.DeleteAsync(doc.FilePath);

        await _auditLogService.LogAsync(
            performedByUserId, organizationId,
            AppConstants.AuditEvents.DocumentDeleted, "EmployeeDocument", documentId,
            oldValues: $"{{\"FileName\":\"{doc.FileName}\"}}");

        return ApiResponse<bool>.SuccessResult(true, "Document deleted successfully.");
    }

    public async Task<(EmployeeDocument? Document, string? AbsolutePath)> GetDownloadInfoAsync(Guid documentId, Guid organizationId)
    {
        var doc = await _documentRepository.GetByIdAsync(documentId, organizationId);
        if (doc == null) return (null, null);

        var absolutePath = _storageService.GetAbsolutePath(doc.FilePath);
        return (doc, absolutePath);
    }

    private static DocumentResponseDto MapToDto(EmployeeDocument d) => new()
    {
        Id = d.Id,
        EmployeeId = d.EmployeeId,
        OrganizationId = d.OrganizationId,
        DocumentType = d.DocumentType,
        DocumentTypeName = d.DocumentType.ToString(),
        FileName = d.FileName,
        FileSize = d.FileSize,
        FileSizeFormatted = FormatFileSize(d.FileSize),
        ContentType = d.ContentType,
        Description = d.Description,
        UploadedBy = d.UploadedByUser?.FullName ?? "Unknown",
        CreatedAt = d.CreatedAt
    };

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
