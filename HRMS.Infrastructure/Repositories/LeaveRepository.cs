using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class LeaveRepository : ILeaveRepository
{
    private readonly ApplicationDbContext _context;

    public LeaveRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaveType>> GetLeaveTypesAsync(Guid organizationId)
    {
        return await _context.Set<LeaveType>()
            .Where(lt => lt.OrganizationId == organizationId)
            .OrderBy(lt => lt.Name)
            .ToListAsync();
    }

    public async Task<LeaveType?> GetLeaveTypeByIdAsync(Guid id, Guid organizationId)
    {
        return await _context.Set<LeaveType>()
            .FirstOrDefaultAsync(lt => lt.Id == id && lt.OrganizationId == organizationId);
    }

    public async Task<LeaveType> AddLeaveTypeAsync(LeaveType leaveType)
    {
        await _context.Set<LeaveType>().AddAsync(leaveType);
        await _context.SaveChangesAsync();
        return leaveType;
    }

    public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(Guid id)
    {
        return await _context.Set<LeaveRequest>()
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.User)
            .Include(lr => lr.ApprovedByUser)
            .FirstOrDefaultAsync(lr => lr.Id == id);
    }

    public async Task<IEnumerable<LeaveRequest>> GetEmployeeLeavesAsync(Guid employeeId)
    {
        return await _context.Set<LeaveRequest>()
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ApprovedByUser)
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingLeavesAsync(Guid organizationId)
    {
        return await _context.Set<LeaveRequest>()
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.User)
            .Where(lr => lr.OrganizationId == organizationId && lr.Status == LeaveStatus.Pending)
            .OrderBy(lr => lr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetApprovedLeavesForYearAsync(Guid employeeId, Guid leaveTypeId, int year)
    {
        return await _context.Set<LeaveRequest>()
            .Where(lr => lr.EmployeeId == employeeId &&
                         lr.LeaveTypeId == leaveTypeId &&
                         lr.Status == LeaveStatus.Approved &&
                         lr.StartDate.Year == year)
            .ToListAsync();
    }

    public async Task<LeaveRequest> AddLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        await _context.Set<LeaveRequest>().AddAsync(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest;
    }

    public async Task<LeaveRequest> UpdateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        _context.Set<LeaveRequest>().Update(leaveRequest);
        await _context.SaveChangesAsync();
        return leaveRequest;
    }

    public async Task<int> GetUsedDaysAsync(Guid employeeId, Guid leaveTypeId, int year)
    {
        return await _context.Set<LeaveRequest>()
            .Where(lr => lr.EmployeeId == employeeId &&
                         lr.LeaveTypeId == leaveTypeId &&
                         lr.Status == LeaveStatus.Approved &&
                         lr.StartDate.Year == year)
            .SumAsync(lr => lr.TotalDays);
    }

    public async Task<(IEnumerable<LeaveRequest> Items, int TotalCount)> GetAllLeavesAsync(Guid organizationId, LeaveStatus? status, int page, int pageSize)
    {
        var query = _context.Set<LeaveRequest>()
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.User)
            .Where(lr => lr.OrganizationId == organizationId);

        if (status.HasValue)
        {
            query = query.Where(lr => lr.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(lr => lr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
