using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OTS_ORDER.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ORDERS",
                columns: table => new
                {
                    ORDER_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ORDER_NO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CUSTOMER_EMAIL = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ORDER_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric", nullable: false),
                    ORDER_STATUS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERS", x => x.ORDER_ID);
                });

            migrationBuilder.CreateTable(
                name: "ORDER_DETAILS",
                columns: table => new
                {
                    ORDER_DETAIL_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TICKET_NUMBER = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ORDER_ID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_DETAILS", x => x.ORDER_DETAIL_ID);
                    table.ForeignKey(
                        name: "FK_ORDER_DETAILS_ORDERS_ORDER_ID",
                        column: x => x.ORDER_ID,
                        principalTable: "ORDERS",
                        principalColumn: "ORDER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAYMENTS",
                columns: table => new
                {
                    PAYMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ORDER_ID = table.Column<long>(type: "bigint", nullable: false),
                    PAYMENT_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PAYMENT_METHOD = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PAYMENT_STATUS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENTS", x => x.PAYMENT_ID);
                    table.ForeignKey(
                        name: "FK_PAYMENTS_ORDERS_ORDER_ID",
                        column: x => x.ORDER_ID,
                        principalTable: "ORDERS",
                        principalColumn: "ORDER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_DETAILS_ORDER_ID",
                table: "ORDER_DETAILS",
                column: "ORDER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENTS_ORDER_ID",
                table: "PAYMENTS",
                column: "ORDER_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ORDER_DETAILS");

            migrationBuilder.DropTable(
                name: "PAYMENTS");

            migrationBuilder.DropTable(
                name: "ORDERS");
        }
    }
}
