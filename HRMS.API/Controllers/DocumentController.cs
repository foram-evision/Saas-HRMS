using System.Security.Claims;
using HRMS.Application.Common;
using HRMS.Application.DTOs.Document;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Upload a new document for an employee. (Admin/HR only)
    /// </summary>
    [HttpPost("{employeeId}/upload")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(Guid employeeId, [FromForm] UploadDocumentDto dto)
    {
        var orgId = GetOrganizationId();
        var userId = GetUserId();

        if (orgId == null || userId == null)
            return BadRequest(ApiResponse<DocumentResponseDto>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _documentService.UploadAsync(employeeId, orgId.Value, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all documents for a specific employee. 
    /// Employees can only view their own documents; HR/Admin can view all within org.
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeDocuments(Guid employeeId)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<List<DocumentResponseDto>>.Failure(AppConstants.Messages.NoOrganization));

        // Authorization check: if not HR/Admin, ensure employeeId matches current user's employeeId
        if (!User.IsInRole(AppConstants.Roles.Admin) && !User.IsInRole(AppConstants.Roles.HR))
        {
            var myEmployeeId = GetEmployeeId();
            if (myEmployeeId != employeeId)
                return Forbid();
        }

        var result = await _documentService.GetByEmployeeAsync(employeeId, orgId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Get metadata for a specific document.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<DocumentResponseDto>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _documentService.GetByIdAsync(id, orgId.Value);
        
        // Auth check for standard employees
        if (result.Success && !User.IsInRole(AppConstants.Roles.Admin) && !User.IsInRole(AppConstants.Roles.HR))
        {
            if (result.Data!.EmployeeId != GetEmployeeId())
                return Forbid();
        }

        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Soft delete a document. (Admin/HR only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var orgId = GetOrganizationId();
        var userId = GetUserId();

        if (orgId == null || userId == null)
            return BadRequest(ApiResponse<bool>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _documentService.DeleteAsync(id, orgId.Value, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Download the actual file content of a document.
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<bool>.Failure(AppConstants.Messages.NoOrganization));

        var (doc, absolutePath) = await _documentService.GetDownloadInfoAsync(id, orgId.Value);

        if (doc == null || absolutePath == null || !System.IO.File.Exists(absolutePath))
            return NotFound(ApiResponse<bool>.Failure(AppConstants.DocumentMessages.DocumentNotFound));

        // Auth check for standard employees
        if (!User.IsInRole(AppConstants.Roles.Admin) && !User.IsInRole(AppConstants.Roles.HR))
        {
            if (doc.EmployeeId != GetEmployeeId())
                return Forbid();
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(absolutePath);
        return File(fileBytes, doc.ContentType, doc.FileName);
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirstValue("organizationId")
                 ?? User.FindFirstValue("OrganizationId")
                 ?? User.FindFirstValue("org_id");

        if (Guid.TryParse(claim, out var orgId) && orgId != Guid.Empty)
            return orgId;

        return null;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (Guid.TryParse(claim, out var userId) && userId != Guid.Empty)
            return userId;

        return null;
    }

    private Guid? GetEmployeeId()
    {
        var claim = User.FindFirstValue("employeeId") ?? User.FindFirstValue("EmployeeId");
        if (Guid.TryParse(claim, out var empId) && empId != Guid.Empty)
            return empId;

        return null;
    }
}
