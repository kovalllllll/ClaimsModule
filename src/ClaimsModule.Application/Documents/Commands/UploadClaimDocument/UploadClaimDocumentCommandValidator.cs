using ClaimsModule.Domain.Enums;
using FluentValidation;

namespace ClaimsModule.Application.Documents.Commands.UploadClaimDocument;

public sealed class UploadClaimDocumentCommandValidator : AbstractValidator<UploadClaimDocumentCommand>
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain",
        "text/csv"
    };

    private const long MaxFileSizeBytes = 50 * 1024 * 1024;

    public UploadClaimDocumentCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("ClaimId is required.");

        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("OrganisationId is required.");

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("DocumentType is not a recognised value.");

        RuleFor(x => x.DocumentName)
            .NotEmpty().WithMessage("Document name is required.")
            .MaximumLength(255).WithMessage("Document name must not exceed 255 characters.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than zero.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("File size must not exceed 50 MB.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => AllowedMimeTypes.Contains(ct))
            .WithMessage("File type is not permitted. Allowed types: PDF, JPEG, PNG, DOCX, XLSX, TXT, CSV.");

        RuleFor(x => x.FileContent)
            .NotNull().WithMessage("File content is required.");
    }
}
