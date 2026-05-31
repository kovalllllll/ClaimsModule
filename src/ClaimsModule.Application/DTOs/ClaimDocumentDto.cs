namespace ClaimsModule.Application.DTOs;

public sealed class ClaimDocumentDto
{
    public Guid Id { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string DocumentName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
    public Guid? UploadedByUserId { get; init; }
    public string? Notes { get; init; }
    public string SasUrl { get; init; } = string.Empty;
}
