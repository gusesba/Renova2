namespace Renova.Tests.Endpoints.Auth.Cadastro;

public class Integracao
{
    [Fact]
    //Input: payload de cadastro válido
    //Grava usuário no banco com senha hash
    //Retorna: usuário e token
    public async Task PostCadastro_DeveSalvarComSenhaHashERetornarUsuarioToken()
    {

    }

    [Fact]
    //Input: payload com email já cadastrado
    //Não grava novo usuário no banco
    //Retorna: conflito de cadastro
    public async Task PostCadastro_DeveRetornarConflitoQuandoEmailJaExistir()
    {

    }

    [Fact]
    //Input: payload de cadastro inválido
    //Não grava usuário no banco
    //Retorna: erro de validação
    public async Task PostCadastro_DeveRetornarErroValidacaoQuandoPayloadForInvalido()
    {

    }
}
