using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/qz")]
    [Authorize]
    public class QzController(IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;

        [HttpGet("certificate")]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCertificate()
        {
            string? certificate = ReadConfiguredValue("QzTray:Certificate", "QzTray:CertificatePath");

            if (string.IsNullOrWhiteSpace(certificate))
            {
                return NotFound("Certificado QZ Tray nao configurado.");
            }

            return Content(certificate, "text/plain", Encoding.UTF8);
        }

        [HttpPost("sign")]
        [Consumes("text/plain")]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Sign(CancellationToken cancellationToken)
        {
            using StreamReader reader = new(Request.Body, Encoding.UTF8);
            string payload = await reader.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrEmpty(payload))
            {
                return BadRequest("Payload para assinatura nao informado.");
            }

            string? privateKey = ReadConfiguredValue("QzTray:PrivateKey", "QzTray:PrivateKeyPath");

            if (string.IsNullOrWhiteSpace(privateKey))
            {
                return NotFound("Chave privada QZ Tray nao configurada.");
            }

            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);

            byte[] signature = rsa.SignData(
                Encoding.UTF8.GetBytes(payload),
                HashAlgorithmName.SHA512,
                RSASignaturePadding.Pkcs1);

            return Content(Convert.ToBase64String(signature), "text/plain", Encoding.UTF8);
        }

        private string? ReadConfiguredValue(string valueKey, string pathKey)
        {
            string? directValue = _configuration[valueKey];

            if (!string.IsNullOrWhiteSpace(directValue))
            {
                return directValue.Replace("\\n", Environment.NewLine, StringComparison.Ordinal);
            }

            string? path = _configuration[pathKey];

            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return null;
            }

            return System.IO.File.ReadAllText(path, Encoding.UTF8);
        }
    }
}
