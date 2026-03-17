namespace Renova.Services.Features.People;

// Centraliza os valores aceitos pelo modulo 03 para evitar strings soltas nos services.
public static class PeopleStatusValues
{
    public static class TipoPessoa
    {
        public const string Fisica = "fisica";
        public const string Juridica = "juridica";

        public static readonly IReadOnlySet<string> Todos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Fisica,
                Juridica,
            };
    }

    public static class StatusRelacao
    {
        public const string Ativo = "ativo";
        public const string Inativo = "inativo";

        public static readonly IReadOnlySet<string> Todos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Ativo,
                Inativo,
            };
    }

    public static class PoliticaFimConsignacao
    {
        public const string Devolver = "devolver";
        public const string Doar = "doar";

        public static readonly IReadOnlySet<string> Todos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Devolver,
                Doar,
            };
    }
}
