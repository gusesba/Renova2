namespace Renova.Services.Features.Access;

// Representa os valores aceitos de status usados pelo modulo de acesso.
public static class AccessStatusValues
{
    // Representa os status possiveis de um usuario do sistema.
    public static class Usuario
    {
        public const string Ativo = "ativo";
        public const string Inativo = "inativo";
        public const string Bloqueado = "bloqueado";

        public static readonly IReadOnlySet<string> Todos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Ativo,
                Inativo,
                Bloqueado,
            };
    }

    // Representa os status possiveis de um vinculo de usuario com loja.
    public static class VinculoLoja
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
}
