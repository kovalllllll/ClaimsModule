using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Documents;
using MediatR;

namespace ClaimsModule.Application.Documents.Commands.UploadClaimDocument;

public sealed class UploadClaimDocumentCommandHandler(
    IClaimRepository claims,
    IDocumentRepository documents,
    IApiIdempotencyRepository idempotency,
    IUnitOfWork unitOfWork,
    IStorageService storage,
    ICurrentUserService currentUser,
    ISystemClock clock)
    : IRequestHandler<UploadClaimDocumentCommand, Guid>
{
    public const string IdempotencyOperation = "UploadClaimDocument";

    public async Task<Guid> Handle(UploadClaimDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await idempotency.FindAsync(
                request.OrganisationId,
                IdempotencyOperation,
                request.IdempotencyKey,
                cancellationToken);

            if (existing is not null)
            {
                var cached = await documents.GetByIdAsync(
                    request.ClaimId,
                    existing.ResourceId,
                    request.OrganisationId,
                    cancellationToken);

                if (cached is not null)
                    return cached.Id;

                throw new KeyNotFoundException(
                    $"Document {existing.ResourceId} from idempotency record was not found.");
            }
        }

        if (!await claims.ExistsAsync(request.ClaimId, request.OrganisationId, cancellationToken))
            throw new KeyNotFoundException($"Claim {request.ClaimId} not found.");

        var sanitisedName = SanitiseFilename(request.DocumentName);
        var blobPath = $"{request.OrganisationId}/{request.ClaimId}/{sanitisedName}";

        await storage.UploadAsync(blobPath, request.FileContent, request.ContentType, cancellationToken);

        var now = clock.UtcNow;
        var document = ClaimDocument.Create(
            claimId: request.ClaimId,
            organisationId: request.OrganisationId,
            documentType: request.DocumentType,
            documentName: request.DocumentName,
            blobPath: blobPath,
            contentType: request.ContentType,
            fileSizeBytes: request.FileSizeBytes,
            uploadedAt: now,
            uploadedByUserId: currentUser.UserId,
            notes: request.Notes);

        await documents.AddAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            await idempotency.AddAsync(new ApiIdempotencyRecord
            {
                Id = EntityId.New(),
                OrganisationId = request.OrganisationId,
                Key = request.IdempotencyKey,
                Operation = IdempotencyOperation,
                ResourceId = document.Id,
                CreatedAt = now
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return document.Id;
    }

    private static string SanitiseFilename(string name)
    {
        var stripped = name
            .Replace("..", "_")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(":", "_");

        var invalid = Path.GetInvalidFileNameChars();
        var clean = string.Concat(stripped.Select(c => invalid.Contains(c) ? '_' : c));
        return $"{Guid.NewGuid():N}_{clean}";
    }
}
