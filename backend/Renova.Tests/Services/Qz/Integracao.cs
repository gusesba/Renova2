using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Qz
{
    public class Integracao
    {
        [Fact]
        public async Task GetCertificateDeveRetornarCertificadoConfigurado()
        {
            const string certificate = "-----BEGIN CERTIFICATE-----\nteste\n-----END CERTIFICATE-----";
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
    }
}
