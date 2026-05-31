using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Documents.Queries.GetClaimDocuments;

public sealed record GetClaimDocumentsQuery(Guid ClaimId, Guid OrganisationId) : IRequest<IReadOnlyList<ClaimDocumentDto>>;
