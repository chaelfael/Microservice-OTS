using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTS_ORDER.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PAID_AMT",
                table: "PAYMENTS",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PAID_AMT",
                table: "PAYMENTS");
        }
    }
}
