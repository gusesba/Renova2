using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLojaConfiguracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loja_configuracao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loja_configuracao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_exibicao = table.Column<string>(type: "text", nullable: false),
                    cabecalho_impressao = table.Column<string>(type: "text", nullable: false),
                    rodape_impressao = table.Column<string>(type: "text", nullable: false),
                    usa_modelo_unico_etiqueta = table.Column<bool>(type: "boolean", nullable: false),
                    usa_modelo_unico_recibo = table.Column<bool>(type: "boolean", nullable: false),
                    fuso_horario = table.Column<string>(type: "text", nullable: false),
                    moeda = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loja_configuracao", x => x.id);
                    table.ForeignKey(
                        name: "FK_loja_configuracao_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_loja_configuracao_loja_id",
                table: "loja_configuracao",
                column: "loja_id",
                unique: true);
        }
    }
}
