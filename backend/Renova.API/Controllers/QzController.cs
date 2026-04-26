using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetCertificate()
        {
            ConfiguredQzValue certificateConfig = ReadConfiguredValue("QzTray:Certificate", "QzTray:CertificatePath");
            string? certificate = certificateConfig.Value;

            if (string.IsNullOrWhiteSpace(certificate))
            {
                return NotFound(certificateConfig.ErrorMessage ?? "Certificado QZ Tray nao configurado.");
            }

            try
            {
                _ = X509Certificate2.CreateFromPem(certificate);
            }
            catch (Exception exception) when (exception is CryptographicException or ArgumentException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Certificado QZ Tray invalido ou corrompido: {exception.Message}");
            }

            return Content(certificate, "text/plain", Encoding.UTF8);
        }

        [HttpGet("diagnostics")]
        [ProducesResponseType(typeof(QzDiagnosticsResponse), StatusCodes.Status200OK)]
        public IActionResult GetDiagnostics()
        {
            ConfiguredQzValue certificateConfig = ReadConfiguredValue("QzTray:Certificate", "QzTray:CertificatePath");
            ConfiguredQzValue privateKeyConfig = ReadConfiguredValue("QzTray:PrivateKey", "QzTray:PrivateKeyPath");
            List<string> messages = [];

            X509Certificate2? certificate = null;
            RSA? privateKey = null;

            bool certificateConfigured = !string.IsNullOrWhiteSpace(certificateConfig.Value);
            bool privateKeyConfigured = !string.IsNullOrWhiteSpace(privateKeyConfig.Value);
            bool certificateValid = false;
            bool privateKeyValid = false;
            bool keyMatchesCertificate = false;

            if (!certificateConfigured)
            {
                messages.Add(certificateConfig.ErrorMessage ?? "Certificado QZ Tray nao configurado.");
            }
            else
            {
                try
                {
                    certificate = X509Certificate2.CreateFromPem(certificateConfig.Value!);
                    certificateValid = true;
                }
                catch (Exception exception) when (exception is CryptographicException or ArgumentException)
                {
                    messages.Add($"Certificado QZ Tray invalido ou corrompido: {exception.Message}");
                }
            }

            if (!privateKeyConfigured)
            {
                messages.Add(privateKeyConfig.ErrorMessage ?? "Chave privada QZ Tray nao configurada.");
            }
            else
            {
                try
                {
                    privateKey = RSA.Create();
                    privateKey.ImportFromPem(privateKeyConfig.Value);
                    privateKeyValid = true;
                }
                catch (Exception exception) when (exception is CryptographicException or ArgumentException)
                {
                    messages.Add($"Chave privada QZ Tray invalida ou corrompida: {exception.Message}");
                }
            }

            if (certificate is not null && privateKey is not null)
            {
                using RSA? publicKey = certificate.GetRSAPublicKey();

                if (publicKey is null)
                {
                    messages.Add("Certificado QZ Tray nao contem uma chave publica RSA.");
                }
                else
                {
                    byte[] payload = Encoding.UTF8.GetBytes("renova-qz-diagnostics");
                    byte[] signature = privateKey.SignData(payload, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                    keyMatchesCertificate = publicKey.VerifyData(
                        payload,
                        signature,
                        HashAlgorithmName.SHA512,
                        RSASignaturePadding.Pkcs1);

                    if (!keyMatchesCertificate)
                    {
                        messages.Add("A chave privada QZ Tray nao corresponde ao certificado publico configurado.");
                    }
                }
            }

            privateKey?.Dispose();
            certificate?.Dispose();

            if (messages.Count == 0)
            {
                messages.Add("Certificado e chave privada QZ Tray configurados corretamente.");
            }

            return Ok(new QzDiagnosticsResponse(
                certificateConfigured,
                privateKeyConfigured,
                certificateValid,
                privateKeyValid,
                keyMatchesCertificate,
                certificateConfig.Source,
                certificateConfig.Path,
                certificateConfig.PathExists,
                privateKeyConfig.Source,
                privateKeyConfig.Path,
                privateKeyConfig.PathExists,
                messages));
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

            ConfiguredQzValue privateKeyConfig = ReadConfiguredValue("QzTray:PrivateKey", "QzTray:PrivateKeyPath");
            string? privateKey = privateKeyConfig.Value;

            if (string.IsNullOrWhiteSpace(privateKey))
            {
                return NotFound(privateKeyConfig.ErrorMessage ?? "Chave privada QZ Tray nao configurada.");
            }

            using RSA rsa = RSA.Create();

            try
            {
                rsa.ImportFromPem(privateKey);
            }
            catch (Exception exception) when (exception is CryptographicException or ArgumentException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Chave privada QZ Tray invalida ou corrompida: {exception.Message}");
            }

            byte[] signature = rsa.SignData(
                Encoding.UTF8.GetBytes(payload),
                HashAlgorithmName.SHA512,
                RSASignaturePadding.Pkcs1);

            return Content(Convert.ToBase64String(signature), "text/plain", Encoding.UTF8);
        }

        private ConfiguredQzValue ReadConfiguredValue(string valueKey, string pathKey)
        {
            string? directValue = _configuration[valueKey];

            if (!string.IsNullOrWhiteSpace(directValue))
            {
                return new ConfiguredQzValue(
                    directValue.Replace("\\n", Environment.NewLine, StringComparison.Ordinal),
                    "environment",
                    null,
                    null,
                    null);
            }

            string? path = _configuration[pathKey];

            if (string.IsNullOrWhiteSpace(path))
            {
                return new ConfiguredQzValue(null, "missing", null, null, "Configuracao de caminho nao informada.");
            }

            if (!System.IO.File.Exists(path))
            {
                return new ConfiguredQzValue(null, "path", path, false, $"Arquivo nao encontrado em {path}.");
            }

            return new ConfiguredQzValue(System.IO.File.ReadAllText(path, Encoding.UTF8), "path", path, true, null);
        }

        private sealed record ConfiguredQzValue(
            string? Value,
            string Source,
            string? Path,
            bool? PathExists,
            string? ErrorMessage);

        public sealed record QzDiagnosticsResponse(
            bool CertificateConfigured,
            bool PrivateKeyConfigured,
            bool CertificateValid,
            bool PrivateKeyValid,
            bool KeyMatchesCertificate,
            string CertificateSource,
            string? CertificatePath,
            bool? CertificatePathExists,
            string PrivateKeySource,
            string? PrivateKeyPath,
            bool? PrivateKeyPathExists,
            IReadOnlyList<string> Messages);
    }
}
