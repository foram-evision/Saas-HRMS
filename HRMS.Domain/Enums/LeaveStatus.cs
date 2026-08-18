namespace HRMS.Domain.Enums;

/// <summary>
/// Represents the approval status of a leave request.
/// </summary>
public enum LeaveStatus
{
    /// <summary>Leave request submitted and awaiting HR/Admin action.</summary>
    Pending = 1,

    /// <summary>Leave request approved by HR or Admin.</summary>
    Approved = 2,

    /// <summary>Leave request rejected by HR or Admin.</summary>
    Rejected = 3,

    /// <summary>Leave request cancelled by the employee before it was actioned.</summary>
    Cancelled = 4
}
