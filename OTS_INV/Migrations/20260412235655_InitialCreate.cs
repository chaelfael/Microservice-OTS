using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OTS_INV.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TICKET_CLASS",
                columns: table => new
                {
                    TICKET_CLASS_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TICKET_CLASS_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TICKET_CLASS_NAME = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PRICE = table.Column<decimal>(type: "numeric", nullable: false),
                    CAPACITY = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TICKET_CLASS", x => x.TICKET_CLASS_ID);
                });

            migrationBuilder.CreateTable(
                name: "TICKET",
                columns: table => new
                {
                    TICKET_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TICKET_NUMBER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IS_SOLD = table.Column<bool>(type: "boolean", nullable: false),
                    TICKET_CLASS_ID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TICKET", x => x.TICKET_ID);
                    table.ForeignKey(
                        name: "FK_TICKET_TICKET_CLASS_TICKET_CLASS_ID",
                        column: x => x.TICKET_CLASS_ID,
                        principalTable: "TICKET_CLASS",
                        principalColumn: "TICKET_CLASS_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TICKET_TICKET_CLASS_ID",
                table: "TICKET",
                column: "TICKET_CLASS_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TICKET");

            migrationBuilder.DropTable(
                name: "TICKET_CLASS");
        }
    }
}
