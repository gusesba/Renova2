using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Produto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Valor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LojaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cor_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Marca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Valor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LojaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Marca_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoReferencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Valor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LojaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoReferencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProdutoReferencia_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tamanho",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Valor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LojaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tamanho", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tamanho_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    MarcaId = table.Column<int>(type: "integer", nullable: false),
                    TamanhoId = table.Column<int>(type: "integer", nullable: false),
                    CorId = table.Column<int>(type: "integer", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Entrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LojaId = table.Column<int>(type: "integer", nullable: false),
                    Situacao = table.Column<int>(type: "integer", nullable: false),
                    Consignado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProdutoEstoque_Cliente_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProdutoEstoque_Cor_CorId",
                        column: x => x.CorId,
                        principalTable: "Cor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProdutoEstoque_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProdutoEstoque_Marca_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProdutoEstoque_ProdutoReferencia_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "ProdutoReferencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProdutoEstoque_Tamanho_TamanhoId",
                        column: x => x.TamanhoId,
                        principalTable: "Tamanho",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cor_LojaId_Valor",
                table: "Cor",
                columns: new[] { "LojaId", "Valor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marca_LojaId_Valor",
                table: "Marca",
                columns: new[] { "LojaId", "Valor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoEstoque_CorId",
                table: "ProdutoEstoque",
                column: "CorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoEstoque_FornecedorId",
                table: "ProdutoEstoque",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoEstoque_LojaId",
                table: "ProdutoEstoque",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoEstoque_MarcaId",
                table: "ProdutoEstoque",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoEstoque_ProdutoId",
                table: "ProdutoEstoque",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoEstoque_TamanhoId",
                table: "ProdutoEstoque",
                column: "TamanhoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoReferencia_LojaId_Valor",
                table: "ProdutoReferencia",
                columns: new[] { "LojaId", "Valor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tamanho_LojaId_Valor",
                table: "Tamanho",
                columns: new[] { "LojaId", "Valor" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProdutoEstoque");

            migrationBuilder.DropTable(
                name: "Cor");

            migrationBuilder.DropTable(
                name: "Marca");

            migrationBuilder.DropTable(
                name: "ProdutoReferencia");

            migrationBuilder.DropTable(
                name: "Tamanho");
        }
    }
}
