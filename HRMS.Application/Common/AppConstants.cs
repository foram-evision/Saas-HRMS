namespace HRMS.Application.Common;

/// <summary>
/// Application-wide constants for roles, standard messages, and audit event names.
/// </summary>
public static class AppConstants
{
    // ─── Roles ────────────────────────────────────────────────────────────────

    public static class Roles
    {
        // CHANGED TO ALL-CAPS TO MATCH THE JWT TOKEN AND DATABASE SEED DATA PERFECTLY
        public const string Admin = "ADMIN";
        public const string HR = "HR";
    }

    // ─── Standard API Messages ────────────────────────────────────────────────

    public static class Messages
    {
        // Existing messages (unchanged — do not break any existing callers)
        public const string UserNotFound = "User not found.";
        public const string InvalidCredentials = "Invalid email or password.";
        public const string OrganizationNotFound = "Organization not found.";
        public const string EmployeeNotFound = "Employee not found.";
        public const string Unauthorized = "You are not authorized to perform this action.";
        public const string UserAlreadyExists = "A user with this email already exists.";
        public const string OrganizationAlreadyExists = "You already belong to an organization. Cannot create another.";
        public const string InvalidToken = "Invalid or expired token.";
        public const string NoOrganization = "User does not belong to any organization.";

        // New enterprise auth messages
        /// <summary>
        /// Returned when a previously rotated (used) refresh token is presented again.
        /// Indicates a potential token theft — all sessions are revoked as a security measure.
        /// </summary>
        public const string TokenReuseDetected =
            "Security violation detected: a revoked token was reused. All your sessions have been terminated for safety. Please log in again.";

        /// <summary>Returned when the session's absolute maximum lifetime has been exceeded.</summary>
        public const string SessionAbsoluteExpired =
            "Your session has reached its maximum lifetime. Please log in again.";

        /// <summary>Returned when a session lookup by SessionId returns no result.</summary>
        public const string SessionNotFound = "Session not found.";

        /// <summary>Returned when trying to revoke a session that is already revoked.</summary>
        public const string SessionAlreadyRevoked = "Session is already revoked.";

        /// <summary>Returned when a user tries to revoke a session belonging to another user.</summary>
        public const string SessionUnauthorized = "You are not authorized to revoke this session.";

        // New HRMS domain messages
        public const string DepartmentNotFound = "Department not found.";
        public const string LeaveRequestNotFound = "Leave request not found.";
        public const string AttendanceNotFound = "Attendance record not found.";
    }

    // ─── Audit Event Names ────────────────────────────────────────────────────

    /// <summary>
    /// Strongly-typed constants for the Event column in AuthAuditLogs and AuditLogs.
    /// Using constants prevents typo-based inconsistencies across the codebase.
    /// </summary>
    public static class AuditEvents
    {
        /// <summary>Successful login — new session created.</summary>
        public const string Login = "Login";

        /// <summary>Failed login attempt — invalid credentials.</summary>
        public const string LoginFailed = "LoginFailed";

        /// <summary>Successful token refresh — session rotated.</summary>
        public const string TokenRefreshed = "TokenRefreshed";

        /// <summary>User explicitly logged out from the current device.</summary>
        public const string Logout = "Logout";

        /// <summary>User explicitly logged out from all devices.</summary>
        public const string LogoutAll = "LogoutAll";

        /// <summary>A specific session was revoked via the sessions API.</summary>
        public const string SessionRevoked = "SessionRevoked";

        /// <summary>
        /// A previously revoked refresh token was presented — token theft suspected.
        /// All user sessions are revoked in response.
        /// </summary>
        public const string TokenReuseDetected = "TokenReuseDetected";

        /// <summary>A session expired (sliding or absolute) and was cleaned up.</summary>
        public const string SessionExpired = "SessionExpired";

        // Domain & Business Audit Events
        public const string CheckIn = "CheckIn";
        public const string CheckOut = "CheckOut";
        public const string AttendanceMarked = "AttendanceMarked";
        public const string LeaveTypeCreated = "LeaveTypeCreated";
        public const string LeaveApplied = "LeaveApplied";
        public const string LeaveApproved = "LeaveApproved";
        public const string LeaveRejected = "LeaveRejected";
        public const string LeaveCancelled = "LeaveCancelled";
        public const string SalaryGenerated = "SalaryGenerated";
        public const string SalaryPaid = "SalaryPaid";
        public const string DepartmentCreated = "DepartmentCreated";
        public const string DepartmentUpdated = "DepartmentUpdated";
        public const string DepartmentDeleted = "DepartmentDeleted";
        public const string EmployeeAssignedToDepartment = "EmployeeAssignedToDepartment";
        public const string EmployeeCreated = "EmployeeCreated";
        public const string EmployeeUpdated = "EmployeeUpdated";
        public const string EmployeeDeleted = "EmployeeDeleted";
        public const string DocumentUploaded = "DocumentUploaded";
        public const string DocumentDeleted = "DocumentDeleted";
    }

    // ─── File Upload Settings ──────────────────────────────────────────────────

    public static class FileUpload
    {
        /// <summary>Maximum allowed file upload size in bytes (default: 10 MB).</summary>
        public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        /// <summary>Allowed MIME types for document uploads.</summary>
        public static readonly string[] AllowedContentTypes =
        [
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/gif",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        ];

        /// <summary>Allowed file extensions (lowercase, with dot).</summary>
        public static readonly string[] AllowedExtensions =
        [
            ".pdf", ".jpg", ".jpeg", ".png", ".gif",
            ".doc", ".docx", ".xls", ".xlsx"
        ];
    }

    // ─── Document Messages ────────────────────────────────────────────────────

    public static class DocumentMessages
    {
        public const string DocumentNotFound = "Document not found or access denied.";
        public const string FileTooLarge = "File size exceeds the maximum limit of 10 MB.";
        public const string InvalidFileType = "File type is not allowed. Supported: PDF, JPEG, PNG, DOC, DOCX, XLS, XLSX.";
    }
}