using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saga.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeSagaFieldsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_states_payment_id",
                schema: "saga",
                table: "order_states");

            migrationBuilder.AlterColumn<Guid>(
                name: "payment_id",
                schema: "saga",
                table: "order_states",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "currency",
                schema: "saga",
                table: "order_states",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                schema: "saga",
                table: "order_states",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_order_states_payment_id",
                schema: "saga",
                table: "order_states",
                column: "payment_id",
                unique: true,
                filter: "[payment_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_states_payment_id",
                schema: "saga",
                table: "order_states");

            migrationBuilder.AlterColumn<Guid>(
                name: "payment_id",
                schema: "saga",
                table: "order_states",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "currency",
                schema: "saga",
                table: "order_states",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                schema: "saga",
                table: "order_states",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_states_payment_id",
                schema: "saga",
                table: "order_states",
                column: "payment_id",
                unique: true);
        }
    }
}
