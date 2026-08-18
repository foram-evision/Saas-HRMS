namespace HRMS.Domain.Enums;

/// <summary>
/// Classifies the type of document uploaded for an employee.
/// </summary>
public enum DocumentType
{
    /// <summary>Employee's curriculum vitae / resume.</summary>
    Resume = 1,

    /// <summary>Official offer letter issued to the employee.</summary>
    OfferLetter = 2,

    /// <summary>Government-issued identity proof (Aadhaar, Passport, etc.).</summary>
    IdentityProof = 3,

    /// <summary>Academic or professional certificates and degrees.</summary>
    Certificate = 4,

    /// <summary>Any other supporting document not covered by the above types.</summary>
    Other = 5
}
