using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Hickory.Api.src.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeArticles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HelpfulCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NotHelpfulCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeArticles_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KnowledgeArticles_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KnowledgeArticles_Users_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTags",
                columns: table => new
                {
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTags", x => new { x.ArticleId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ArticleTags_KnowledgeArticles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "KnowledgeArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTags_TagId",
                table: "ArticleTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_AuthorId",
                table: "KnowledgeArticles",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_CategoryId",
                table: "KnowledgeArticles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_CreatedAt",
                table: "KnowledgeArticles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_LastUpdatedById",
                table: "KnowledgeArticles",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_PublishedAt",
                table: "KnowledgeArticles",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_SearchVector",
                table: "KnowledgeArticles",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_Status",
                table: "KnowledgeArticles",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleTags");

            migrationBuilder.DropTable(
                name: "KnowledgeArticles");
        }
    }
}
