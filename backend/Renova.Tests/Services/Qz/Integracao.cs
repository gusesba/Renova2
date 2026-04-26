using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Qz
{
    public class Integracao
    {
        private static readonly JsonSerializerOptions CaseInsensitiveJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        [Fact]
        public async Task GetCertificateDeveRetornarCertificadoConfigurado()
        {
            using RSA rsa = RSA.Create(2048);
            string certificate = CriarCertificadoPem(rsa);
            await using RenovaApiFactory factory = new(new Dictionary<string, string?>
            {
                ["QzTray:Certificate"] = certificate
            });

            HttpClient client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "1");
            HttpResponseMessage response = await client.GetAsync("/api/qz/certificate");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(certificate, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DiagnosticsDeveIndicarConfiguracaoValidaQuandoCertificadoEChaveCombinam()
        {
            using RSA rsa = RSA.Create(2048);
            string certificate = CriarCertificadoPem(rsa);
            string privateKey = rsa.ExportRSAPrivateKeyPem();
            await using RenovaApiFactory factory = new(new Dictionary<string, string?>
            {
                ["QzTray:Certificate"] = certificate,
                ["QzTray:PrivateKey"] = privateKey
            });

            HttpClient client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "1");
            HttpResponseMessage response = await client.GetAsync("/api/qz/diagnostics");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            QzDiagnosticsResponse? diagnostics = JsonSerializer.Deserialize<QzDiagnosticsResponse>(
                await response.Content.ReadAsStringAsync(),
                CaseInsensitiveJsonOptions);

            Assert.NotNull(diagnostics);
            Assert.True(diagnostics.CertificateConfigured);
            Assert.True(diagnostics.PrivateKeyConfigured);
            Assert.True(diagnostics.CertificateValid);
            Assert.True(diagnostics.PrivateKeyValid);
            Assert.True(diagnostics.KeyMatchesCertificate);
        }

        [Fact]
        public async Task DiagnosticsDeveIndicarErroQuandoChaveNaoCombinaComCertificado()
        {
            using RSA certificateRsa = RSA.Create(2048);
            using RSA privateKeyRsa = RSA.Create(2048);
            string certificate = CriarCertificadoPem(certificateRsa);
            string privateKey = privateKeyRsa.ExportRSAPrivateKeyPem();
            await using RenovaApiFactory factory = new(new Dictionary<string, string?>
            {
                ["QzTray:Certificate"] = certificate,
                ["QzTray:PrivateKey"] = privateKey
            });

            HttpClient client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "1");
            HttpResponseMessage response = await client.GetAsync("/api/qz/diagnostics");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            QzDiagnosticsResponse? diagnostics = JsonSerializer.Deserialize<QzDiagnosticsResponse>(
                await response.Content.ReadAsStringAsync(),
                CaseInsensitiveJsonOptions);

            Assert.NotNull(diagnostics);
            Assert.True(diagnostics.CertificateValid);
            Assert.True(diagnostics.PrivateKeyValid);
            Assert.False(diagnostics.KeyMatchesCertificate);
            Assert.Contains(
                diagnostics.Messages,
                message => message.Contains("nao corresponde", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task SignDeveAssinarPayloadComChavePrivadaConfigurada()
        {
            using RSA rsa = RSA.Create(2048);
            string privateKey = rsa.ExportRSAPrivateKeyPem();
            const string payload = "sample-qz-payload";
            await using RenovaApiFactory factory = new(new Dictionary<string, string?>
            {
                ["QzTray:PrivateKey"] = privateKey
            });

            HttpClient client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "1");
            using StringContent content = new(payload, Encoding.UTF8, "text/plain");
            HttpResponseMessage response = await client.PostAsync("/api/qz/sign", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string signatureText = await response.Content.ReadAsStringAsync();
            byte[] signature = Convert.FromBase64String(signatureText);

            Assert.True(rsa.VerifyData(
                Encoding.UTF8.GetBytes(payload),
                signature,
                HashAlgorithmName.SHA512,
                RSASignaturePadding.Pkcs1));
        }

        private static string CriarCertificadoPem(RSA rsa)
        {
            CertificateRequest request = new(
                "CN=Renova QZ Tray Test",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            using X509Certificate2 certificate = request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(1));

            return certificate.ExportCertificatePem();
        }

        private sealed record QzDiagnosticsResponse(
            bool CertificateConfigured,
            bool PrivateKeyConfigured,
            bool CertificateValid,
            bool PrivateKeyValid,
            bool KeyMatchesCertificate,
            IReadOnlyList<string> Messages);
    }
}
