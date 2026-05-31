using System.Text.Json;

namespace ClaimsModule.Application.Common.Audit;

public static class AuditJsonValues
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Status(string status) => Serialize(new { status });

    public static string Reason(string? reason) => Serialize(new { reason });

    public static string RejectionReason(string rejectionReason) => Serialize(new { rejectionReason });

    public static string JournalEntry(string journal) => Serialize(new { journal });

    public static string FailureReason(string failureReason) => Serialize(new { failureReason });

    public static string ValidationIssue(string message, string? severity = null) =>
        Serialize(new { message, severity });

    public static string Notes(string? notes) => Serialize(new { notes });

    private static string Serialize(object value) =>
        JsonSerializer.Serialize(value, Options);
}
