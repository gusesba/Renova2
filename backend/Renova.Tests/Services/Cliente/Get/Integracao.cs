using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Get
{
    public class Integracao
    {
        [Fact]
        // Input: usuario autenticado, lojaId obrigatorio e clientes cadastrados
        // Retorna apenas clientes da loja informada
        // Retorna: ok com lista paginada de clientes
        public async Task GetClientesDeveRetornarOkComClientesDaLojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            LojaModel lojaExterna = await CriarLojaAsync(factory, outroUsuario.Usuario.Id, "Loja Externa");

            await CriarClienteAsync(factory, loja.Id, "Bruno", "44999990000");
            await CriarClienteAsync(factory, loja.Id, "Ana", "44999990001");
            await CriarClienteAsync(factory, outraLoja.Id, "Carla", "44999990002");
            await CriarClienteAsync(factory, lojaExterna.Id, "Daniel", "44999990003");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/cliente?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ClienteDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ClienteDto>>();

            Assert.NotNull(body);
            Assert.Equal(2, body.TotalItens);
            Assert.Collection(body.Itens,
                cliente => Assert.Equal("Ana", cliente.Nome),
                cliente => Assert.Equal("Bruno", cliente.Nome));
        }

        [Fact]
        // Input: query string com pagina e tamanho da pagina
        // Retorna apenas os itens da pagina solicitada
        // Retorna: ok com metadados de paginacao coerentes
        public async Task GetClientesDeveRetornarPaginaSolicitadaQuandoPaginacaoForInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            await CriarClienteAsync(factory, loja.Id, "Ana", "1");
            await CriarClienteAsync(factory, loja.Id, "Bruno", "2");
            await CriarClienteAsync(factory, loja.Id, "Carla", "3");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/cliente?lojaId={loja.Id}&pagina=2&tamanhoPagina=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ClienteDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ClienteDto>>();

            Assert.NotNull(body);
            Assert.Equal(3, body.TotalItens);
            Assert.Equal(2, body.Pagina);
            Assert.Equal(1, body.TamanhoPagina);
            Assert.Equal(3, body.TotalPaginas);
            ClienteDto item = Assert.Single(body.Itens);
            Assert.Equal("Bruno", item.Nome);
        }

        [Fact]
        // Input: query string com campo e direcao de ordenacao
        // Retorna clientes ordenados pelo campo solicitado
        // Retorna: ok com ordenacao aplicada
        public async Task GetClientesDeveOrdenarResultadoQuandoOrdenacaoForInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            await CriarClienteAsync(factory, loja.Id, "Carlos", "3");
            await CriarClienteAsync(factory, loja.Id, "Ana", "1");
            await CriarClienteAsync(factory, loja.Id, "Bruno", "2");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/cliente?lojaId={loja.Id}&ordenarPor=nome&direcao=desc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ClienteDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ClienteDto>>();

            Assert.NotNull(body);
            Assert.Collection(body.Itens,
                cliente => Assert.Equal("Carlos", cliente.Nome),
                cliente => Assert.Equal("Bruno", cliente.Nome),
                cliente => Assert.Equal("Ana", cliente.Nome));
        }

        [Fact]
        // Input: query string com filtro por nome
        // Retorna somente clientes que correspondam ao nome filtrado
        // Retorna: ok com lista filtrada
        public async Task GetClientesDeveFiltrarPorNomeQuandoFiltroForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            await CriarClienteAsync(factory, loja.Id, "Ana Paula", "1");
            await CriarClienteAsync(factory, loja.Id, "Mariana", "2");
            await CriarClienteAsync(factory, loja.Id, "Carlos", "3");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/cliente?lojaId={loja.Id}&nome=ana");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ClienteDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ClienteDto>>();

            Assert.NotNull(body);
            Assert.Equal(2, body.TotalItens);
            Assert.Collection(body.Itens,
                cliente => Assert.Equal("Ana Paula", cliente.Nome),
                cliente => Assert.Equal("Mariana", cliente.Nome));
        }

        [Fact]
        // Input: query string com filtro por contato
        // Retorna somente clientes que correspondam ao contato filtrado
        // Retorna: ok com lista filtrada
        public async Task GetClientesDeveFiltrarPorContatoQuandoFiltroForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            await CriarClienteAsync(factory, loja.Id, "Ana", "44999990000");
            await CriarClienteAsync(factory, loja.Id, "Bruno", "11999990000");
            await CriarClienteAsync(factory, loja.Id, "Carlos", "44911112222");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/cliente?lojaId={loja.Id}&contato=1199");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ClienteDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ClienteDto>>();

            Assert.NotNull(body);
            ClienteDto item = Assert.Single(body.Itens);
            Assert.Equal("Bruno", item.Nome);
        }

        [Fact]
        // Input: query string sem lojaId
        // Nao executa a consulta sem o identificador obrigatorio da loja
        // Retorna: bad request
        public async Task GetClientesDeveRetornarBadRequestQuandoLojaIdNaoForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync("/api/cliente");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        // Input: query string combinando filtros, ordenacao e paginacao
        // Retorna resultado consistente com a consulta composta
        // Retorna: ok com subconjunto correto
        public async Task GetClientesDeveCombinarFiltrosOrdenacaoEPaginacao()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            await CriarClienteAsync(factory, loja.Id, "Ana Paula", "119999");
            await CriarClienteAsync(factory, loja.Id, "Ana Clara", "118888");
            await CriarClienteAsync(factory, loja.Id, "Beatriz", "117777");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/cliente?lojaId={loja.Id}&nome=Ana&contato=11&ordenarPor=nome&direcao=desc&pagina=2&tamanhoPagina=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ClienteDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ClienteDto>>();

            Assert.NotNull(body);
            Assert.Equal(2, body.TotalItens);
            Assert.Equal(2, body.TotalPaginas);
            ClienteDto item = Assert.Single(body.Itens);
            Assert.Equal("Ana Clara", item.Nome);
        }

        [Fact]
        // Input: requisicao sem usuario autenticado
        // Nao retorna clientes
        // Retorna: unauthorized
        public async Task GetClientesDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/cliente?lojaId=1");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        // Input: usuario autenticado e lojaId valido sem clientes cadastrados
        // Nao retorna clientes de outras lojas
        // Retorna: ok com lista vazia
        public async Task GetClientesDeveRetornarOkComListaVaziaQuandoLojaNaoPossuirClientes()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");

            await CriarClienteAsync(factory, outraLoja.Id, "Ana", "1");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/cliente?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ClienteDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ClienteDto>>();

            Assert.NotNull(body);
            Assert.Empty(body.Itens);
            Assert.Equal(0, body.TotalItens);
            Assert.Equal(0, body.TotalPaginas);
        }

        [Fact]
        // Input: filtro por lojaId de outro usuario
        // Nao retorna clientes de loja nao autorizada
        // Retorna: unauthorized
        public async Task GetClientesDeveRetornarUnauthorizedQuandoLojaFiltradaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");

            await CriarClienteAsync(factory, loja.Id, "Ana", "1");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/cliente?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetClientesDeveRetornarOkQuandoUsuarioForFuncionarioDaLoja()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "dona-cliente@renova.com");
            UsuarioTokenDto funcionario = await CriarUsuarioAutenticadoAsync(client, "funcionario-cliente@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");
            await CriarClienteAsync(factory, loja.Id, "Ana", "44999990000");
            await VincularFuncionarioAsync(factory, funcionario.Usuario.Id, loja.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", funcionario.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/cliente?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ClienteDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ClienteDto>>();

            Assert.NotNull(body);
            ClienteDto item = Assert.Single(body.Itens);
            Assert.Equal("Ana", item.Nome);
        }

        private static async Task<UsuarioTokenDto> CriarUsuarioAutenticadoAsync(HttpClient client, string email)
        {
            CadastroCommand command = new()
            {
                Nome = "Usuario de Teste",
                Email = email,
                Senha = "Senha@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

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

        private static async Task VincularFuncionarioAsync(RenovaApiFactory factory, int usuarioId, int lojaId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            _ = context.Funcionarios.Add(new FuncionarioModel
            {
                UsuarioId = usuarioId,
                LojaId = lojaId
            });
            _ = await context.SaveChangesAsync();
        }
    }
}
