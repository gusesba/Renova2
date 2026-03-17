namespace Renova.Services.Features.Stores.Contracts;

// Contratos HTTP e de aplicacao do modulo de lojas e estrutura operacional.
public sealed record StoreConfigurationRequest(
    string NomeExibicao,
    string CabecalhoImpressao,
    string RodapeImpressao,
    bool UsaModeloUnicoEtiqueta,
    bool UsaModeloUnicoRecibo,
    string FusoHorario,
    string Moeda);

public sealed record CreateStoreRequest(
    string NomeFantasia,
    string RazaoSocial,
    string Documento,
    string Telefone,
    string Email,
    string Logradouro,
    string Numero,
    string Complemento,
    string Bairro,
    string Cidade,
    string Uf,
    string Cep,
    StoreConfigurationRequest Configuracao);

public sealed record UpdateStoreRequest(
    string NomeFantasia,
    string RazaoSocial,
    string Documento,
    string Telefone,
    string Email,
    string Logradouro,
    string Numero,
    string Complemento,
    string Bairro,
    string Cidade,
    string Uf,
    string Cep,
    string StatusLoja);

public sealed record UpdateStoreConfigurationRequest(
    string NomeExibicao,
    string CabecalhoImpressao,
    string RodapeImpressao,
    bool UsaModeloUnicoEtiqueta,
    bool UsaModeloUnicoRecibo,
    string FusoHorario,
    string Moeda);

public sealed record StoreConfigurationResponse(
    Guid Id,
    Guid LojaId,
    string NomeExibicao,
    string CabecalhoImpressao,
    string RodapeImpressao,
    bool UsaModeloUnicoEtiqueta,
    bool UsaModeloUnicoRecibo,
    string FusoHorario,
    string Moeda);

public sealed record StoreResponse(
    Guid Id,
    string NomeFantasia,
    string RazaoSocial,
    string Documento,
    string Telefone,
    string Email,
    string Logradouro,
    string Numero,
    string Complemento,
    string Bairro,
    string Cidade,
    string Uf,
    string Cep,
    string StatusLoja,
    bool Ativo,
    Guid ConjuntoCatalogoId,
    bool EhLojaAtiva,
    bool EhResponsavel,
    bool PodeGerenciar,
    StoreConfigurationResponse Configuracao);
