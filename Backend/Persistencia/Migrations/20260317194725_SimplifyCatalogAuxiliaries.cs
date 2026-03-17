using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyCatalogAuxiliaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_peca_categoria_categoria_id",
                table: "peca");

            migrationBuilder.DropForeignKey(
                name: "FK_peca_colecao_colecao_id",
                table: "peca");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "colecao");

            migrationBuilder.DropIndex(
                name: "IX_peca_categoria_id",
                table: "peca");

            migrationBuilder.DropIndex(
                name: "IX_peca_colecao_id",
                table: "peca");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "tamanho");

            migrationBuilder.DropColumn(
                name: "inativado_em",
                table: "tamanho");

            migrationBuilder.DropColumn(
                name: "ordem_exibicao",
                table: "tamanho");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "produto_nome");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "produto_nome");

            migrationBuilder.DropColumn(
                name: "inativado_em",
                table: "produto_nome");

            migrationBuilder.DropColumn(
                name: "categoria_id",
                table: "peca");

            migrationBuilder.DropColumn(
                name: "colecao_id",
                table: "peca");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "marca");

            migrationBuilder.DropColumn(
                name: "inativado_em",
                table: "marca");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "cor");

            migrationBuilder.DropColumn(
                name: "hexadecimal",
                table: "cor");

            migrationBuilder.DropColumn(
                name: "inativado_em",
                table: "cor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "tamanho",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "inativado_em",
                table: "tamanho",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ordem_exibicao",
                table: "tamanho",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "produto_nome",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "produto_nome",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "inativado_em",
                table: "produto_nome",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "categoria_id",
                table: "peca",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "colecao_id",
                table: "peca",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "marca",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "inativado_em",
                table: "marca",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "cor",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "hexadecimal",
                table: "cor",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "inativado_em",
                table: "cor",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.id);
                    table.ForeignKey(
                        name: "FK_categoria_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "colecao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ano_referencia = table.Column<int>(type: "integer", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_colecao", x => x.id);
                    table.ForeignKey(
                        name: "FK_colecao_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_peca_categoria_id",
                table: "peca",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_colecao_id",
                table: "peca",
                column: "colecao_id");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_loja_id_nome",
                table: "categoria",
                columns: new[] { "loja_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_colecao_loja_id_nome",
                table: "colecao",
                columns: new[] { "loja_id", "nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_peca_categoria_categoria_id",
                table: "peca",
                column: "categoria_id",
                principalTable: "categoria",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_peca_colecao_colecao_id",
                table: "peca",
                column: "colecao_id",
                principalTable: "colecao",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
