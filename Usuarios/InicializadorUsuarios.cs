using FerreAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FerreAPI.Usuarios;

public static class InicializadorUsuarios
{
    public static async Task CrearAdministradorInicialAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FerreDbContext>();

        if (await db.Usuarios.AnyAsync())
            return;

        var email = app.Configuration["AdminInicial:Email"];
        var password = app.Configuration["AdminInicial:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            app.Logger.LogWarning("No hay usuarios y no se configuró AdminInicial. No se creó el administrador.");
            return;
        }

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
        var admin = new Usuario(email, "Administrador", Rol.Administrador);
        admin.EstablecerPasswordHash(hasher.HashPassword(admin, password));

        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();

        app.Logger.LogInformation("Administrador inicial creado: {Email}", admin.Email);
    }
}