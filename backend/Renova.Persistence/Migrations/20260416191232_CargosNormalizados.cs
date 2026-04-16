using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Renova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CargosNormalizados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CargoId",
                table: "Funcionario",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cargo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LojaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cargo_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Funcionalidade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Chave = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Grupo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionalidade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CargoFuncionalidade",
                columns: table => new
                {
                    CargoId = table.Column<int>(type: "integer", nullable: false),
                    FuncionalidadeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoFuncionalidade", x => new { x.CargoId, x.FuncionalidadeId });
                    table.ForeignKey(
                        name: "FK_CargoFuncionalidade_Cargo_CargoId",
                        column: x => x.CargoId,
                        principalTable: "Cargo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CargoFuncionalidade_Funcionalidade_FuncionalidadeId",
                        column: x => x.FuncionalidadeId,
                        principalTable: "Funcionalidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Funcionalidade",
                columns: new[] { "Id", "Chave", "Descricao", "Grupo" },
                values: new object[,]
                {
                    { 1, "clientes.visualizar", "Listar os clientes da loja.", "Clientes" },
                    { 2, "clientes.visualizar_detalhe", "Visualizar o detalhe e historico de um cliente.", "Clientes" },
                    { 3, "clientes.adicionar", "Cadastrar novos clientes.", "Clientes" },
                    { 4, "clientes.editar", "Editar clientes existentes.", "Clientes" },
                    { 5, "clientes.excluir", "Excluir clientes sem relacionamentos ativos.", "Clientes" },
                    { 6, "clientes.exportar_fechamento", "Exportar o fechamento de clientes.", "Clientes" },
                    { 7, "produtos.visualizar", "Listar os produtos da loja.", "Produtos" },
                    { 8, "produtos.visualizar_item", "Visualizar os dados de um produto.", "Produtos" },
                    { 9, "produtos.adicionar", "Cadastrar novos produtos no estoque.", "Produtos" },
                    { 10, "produtos.editar", "Editar produtos existentes.", "Produtos" },
                    { 11, "produtos.excluir", "Excluir produtos sem movimentacoes.", "Produtos" },
                    { 12, "produtos.emprestados.visualizar", "Consultar produtos emprestados por cliente.", "Produtos" },
                    { 13, "produtos.auxiliares.visualizar", "Consultar referencias, marcas, tamanhos e cores.", "Produtos" },
                    { 14, "produtos.auxiliares.adicionar_referencia", "Cadastrar novas referencias de produto.", "Produtos" },
                    { 15, "produtos.auxiliares.adicionar_marca", "Cadastrar novas marcas.", "Produtos" },
                    { 16, "produtos.auxiliares.adicionar_tamanho", "Cadastrar novos tamanhos.", "Produtos" },
                    { 17, "produtos.auxiliares.adicionar_cor", "Cadastrar novas cores.", "Produtos" },
                    { 18, "solicitacoes.visualizar", "Listar solicitacoes da loja.", "Solicitacoes" },
                    { 19, "solicitacoes.adicionar", "Cadastrar novas solicitacoes.", "Solicitacoes" },
                    { 20, "movimentacoes.visualizar", "Listar movimentacoes da loja.", "Movimentacoes" },
                    { 21, "movimentacoes.adicionar", "Registrar novas movimentacoes.", "Movimentacoes" },
                    { 22, "movimentacoes.destinacao.visualizar", "Consultar sugestoes para doacao e devolucao.", "Movimentacoes" },
                    { 23, "movimentacoes.destinacao.executar", "Executar destinacoes de doacao e devolucao.", "Movimentacoes" },
                    { 24, "pagamentos.visualizar", "Listar pagamentos da loja.", "Pagamentos" },
                    { 25, "pagamentos.manuais.adicionar", "Lancar pagamentos manuais.", "Pagamentos" },
                    { 26, "pagamentos.credito.visualizar", "Listar pagamentos de credito.", "Pagamentos" },
                    { 27, "pagamentos.credito.adicionar", "Adicionar credito para cliente.", "Pagamentos" },
                    { 28, "pagamentos.credito.resgatar", "Resgatar credito de cliente.", "Pagamentos" },
                    { 29, "pagamentos.pendencias.visualizar", "Visualizar pendencias de credito.", "Pagamentos" },
                    { 30, "pagamentos.pendencias.atualizar", "Atualizar pendencias de credito.", "Pagamentos" },
                    { 31, "pagamentos.fechamento.visualizar", "Visualizar o fechamento da loja.", "Pagamentos" },
                    { 32, "gastos_loja.visualizar", "Listar gastos da loja.", "GastosLoja" },
                    { 33, "gastos_loja.adicionar", "Cadastrar gastos e recebimentos da loja.", "GastosLoja" },
                    { 34, "lojas.visualizar", "Visualizar as lojas acessiveis.", "Lojas" },
                    { 35, "lojas.adicionar", "Cadastrar novas lojas.", "Lojas" },
                    { 36, "lojas.editar", "Editar lojas existentes.", "Lojas" },
                    { 37, "lojas.excluir", "Excluir lojas sem registros ativos.", "Lojas" },
                    { 38, "config_loja.visualizar", "Visualizar configuracoes da loja.", "ConfiguracaoLoja" },
                    { 39, "config_loja.editar", "Editar configuracoes da loja.", "ConfiguracaoLoja" },
                    { 40, "funcionarios.visualizar", "Listar funcionarios da loja.", "Funcionarios" },
                    { 41, "funcionarios.adicionar", "Vincular funcionarios a loja.", "Funcionarios" },
                    { 42, "funcionarios.editar", "Alterar o cargo de funcionarios.", "Funcionarios" },
                    { 43, "funcionarios.remover", "Remover funcionarios da loja.", "Funcionarios" },
                    { 44, "cargos.visualizar", "Listar cargos e funcionalidades da loja.", "Cargos" },
                    { 45, "cargos.adicionar", "Cadastrar novos cargos.", "Cargos" },
                    { 46, "cargos.editar", "Editar cargos existentes.", "Cargos" },
                    { 47, "cargos.excluir", "Excluir cargos sem funcionarios vinculados.", "Cargos" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Funcionario_CargoId",
                table: "Funcionario",
                column: "CargoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargo_LojaId_Nome",
                table: "Cargo",
                columns: new[] { "LojaId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CargoFuncionalidade_FuncionalidadeId",
                table: "CargoFuncionalidade",
                column: "FuncionalidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Funcionalidade_Chave",
                table: "Funcionalidade",
                column: "Chave",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "Cargo" ("Nome", "LojaId")
                SELECT 'Acesso completo', l."Id"
                FROM "Loja" l
                WHERE EXISTS (
                    SELECT 1
                    FROM "Funcionario" f
                    WHERE f."LojaId" = l."Id"
                );

                INSERT INTO "CargoFuncionalidade" ("CargoId", "FuncionalidadeId")
                SELECT c."Id", f."Id"
                FROM "Cargo" c
                CROSS JOIN "Funcionalidade" f
                WHERE c."Nome" = 'Acesso completo';

                UPDATE "Funcionario" funcionario
                SET "CargoId" = cargo."Id"
                FROM "Cargo" cargo
                WHERE cargo."LojaId" = funcionario."LojaId"
                  AND cargo."Nome" = 'Acesso completo'
                  AND funcionario."CargoId" IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "CargoId",
                table: "Funcionario",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Funcionario_Cargo_CargoId",
                table: "Funcionario",
                column: "CargoId",
                principalTable: "Cargo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Funcionario_Cargo_CargoId",
                table: "Funcionario");

            migrationBuilder.DropTable(
                name: "CargoFuncionalidade");

            migrationBuilder.DropTable(
                name: "Cargo");

            migrationBuilder.DropTable(
                name: "Funcionalidade");

            migrationBuilder.DropIndex(
                name: "IX_Funcionario_CargoId",
                table: "Funcionario");

            migrationBuilder.DropColumn(
                name: "CargoId",
                table: "Funcionario");
        }
    }
}
