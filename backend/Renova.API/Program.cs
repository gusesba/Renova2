using Microsoft.EntityFrameworkCore;
using Renova.Persistence;
using Renova.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddDbContext<RenovaDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        var databaseName = builder.Configuration["TestDatabaseName"] ?? "renova-api-tests";
        options.UseInMemoryDatabase(databaseName);
        return;
    }

    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IRenovaService, RenovaService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Renova.API v1");
    c.RoutePrefix = string.Empty;
});
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program
{
}
