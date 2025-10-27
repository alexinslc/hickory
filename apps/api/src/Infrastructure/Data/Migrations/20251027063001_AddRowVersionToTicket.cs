using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hickory.Api.src.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tickets",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Tickets");
        }
    }
}
