using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClienteDetalhePerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProdutoEstoque_LojaId_FornecedorId_Entrada",
                table: "ProdutoEstoque",
                columns: new[] { "LojaId", "FornecedorId", "Entrada" });

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoEstoque_LojaId_Situacao",
                table: "ProdutoEstoque",
                columns: new[] { "LojaId", "Situacao" });

            migrationBuilder.CreateIndex(
                name: "IX_PagamentoCredito_LojaId_ClienteId_Tipo_Data",
                table: "PagamentoCredito",
                columns: new[] { "LojaId", "ClienteId", "Tipo", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacao_LojaId_ClienteId_Tipo_Data",
                table: "Movimentacao",
                columns: new[] { "LojaId", "ClienteId", "Tipo", "Data" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProdutoEstoque_LojaId_FornecedorId_Entrada",
                table: "ProdutoEstoque");

            migrationBuilder.DropIndex(
                name: "IX_ProdutoEstoque_LojaId_Situacao",
                table: "ProdutoEstoque");

            migrationBuilder.DropIndex(
                name: "IX_PagamentoCredito_LojaId_ClienteId_Tipo_Data",
                table: "PagamentoCredito");

            migrationBuilder.DropIndex(
                name: "IX_Movimentacao_LojaId_ClienteId_Tipo_Data",
                table: "Movimentacao");
        }
    }
}
