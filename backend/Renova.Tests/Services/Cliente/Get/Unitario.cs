using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Queries.Cliente;
using Renova.Service.Services.Cliente;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Get
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        // Input: usuario autenticado, lojaId obrigatorio e clientes cadastrados na loja informada
        // Retorna apenas clientes da loja solicitada, desde que ela pertenca ao usuario informado
        // Retorna: lista paginada de clientes da loja
        public async Task GetAllAsyncDeveRetornarApenasClientesDaLojaInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            UsuarioModel outroUsuario = await CriarUsuarioAsync(context, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(context, usuario.Id, "Loja Bairro");
            LojaModel lojaExterna = await CriarLojaAsync(context, outroUsuario.Id, "Loja Externa");

            await CriarClienteAsync(context, loja.Id, "Bruno", "44999990000");
            await CriarClienteAsync(context, loja.Id, "Ana", "44999990001");
            await CriarClienteAsync(context, outraLoja.Id, "Carla", "44999990002");
            await CriarClienteAsync(context, lojaExterna.Id, "Daniel", "44999990003");

            ClienteService service = new(context);
            PaginacaoDto<ClienteDto> resultado = await service.GetAllAsync(
                new ObterClientesQuery { LojaId = loja.Id },
                new ObterClientesParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(1, resultado.Pagina);
            Assert.Equal(10, resultado.TamanhoPagina);
            Assert.Equal(1, resultado.TotalPaginas);
            Assert.Collection(resultado.Itens,
                cliente => Assert.Equal("Ana", cliente.Nome),
                cliente => Assert.Equal("Bruno", cliente.Nome));
        }

        [Fact]
        // Input: pagina, tamanho da pagina e massa com mais registros do que o limite
        // Retorna somente a pagina solicitada
        // Retorna: metadados de paginacao coerentes com total de registros
        public async Task GetAllAsyncDeveAplicarPaginacaoQuandoPaginaETamanhoForemInformados()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            await CriarClienteAsync(context, loja.Id, "Ana", "1");
            await CriarClienteAsync(context, loja.Id, "Bruno", "2");
            await CriarClienteAsync(context, loja.Id, "Carla", "3");

            ClienteService service = new(context);
            PaginacaoDto<ClienteDto> resultado = await service.GetAllAsync(
                new ObterClientesQuery
                {
                    LojaId = loja.Id,
                    Pagina = 2,
                    TamanhoPagina = 1
                },
                new ObterClientesParametros { UsuarioId = usuario.Id });

            Assert.Equal(3, resultado.TotalItens);
            Assert.Equal(3, resultado.TotalPaginas);
            Assert.Equal(2, resultado.Pagina);
            Assert.Equal(1, resultado.TamanhoPagina);
            ClienteDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Bruno", item.Nome);
        }

        [Fact]
        // Input: ordenacao por nome ascendente
        // Retorna clientes ordenados alfabeticamente
        // Retorna: lista em ordem crescente pelo campo solicitado
        public async Task GetAllAsyncDeveOrdenarPorNomeAscQuandoOrdenacaoForInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            await CriarClienteAsync(context, loja.Id, "Carlos", "3");
            await CriarClienteAsync(context, loja.Id, "Ana", "1");
            await CriarClienteAsync(context, loja.Id, "Bruno", "2");

            ClienteService service = new(context);
            PaginacaoDto<ClienteDto> resultado = await service.GetAllAsync(
                new ObterClientesQuery
                {
                    LojaId = loja.Id,
                    OrdenarPor = "nome",
                    Direcao = "asc"
                },
                new ObterClientesParametros { UsuarioId = usuario.Id });

            Assert.Collection(resultado.Itens,
                cliente => Assert.Equal("Ana", cliente.Nome),
                cliente => Assert.Equal("Bruno", cliente.Nome),
                cliente => Assert.Equal("Carlos", cliente.Nome));
        }

        [Fact]
        // Input: ordenacao por nome descendente
        // Retorna clientes ordenados de forma decrescente
        // Retorna: lista em ordem decrescente pelo campo solicitado
        public async Task GetAllAsyncDeveOrdenarPorNomeDescQuandoOrdenacaoForInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            await CriarClienteAsync(context, loja.Id, "Carlos", "3");
            await CriarClienteAsync(context, loja.Id, "Ana", "1");
            await CriarClienteAsync(context, loja.Id, "Bruno", "2");

            ClienteService service = new(context);
            PaginacaoDto<ClienteDto> resultado = await service.GetAllAsync(
                new ObterClientesQuery
                {
                    LojaId = loja.Id,
                    OrdenarPor = "nome",
                    Direcao = "desc"
                },
                new ObterClientesParametros { UsuarioId = usuario.Id });

            Assert.Collection(resultado.Itens,
                cliente => Assert.Equal("Carlos", cliente.Nome),
                cliente => Assert.Equal("Bruno", cliente.Nome),
                cliente => Assert.Equal("Ana", cliente.Nome));
        }

        [Fact]
        // Input: filtro por nome parcial
        // Retorna somente clientes cujo nome corresponda ao filtro
        // Retorna: lista filtrada por nome
        public async Task GetAllAsyncDeveFiltrarPorNomeQuandoFiltroForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            await CriarClienteAsync(context, loja.Id, "Ana Paula", "1");
            await CriarClienteAsync(context, loja.Id, "Mariana", "2");
            await CriarClienteAsync(context, loja.Id, "Carlos", "3");

            ClienteService service = new(context);
            PaginacaoDto<ClienteDto> resultado = await service.GetAllAsync(
                new ObterClientesQuery
                {
                    LojaId = loja.Id,
                    Nome = "ana"
                },
                new ObterClientesParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Collection(resultado.Itens,
                cliente => Assert.Equal("Ana Paula", cliente.Nome),
                cliente => Assert.Equal("Mariana", cliente.Nome));
        }

        [Fact]
        // Input: filtro por contato
        // Retorna somente clientes cujo contato corresponda ao filtro
        // Retorna: lista filtrada por contato
        public async Task GetAllAsyncDeveFiltrarPorContatoQuandoFiltroForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            await CriarClienteAsync(context, loja.Id, "Ana", "44999990000");
            await CriarClienteAsync(context, loja.Id, "Bruno", "11999990000");
            await CriarClienteAsync(context, loja.Id, "Carlos", "44911112222");

            ClienteService service = new(context);
            PaginacaoDto<ClienteDto> resultado = await service.GetAllAsync(
                new ObterClientesQuery
                {
                    LojaId = loja.Id,
                    Contato = "1199"
                },
                new ObterClientesParametros { UsuarioId = usuario.Id });

            ClienteDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Bruno", item.Nome);
        }

        [Fact]
        // Input: lojaId obrigatorio nao informado
        // Nao executa a busca sem identificar a loja alvo
        // Retorna: erro de validacao/regra
        public async Task GetAllAsyncDeveFalharQuandoLojaIdNaoForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            ClienteService service = new(context);

            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.GetAllAsync(
                new ObterClientesQuery(),
                new ObterClientesParametros { UsuarioId = usuario.Id }));
        }

        [Fact]
        // Input: combinacao de filtro, ordenacao e paginacao
        // Aplica os criterios na ordem correta
        // Retorna: subconjunto consistente com a consulta composta
        public async Task GetAllAsyncDeveCombinarFiltrosOrdenacaoEPaginacao()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            await CriarClienteAsync(context, loja.Id, "Ana Paula", "119999");
            await CriarClienteAsync(context, loja.Id, "Ana Clara", "118888");
            await CriarClienteAsync(context, loja.Id, "Beatriz", "117777");

            ClienteService service = new(context);
            PaginacaoDto<ClienteDto> resultado = await service.GetAllAsync(
                new ObterClientesQuery
                {
                    LojaId = loja.Id,
                    Nome = "Ana",
                    Contato = "11",
                    OrdenarPor = "nome",
                    Direcao = "desc",
                    Pagina = 2,
                    TamanhoPagina = 1
                },
                new ObterClientesParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(2, resultado.TotalPaginas);
            ClienteDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Ana Clara", item.Nome);
        }

        [Fact]
        // Input: usuario autenticado e lojaId valido sem clientes cadastrados
        // Nao retorna clientes de outras lojas
        // Retorna: lista vazia
        public async Task GetAllAsyncDeveRetornarListaVaziaQuandoLojaNaoPossuirClientes()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(context, usuario.Id, "Loja Bairro");

            await CriarClienteAsync(context, outraLoja.Id, "Ana", "1");

            ClienteService service = new(context);
            PaginacaoDto<ClienteDto> resultado = await service.GetAllAsync(
                new ObterClientesQuery { LojaId = loja.Id },
                new ObterClientesParametros { UsuarioId = usuario.Id });

            Assert.Empty(resultado.Itens);
            Assert.Equal(0, resultado.TotalItens);
            Assert.Equal(0, resultado.TotalPaginas);
        }

        [Fact]
        // Input: usuario autenticado inexistente
        // Nao consulta clientes para usuario invalido
        // Retorna: erro de autenticacao/regra
        public async Task GetAllAsyncDeveFalharQuandoUsuarioAutenticadoNaoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteService service = new(context);

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAllAsync(
                new ObterClientesQuery { LojaId = loja.Id },
                new ObterClientesParametros { UsuarioId = 999 }));
        }

        private static async Task<UsuarioModel> CriarUsuarioAsync(RenovaDbContext context, string email)
        {
            UsuarioModel usuario = new()
            {
                Nome = "Usuario de Teste",
                Email = email,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();

            return usuario;
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, int usuarioId, string nome)
        {
            LojaModel loja = new()
            {
                Nome = nome,
                UsuarioId = usuarioId
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();

            return loja;
        }

        private static async Task<ClienteModel> CriarClienteAsync(RenovaDbContext context, int lojaId, string nome, string contato)
        {
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
    }
}
