using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Settings;

namespace Renova.Tests.Infrastructure
{
    public class RenovaApiFactory : WebApplicationFactory<Program>
    {
        private readonly IReadOnlyDictionary<string, string?> _configuration;
        private readonly string _databaseName = $"renova-api-tests-{Guid.NewGuid()}";
        private readonly JwtSettings _jwtSettings = JwtTokenAssert.CreateTestingSettings();

        public RenovaApiFactory(IReadOnlyDictionary<string, string?>? configuration = null)
        {
            _configuration = configuration ?? new Dictionary<string, string?>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _ = builder.UseEnvironment("Testing");
            _ = builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                _ = context;
                Dictionary<string, string?> values = new()
                {
                    ["TestDatabaseName"] = _databaseName,
                    [$"{JwtSettings.SectionName}:SecretKey"] = _jwtSettings.SecretKey,
                    [$"{JwtSettings.SectionName}:Issuer"] = _jwtSettings.Issuer,
                    [$"{JwtSettings.SectionName}:Audience"] = _jwtSettings.Audience,
                    [$"{JwtSettings.SectionName}:ExpirationMinutes"] = _jwtSettings.ExpirationMinutes.ToString()
                };

                foreach (KeyValuePair<string, string?> item in _configuration)
                {
                    values[item.Key] = item.Value;
                }

                _ = configurationBuilder.AddInMemoryCollection(values);
            });
            _ = builder.ConfigureTestServices(services =>
            {
                _ = services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            });
        }
    }
}
