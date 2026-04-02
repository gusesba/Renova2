using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Loja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Loja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Loja", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Loja_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateIndex(
                name: "IX_Loja_UsuarioId_Nome",
                table: "Loja",
                columns: ["UsuarioId", "Nome"],
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "Loja");
        }
    }
}