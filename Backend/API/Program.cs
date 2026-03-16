using Renova.Api.Infrastructure.DependencyInjection;
using Renova.Api.Infrastructure.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddRenovaApi(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.Services.InitializeRenovaApiAsync(builder.Environment.IsDevelopment());

app.UseRenovaApi();

app.Run();
