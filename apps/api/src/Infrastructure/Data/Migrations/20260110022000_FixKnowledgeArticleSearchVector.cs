using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Hickory.Api.src.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixKnowledgeArticleSearchVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing SearchVector column
            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "KnowledgeArticles");

            // Recreate SearchVector as a computed column using to_tsvector
            // Weight 'A' for title (highest priority), 'B' for content
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "KnowledgeArticles",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "setweight(to_tsvector('english', coalesce(\"Title\", '')), 'A') || setweight(to_tsvector('english', coalesce(\"Content\", '')), 'B')",
                stored: true);

            // Recreate the GIN index for full-text search
            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_SearchVector",
                table: "KnowledgeArticles",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the computed column and index
            migrationBuilder.DropIndex(
                name: "IX_KnowledgeArticles_SearchVector",
                table: "KnowledgeArticles");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "KnowledgeArticles");

            // Restore the original nullable SearchVector column
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "KnowledgeArticles",
                type: "tsvector",
                nullable: true);

            // Recreate the original index
            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_SearchVector",
                table: "KnowledgeArticles",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "gin");
        }
    }
}
