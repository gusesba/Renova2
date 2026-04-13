using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PagamentoCreditoFormaPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfigLojaFormaPagamentoId",
                table: "PagamentoCredito",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagamentoCredito_ConfigLojaFormaPagamentoId",
                table: "PagamentoCredito",
                column: "ConfigLojaFormaPagamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PagamentoCredito_ConfigLojaFormaPagamento_ConfigLojaFormaPa~",
                table: "PagamentoCredito",
                column: "ConfigLojaFormaPagamentoId",
                principalTable: "ConfigLojaFormaPagamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PagamentoCredito_ConfigLojaFormaPagamento_ConfigLojaFormaPa~",
                table: "PagamentoCredito");

            migrationBuilder.DropIndex(
                name: "IX_PagamentoCredito_ConfigLojaFormaPagamentoId",
                table: "PagamentoCredito");

            migrationBuilder.DropColumn(
                name: "ConfigLojaFormaPagamentoId",
                table: "PagamentoCredito");
        }
    }
}
