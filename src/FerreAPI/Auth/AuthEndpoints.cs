using FerreAPI.Data;
using FerreAPI.Usuarios;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FerreAPI.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/auth").WithTags("Autenticación");

        grupo.MapPost("/login", Login).AllowAnonymous();

        return app;
    }

    private static async Task<Results<Ok<LoginResponse>, ProblemHttpResult>> Login(
    LoginRequest req, FerreDbContext db, IPasswordHasher<Usuario> hasher,
    GeneradorTokens generador, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("FerreAPI.Auth");

        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return CredencialesInvalidas();

        var email = req.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario is null || !usuario.Activo)
        {
            logger.LoginFallido(email);
            return CredencialesInvalidas();
        }

        var resultado = hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, req.Password);
        if (resultado == PasswordVerificationResult.Failed)
        {
            logger.LoginFallido(email);
            return CredencialesInvalidas();
        }

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.EstablecerPasswordHash(hasher.HashPassword(usuario, req.Password));
            await db.SaveChangesAsync();
        }

        logger.LoginExitoso(usuario.Id);

        var (token, expiraEn) = generador.Generar(usuario);
        return TypedResults.Ok(new LoginResponse(token, expiraEn, usuario.Nombre, usuario.Rol.ToString()));
    }

    private static ProblemHttpResult CredencialesInvalidas() =>
        TypedResults.Problem(
            title: "Credenciales inválidas",
            detail: "El email o la contraseña son incorrectos.",
            statusCode: StatusCodes.Status401Unauthorized);
}