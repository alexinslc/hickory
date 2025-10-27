using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Hickory.Api.src.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchVectorToTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Tickets",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('english', coalesce(\"TicketNumber\", '') || ' ' || coalesce(\"Title\", '') || ' ' || coalesce(\"Description\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SearchVector",
                table: "Tickets",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_SearchVector",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Tickets");
        }
    }
}
