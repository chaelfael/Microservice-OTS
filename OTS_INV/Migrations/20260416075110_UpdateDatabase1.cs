using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTS_INV.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IS_SOLD",
                table: "TICKET");

            migrationBuilder.AddColumn<string>(
                name: "RESERVED_BY_ORDER_NO",
                table: "TICKET",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "STATUS",
                table: "TICKET",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RESERVED_BY_ORDER_NO",
                table: "TICKET");

            migrationBuilder.DropColumn(
                name: "STATUS",
                table: "TICKET");

            migrationBuilder.AddColumn<bool>(
                name: "IS_SOLD",
                table: "TICKET",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
