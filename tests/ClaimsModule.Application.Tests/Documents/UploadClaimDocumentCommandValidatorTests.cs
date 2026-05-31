using ClaimsModule.Application.Documents.Commands.UploadClaimDocument;
using ClaimsModule.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Documents;

public sealed class UploadClaimDocumentCommandValidatorTests
{
    private readonly UploadClaimDocumentCommandValidator _validator = new();

    [Fact]
    public void ContentType_legacy_doc_is_rejected()
    {
        var command = CreateCommand(contentType: "application/msword");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("text/plain")]
    [InlineData("text/csv")]
    public void ContentType_frs_allowlist_is_accepted(string contentType)
    {
        var command = CreateCommand(contentType: contentType);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.ContentType);
    }

    private static UploadClaimDocumentCommand CreateCommand(string contentType) =>
        new(
            ClaimId: Guid.NewGuid(),
            OrganisationId: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            DocumentType: DocumentType.Other,
            DocumentName: "test.pdf",
            ContentType: contentType,
            FileSizeBytes: 1024,
            FileContent: new MemoryStream([1, 2, 3]));
}
