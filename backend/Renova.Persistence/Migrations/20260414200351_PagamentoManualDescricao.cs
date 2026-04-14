using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PagamentoManualDescricao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagamento_Movimentacao_MovimentacaoId",
                table: "Pagamento");

            migrationBuilder.AlterColumn<int>(
                name: "MovimentacaoId",
                table: "Pagamento",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Pagamento",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagamento_Movimentacao_MovimentacaoId",
                table: "Pagamento",
                column: "MovimentacaoId",
                principalTable: "Movimentacao",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagamento_Movimentacao_MovimentacaoId",
                table: "Pagamento");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Pagamento");

            migrationBuilder.AlterColumn<int>(
                name: "MovimentacaoId",
                table: "Pagamento",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagamento_Movimentacao_MovimentacaoId",
                table: "Pagamento",
                column: "MovimentacaoId",
                principalTable: "Movimentacao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
