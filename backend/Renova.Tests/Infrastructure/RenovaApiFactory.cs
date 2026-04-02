using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using Renova.Domain.Settings;

namespace Renova.Tests.Infrastructure
{
    public class RenovaApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"renova-api-tests-{Guid.NewGuid()}";
        private readonly JwtSettings _jwtSettings = JwtTokenAssert.CreateTestingSettings();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _ = builder.UseEnvironment("Testing");
            _ = builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                _ = context;
                _ = configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TestDatabaseName"] = _databaseName,
                    [$"{JwtSettings.SectionName}:SecretKey"] = _jwtSettings.SecretKey,
                    [$"{JwtSettings.SectionName}:Issuer"] = _jwtSettings.Issuer,
                    [$"{JwtSettings.SectionName}:Audience"] = _jwtSettings.Audience,
                    [$"{JwtSettings.SectionName}:ExpirationMinutes"] = _jwtSettings.ExpirationMinutes.ToString()
                });
            });
        }
    }
}