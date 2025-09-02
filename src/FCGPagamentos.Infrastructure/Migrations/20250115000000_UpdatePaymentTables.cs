using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FCGPagamentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Criar tabela payment_events
            migrationBuilder.CreateTable(
                name: "payment_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    event_payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_events_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Adicionar índices para payment_events
            migrationBuilder.CreateIndex(
                name: "IX_payment_events_payment_id",
                table: "payment_events",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_events_event_type",
                table: "payment_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_payment_events_occurred_at",
                table: "payment_events",
                column: "occurred_at");

            // Atualizar tabela payments - remover colunas antigas e adicionar novas
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "payments");

            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Method",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Atualizar índice da tabela payments
            migrationBuilder.DropIndex(
                name: "IX_payments_UserId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_GameId",
                table: "payments");

            migrationBuilder.CreateIndex(
                name: "IX_payments_OrderId",
                table: "payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_CorrelationId",
                table: "payments",
                column: "CorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverter mudanças na tabela payments
            migrationBuilder.DropIndex(
                name: "IX_payments_OrderId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_CorrelationId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "payments");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GameId",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_payments_UserId",
                table: "payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_GameId",
                table: "payments",
                column: "GameId");

            // Remover tabela payment_events
            migrationBuilder.DropTable(
                name: "payment_events");
        }
    }
}
