using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Hickory.Api.src.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "KnowledgeArticles",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "setweight(to_tsvector('english', coalesce(\"Title\", '')), 'A') || setweight(to_tsvector('english', coalesce(\"Content\", '')), 'B')",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedToId_Status_Priority",
                table: "Tickets",
                columns: new[] { "AssignedToId", "Status", "Priority" },
                filter: "\"AssignedToId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CategoryId_Status_CreatedAt",
                table: "Tickets",
                columns: new[] { "CategoryId", "Status", "CreatedAt" },
                filter: "\"CategoryId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SubmitterId_Status_CreatedAt",
                table: "Tickets",
                columns: new[] { "SubmitterId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UpdatedAt",
                table: "Tickets",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UpdatedAt_Status",
                table: "Tickets",
                columns: new[] { "UpdatedAt", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_Status_CategoryId_PublishedAt",
                table: "KnowledgeArticles",
                columns: new[] { "Status", "CategoryId", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TicketId_IsInternal_CreatedAt",
                table: "Comments",
                columns: new[] { "TicketId", "IsInternal", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_AssignedToId_Status_Priority",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CategoryId_Status_CreatedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SubmitterId_Status_CreatedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UpdatedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UpdatedAt_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_KnowledgeArticles_Status_CategoryId_PublishedAt",
                table: "KnowledgeArticles");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TicketId_IsInternal_CreatedAt",
                table: "Comments");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "KnowledgeArticles",
                type: "tsvector",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "setweight(to_tsvector('english', coalesce(\"Title\", '')), 'A') || setweight(to_tsvector('english', coalesce(\"Content\", '')), 'B')");
        }
    }
}
