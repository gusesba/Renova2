using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IndicesLojaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tamanho_LojaId",
                table: "Tamanho",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoReferencia_LojaId",
                table: "ProdutoReferencia",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_Marca_LojaId",
                table: "Marca",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cor_LojaId",
                table: "Cor",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_LojaId",
                table: "Cliente",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargo_LojaId",
                table: "Cargo",
                column: "LojaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tamanho_LojaId",
                table: "Tamanho");

            migrationBuilder.DropIndex(
                name: "IX_ProdutoReferencia_LojaId",
                table: "ProdutoReferencia");

            migrationBuilder.DropIndex(
                name: "IX_Marca_LojaId",
                table: "Marca");

            migrationBuilder.DropIndex(
                name: "IX_Cor_LojaId",
                table: "Cor");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_LojaId",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_Cargo_LojaId",
                table: "Cargo");
        }
    }
}
