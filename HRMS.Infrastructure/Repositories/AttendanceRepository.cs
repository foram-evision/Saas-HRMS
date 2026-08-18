using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly ApplicationDbContext _context;

    public AttendanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Attendance?> GetTodayAsync(Guid employeeId, DateOnly today)
    {
        return await _context.Set<Attendance>()
            .Include(a => a.Employee)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);
    }

    public async Task<IEnumerable<Attendance>> GetMonthlyAsync(Guid employeeId, int month, int year)
    {
        return await _context.Set<Attendance>()
            .Include(a => a.Employee)
                .ThenInclude(e => e.User)
            .Where(a => a.EmployeeId == employeeId && a.Date.Month == month && a.Date.Year == year)
            .OrderBy(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetOrgDailyAsync(Guid organizationId, DateOnly date)
    {
        return await _context.Set<Attendance>()
            .Include(a => a.Employee)
                .ThenInclude(e => e.User)
            .Where(a => a.OrganizationId == organizationId && a.Date == date)
            .OrderBy(a => a.Employee.User.FullName)
            .ToListAsync();
    }

    public async Task<Attendance> AddAsync(Attendance attendance)
    {
        await _context.Set<Attendance>().AddAsync(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task<Attendance> UpdateAsync(Attendance attendance)
    {
        _context.Set<Attendance>().Update(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task<IEnumerable<Attendance>> GetByEmployeeAsync(Guid employeeId, DateOnly fromDate, DateOnly toDate)
    {
        return await _context.Set<Attendance>()
            .Where(a => a.EmployeeId == employeeId && a.Date >= fromDate && a.Date <= toDate)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<Attendance> UpsertAsync(Attendance attendance)
    {
        var existing = await _context.Set<Attendance>()
            .FirstOrDefaultAsync(a => a.EmployeeId == attendance.EmployeeId && a.Date == attendance.Date);

        if (existing != null)
        {
            existing.Status = attendance.Status;
            existing.CheckIn = attendance.CheckIn;
            existing.CheckOut = attendance.CheckOut;
            existing.TotalHours = attendance.TotalHours;
            existing.Notes = attendance.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.Set<Attendance>().Update(existing);
        }
        else
        {
            await _context.Set<Attendance>().AddAsync(attendance);
        }

        await _context.SaveChangesAsync();
        return existing ?? attendance;
    }
}
