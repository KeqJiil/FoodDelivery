using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "deliveries");

            migrationBuilder.EnsureSchema(
                name: "restaurants");

            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.EnsureSchema(
                name: "order_requests");

            migrationBuilder.EnsureSchema(
                name: "saga");

            migrationBuilder.EnsureSchema(
                name: "payments");

            migrationBuilder.CreateTable(
                name: "deliveries",
                schema: "deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    failure_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deliveries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_requests",
                schema: "order_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_states",
                schema: "saga",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    current_state = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    failed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    approval_timeout_token_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    payment_timeout_token_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    payment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    currency = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_states", x => x.correlation_id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    restaurant_ref_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    failure_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "restaurants",
                schema: "restaurants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_lines",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    menu_item_ref_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quantity = table.Column<byte>(type: "tinyint", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_lines_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "ordering",
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                schema: "restaurants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    restaurant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_menu_items_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalSchema: "restaurants",
                        principalTable: "restaurants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "OpeningWindow",
                schema: "restaurants",
                columns: table => new
                {
                    ScheduleRestaurantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OpenDay = table.Column<int>(type: "int", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CloseDay = table.Column<int>(type: "int", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningWindow", x => new { x.ScheduleRestaurantId, x.Id });
                    table.ForeignKey(
                        name: "FK_OpeningWindow_restaurants_ScheduleRestaurantId",
                        column: x => x.ScheduleRestaurantId,
                        principalSchema: "restaurants",
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_delivery_order_id",
                schema: "deliveries",
                table: "deliveries",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_restaurant_id",
                schema: "restaurants",
                table: "menu_items",
                column: "restaurant_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_lines_order_id",
                schema: "ordering",
                table: "order_lines",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "menu_ref_index",
                schema: "ordering",
                table: "order_lines",
                column: "menu_item_ref_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_restaurant_id",
                schema: "order_requests",
                table: "order_requests",
                columns: new[] { "order_id", "restaurant_id" },
                unique: true,
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_order_states_payment_id",
                schema: "saga",
                table: "order_states",
                column: "payment_id",
                unique: true,
                filter: "[payment_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_payment_order_id",
                schema: "payments",
                table: "payments",
                column: "order_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deliveries",
                schema: "deliveries");

            migrationBuilder.DropTable(
                name: "menu_items",
                schema: "restaurants");

            migrationBuilder.DropTable(
                name: "OpeningWindow",
                schema: "restaurants");

            migrationBuilder.DropTable(
                name: "order_lines",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "order_requests",
                schema: "order_requests");

            migrationBuilder.DropTable(
                name: "order_states",
                schema: "saga");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "restaurants",
                schema: "restaurants");

            migrationBuilder.DropTable(
                name: "orders",
                schema: "ordering");
        }
    }
}
