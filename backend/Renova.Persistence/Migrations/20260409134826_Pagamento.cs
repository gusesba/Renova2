using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Pagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigLoja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LojaId = table.Column<int>(type: "integer", nullable: false),
                    PercentualRepasseFornecedor = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigLoja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigLoja_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovimentacaoId = table.Column<int>(type: "integer", nullable: false),
                    LojaId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Natureza = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagamento_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagamento_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagamento_Movimentacao_MovimentacaoId",
                        column: x => x.MovimentacaoId,
                        principalTable: "Movimentacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigLoja_LojaId",
                table: "ConfigLoja",
                column: "LojaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_ClienteId",
                table: "Pagamento",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_LojaId",
                table: "Pagamento",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_MovimentacaoId",
                table: "Pagamento",
                column: "MovimentacaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigLoja");

            migrationBuilder.DropTable(
                name: "Pagamento");
        }
    }
}
