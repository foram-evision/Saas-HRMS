using FluentValidation;
using HRMS.Application.Common;
using HRMS.Application.DTOs.Document;
using Microsoft.AspNetCore.Http;

namespace HRMS.Application.Validators;

/// <summary>Validates incoming requests for document uploads.</summary>
public class UploadDocumentValidator : AbstractValidator<UploadDocumentDto>
{
    public UploadDocumentValidator()
    {
        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("Invalid document type.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required.")
            .Must(BeAValidFile).WithMessage(AppConstants.DocumentMessages.InvalidFileType)
            .Must(BeWithinSizeLimit).WithMessage(AppConstants.DocumentMessages.FileTooLarge)
            .When(x => x.File != null);
    }

    private bool BeAValidFile(IFormFile file)
    {
        if (file == null) return false;

        // Check content type
        if (!AppConstants.FileUpload.AllowedContentTypes.Contains(file.ContentType.ToLower()))
            return false;

        // Check extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AppConstants.FileUpload.AllowedExtensions.Contains(extension);
    }

    private bool BeWithinSizeLimit(IFormFile file)
    {
        return file != null && file.Length <= AppConstants.FileUpload.MaxFileSizeBytes;
    }
}
