using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hickory.Api.src.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp_Action_UserId",
                table: "AuditLogs",
                columns: new[] { "Timestamp", "Action", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
