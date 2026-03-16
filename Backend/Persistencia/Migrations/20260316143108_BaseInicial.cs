using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class BaseInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conjunto_catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conjunto_catalogo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    modulo = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pessoa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_pessoa = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    nome_social = table.Column<string>(type: "text", nullable: false),
                    documento = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    logradouro = table.Column<string>(type: "text", nullable: false),
                    numero = table.Column<string>(type: "text", nullable: false),
                    complemento = table.Column<string>(type: "text", nullable: false),
                    bairro = table.Column<string>(type: "text", nullable: false),
                    cidade = table.Column<string>(type: "text", nullable: false),
                    uf = table.Column<string>(type: "text", nullable: false),
                    cep = table.Column<string>(type: "text", nullable: false),
                    observacoes = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pessoa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conjunto_catalogo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.id);
                    table.ForeignKey(
                        name: "FK_categoria_conjunto_catalogo_conjunto_catalogo_id",
                        column: x => x.conjunto_catalogo_id,
                        principalTable: "conjunto_catalogo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "colecao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conjunto_catalogo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    ano_referencia = table.Column<int>(type: "integer", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_colecao", x => x.id);
                    table.ForeignKey(
                        name: "FK_colecao_conjunto_catalogo_conjunto_catalogo_id",
                        column: x => x.conjunto_catalogo_id,
                        principalTable: "conjunto_catalogo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conjunto_catalogo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    hexadecimal = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cor", x => x.id);
                    table.ForeignKey(
                        name: "FK_cor_conjunto_catalogo_conjunto_catalogo_id",
                        column: x => x.conjunto_catalogo_id,
                        principalTable: "conjunto_catalogo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "loja",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_fantasia = table.Column<string>(type: "text", nullable: false),
                    razao_social = table.Column<string>(type: "text", nullable: false),
                    documento = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    logradouro = table.Column<string>(type: "text", nullable: false),
                    numero = table.Column<string>(type: "text", nullable: false),
                    complemento = table.Column<string>(type: "text", nullable: false),
                    bairro = table.Column<string>(type: "text", nullable: false),
                    cidade = table.Column<string>(type: "text", nullable: false),
                    uf = table.Column<string>(type: "text", nullable: false),
                    cep = table.Column<string>(type: "text", nullable: false),
                    status_loja = table.Column<string>(type: "text", nullable: false),
                    conjunto_catalogo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loja", x => x.id);
                    table.ForeignKey(
                        name: "FK_loja_conjunto_catalogo_conjunto_catalogo_id",
                        column: x => x.conjunto_catalogo_id,
                        principalTable: "conjunto_catalogo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "marca",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conjunto_catalogo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marca", x => x.id);
                    table.ForeignKey(
                        name: "FK_marca_conjunto_catalogo_conjunto_catalogo_id",
                        column: x => x.conjunto_catalogo_id,
                        principalTable: "conjunto_catalogo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "produto_nome",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conjunto_catalogo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto_nome", x => x.id);
                    table.ForeignKey(
                        name: "FK_produto_nome_conjunto_catalogo_conjunto_catalogo_id",
                        column: x => x.conjunto_catalogo_id,
                        principalTable: "conjunto_catalogo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tamanho",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conjunto_catalogo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    ordem_exibicao = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tamanho", x => x.id);
                    table.ForeignKey(
                        name: "FK_tamanho_conjunto_catalogo_conjunto_catalogo_id",
                        column: x => x.conjunto_catalogo_id,
                        principalTable: "conjunto_catalogo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pessoa_conta_bancaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    banco = table.Column<string>(type: "text", nullable: false),
                    agencia = table.Column<string>(type: "text", nullable: false),
                    conta = table.Column<string>(type: "text", nullable: false),
                    tipo_conta = table.Column<string>(type: "text", nullable: false),
                    pix_tipo = table.Column<string>(type: "text", nullable: false),
                    pix_chave = table.Column<string>(type: "text", nullable: false),
                    favorecido_nome = table.Column<string>(type: "text", nullable: false),
                    favorecido_documento = table.Column<string>(type: "text", nullable: false),
                    principal = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pessoa_conta_bancaria", x => x.id);
                    table.ForeignKey(
                        name: "FK_pessoa_conta_bancaria_pessoa_pessoa_id",
                        column: x => x.pessoa_id,
                        principalTable: "pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: false),
                    senha_hash = table.Column<string>(type: "text", nullable: false),
                    senha_salt = table.Column<string>(type: "text", nullable: false),
                    status_usuario = table.Column<string>(type: "text", nullable: false),
                    ultimo_login_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_pessoa_pessoa_id",
                        column: x => x.pessoa_id,
                        principalTable: "pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alerta_operacional",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_alerta = table.Column<string>(type: "text", nullable: false),
                    severidade = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    referencia_tipo = table.Column<string>(type: "text", nullable: false),
                    referencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status_alerta = table.Column<string>(type: "text", nullable: false),
                    gerado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolvido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerta_operacional", x => x.id);
                    table.ForeignKey(
                        name: "FK_alerta_operacional_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cargo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargo", x => x.id);
                    table.ForeignKey(
                        name: "FK_cargo_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conta_credito_loja",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    saldo_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_comprometido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status_conta = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conta_credito_loja", x => x.id);
                    table.ForeignKey(
                        name: "FK_conta_credito_loja_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_conta_credito_loja_pessoa_pessoa_id",
                        column: x => x.pessoa_id,
                        principalTable: "pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "loja_configuracao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_exibicao = table.Column<string>(type: "text", nullable: false),
                    cabecalho_impressao = table.Column<string>(type: "text", nullable: false),
                    rodape_impressao = table.Column<string>(type: "text", nullable: false),
                    usa_modelo_unico_etiqueta = table.Column<bool>(type: "boolean", nullable: false),
                    usa_modelo_unico_recibo = table.Column<bool>(type: "boolean", nullable: false),
                    fuso_horario = table.Column<string>(type: "text", nullable: false),
                    moeda = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loja_configuracao", x => x.id);
                    table.ForeignKey(
                        name: "FK_loja_configuracao_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "loja_regra_comercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    percentual_repasse_dinheiro = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    percentual_repasse_credito = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    permite_pagamento_misto = table.Column<bool>(type: "boolean", nullable: false),
                    tempo_maximo_exposicao_dias = table.Column<int>(type: "integer", nullable: false),
                    politica_desconto_json = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loja_regra_comercial", x => x.id);
                    table.ForeignKey(
                        name: "FK_loja_regra_comercial_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "meio_pagamento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    tipo_meio_pagamento = table.Column<string>(type: "text", nullable: false),
                    taxa_percentual = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    prazo_recebimento_dias = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meio_pagamento", x => x.id);
                    table.ForeignKey(
                        name: "FK_meio_pagamento_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pessoa_loja",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    eh_cliente = table.Column<bool>(type: "boolean", nullable: false),
                    eh_fornecedor = table.Column<bool>(type: "boolean", nullable: false),
                    aceita_credito_loja = table.Column<bool>(type: "boolean", nullable: false),
                    politica_padrao_fim_consignacao = table.Column<string>(type: "text", nullable: false),
                    observacoes_internas = table.Column<string>(type: "text", nullable: false),
                    status_relacao = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pessoa_loja", x => x.id);
                    table.ForeignKey(
                        name: "FK_pessoa_loja_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pessoa_loja_pessoa_pessoa_id",
                        column: x => x.pessoa_id,
                        principalTable: "pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "auditoria_evento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "text", nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "text", nullable: false),
                    antes_json = table.Column<string>(type: "text", nullable: true),
                    depois_json = table.Column<string>(type: "text", nullable: true),
                    ocorrido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_evento", x => x.id);
                    table.ForeignKey(
                        name: "FK_auditoria_evento_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_auditoria_evento_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fechamento_pessoa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    periodo_fim = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status_fechamento = table.Column<string>(type: "text", nullable: false),
                    valor_vendido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_a_receber = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_comprado_na_loja = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_final = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    resumo_texto = table.Column<string>(type: "text", nullable: false),
                    pdf_url = table.Column<string>(type: "text", nullable: true),
                    excel_url = table.Column<string>(type: "text", nullable: true),
                    gerado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    gerado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conferido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    conferido_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fechamento_pessoa", x => x.id);
                    table.ForeignKey(
                        name: "FK_fechamento_pessoa_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fechamento_pessoa_pessoa_pessoa_id",
                        column: x => x.pessoa_id,
                        principalTable: "pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fechamento_pessoa_usuario_conferido_por_usuario_id",
                        column: x => x.conferido_por_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fechamento_pessoa_usuario_gerado_por_usuario_id",
                        column: x => x.gerado_por_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "peca",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_peca = table.Column<string>(type: "text", nullable: false),
                    codigo_interno = table.Column<string>(type: "text", nullable: false),
                    codigo_barras = table.Column<string>(type: "text", nullable: false),
                    produto_nome_id = table.Column<Guid>(type: "uuid", nullable: false),
                    marca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tamanho_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colecao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    observacoes = table.Column<string>(type: "text", nullable: false),
                    data_entrada = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    quantidade_inicial = table.Column<int>(type: "integer", nullable: false),
                    quantidade_atual = table.Column<int>(type: "integer", nullable: false),
                    preco_venda_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status_peca = table.Column<string>(type: "text", nullable: false),
                    localizacao_fisica = table.Column<string>(type: "text", nullable: false),
                    responsavel_cadastro_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_peca", x => x.id);
                    table.ForeignKey(
                        name: "FK_peca_categoria_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peca_colecao_colecao_id",
                        column: x => x.colecao_id,
                        principalTable: "colecao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peca_cor_cor_id",
                        column: x => x.cor_id,
                        principalTable: "cor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peca_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peca_marca_marca_id",
                        column: x => x.marca_id,
                        principalTable: "marca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peca_pessoa_fornecedor_pessoa_id",
                        column: x => x.fornecedor_pessoa_id,
                        principalTable: "pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peca_produto_nome_produto_nome_id",
                        column: x => x.produto_nome_id,
                        principalTable: "produto_nome",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peca_tamanho_tamanho_id",
                        column: x => x.tamanho_id,
                        principalTable: "tamanho",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peca_usuario_responsavel_cadastro_usuario_id",
                        column: x => x.responsavel_cadastro_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_acesso_evento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evento = table.Column<string>(type: "text", nullable: false),
                    ocorrido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ip = table.Column<string>(type: "text", nullable: false),
                    user_agent = table.Column<string>(type: "text", nullable: false),
                    detalhes_json = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_acesso_evento", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_acesso_evento_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_loja",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_vinculo = table.Column<string>(type: "text", nullable: false),
                    eh_responsavel = table.Column<bool>(type: "boolean", nullable: false),
                    data_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_loja", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_loja_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_loja_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_sessao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revogado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ip = table.Column<string>(type: "text", nullable: false),
                    user_agent = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_sessao", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_sessao_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "venda",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_venda = table.Column<string>(type: "text", nullable: false),
                    status_venda = table.Column<string>(type: "text", nullable: false),
                    data_hora_venda = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    vendedor_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comprador_pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxa_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observacoes = table.Column<string>(type: "text", nullable: false),
                    cancelada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelada_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venda", x => x.id);
                    table.ForeignKey(
                        name: "FK_venda_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venda_pessoa_comprador_pessoa_id",
                        column: x => x.comprador_pessoa_id,
                        principalTable: "pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venda_usuario_cancelada_por_usuario_id",
                        column: x => x.cancelada_por_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venda_usuario_vendedor_usuario_id",
                        column: x => x.vendedor_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cargo_permissao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cargo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permissao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargo_permissao", x => x.id);
                    table.ForeignKey(
                        name: "FK_cargo_permissao_cargo_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cargo_permissao_permissao_permissao_id",
                        column: x => x.permissao_id,
                        principalTable: "permissao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimentacao_credito_loja",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conta_credito_loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_movimentacao = table.Column<string>(type: "text", nullable: false),
                    origem_tipo = table.Column<string>(type: "text", nullable: false),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_posterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observacoes = table.Column<string>(type: "text", nullable: false),
                    movimentado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    movimentado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimentacao_credito_loja", x => x.id);
                    table.ForeignKey(
                        name: "FK_movimentacao_credito_loja_conta_credito_loja_conta_credito_~",
                        column: x => x.conta_credito_loja_id,
                        principalTable: "conta_credito_loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimentacao_credito_loja_usuario_movimentado_por_usuario_id",
                        column: x => x.movimentado_por_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fornecedor_regra_comercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    percentual_repasse_dinheiro = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    percentual_repasse_credito = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    permite_pagamento_misto = table.Column<bool>(type: "boolean", nullable: false),
                    tempo_maximo_exposicao_dias = table.Column<int>(type: "integer", nullable: false),
                    politica_desconto_json = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fornecedor_regra_comercial", x => x.id);
                    table.ForeignKey(
                        name: "FK_fornecedor_regra_comercial_pessoa_loja_pessoa_loja_id",
                        column: x => x.pessoa_loja_id,
                        principalTable: "pessoa_loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fechamento_pessoa_movimento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fechamento_pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_movimento = table.Column<string>(type: "text", nullable: false),
                    origem_tipo = table.Column<string>(type: "text", nullable: false),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_movimento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fechamento_pessoa_movimento", x => x.id);
                    table.ForeignKey(
                        name: "FK_fechamento_pessoa_movimento_fechamento_pessoa_fechamento_pe~",
                        column: x => x.fechamento_pessoa_id,
                        principalTable: "fechamento_pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fechamento_pessoa_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fechamento_pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_peca_snapshot = table.Column<string>(type: "text", nullable: false),
                    valor_venda_snapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_repasse_snapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_evento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fechamento_pessoa_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_fechamento_pessoa_item_fechamento_pessoa_fechamento_pessoa_~",
                        column: x => x.fechamento_pessoa_id,
                        principalTable: "fechamento_pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fechamento_pessoa_item_peca_peca_id",
                        column: x => x.peca_id,
                        principalTable: "peca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimentacao_estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_movimentacao = table.Column<string>(type: "text", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    saldo_anterior = table.Column<int>(type: "integer", nullable: false),
                    saldo_posterior = table.Column<int>(type: "integer", nullable: false),
                    origem_tipo = table.Column<string>(type: "text", nullable: false),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    movimentado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    movimentado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimentacao_estoque", x => x.id);
                    table.ForeignKey(
                        name: "FK_movimentacao_estoque_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimentacao_estoque_peca_peca_id",
                        column: x => x.peca_id,
                        principalTable: "peca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimentacao_estoque_usuario_movimentado_por_usuario_id",
                        column: x => x.movimentado_por_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "peca_condicao_comercial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem_regra = table.Column<string>(type: "text", nullable: false),
                    percentual_repasse_dinheiro = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    percentual_repasse_credito = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    permite_pagamento_misto = table.Column<bool>(type: "boolean", nullable: false),
                    tempo_maximo_exposicao_dias = table.Column<int>(type: "integer", nullable: false),
                    politica_desconto_json = table.Column<string>(type: "text", nullable: true),
                    data_inicio_consignacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    data_fim_consignacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    destino_padrao_fim_consignacao = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_peca_condicao_comercial", x => x.id);
                    table.ForeignKey(
                        name: "FK_peca_condicao_comercial_peca_peca_id",
                        column: x => x.peca_id,
                        principalTable: "peca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "peca_historico_preco",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    preco_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_novo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    alterado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    alterado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_peca_historico_preco", x => x.id);
                    table.ForeignKey(
                        name: "FK_peca_historico_preco_peca_peca_id",
                        column: x => x.peca_id,
                        principalTable: "peca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peca_historico_preco_usuario_alterado_por_usuario_id",
                        column: x => x.alterado_por_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "peca_imagem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url_arquivo = table.Column<string>(type: "text", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    tipo_visibilidade = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_peca_imagem", x => x.id);
                    table.ForeignKey(
                        name: "FK_peca_imagem_peca_peca_id",
                        column: x => x.peca_id,
                        principalTable: "peca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_loja_cargo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cargo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_loja_cargo", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_loja_cargo_cargo_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_loja_cargo_usuario_loja_usuario_loja_id",
                        column: x => x.usuario_loja_id,
                        principalTable: "usuario_loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "venda_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    preco_tabela_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_final_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_peca_snapshot = table.Column<string>(type: "text", nullable: false),
                    fornecedor_pessoa_id_snapshot = table.Column<Guid>(type: "uuid", nullable: true),
                    percentual_repasse_dinheiro_snapshot = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    percentual_repasse_credito_snapshot = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    valor_repasse_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venda_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_venda_item_peca_peca_id",
                        column: x => x.peca_id,
                        principalTable: "peca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venda_item_venda_venda_id",
                        column: x => x.venda_id,
                        principalTable: "venda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "venda_pagamento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    meio_pagamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_pagamento = table.Column<string>(type: "text", nullable: false),
                    conta_credito_loja_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxa_percentual_aplicada = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    valor_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    recebido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venda_pagamento", x => x.id);
                    table.ForeignKey(
                        name: "FK_venda_pagamento_conta_credito_loja_conta_credito_loja_id",
                        column: x => x.conta_credito_loja_id,
                        principalTable: "conta_credito_loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venda_pagamento_meio_pagamento_meio_pagamento_id",
                        column: x => x.meio_pagamento_id,
                        principalTable: "meio_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venda_pagamento_venda_venda_id",
                        column: x => x.venda_id,
                        principalTable: "venda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "obrigacao_fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_obrigacao = table.Column<string>(type: "text", nullable: false),
                    data_geracao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    data_vencimento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valor_original = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_em_aberto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status_obrigacao = table.Column<string>(type: "text", nullable: false),
                    observacoes = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obrigacao_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "FK_obrigacao_fornecedor_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_obrigacao_fornecedor_peca_peca_id",
                        column: x => x.peca_id,
                        principalTable: "peca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_obrigacao_fornecedor_pessoa_pessoa_id",
                        column: x => x.pessoa_id,
                        principalTable: "pessoa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_obrigacao_fornecedor_venda_item_venda_item_id",
                        column: x => x.venda_item_id,
                        principalTable: "venda_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "liquidacao_obrigacao_fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    obrigacao_fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_liquidacao = table.Column<string>(type: "text", nullable: false),
                    meio_pagamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conta_credito_loja_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    comprovante_url = table.Column<string>(type: "text", nullable: true),
                    liquidado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    liquidado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    observacoes = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_liquidacao_obrigacao_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "FK_liquidacao_obrigacao_fornecedor_conta_credito_loja_conta_cr~",
                        column: x => x.conta_credito_loja_id,
                        principalTable: "conta_credito_loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_liquidacao_obrigacao_fornecedor_meio_pagamento_meio_pagamen~",
                        column: x => x.meio_pagamento_id,
                        principalTable: "meio_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_liquidacao_obrigacao_fornecedor_obrigacao_fornecedor_obriga~",
                        column: x => x.obrigacao_fornecedor_id,
                        principalTable: "obrigacao_fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_liquidacao_obrigacao_fornecedor_usuario_liquidado_por_usuar~",
                        column: x => x.liquidado_por_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimentacao_financeira",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loja_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_movimentacao = table.Column<string>(type: "text", nullable: false),
                    direcao = table.Column<string>(type: "text", nullable: false),
                    meio_pagamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    venda_pagamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    liquidacao_obrigacao_fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_bruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    competencia_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    movimentado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    movimentado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    atualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimentacao_financeira", x => x.id);
                    table.ForeignKey(
                        name: "FK_movimentacao_financeira_liquidacao_obrigacao_fornecedor_liq~",
                        column: x => x.liquidacao_obrigacao_fornecedor_id,
                        principalTable: "liquidacao_obrigacao_fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimentacao_financeira_loja_loja_id",
                        column: x => x.loja_id,
                        principalTable: "loja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimentacao_financeira_meio_pagamento_meio_pagamento_id",
                        column: x => x.meio_pagamento_id,
                        principalTable: "meio_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimentacao_financeira_usuario_movimentado_por_usuario_id",
                        column: x => x.movimentado_por_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimentacao_financeira_venda_pagamento_venda_pagamento_id",
                        column: x => x.venda_pagamento_id,
                        principalTable: "venda_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alerta_operacional_loja_id_status_alerta_severidade",
                table: "alerta_operacional",
                columns: new[] { "loja_id", "status_alerta", "severidade" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_evento_loja_id",
                table: "auditoria_evento",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_evento_usuario_id",
                table: "auditoria_evento",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_loja_id",
                table: "cargo",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_permissao_cargo_id_permissao_id",
                table: "cargo_permissao",
                columns: new[] { "cargo_id", "permissao_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cargo_permissao_permissao_id",
                table: "cargo_permissao",
                column: "permissao_id");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_conjunto_catalogo_id",
                table: "categoria",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_colecao_conjunto_catalogo_id",
                table: "colecao",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_conta_credito_loja_loja_id_pessoa_id",
                table: "conta_credito_loja",
                columns: new[] { "loja_id", "pessoa_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conta_credito_loja_pessoa_id",
                table: "conta_credito_loja",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "IX_cor_conjunto_catalogo_id",
                table: "cor",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_fechamento_pessoa_conferido_por_usuario_id",
                table: "fechamento_pessoa",
                column: "conferido_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_fechamento_pessoa_gerado_por_usuario_id",
                table: "fechamento_pessoa",
                column: "gerado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_fechamento_pessoa_loja_id_pessoa_id_periodo_inicio_periodo_~",
                table: "fechamento_pessoa",
                columns: new[] { "loja_id", "pessoa_id", "periodo_inicio", "periodo_fim" });

            migrationBuilder.CreateIndex(
                name: "IX_fechamento_pessoa_pessoa_id",
                table: "fechamento_pessoa",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "IX_fechamento_pessoa_item_fechamento_pessoa_id",
                table: "fechamento_pessoa_item",
                column: "fechamento_pessoa_id");

            migrationBuilder.CreateIndex(
                name: "IX_fechamento_pessoa_item_peca_id",
                table: "fechamento_pessoa_item",
                column: "peca_id");

            migrationBuilder.CreateIndex(
                name: "IX_fechamento_pessoa_movimento_fechamento_pessoa_id",
                table: "fechamento_pessoa_movimento",
                column: "fechamento_pessoa_id");

            migrationBuilder.CreateIndex(
                name: "IX_fornecedor_regra_comercial_pessoa_loja_id",
                table: "fornecedor_regra_comercial",
                column: "pessoa_loja_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_liquidacao_obrigacao_fornecedor_conta_credito_loja_id",
                table: "liquidacao_obrigacao_fornecedor",
                column: "conta_credito_loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_liquidacao_obrigacao_fornecedor_liquidado_por_usuario_id",
                table: "liquidacao_obrigacao_fornecedor",
                column: "liquidado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_liquidacao_obrigacao_fornecedor_meio_pagamento_id",
                table: "liquidacao_obrigacao_fornecedor",
                column: "meio_pagamento_id");

            migrationBuilder.CreateIndex(
                name: "IX_liquidacao_obrigacao_fornecedor_obrigacao_fornecedor_id",
                table: "liquidacao_obrigacao_fornecedor",
                column: "obrigacao_fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_loja_conjunto_catalogo_id",
                table: "loja",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_loja_documento",
                table: "loja",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loja_configuracao_loja_id",
                table: "loja_configuracao",
                column: "loja_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loja_regra_comercial_loja_id",
                table: "loja_regra_comercial",
                column: "loja_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_marca_conjunto_catalogo_id",
                table: "marca",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_meio_pagamento_loja_id",
                table: "meio_pagamento",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_credito_loja_conta_credito_loja_id",
                table: "movimentacao_credito_loja",
                column: "conta_credito_loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_credito_loja_movimentado_por_usuario_id",
                table: "movimentacao_credito_loja",
                column: "movimentado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_estoque_loja_id",
                table: "movimentacao_estoque",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_estoque_movimentado_por_usuario_id",
                table: "movimentacao_estoque",
                column: "movimentado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_estoque_peca_id_movimentado_em",
                table: "movimentacao_estoque",
                columns: new[] { "peca_id", "movimentado_em" });

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_financeira_liquidacao_obrigacao_fornecedor_id",
                table: "movimentacao_financeira",
                column: "liquidacao_obrigacao_fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_financeira_loja_id_movimentado_em",
                table: "movimentacao_financeira",
                columns: new[] { "loja_id", "movimentado_em" });

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_financeira_meio_pagamento_id",
                table: "movimentacao_financeira",
                column: "meio_pagamento_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_financeira_movimentado_por_usuario_id",
                table: "movimentacao_financeira",
                column: "movimentado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacao_financeira_venda_pagamento_id",
                table: "movimentacao_financeira",
                column: "venda_pagamento_id");

            migrationBuilder.CreateIndex(
                name: "IX_obrigacao_fornecedor_loja_id_pessoa_id_status_obrigacao",
                table: "obrigacao_fornecedor",
                columns: new[] { "loja_id", "pessoa_id", "status_obrigacao" });

            migrationBuilder.CreateIndex(
                name: "IX_obrigacao_fornecedor_peca_id",
                table: "obrigacao_fornecedor",
                column: "peca_id");

            migrationBuilder.CreateIndex(
                name: "IX_obrigacao_fornecedor_pessoa_id",
                table: "obrigacao_fornecedor",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "IX_obrigacao_fornecedor_venda_item_id",
                table: "obrigacao_fornecedor",
                column: "venda_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_categoria_id",
                table: "peca",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_codigo_barras",
                table: "peca",
                column: "codigo_barras");

            migrationBuilder.CreateIndex(
                name: "IX_peca_colecao_id",
                table: "peca",
                column: "colecao_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_cor_id",
                table: "peca",
                column: "cor_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_fornecedor_pessoa_id",
                table: "peca",
                column: "fornecedor_pessoa_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_loja_id_codigo_interno",
                table: "peca",
                columns: new[] { "loja_id", "codigo_interno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_peca_loja_id_status_peca_data_entrada",
                table: "peca",
                columns: new[] { "loja_id", "status_peca", "data_entrada" });

            migrationBuilder.CreateIndex(
                name: "IX_peca_marca_id",
                table: "peca",
                column: "marca_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_produto_nome_id",
                table: "peca",
                column: "produto_nome_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_responsavel_cadastro_usuario_id",
                table: "peca",
                column: "responsavel_cadastro_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_tamanho_id",
                table: "peca",
                column: "tamanho_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_condicao_comercial_peca_id",
                table: "peca_condicao_comercial",
                column: "peca_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_peca_historico_preco_alterado_por_usuario_id",
                table: "peca_historico_preco",
                column: "alterado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_historico_preco_peca_id",
                table: "peca_historico_preco",
                column: "peca_id");

            migrationBuilder.CreateIndex(
                name: "IX_peca_imagem_peca_id",
                table: "peca_imagem",
                column: "peca_id");

            migrationBuilder.CreateIndex(
                name: "IX_pessoa_documento",
                table: "pessoa",
                column: "documento");

            migrationBuilder.CreateIndex(
                name: "IX_pessoa_conta_bancaria_pessoa_id",
                table: "pessoa_conta_bancaria",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "IX_pessoa_loja_loja_id",
                table: "pessoa_loja",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_pessoa_loja_pessoa_id_loja_id",
                table: "pessoa_loja",
                columns: new[] { "pessoa_id", "loja_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_produto_nome_conjunto_catalogo_id",
                table: "produto_nome",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_tamanho_conjunto_catalogo_id",
                table: "tamanho",
                column: "conjunto_catalogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_pessoa_id",
                table: "usuario",
                column: "pessoa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_acesso_evento_usuario_id",
                table: "usuario_acesso_evento",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_loja_loja_id",
                table: "usuario_loja",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_loja_usuario_id_loja_id",
                table: "usuario_loja",
                columns: new[] { "usuario_id", "loja_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_loja_cargo_cargo_id",
                table: "usuario_loja_cargo",
                column: "cargo_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_loja_cargo_usuario_loja_id_cargo_id",
                table: "usuario_loja_cargo",
                columns: new[] { "usuario_loja_id", "cargo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_sessao_usuario_id",
                table: "usuario_sessao",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_venda_cancelada_por_usuario_id",
                table: "venda",
                column: "cancelada_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_venda_comprador_pessoa_id",
                table: "venda",
                column: "comprador_pessoa_id");

            migrationBuilder.CreateIndex(
                name: "IX_venda_loja_id_data_hora_venda",
                table: "venda",
                columns: new[] { "loja_id", "data_hora_venda" });

            migrationBuilder.CreateIndex(
                name: "IX_venda_loja_id_numero_venda",
                table: "venda",
                columns: new[] { "loja_id", "numero_venda" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_venda_vendedor_usuario_id",
                table: "venda",
                column: "vendedor_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_venda_item_peca_id",
                table: "venda_item",
                column: "peca_id");

            migrationBuilder.CreateIndex(
                name: "IX_venda_item_venda_id",
                table: "venda_item",
                column: "venda_id");

            migrationBuilder.CreateIndex(
                name: "IX_venda_pagamento_conta_credito_loja_id",
                table: "venda_pagamento",
                column: "conta_credito_loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_venda_pagamento_meio_pagamento_id",
                table: "venda_pagamento",
                column: "meio_pagamento_id");

            migrationBuilder.CreateIndex(
                name: "IX_venda_pagamento_venda_id",
                table: "venda_pagamento",
                column: "venda_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerta_operacional");

            migrationBuilder.DropTable(
                name: "auditoria_evento");

            migrationBuilder.DropTable(
                name: "cargo_permissao");

            migrationBuilder.DropTable(
                name: "fechamento_pessoa_item");

            migrationBuilder.DropTable(
                name: "fechamento_pessoa_movimento");

            migrationBuilder.DropTable(
                name: "fornecedor_regra_comercial");

            migrationBuilder.DropTable(
                name: "loja_configuracao");

            migrationBuilder.DropTable(
                name: "loja_regra_comercial");

            migrationBuilder.DropTable(
                name: "movimentacao_credito_loja");

            migrationBuilder.DropTable(
                name: "movimentacao_estoque");

            migrationBuilder.DropTable(
                name: "movimentacao_financeira");

            migrationBuilder.DropTable(
                name: "peca_condicao_comercial");

            migrationBuilder.DropTable(
                name: "peca_historico_preco");

            migrationBuilder.DropTable(
                name: "peca_imagem");

            migrationBuilder.DropTable(
                name: "pessoa_conta_bancaria");

            migrationBuilder.DropTable(
                name: "usuario_acesso_evento");

            migrationBuilder.DropTable(
                name: "usuario_loja_cargo");

            migrationBuilder.DropTable(
                name: "usuario_sessao");

            migrationBuilder.DropTable(
                name: "permissao");

            migrationBuilder.DropTable(
                name: "fechamento_pessoa");

            migrationBuilder.DropTable(
                name: "pessoa_loja");

            migrationBuilder.DropTable(
                name: "liquidacao_obrigacao_fornecedor");

            migrationBuilder.DropTable(
                name: "venda_pagamento");

            migrationBuilder.DropTable(
                name: "cargo");

            migrationBuilder.DropTable(
                name: "usuario_loja");

            migrationBuilder.DropTable(
                name: "obrigacao_fornecedor");

            migrationBuilder.DropTable(
                name: "conta_credito_loja");

            migrationBuilder.DropTable(
                name: "meio_pagamento");

            migrationBuilder.DropTable(
                name: "venda_item");

            migrationBuilder.DropTable(
                name: "peca");

            migrationBuilder.DropTable(
                name: "venda");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "colecao");

            migrationBuilder.DropTable(
                name: "cor");

            migrationBuilder.DropTable(
                name: "marca");

            migrationBuilder.DropTable(
                name: "produto_nome");

            migrationBuilder.DropTable(
                name: "tamanho");

            migrationBuilder.DropTable(
                name: "loja");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "conjunto_catalogo");

            migrationBuilder.DropTable(
                name: "pessoa");
        }
    }
}
