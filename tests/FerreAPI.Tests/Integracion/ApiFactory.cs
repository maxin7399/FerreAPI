using FerreAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FerreAPI.Tests.Integracion;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Admin123!Test";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
    .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var opciones = new DbContextOptionsBuilder<FerreDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using var db = new FerreDbContext(opciones);
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:FerreApi", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Clave", "clave-exclusiva-de-pruebas-de-integracion-0123456789abcdef");
        builder.UseSetting("AdminInicial:Email", AdminEmail);
        builder.UseSetting("AdminInicial:Password", AdminPassword);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}