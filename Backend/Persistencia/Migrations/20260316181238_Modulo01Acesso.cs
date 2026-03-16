using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Modulo01Acesso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "loja_ativa_id",
                table: "usuario_sessao",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "usuario_recuperacao_acesso",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    solicitado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    utilizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ip = table.Column<string>(type: "text", nullable: false),
                    user_agent = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_recuperacao_acesso", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_recuperacao_acesso_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuario_sessao_loja_ativa_id",
                table: "usuario_sessao",
                column: "loja_ativa_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_recuperacao_acesso_usuario_id",
                table: "usuario_recuperacao_acesso",
                column: "usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_sessao_loja_loja_ativa_id",
                table: "usuario_sessao",
                column: "loja_ativa_id",
                principalTable: "loja",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuario_sessao_loja_loja_ativa_id",
                table: "usuario_sessao");

            migrationBuilder.DropTable(
                name: "usuario_recuperacao_acesso");

            migrationBuilder.DropIndex(
                name: "IX_usuario_sessao_loja_ativa_id",
                table: "usuario_sessao");

            migrationBuilder.DropColumn(
                name: "loja_ativa_id",
                table: "usuario_sessao");
        }
    }
}
