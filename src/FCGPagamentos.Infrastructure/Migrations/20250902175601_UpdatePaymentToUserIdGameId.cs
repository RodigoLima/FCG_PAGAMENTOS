using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCGPagamentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentToUserIdGameId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "payments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_OrderId",
                table: "payments",
                newName: "IX_payments_UserId");

            migrationBuilder.AddColumn<string>(
                name: "GameId",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_payments_GameId",
                table: "payments",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payments_GameId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "payments",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_UserId",
                table: "payments",
                newName: "IX_payments_OrderId");
        }
    }
}
