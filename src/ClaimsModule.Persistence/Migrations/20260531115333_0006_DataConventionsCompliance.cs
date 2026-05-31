using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsModule.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _0006_DataConventionsCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ReserveHistory",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserCreated",
                table: "ReserveHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserModified",
                table: "ReserveHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Policies",
                type: "datetimeoffset(7)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Policies",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Policies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Policies",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserCreated",
                table: "Policies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserModified",
                table: "Policies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ClaimSequences",
                type: "datetimeoffset(7)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ClaimSequences",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserCreated",
                table: "ClaimSequences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserModified",
                table: "ClaimSequences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ClaimAuditLog",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserModified",
                table: "ClaimAuditLog",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CauseOfLossCodes",
                type: "datetimeoffset(7)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "CauseOfLossCodes",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CauseOfLossCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CauseOfLossCodes",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserCreated",
                table: "CauseOfLossCodes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserModified",
                table: "CauseOfLossCodes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ApiIdempotencyRecords",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserCreated",
                table: "ApiIdempotencyRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserModified",
                table: "ApiIdempotencyRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "CauseOfLossCodes",
                keyColumn: "CauseOfLossCodeId",
                keyValue: new Guid("bbbbbbbb-0001-0000-0000-000000000010"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "PolicyId",
                keyValue: new Guid("aaaaaaaa-0001-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "PolicyId",
                keyValue: new Guid("aaaaaaaa-0001-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "PolicyId",
                keyValue: new Guid("aaaaaaaa-0001-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "PolicyId",
                keyValue: new Guid("aaaaaaaa-0001-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "PolicyId",
                keyValue: new Guid("aaaaaaaa-0001-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "DeletedAt", "UpdatedAt", "UserCreated", "UserModified" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ReserveHistory");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "ReserveHistory");

            migrationBuilder.DropColumn(
                name: "UserModified",
                table: "ReserveHistory");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "UserModified",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ClaimSequences");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClaimSequences");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "ClaimSequences");

            migrationBuilder.DropColumn(
                name: "UserModified",
                table: "ClaimSequences");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClaimAuditLog");

            migrationBuilder.DropColumn(
                name: "UserModified",
                table: "ClaimAuditLog");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CauseOfLossCodes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CauseOfLossCodes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CauseOfLossCodes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CauseOfLossCodes");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "CauseOfLossCodes");

            migrationBuilder.DropColumn(
                name: "UserModified",
                table: "CauseOfLossCodes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ApiIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "ApiIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "UserModified",
                table: "ApiIdempotencyRecords");
        }
    }
}
