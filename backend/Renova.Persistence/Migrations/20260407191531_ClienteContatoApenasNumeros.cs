using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClienteContatoApenasNumeros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Cliente"
                SET "Contato" = regexp_replace("Contato", '[^0-9]', '', 'g')
                WHERE "Contato" ~ '[^0-9]';
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cliente_Contato_ApenasNumeros",
                table: "Cliente",
                sql: "\"Contato\" !~ '[^0-9]' AND length(\"Contato\") IN (10, 11)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Cliente_Contato_ApenasNumeros",
                table: "Cliente");
        }
    }
}
