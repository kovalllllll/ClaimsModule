namespace ClaimsModule.Application.Common;

public static class MockUserNames
{
    private static readonly Dictionary<Guid, string> Names = new()
    {
        [Guid.Parse("11111111-0000-0000-0000-000000000001")] = "John Handler",
        [Guid.Parse("22222222-0000-0000-0000-000000000002")] = "Sarah Supervisor",
        [Guid.Parse("33333333-0000-0000-0000-000000000003")] = "Mike Manager"
    };

    public static string? Resolve(Guid? userId) =>
        userId.HasValue && Names.TryGetValue(userId.Value, out var name) ? name : null;

    public static string Display(Guid? userId) =>
        Resolve(userId) ?? userId?.ToString() ?? "Unknown";

    public static IReadOnlyList<Guid> FindIdsByNameSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return [];

        var term = search.Trim();
        return Names
            .Where(kv => kv.Value.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Select(kv => kv.Key)
            .ToList();
    }
}
