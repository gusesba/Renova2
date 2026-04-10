using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Movimentacao;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Movimentacao.Criar
{
    public class Integracao
    {
        [Fact]
        public async Task PostMovimentacaoDeveRetornarCreatedQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Produto B", "44999990002");
            _ = await CriarConfigLojaAsync(factory, loja.Id, 45m, 60m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produtoA.Id, produtoB.Id]
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            MovimentacaoDto? body = await response.Content.ReadFromJsonAsync<MovimentacaoDto>();
            Assert.NotNull(body);
            Assert.True(body.Id > 0);
            Assert.Equal(TipoMovimentacao.Venda, body.Tipo);
            Assert.Equal(2, body.ProdutoIds.Count);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = Assert.Single(context.Movimentacoes);
            Assert.Equal(2, context.MovimentacoesProdutos.Count());
            Assert.Equal(3, context.Pagamentos.Count());
            Assert.Empty(context.ClientesCreditos);
            Assert.Contains(context.Pagamentos, item => item.ClienteId == produtoA.FornecedorId && item.Natureza == NaturezaPagamento.Pagar && item.Valor == 89.94m);
            Assert.Contains(context.Pagamentos, item => item.ClienteId == produtoB.FornecedorId && item.Natureza == NaturezaPagamento.Pagar && item.Valor == 89.94m);
            Assert.All(context.ProdutosEstoque.ToList(), item => Assert.Equal(SituacaoProduto.Vendido, item.Situacao));
        }

        [Fact]
        public async Task PostMovimentacaoDeveCriarUmaOrdemParaCadaFornecedorQuandoVendaTiverProdutosDeFornecedoresDiferentes()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-multi@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Produto B", "44999990002");
            _ = await CriarConfigLojaAsync(factory, loja.Id, 45m, 60m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produtoA.Id, produtoB.Id]
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            List<PagamentoModel> pagamentos = await context.Pagamentos.OrderBy(item => item.ClienteId).ToListAsync();

            Assert.Equal(3, pagamentos.Count);
            Assert.Contains(pagamentos, item => item.ClienteId == cliente.Id && item.Natureza == NaturezaPagamento.Receber && item.Valor == 299.80m);
            Assert.Contains(pagamentos, item => item.ClienteId == produtoA.FornecedorId && item.Natureza == NaturezaPagamento.Pagar && item.Valor == 89.94m);
            Assert.Contains(pagamentos, item => item.ClienteId == produtoB.FornecedorId && item.Natureza == NaturezaPagamento.Pagar && item.Valor == 89.94m);
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001");

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Emprestimo,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarBadRequestQuandoPayloadForInvalido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = default,
                ClienteId = 0,
                LojaId = loja.Id,
                ProdutoIds = []
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarBadRequestComMensagemQuandoLojaNaoPossuirConfiguracaoDeRepasse()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-sem-config@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("configuracao de repasse", body, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarBadRequestQuandoClienteNaoPertencerALojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            ClienteModel clienteOutraLoja = await CriarClienteAsync(factory, outraLoja.Id, "Cliente B", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Doacao,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteOutraLoja.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarBadRequestQuandoProdutoNaoPertencerALojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produtoOutraLoja = await CriarProdutoAsync(factory, outraLoja.Id, "Produto B", "44999990001");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoDono,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produtoOutraLoja.Id]
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarBadRequestQuandoSituacaoDoProdutoNaoForCompativelComOTipo()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001", SituacaoProduto.Vendido);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains(produto.Id.ToString(), body);
        }

        [Fact]
        public async Task PostMovimentacaoDevePermitirVendaDeProdutoEmprestadoQuandoForParaOMesmoCliente()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-emprestimo@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001", SituacaoProduto.Emprestado);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, cliente.Id, TipoMovimentacao.Emprestimo, produto.Id);
            _ = await CriarConfigLojaAsync(factory, loja.Id, 45m, 60m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            Assert.Equal(SituacaoProduto.Vendido, await context.ProdutosEstoque.Where(item => item.Id == produto.Id).Select(item => item.Situacao).SingleAsync());
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarBadRequestQuandoVendaDeProdutoEmprestadoForParaClienteDiferente()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-emprestimo-bloqueio@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel clienteEmprestimo = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ClienteModel clienteVenda = await CriarClienteAsync(factory, loja.Id, "Cliente B", "44999990003");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001", SituacaoProduto.Emprestado);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, clienteEmprestimo.Id, TipoMovimentacao.Emprestimo, produto.Id);
            _ = await CriarConfigLojaAsync(factory, loja.Id, 45m, 60m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteVenda.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains(produto.Id.ToString(), body);
            Assert.Contains("disponiveis para venda deste cliente", body);
        }

        [Fact]
        public async Task PostMovimentacaoDevePermitirDevolucaoVendaQuandoUltimaMovimentacaoForVendaDoMesmoCliente()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-devolucao-venda@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001", SituacaoProduto.Vendido);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, cliente.Id, TipoMovimentacao.Venda, produto.Id);
            _ = await CriarConfigLojaAsync(factory, loja.Id, 45m, 60m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoVenda,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarBadRequestQuandoDevolucaoVendaForParaClienteDiferenteDaUltimaVenda()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-devolucao-venda-bloqueio@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel clienteVenda = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ClienteModel clienteDevolucao = await CriarClienteAsync(factory, loja.Id, "Cliente B", "44999990003");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001", SituacaoProduto.Vendido);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, clienteVenda.Id, TipoMovimentacao.Venda, produto.Id);
            _ = await CriarConfigLojaAsync(factory, loja.Id, 45m, 60m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoVenda,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteDevolucao.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains(produto.Id.ToString(), body);
            Assert.Contains(TipoMovimentacao.Venda.ToString(), body);
        }

        [Fact]
        public async Task PostMovimentacaoDevePermitirDevolucaoEmprestimoQuandoUltimaMovimentacaoForEmprestimoDoMesmoCliente()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-devolucao-emprestimo@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001", SituacaoProduto.Emprestado);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, cliente.Id, TipoMovimentacao.Emprestimo, produto.Id);
            _ = await CriarConfigLojaAsync(factory, loja.Id, 45m, 60m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoEmprestimo,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarBadRequestQuandoDevolucaoEmprestimoForParaClienteDiferenteDoUltimoEmprestimo()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-devolucao-emprestimo-bloqueio@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel clienteEmprestimo = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ClienteModel clienteDevolucao = await CriarClienteAsync(factory, loja.Id, "Cliente B", "44999990003");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001", SituacaoProduto.Emprestado);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, clienteEmprestimo.Id, TipoMovimentacao.Emprestimo, produto.Id);
            _ = await CriarConfigLojaAsync(factory, loja.Id, 45m, 60m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoEmprestimo,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteDevolucao.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains(produto.Id.ToString(), body);
            Assert.Contains(TipoMovimentacao.Emprestimo.ToString(), body);
        }

        [Fact]
        public async Task PostMovimentacaoDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao", new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoEmprestimo,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static async Task<UsuarioTokenDto> CriarUsuarioAutenticadoAsync(HttpClient client, string email)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", new CadastroCommand
            {
                Nome = "Usuario de Teste",
                Email = email,
                Senha = "Senha@123"
            });

            _ = response.EnsureSuccessStatusCode();
            UsuarioTokenDto? resultado = await response.Content.ReadFromJsonAsync<UsuarioTokenDto>();
            return Assert.IsType<UsuarioTokenDto>(resultado);
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaApiFactory factory, int usuarioId, string nome)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            LojaModel loja = new()
            {
                Nome = nome,
                UsuarioId = usuarioId
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();
            return loja;
        }

        private static async Task<ClienteModel> CriarClienteAsync(RenovaApiFactory factory, int lojaId, string nome, string contato)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ClienteModel cliente = new()
            {
                Nome = nome,
                Contato = contato,
                LojaId = lojaId
            };

            _ = context.Clientes.Add(cliente);
            _ = await context.SaveChangesAsync();
            return cliente;
        }

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaApiFactory factory, int lojaId, string descricao, string contatoFornecedor, SituacaoProduto situacao = SituacaoProduto.Estoque)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ProdutoReferenciaModel produto = new()
            {
                Valor = $"{descricao} Referencia",
                LojaId = lojaId
            };

            MarcaModel marca = new()
            {
                Valor = $"{descricao} Marca",
                LojaId = lojaId
            };

            TamanhoModel tamanho = new()
            {
                Valor = "M",
                LojaId = lojaId
            };

            CorModel cor = new()
            {
                Valor = "Azul",
                LojaId = lojaId
            };

            ClienteModel fornecedor = new()
            {
                Nome = $"{descricao} Fornecedor",
                Contato = contatoFornecedor,
                LojaId = lojaId
            };

            _ = context.ProdutosReferencia.Add(produto);
            _ = context.Marcas.Add(marca);
            _ = context.Tamanhos.Add(tamanho);
            _ = context.Cores.Add(cor);
            _ = context.Clientes.Add(fornecedor);
            _ = await context.SaveChangesAsync();

            ProdutoEstoqueModel item = new()
            {
                Preco = 149.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = descricao,
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = situacao,
                Consignado = false
            };

            _ = context.ProdutosEstoque.Add(item);
            _ = await context.SaveChangesAsync();
            return item;
        }

        private static async Task<MovimentacaoModel> CriarMovimentacaoAsync(
            RenovaApiFactory factory,
            int lojaId,
            int clienteId,
            TipoMovimentacao tipo,
            params int[] produtoIds)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            MovimentacaoModel movimentacao = new()
            {
                Tipo = tipo,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteId,
                LojaId = lojaId,
                Produtos = produtoIds
                    .Select(produtoId => new MovimentacaoProdutoModel
                    {
                        ProdutoId = produtoId
                    })
                    .ToList()
            };

            _ = context.Movimentacoes.Add(movimentacao);
            _ = await context.SaveChangesAsync();
            return movimentacao;
        }

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(
            RenovaApiFactory factory,
            int lojaId,
            decimal percentualRepasseFornecedor,
            decimal percentualRepasseVendedorCredito)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ConfigLojaModel config = new()
            {
                LojaId = lojaId,
                PercentualRepasseFornecedor = percentualRepasseFornecedor,
                PercentualRepasseVendedorCredito = percentualRepasseVendedorCredito
            };

            _ = context.ConfiguracoesLoja.Add(config);
            _ = await context.SaveChangesAsync();
            return config;
        }

    }
}
