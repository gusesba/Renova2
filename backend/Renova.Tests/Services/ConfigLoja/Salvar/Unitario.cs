using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.ConfigLoja;
using Renova.Service.Parameters.ConfigLoja;
using Renova.Service.Services.ConfigLoja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.ConfigLoja.Salvar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task SaveAsyncDeveCriarConfiguracaoDeRepasseQuandoLojaAindaNaoPossuirRegistro()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            ConfigLojaDto resultado = await service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 6,
                FormasPagamento =
                [
                    new SalvarConfigLojaFormaPagamentoCommand { Nome = "Cartao credito", PercentualAjuste = 4.5m },
                    new SalvarConfigLojaFormaPagamentoCommand { Nome = "Pix", PercentualAjuste = -3m }
                ],
                DescontosPermanencia =
                [
                    new SalvarConfigLojaDescontoPermanenciaCommand { APartirDeMeses = 3, PercentualDesconto = 10m },
                    new SalvarConfigLojaDescontoPermanenciaCommand { APartirDeMeses = 6, PercentualDesconto = 15m }
                ]
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            });

            Assert.Equal(loja.Id, resultado.LojaId);
            Assert.Equal(45m, resultado.PercentualRepasseFornecedor);
            Assert.Equal(45m, resultado.PercentualRepasseVendedorCredito);
            Assert.Equal(6, resultado.TempoPermanenciaProdutoMeses);
            Assert.Collection(resultado.DescontosPermanencia,
                item =>
                {
                    Assert.Equal(3, item.APartirDeMeses);
                    Assert.Equal(10m, item.PercentualDesconto);
                },
                item =>
                {
                    Assert.Equal(6, item.APartirDeMeses);
                    Assert.Equal(15m, item.PercentualDesconto);
                });
            Assert.Collection(resultado.FormasPagamento,
                item =>
                {
                    Assert.Equal("Cartao credito", item.Nome);
                    Assert.Equal(4.5m, item.PercentualAjuste);
                },
                item =>
                {
                    Assert.Equal("Pix", item.Nome);
                    Assert.Equal(-3m, item.PercentualAjuste);
                });
            ConfigLojaModel configSalva = await context.ConfiguracoesLoja
                .Include(item => item.FormasPagamento)
                .SingleAsync();
            Assert.Equal(loja.Id, configSalva.LojaId);
            Assert.Equal(45m, configSalva.PercentualRepasseFornecedor);
            Assert.Equal(45m, configSalva.PercentualRepasseVendedorCredito);
            Assert.Equal(6, configSalva.TempoPermanenciaProdutoMeses);
            Assert.Collection(configSalva.DescontosPermanencia.OrderBy(item => item.APartirDeMeses),
                item =>
                {
                    Assert.Equal(3, item.APartirDeMeses);
                    Assert.Equal(10m, item.PercentualDesconto);
                },
                item =>
                {
                    Assert.Equal(6, item.APartirDeMeses);
                    Assert.Equal(15m, item.PercentualDesconto);
                });
            Assert.Collection(configSalva.FormasPagamento.OrderBy(item => item.Nome),
                item =>
                {
                    Assert.Equal("Cartao credito", item.Nome);
                    Assert.Equal(4.5m, item.PercentualAjuste);
                },
                item =>
                {
                    Assert.Equal("Pix", item.Nome);
                    Assert.Equal(-3m, item.PercentualAjuste);
                });
        }

        [Fact]
        public async Task SaveAsyncDeveAtualizarConfiguracaoDeRepasseQuandoLojaJaPossuirRegistro()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m, 45m, 6);
            ConfigLojaService service = new(context);

            ConfigLojaDto resultado = await service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 50m,
                PercentualRepasseVendedorCredito = 60m,
                TempoPermanenciaProdutoMeses = 9,
                FormasPagamento =
                [
                    new SalvarConfigLojaFormaPagamentoCommand { Nome = "Debito", PercentualAjuste = 2m }
                ],
                DescontosPermanencia =
                [
                    new SalvarConfigLojaDescontoPermanenciaCommand { APartirDeMeses = 9, PercentualDesconto = 20m }
                ]
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            });

            Assert.Equal(50m, resultado.PercentualRepasseFornecedor);
            Assert.Equal(60m, resultado.PercentualRepasseVendedorCredito);
            Assert.Equal(9, resultado.TempoPermanenciaProdutoMeses);
            Assert.Single(resultado.DescontosPermanencia);
            Assert.Equal(9, resultado.DescontosPermanencia[0].APartirDeMeses);
            Assert.Equal(20m, resultado.DescontosPermanencia[0].PercentualDesconto);
            Assert.Single(resultado.FormasPagamento);
            Assert.Equal("Debito", resultado.FormasPagamento[0].Nome);
            Assert.Equal(2m, resultado.FormasPagamento[0].PercentualAjuste);
            Assert.Single(context.ConfiguracoesLoja);
            ConfigLojaModel configAtualizada = await context.ConfiguracoesLoja
                .Include(item => item.FormasPagamento)
                .SingleAsync();
            Assert.Equal(50m, configAtualizada.PercentualRepasseFornecedor);
            Assert.Equal(60m, configAtualizada.PercentualRepasseVendedorCredito);
            Assert.Equal(9, configAtualizada.TempoPermanenciaProdutoMeses);
            Assert.Single(configAtualizada.DescontosPermanencia);
            Assert.Equal(9, configAtualizada.DescontosPermanencia[0].APartirDeMeses);
            Assert.Equal(20m, configAtualizada.DescontosPermanencia[0].PercentualDesconto);
            Assert.Single(configAtualizada.FormasPagamento);
            Assert.Equal("Debito", configAtualizada.FormasPagamento[0].Nome);
            Assert.Equal(2m, configAtualizada.FormasPagamento[0].PercentualAjuste);
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirPercentualDeRepasseAoFornecedorForaDoIntervaloPermitido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 150m,
                PercentualRepasseVendedorCredito = 100m
                ,
                TempoPermanenciaProdutoMeses = 6
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirPercentualDeRepasseAoVendedorEmCreditoForaDoIntervaloPermitido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 150m,
                TempoPermanenciaProdutoMeses = 6
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirQuandoRepasseEmCreditoForMenorQueRepasseNormal()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 30m,
                TempoPermanenciaProdutoMeses = 6
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));

            Assert.Contains("maior ou igual", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirQuandoTempoPermanenciaProdutoForMenorQueUmMes()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 0
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirQuandoHouverMesesDuplicadosNosDescontosDePermanencia()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 6,
                DescontosPermanencia =
                [
                    new SalvarConfigLojaDescontoPermanenciaCommand { APartirDeMeses = 3, PercentualDesconto = 10m },
                    new SalvarConfigLojaDescontoPermanenciaCommand { APartirDeMeses = 3, PercentualDesconto = 15m }
                ]
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirAlteracaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 6
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = 9999
            }));
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirQuandoHouverFormasPagamentoDuplicadas()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria-forma@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 6,
                FormasPagamento =
                [
                    new SalvarConfigLojaFormaPagamentoCommand { Nome = "Pix", PercentualAjuste = 0m },
                    new SalvarConfigLojaFormaPagamentoCommand { Nome = " pix ", PercentualAjuste = 1m }
                ]
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, string nomeLoja, string emailUsuario)
        {
            UsuarioModel usuario = new()
            {
                Nome = "Usuario de Teste",
                Email = emailUsuario,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();

            LojaModel loja = new()
            {
                Nome = nomeLoja,
                UsuarioId = usuario.Id
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();

            return loja;
        }

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(RenovaDbContext context, int lojaId, decimal percentualRepasseFornecedor, decimal percentualRepasseVendedorCredito, int tempoPermanenciaProdutoMeses)
        {
            ConfigLojaModel config = new()
            {
                LojaId = lojaId,
                PercentualRepasseFornecedor = percentualRepasseFornecedor,
                PercentualRepasseVendedorCredito = percentualRepasseVendedorCredito,
                TempoPermanenciaProdutoMeses = tempoPermanenciaProdutoMeses,
                FormasPagamento =
                [
                    new ConfigLojaFormaPagamentoModel
                    {
                        Nome = "Dinheiro",
                        PercentualAjuste = 0m
                    }
                ],
                DescontosPermanencia =
                [
                    new ConfigLojaDescontoPermanenciaModel
                    {
                        APartirDeMeses = 6,
                        PercentualDesconto = 12m
                    }
                ]
            };

            _ = context.ConfiguracoesLoja.Add(config);
            _ = await context.SaveChangesAsync();

            return config;
        }
    }
}
