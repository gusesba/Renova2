using Renova.Services.Features.SupplierPayments;

namespace Renova.Services.Features.Financial;

// Centraliza os valores fixos do modulo 12 para filtros, conciliacao e lancamentos avulsos.
public static class FinancialValues
{
    public static class Directions
    {
        public const string Entrada = "entrada";
        public const string Saida = "saida";

        public static readonly IReadOnlyList<string> Todos =
        [
            Entrada,
            Saida,
        ];
    }

    public static class MovementTypes
    {
        public const string Venda = "venda";
        public const string EstornoVenda = "estorno_venda";
        public const string PagamentoFornecedor = "pagamento_fornecedor";
        public const string Despesa = "despesa";
        public const string ReceitaAvulsa = "receita_avulsa";
        public const string Ajuste = "ajuste";
        public const string Estorno = "estorno";

        public static readonly IReadOnlyList<string> Todos =
        [
            Venda,
            EstornoVenda,
            PagamentoFornecedor,
            Despesa,
            ReceitaAvulsa,
            Ajuste,
            Estorno,
        ];

        public static readonly IReadOnlyList<string> Manuais =
        [
            Despesa,
            ReceitaAvulsa,
            Ajuste,
            Estorno,
        ];
    }

    public static class OriginTypes
    {
        public const string Venda = "venda";
        public const string ObrigacaoFornecedor = "obrigacao_fornecedor";
        public const string Avulso = "avulso";
    }

    /// <summary>
    /// Normaliza e valida a direcao usada no livro razao.
    /// </summary>
    public static string NormalizeDirection(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!Directions.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Direcao financeira invalida.");
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza e valida qualquer tipo de movimentacao financeira.
    /// </summary>
    public static string NormalizeMovementType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!MovementTypes.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Tipo de movimentacao financeira invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Restringe o cadastro manual aos tipos avulsos permitidos.
    /// </summary>
    public static string NormalizeManualMovementType(string value)
    {
        var normalized = NormalizeMovementType(value);
        if (!MovementTypes.Manuais.Contains(normalized))
        {
            throw new InvalidOperationException("O tipo informado nao pode ser criado manualmente.");
        }

        return normalized;
    }

    /// <summary>
    /// Garante a direcao compativel com o tipo manual informado.
    /// </summary>
    public static string NormalizeManualDirection(string movementType, string direction)
    {
        var normalizedDirection = NormalizeDirection(direction);
        var normalizedType = NormalizeManualMovementType(movementType);

        return normalizedType switch
        {
            MovementTypes.Despesa when normalizedDirection != Directions.Saida =>
                throw new InvalidOperationException("Lancamentos de despesa precisam sair do financeiro."),
            MovementTypes.ReceitaAvulsa when normalizedDirection != Directions.Entrada =>
                throw new InvalidOperationException("Receitas avulsas precisam entrar no financeiro."),
            _ => normalizedDirection,
        };
    }

    /// <summary>
    /// Traduz o tipo do meio de pagamento em texto amigavel.
    /// </summary>
    public static string GetPaymentMethodTypeLabel(string type)
    {
        return SupplierPaymentValues.GetPaymentMethodTypeLabel(type);
    }

    /// <summary>
    /// Monta as opcoes exibidas nos filtros do frontend.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildMovementTypeOptions()
    {
        return
        [
            (MovementTypes.Venda, "Venda"),
            (MovementTypes.EstornoVenda, "Estorno de venda"),
            (MovementTypes.PagamentoFornecedor, "Pagamento ao fornecedor"),
            (MovementTypes.Despesa, "Despesa"),
            (MovementTypes.ReceitaAvulsa, "Receita avulsa"),
            (MovementTypes.Ajuste, "Ajuste"),
            (MovementTypes.Estorno, "Estorno"),
        ];
    }

    /// <summary>
    /// Monta as opcoes validas para o formulario de lancamento avulso.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildManualMovementTypeOptions()
    {
        return BuildMovementTypeOptions()
            .Where(x => MovementTypes.Manuais.Contains(x.Codigo))
            .ToArray();
    }

    /// <summary>
    /// Monta as opcoes de entrada e saida usadas pela tela.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildDirectionOptions()
    {
        return
        [
            (Directions.Entrada, "Entrada"),
            (Directions.Saida, "Saida"),
        ];
    }
}
