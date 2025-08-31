using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCGPagamentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoneyValueObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar colunas para o ValueObject Money
            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                table: "payments",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "payments",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "BRL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover colunas do ValueObject Money
            migrationBuilder.DropColumn(
                name: "amount",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "payments");
        }
    }
}
