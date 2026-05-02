using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProdutoEtiqueta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Etiqueta",
                table: "ProdutoEstoque",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "ProdutoEstoque" produto
                SET "Etiqueta" = sequencia."Etiqueta"
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "LojaId" ORDER BY "Id") - 1 AS "Etiqueta"
                    FROM "ProdutoEstoque"
                ) sequencia
                WHERE produto."Id" = sequencia."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoEstoque_LojaId_Etiqueta",
                table: "ProdutoEstoque",
                columns: new[] { "LojaId", "Etiqueta" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProdutoEstoque_LojaId_Etiqueta",
                table: "ProdutoEstoque");

            migrationBuilder.DropColumn(
                name: "Etiqueta",
                table: "ProdutoEstoque");
        }
    }
}
