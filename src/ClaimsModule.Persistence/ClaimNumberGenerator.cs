using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Domain.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClaimsModule.Persistence;

internal sealed class ClaimNumberGenerator(ClaimsDbContext context) : IClaimNumberGenerator
{
    /// <summary>
    /// Atomically allocates the next sequence value for the given organisation and year.
    /// Uses UPDATE ... OUTPUT to perform a single-statement read-modify-write with an
    /// implicit row-level lock, preventing duplicate allocation under concurrent load.
    /// If no row exists yet for this (Year, OrganisationId) pair, a MERGE with HOLDLOCK
    /// inserts the first counter row and returns sequence value 1.
    /// The counter never decrements; soft-deleted claims retain their numbers permanently.
    /// </summary>
    public async Task<int> AllocateNextSequenceAsync(
        Guid organisationId,
        int year,
        CancellationToken cancellationToken = default)
    {
        // Phase 1: attempt to increment an existing row and capture the PREVIOUS value,
        // which is the sequence number assigned to the caller.
        var allocated = await TryIncrementExistingAsync(organisationId, year, cancellationToken);

        if (allocated.HasValue)
        {
            return allocated.Value;
        }

        // Phase 2: first claim of the year for this org — insert the row with NextValue = 2
        // (the 1 slot is the one we are allocating right now) and return 1.
        // MERGE with HOLDLOCK prevents two concurrent "first claim" threads from both
        // inserting and causing a unique-constraint violation.
        return await AllocateFirstAsync(organisationId, year, cancellationToken);
    }

    private async Task<int?> TryIncrementExistingAsync(
        Guid organisationId,
        int year,
        CancellationToken ct)
    {
        const string sql = """
            UPDATE ClaimSequences WITH (ROWLOCK, UPDLOCK)
            SET    NextValue = NextValue + 1
            OUTPUT DELETED.NextValue
            WHERE  Year           = @year
              AND  OrganisationId = @orgId
            """;

        var yearParam = new SqlParameter("@year", year);
        var orgParam = new SqlParameter("@orgId", organisationId);

        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(yearParam);
        command.Parameters.Add(orgParam);

        if (context.Database.CurrentTransaction is not null)
        {
            command.Transaction = context.Database.CurrentTransaction.GetDbTransaction();
        }

        await context.Database.OpenConnectionAsync(ct);

        var raw = await command.ExecuteScalarAsync(ct);
        return raw is null or DBNull ? null : Convert.ToInt32(raw);
    }

    private async Task<int> AllocateFirstAsync(
        Guid organisationId,
        int year,
        CancellationToken ct)
    {
        // MERGE with HOLDLOCK prevents phantom inserts from concurrent threads
        // hitting this path simultaneously for the same (Year, OrganisationId).
        const string sql = """
            MERGE ClaimSequences WITH (HOLDLOCK) AS target
            USING (VALUES (@year, @orgId)) AS src (Year, OrganisationId)
              ON  target.Year           = src.Year
              AND target.OrganisationId = src.OrganisationId
            WHEN NOT MATCHED THEN
                INSERT (SequenceId, Year, OrganisationId, NextValue)
                VALUES (@sequenceId, src.Year, src.OrganisationId, 2)
            WHEN MATCHED THEN
                UPDATE SET NextValue = target.NextValue + 1
            OUTPUT DELETED.NextValue;
            """;

        var yearParam = new SqlParameter("@year", year);
        var orgParam = new SqlParameter("@orgId", organisationId);
        var seqIdParam = new SqlParameter("@sequenceId", EntityId.New());

        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(yearParam);
        command.Parameters.Add(orgParam);
        command.Parameters.Add(seqIdParam);

        if (context.Database.CurrentTransaction is not null)
        {
            command.Transaction = context.Database.CurrentTransaction.GetDbTransaction();
        }

        await context.Database.OpenConnectionAsync(ct);

        var raw = await command.ExecuteScalarAsync(ct);

        // When MATCHED branch ran (race: another thread inserted between phase 1 and phase 2),
        // DELETED.NextValue holds the value before that update — use it.
        // When NOT MATCHED branch ran, DELETED.NextValue is NULL — sequence 1 was allocated.
        return raw is null or DBNull ? 1 : Convert.ToInt32(raw);
    }
}
