using HRMS.Application.Common;
using HRMS.Application.DTOs.AuditLog;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

/// <summary>
/// Service for writing and querying business audit logs.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(
        Guid performedByUserId,
        Guid? organizationId,
        string action,
        string entityType,
        Guid? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null)
    {
        var log = new AuditLog
        {
            PerformedByUserId = performedByUserId == Guid.Empty ? null : performedByUserId,
            OrganizationId = (organizationId.HasValue && organizationId.Value != Guid.Empty) ? organizationId : null,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(log);
    }

    public async Task<ApiResponse<List<AuditLogResponseDto>>> GetLogsAsync(
        Guid orgId,
        string? entityType = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50)
    {
        var logs = await _auditLogRepository.GetAsync(orgId, entityType, from, to, page, pageSize);
        var dtos = logs.Select(l => new AuditLogResponseDto
        {
            Id = l.Id,
            Action = l.Action,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            PerformedByName = l.PerformedByUser?.FullName ?? "System",
            OldValues = l.OldValues,
            NewValues = l.NewValues,
            IpAddress = l.IpAddress,
            Timestamp = l.Timestamp
        }).ToList();

        return ApiResponse<List<AuditLogResponseDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<List<ActivityFeedItemDto>>> GetActivityFeedAsync(Guid orgId, int page = 1, int pageSize = 10)
    {
        var logs = await _auditLogRepository.GetAsync(orgId, null, null, null, page, pageSize);
        var dtos = logs.Select(l => new ActivityFeedItemDto
        {
            Id = l.Id,
            Description = GenerateActivityDescription(l),
            Action = l.Action,
            EntityType = l.EntityType,
            PerformedBy = l.PerformedByUser?.FullName ?? "System",
            Timestamp = l.Timestamp,
            TimeAgo = GetTimeAgo(l.Timestamp)
        }).ToList();

        return ApiResponse<List<ActivityFeedItemDto>>.SuccessResult(dtos);
    }

    private static string GenerateActivityDescription(AuditLog log)
    {
        var user = log.PerformedByUser?.FullName ?? "System";
        return log.Action switch
        {
            AppConstants.AuditEvents.CheckIn => $"{user} checked in for work.",
            AppConstants.AuditEvents.CheckOut => $"{user} checked out.",
            AppConstants.AuditEvents.AttendanceMarked => $"Attendance for {user} was marked by HR.",
            AppConstants.AuditEvents.LeaveApplied => $"{user} applied for leave.",
            AppConstants.AuditEvents.LeaveApproved => $"Leave request approved by {user}.",
            AppConstants.AuditEvents.LeaveRejected => $"Leave request rejected by {user}.",
            AppConstants.AuditEvents.EmployeeCreated => $"New employee profile created by {user}.",
            AppConstants.AuditEvents.EmployeeAssignedToDepartment => $"{user} was assigned to a new department.",
            AppConstants.AuditEvents.DocumentUploaded => $"{user} uploaded a new document.",
            _ => $"{user} performed action: {log.Action} on {log.EntityType}"
        };
    }

    private static string GetTimeAgo(DateTime timestamp)
    {
        var timespan = DateTime.UtcNow - timestamp;
        if (timespan.TotalMinutes < 1) return "Just now";
        if (timespan.TotalMinutes < 60) return $"{(int)timespan.TotalMinutes} minutes ago";
        if (timespan.TotalHours < 24) return $"{(int)timespan.TotalHours} hours ago";
        if (timespan.TotalDays < 2) return "Yesterday";
        if (timespan.TotalDays < 30) return $"{(int)timespan.TotalDays} days ago";
        return timestamp.ToString("MMM dd, yyyy");
    }
}
