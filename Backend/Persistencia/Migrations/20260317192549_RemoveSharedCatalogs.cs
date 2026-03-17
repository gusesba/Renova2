using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSharedCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""CREATE EXTENSION IF NOT EXISTS pgcrypto;""");

            migrationBuilder.DropForeignKey(
                name: "FK_categoria_conjunto_catalogo_conjunto_catalogo_id",
                table: "categoria");

            migrationBuilder.DropForeignKey(
                name: "FK_colecao_conjunto_catalogo_conjunto_catalogo_id",
                table: "colecao");

            migrationBuilder.DropForeignKey(
                name: "FK_cor_conjunto_catalogo_conjunto_catalogo_id",
                table: "cor");

            migrationBuilder.DropForeignKey(
                name: "FK_loja_conjunto_catalogo_conjunto_catalogo_id",
                table: "loja");

            migrationBuilder.DropForeignKey(
                name: "FK_marca_conjunto_catalogo_conjunto_catalogo_id",
                table: "marca");

            migrationBuilder.DropForeignKey(
                name: "FK_produto_nome_conjunto_catalogo_conjunto_catalogo_id",
                table: "produto_nome");

            migrationBuilder.DropForeignKey(
                name: "FK_tamanho_conjunto_catalogo_conjunto_catalogo_id",
                table: "tamanho");

            migrationBuilder.DropIndex(
                name: "IX_tamanho_conjunto_catalogo_id",
                table: "tamanho");

            migrationBuilder.DropIndex(
                name: "IX_produto_nome_conjunto_catalogo_id",
                table: "produto_nome");

            migrationBuilder.DropIndex(
                name: "IX_marca_conjunto_catalogo_id",
                table: "marca");

            migrationBuilder.DropIndex(
                name: "IX_loja_conjunto_catalogo_id",
                table: "loja");

            migrationBuilder.DropIndex(
                name: "IX_cor_conjunto_catalogo_id",
                table: "cor");

            migrationBuilder.DropIndex(
                name: "IX_colecao_conjunto_catalogo_id",
                table: "colecao");

            migrationBuilder.DropIndex(
                name: "IX_categoria_conjunto_catalogo_id",
                table: "categoria");

            migrationBuilder.RenameColumn(
                name: "conjunto_catalogo_id",
                table: "tamanho",
                newName: "loja_id");

            migrationBuilder.RenameColumn(
                name: "conjunto_catalogo_id",
                table: "produto_nome",
                newName: "loja_id");

            migrationBuilder.RenameColumn(
                name: "conjunto_catalogo_id",
                table: "marca",
                newName: "loja_id");

            migrationBuilder.RenameColumn(
                name: "conjunto_catalogo_id",
                table: "cor",
                newName: "loja_id");

            migrationBuilder.RenameColumn(
                name: "conjunto_catalogo_id",
                table: "colecao",
                newName: "loja_id");

            migrationBuilder.RenameColumn(
                name: "conjunto_catalogo_id",
                table: "categoria",
                newName: "loja_id");

            MigrateAuxiliaryTable(
                migrationBuilder,
                tableName: "produto_nome",
                pecaForeignKeyColumn: "produto_nome_id",
                insertColumns: """
id,
loja_id,
nome,
descricao,
ativo,
inativado_em,
criado_em,
criado_por_usuario_id,
atualizado_em,
atualizado_por_usuario_id,
row_version
""",
                selectColumns: """
map.new_id,
map.target_loja_id,
source.nome,
source.descricao,
source.ativo,
source.inativado_em,
source.criado_em,
source.criado_por_usuario_id,
source.atualizado_em,
source.atualizado_por_usuario_id,
source.row_version
""");

            MigrateAuxiliaryTable(
                migrationBuilder,
                tableName: "marca",
                pecaForeignKeyColumn: "marca_id",
                insertColumns: """
id,
loja_id,
nome,
ativo,
inativado_em,
criado_em,
criado_por_usuario_id,
atualizado_em,
atualizado_por_usuario_id,
row_version
""",
                selectColumns: """
map.new_id,
map.target_loja_id,
source.nome,
source.ativo,
source.inativado_em,
source.criado_em,
source.criado_por_usuario_id,
source.atualizado_em,
source.atualizado_por_usuario_id,
source.row_version
""");

            MigrateAuxiliaryTable(
                migrationBuilder,
                tableName: "tamanho",
                pecaForeignKeyColumn: "tamanho_id",
                insertColumns: """
id,
loja_id,
nome,
ordem_exibicao,
ativo,
inativado_em,
criado_em,
criado_por_usuario_id,
atualizado_em,
atualizado_por_usuario_id,
row_version
""",
                selectColumns: """
map.new_id,
map.target_loja_id,
source.nome,
source.ordem_exibicao,
source.ativo,
source.inativado_em,
source.criado_em,
source.criado_por_usuario_id,
source.atualizado_em,
source.atualizado_por_usuario_id,
source.row_version
""");

            MigrateAuxiliaryTable(
                migrationBuilder,
                tableName: "cor",
                pecaForeignKeyColumn: "cor_id",
                insertColumns: """
id,
loja_id,
nome,
hexadecimal,
ativo,
inativado_em,
criado_em,
criado_por_usuario_id,
atualizado_em,
atualizado_por_usuario_id,
row_version
""",
                selectColumns: """
map.new_id,
map.target_loja_id,
source.nome,
source.hexadecimal,
source.ativo,
source.inativado_em,
source.criado_em,
source.criado_por_usuario_id,
source.atualizado_em,
source.atualizado_por_usuario_id,
source.row_version
""");

            MigrateAuxiliaryTable(
                migrationBuilder,
                tableName: "categoria",
                pecaForeignKeyColumn: "categoria_id",
                insertColumns: """
id,
loja_id,
nome,
descricao,
ativo,
inativado_em,
criado_em,
criado_por_usuario_id,
atualizado_em,
atualizado_por_usuario_id,
row_version
""",
                selectColumns: """
map.new_id,
map.target_loja_id,
source.nome,
source.descricao,
source.ativo,
source.inativado_em,
source.criado_em,
source.criado_por_usuario_id,
source.atualizado_em,
source.atualizado_por_usuario_id,
source.row_version
""");

            MigrateAuxiliaryTable(
                migrationBuilder,
                tableName: "colecao",
                pecaForeignKeyColumn: "colecao_id",
                insertColumns: """
id,
loja_id,
nome,
ano_referencia,
ativo,
inativado_em,
criado_em,
criado_por_usuario_id,
atualizado_em,
atualizado_por_usuario_id,
row_version
""",
                selectColumns: """
map.new_id,
map.target_loja_id,
source.nome,
source.ano_referencia,
source.ativo,
source.inativado_em,
source.criado_em,
source.criado_por_usuario_id,
source.atualizado_em,
source.atualizado_por_usuario_id,
source.row_version
""");

            migrationBuilder.DropTable(
                name: "conjunto_catalogo");

            migrationBuilder.CreateIndex(
                name: "IX_tamanho_loja_id_nome",
                table: "tamanho",
                columns: new[] { "loja_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_produto_nome_loja_id_nome",
                table: "produto_nome",
                columns: new[] { "loja_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_marca_loja_id_nome",
                table: "marca",
                columns: new[] { "loja_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cor_loja_id_nome",
                table: "cor",
                columns: new[] { "loja_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_colecao_loja_id_nome",
                table: "colecao",
                columns: new[] { "loja_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categoria_loja_id_nome",
                table: "categoria",
                columns: new[] { "loja_id", "nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_categoria_loja_loja_id",
                table: "categoria",
                column: "loja_id",
                principalTable: "loja",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_colecao_loja_loja_id",
                table: "colecao",
                column: "loja_id",
                principalTable: "loja",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cor_loja_loja_id",
                table: "cor",
                column: "loja_id",
                principalTable: "loja",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_marca_loja_loja_id",
                table: "marca",
                column: "loja_id",
                principalTable: "loja",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_produto_nome_loja_loja_id",
                table: "produto_nome",
                column: "loja_id",
                principalTable: "loja",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tamanho_loja_loja_id",
                table: "tamanho",
                column: "loja_id",
                principalTable: "loja",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "conjunto_catalogo_id",
                table: "loja");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categoria_loja_loja_id",
                table: "categoria");

            migrationBuilder.DropForeignKey(
                name: "FK_colecao_loja_loja_id",
                table: "colecao");

            migrationBuilder.DropForeignKey(
                name: "FK_cor_loja_loja_id",
                table: "cor");

            migrationBuilder.DropForeignKey(
                name: "FK_marca_loja_loja_id",
                table: "marca");

            migrationBuilder.DropForeignKey(
                name: "FK_produto_nome_loja_loja_id",
                table: "produto_nome");

            migrationBuilder.DropForeignKey(
                name: "FK_tamanho_loja_loja_id",
                table: "tamanho");

            migrationBuilder.DropIndex(
                name: "IX_tamanho_loja_id_nome",
                table: "tamanho");

            migrationBuilder.DropIndex(
                name: "IX_produto_nome_loja_id_nome",
                table: "produto_nome");

            migrationBuilder.DropIndex(
                name: "IX_marca_loja_id_nome",
                table: "marca");

            migrationBuilder.DropIndex(
                name: "IX_cor_loja_id_nome",
                table: "cor");

            migrationBuilder.DropIndex(
                name: "IX_colecao_loja_id_nome",
                table: "colecao");

            migrationBuilder.DropIndex(
                name: "IX_categoria_loja_id_nome",
                table: "categoria");

            migrationBuilder.RenameColumn(
                name: "loja_id",
                table: "tamanho",
                newName: "conjunto_catalogo_id");

            migrationBuilder.RenameColumn(
                name: "loja_id",
                table: "produto_nome",
                newName: "conjunto_catalogo_id");

            migrationBuilder.RenameColumn(
                name: "loja_id",
                table: "marca",
                newName: "conjunto_catalogo_id");

            migrationBuilder.RenameColumn(
                name: "loja_id",
                table: "cor",
                newName: "conjunto_catalogo_id");

            migrationBuilder.RenameColumn(
                name: "loja_id",
                table: "colecao",
                newName: "conjunto_catalogo_id");

            migrationBuilder.RenameColumn(
                name: "loja_id",
                table: "categoria",
                newName: "conjunto_catalogo_id");

            migrationBuilder.AddColumn<Guid>(
                name: "conjunto_catalogo_id",
                table: "loja",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "conjunto_catalogo",
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
                    nome = table.Column<string>(type: "text", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conjunto_catalogo", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tamanho_conjunto_catalogo_id",
                table: "tamanho",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_produto_nome_conjunto_catalogo_id",
                table: "produto_nome",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_marca_conjunto_catalogo_id",
                table: "marca",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_loja_conjunto_catalogo_id",
                table: "loja",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_cor_conjunto_catalogo_id",
                table: "cor",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_colecao_conjunto_catalogo_id",
                table: "colecao",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_conjunto_catalogo_id",
                table: "categoria",
                column: "conjunto_catalogo_id");

            migrationBuilder.AddForeignKey(
                name: "FK_categoria_conjunto_catalogo_conjunto_catalogo_id",
                table: "categoria",
                column: "conjunto_catalogo_id",
                principalTable: "conjunto_catalogo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_colecao_conjunto_catalogo_conjunto_catalogo_id",
                table: "colecao",
                column: "conjunto_catalogo_id",
                principalTable: "conjunto_catalogo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cor_conjunto_catalogo_conjunto_catalogo_id",
                table: "cor",
                column: "conjunto_catalogo_id",
                principalTable: "conjunto_catalogo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_loja_conjunto_catalogo_conjunto_catalogo_id",
                table: "loja",
                column: "conjunto_catalogo_id",
                principalTable: "conjunto_catalogo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_marca_conjunto_catalogo_conjunto_catalogo_id",
                table: "marca",
                column: "conjunto_catalogo_id",
                principalTable: "conjunto_catalogo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_produto_nome_conjunto_catalogo_conjunto_catalogo_id",
                table: "produto_nome",
                column: "conjunto_catalogo_id",
                principalTable: "conjunto_catalogo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tamanho_conjunto_catalogo_conjunto_catalogo_id",
                table: "tamanho",
                column: "conjunto_catalogo_id",
                principalTable: "conjunto_catalogo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <summary>
        /// Redistribui uma tabela auxiliar compartilhada para registros próprios por loja.
        /// </summary>
        private static void MigrateAuxiliaryTable(
            MigrationBuilder migrationBuilder,
            string tableName,
            string pecaForeignKeyColumn,
            string insertColumns,
            string selectColumns)
        {
            migrationBuilder.Sql(
                $"""
CREATE TEMP TABLE tmp_{tableName}_source ON COMMIT DROP AS
SELECT *
FROM {tableName};

DELETE FROM {tableName} AS target
WHERE NOT EXISTS (
    SELECT 1
    FROM loja
    WHERE loja.conjunto_catalogo_id = target.loja_id
);

CREATE TEMP TABLE tmp_{tableName}_primary_store ON COMMIT DROP AS
SELECT
    source.id AS original_id,
    target_store.id AS target_loja_id
FROM tmp_{tableName}_source AS source
JOIN LATERAL (
    SELECT loja.id
    FROM loja
    WHERE loja.conjunto_catalogo_id = source.loja_id
    ORDER BY loja.id
    LIMIT 1
) AS target_store ON TRUE;

UPDATE {tableName} AS target
SET loja_id = mapping.target_loja_id
FROM tmp_{tableName}_primary_store AS mapping
WHERE target.id = mapping.original_id;

CREATE TEMP TABLE tmp_{tableName}_duplicates ON COMMIT DROP AS
SELECT
    source.id AS original_id,
    store.id AS target_loja_id,
    gen_random_uuid() AS new_id
FROM tmp_{tableName}_source AS source
JOIN LATERAL (
    SELECT loja.id
    FROM loja
    WHERE loja.conjunto_catalogo_id = source.loja_id
    ORDER BY loja.id
    LIMIT 1
) AS primary_store ON TRUE
JOIN loja AS store
    ON store.conjunto_catalogo_id = source.loja_id
   AND store.id <> primary_store.id;

INSERT INTO {tableName} (
{insertColumns}
)
SELECT
{selectColumns}
FROM tmp_{tableName}_duplicates AS map
JOIN tmp_{tableName}_source AS source
    ON source.id = map.original_id;

UPDATE peca AS p
SET {pecaForeignKeyColumn} = map.new_id
FROM tmp_{tableName}_duplicates AS map
WHERE p.{pecaForeignKeyColumn} = map.original_id
  AND p.loja_id = map.target_loja_id;
""");
        }
    }
}
