using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Documents.Commands.UploadClaimDocument;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Documents;
using ClaimsModule.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClaimsModule.Application.Tests.Documents;

public sealed class UploadClaimDocumentCommandHandlerTests
{
    private static readonly Guid OrganisationId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid ClaimId = Guid.NewGuid();
    private const string IdempotencyKey = "doc-upload-key-1";

    [Fact]
    public async Task Handle_when_idempotency_record_exists_returns_cached_document_without_upload()
    {
        var cached = ClaimDocument.Create(
            ClaimId,
            OrganisationId,
            DocumentType.Invoice,
            "invoice.pdf",
            $"{OrganisationId}/{ClaimId}/file.pdf",
            "application/pdf",
            100,
            DateTimeOffset.UtcNow,
            Guid.Parse("11111111-0000-0000-0000-000000000001"));

        var idempotency = new Mock<IApiIdempotencyRepository>();
        idempotency.Setup(i => i.FindAsync(
                OrganisationId,
                UploadClaimDocumentCommandHandler.IdempotencyOperation,
                IdempotencyKey,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiIdempotencyRecord
            {
                ResourceId = cached.Id,
                OrganisationId = OrganisationId,
                Key = IdempotencyKey,
                Operation = UploadClaimDocumentCommandHandler.IdempotencyOperation
            });

        var documents = new Mock<IDocumentRepository>();
        documents.Setup(d => d.GetByIdAsync(ClaimId, cached.Id, OrganisationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var storage = new Mock<IStorageService>();

        var handler = new UploadClaimDocumentCommandHandler(
            Mock.Of<IClaimRepository>(),
            documents.Object,
            idempotency.Object,
            Mock.Of<IUnitOfWork>(),
            storage.Object,
            Mock.Of<ICurrentUserService>(),
            Mock.Of<ISystemClock>());

        var result = await handler.Handle(
            new UploadClaimDocumentCommand(
                ClaimId,
                OrganisationId,
                DocumentType.Invoice,
                "invoice.pdf",
                "application/pdf",
                100,
                new MemoryStream([1]),
                IdempotencyKey: IdempotencyKey),
            CancellationToken.None);

        result.Should().Be(cached.Id);
        storage.Verify(
            s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
