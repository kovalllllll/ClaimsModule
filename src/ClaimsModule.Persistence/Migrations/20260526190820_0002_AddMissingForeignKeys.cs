using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsModule.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _0002_AddMissingForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ClaimReserveComponents_ClaimId",
                table: "ClaimReserveComponents",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimAuditLog_ClaimId",
                table: "ClaimAuditLog",
                column: "ClaimId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimAuditLog_Claims_ClaimId",
                table: "ClaimAuditLog",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "ClaimId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimReserveComponents_Claims_ClaimId",
                table: "ClaimReserveComponents",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "ClaimId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimAuditLog_Claims_ClaimId",
                table: "ClaimAuditLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ClaimReserveComponents_Claims_ClaimId",
                table: "ClaimReserveComponents");

            migrationBuilder.DropIndex(
                name: "IX_ClaimReserveComponents_ClaimId",
                table: "ClaimReserveComponents");

            migrationBuilder.DropIndex(
                name: "IX_ClaimAuditLog_ClaimId",
                table: "ClaimAuditLog");
        }
    }
}
