using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsModule.Persistence.Migrations;

/// <summary>
/// BR-A-01 / FRS 14.2: database-level append-only enforcement for ClaimAuditLog only.
/// ReserveHistory is unchanged (approve/reject/GL posting require in-place status updates).
/// </summary>
public partial class _0005_ClaimAuditLogAppendOnlyTrigger : Migration
{
    public const string TriggerName = "TR_ClaimAuditLog_AppendOnly_BR_A_01";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"""
            CREATE OR ALTER TRIGGER [{TriggerName}]
            ON [ClaimAuditLog]
            AFTER UPDATE, DELETE
            AS
            BEGIN
                SET NOCOUNT ON;
                RAISERROR (
                    N'ClaimAuditLog is append-only (BR-A-01). UPDATE and DELETE are not permitted.',
                    16,
                    1);
                ROLLBACK TRANSACTION;
            END;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"""
            DROP TRIGGER IF EXISTS [{TriggerName}] ON [ClaimAuditLog];
            """);
    }
}
