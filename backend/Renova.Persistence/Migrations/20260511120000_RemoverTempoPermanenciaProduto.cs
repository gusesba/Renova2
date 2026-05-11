using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoverTempoPermanenciaProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TempoPermanenciaProdutoMeses",
                table: "ConfigLoja");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TempoPermanenciaProdutoMeses",
                table: "ConfigLoja",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }
    }
}
