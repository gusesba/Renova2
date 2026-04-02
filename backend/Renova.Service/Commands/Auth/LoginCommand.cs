namespace Renova.Service.Commands.Auth
{
    public class LoginCommand
    {
        public required string Email { get; set; }
        public required string Senha { get; set; }
    }
}