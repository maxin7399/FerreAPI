using System.Security.Claims;
using FerreAPI.Auth;
using FerreAPI.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FerreAPI.Usuarios;

public static class UsuarioEndpoints
{
    public static IEndpointRouteBuilder MapUsuarios(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/usuarios")
            .WithTags("Usuarios")
            .RequireAuthorization(Politicas.SoloAdministrador);

        grupo.MapGet("/", ObtenerTodos);
        grupo.MapPost("/", CrearUsuario);
        grupo.MapDelete("/{id:int}", DesactivarUsuario);

        return app;
    }

    private static async Task<Ok<List<UsuarioResponse>>> ObtenerTodos(FerreDbContext db)
    {
        var usuarios = await db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        return TypedResults.Ok(usuarios.Select(u => u.ToResponse()).ToList());
    }

    private static async Task<Results<Created<UsuarioResponse>, ValidationProblem, ProblemHttpResult>> CrearUsuario(
        CrearUsuarioRequest req, FerreDbContext db, IPasswordHasher<Usuario> hasher)
    {
        var errores = req.Validar();
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);

        var usuario = new Usuario(req.Email, req.Nombre, Enum.Parse<Rol>(req.Rol, ignoreCase: true));

        if (await db.Usuarios.AnyAsync(u => u.Email == usuario.Email))
            return EmailDuplicado(usuario.Email);

        usuario.EstablecerPasswordHash(hasher.HashPassword(usuario, req.Password));
        db.Usuarios.Add(usuario);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return EmailDuplicado(usuario.Email);
        }

        return TypedResults.Created($"/api/usuarios/{usuario.Id}", usuario.ToResponse());
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DesactivarUsuario(
        int id, ClaimsPrincipal usuarioActual, FerreDbContext db)
    {
        if (id == usuarioActual.ObtenerUsuarioId())
            return TypedResults.Problem(
                title: "Operación no permitida",
                detail: "No puedes desactivar tu propio usuario.",
                statusCode: StatusCodes.Status400BadRequest);

        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario is null)
            return TypedResults.NotFound();

        usuario.Desactivar();
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static ProblemHttpResult EmailDuplicado(string email) =>
        TypedResults.Problem(
            title: "Email duplicado",
            detail: $"Ya existe un usuario con el email '{email}'.",
            statusCode: StatusCodes.Status409Conflict);
}