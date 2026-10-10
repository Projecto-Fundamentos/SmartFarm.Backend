using Microsoft.EntityFrameworkCore;
using SmartFarm.Cattle.Api.Application.Services;
using SmartFarm.Cattle.Api.Domain.Repositories;
using SmartFarm.Cattle.Api.Infrastructure.Persistence;
using SmartFarm.Cattle.Api.Infrastructure.Repositories;
using SmartFarm.Cattle.Api.Interface.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<CatalogExceptionHandler>();

var connectionString = builder.Configuration.GetConnectionString("CattleDatabase")
    ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:CattleDatabase.");

builder.Services.AddDbContext<CattleDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ICattleCatalogRepository, CattleCatalogRepository>();
builder.Services.AddScoped<CattleCatalogService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUi(options =>
    {
        options.Path = "/swagger";
        options.DocumentPath = "/openapi/v1.json";

        options.SwaggerRoutes.Clear();
        options.SwaggerRoutes.Add(
            new NSwag.AspNetCore.SwaggerUiRoute(
                "SmartFarm Cattle API",
                "/openapi/v1.json"
            )
        );
    });
}

app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", async Task<IResult> (CattleDbContext db) =>
{
    if (!await db.Database.CanConnectAsync())
        return Results.Json(new { status = "Unhealthy", database = "Disconnected" }, statusCode: 503);

    return Results.Ok(new { status = "Healthy", database = "Connected" });
});

app.Run();
