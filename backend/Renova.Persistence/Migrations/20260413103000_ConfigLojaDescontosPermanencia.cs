using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Persistence.Migrations
{
    [DbContext(typeof(RenovaDbContext))]
    [Migration("20260413103000_ConfigLojaDescontosPermanencia")]
    public partial class ConfigLojaDescontosPermanencia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigLojaDescontoPermanencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigLojaId = table.Column<int>(type: "integer", nullable: false),
                    APartirDeMeses = table.Column<int>(type: "integer", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigLojaDescontoPermanencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigLojaDescontoPermanencia_ConfigLoja_ConfigLojaId",
                        column: x => x.ConfigLojaId,
                        principalTable: "ConfigLoja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigLojaDescontoPermanencia_ConfigLojaId_APartirDeMeses",
                table: "ConfigLojaDescontoPermanencia",
                columns: new[] { "ConfigLojaId", "APartirDeMeses" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigLojaDescontoPermanencia");
        }
    }
}
