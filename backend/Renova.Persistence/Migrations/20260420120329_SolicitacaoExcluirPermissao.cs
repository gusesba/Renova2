using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SolicitacaoExcluirPermissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Funcionalidade",
                columns: new[] { "Id", "Chave", "Descricao", "Grupo" },
                values: new object[] { 48, "solicitacoes.excluir", "Excluir solicitacoes da loja.", "Solicitacoes" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Funcionalidade",
                keyColumn: "Id",
                keyValue: 48);
        }
    }
}
