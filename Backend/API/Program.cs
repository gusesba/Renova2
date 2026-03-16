using Renova.Api.Infrastructure.DependencyInjection;
using Renova.Api.Infrastructure.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRenovaApi(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseRenovaApi();

app.Run();
