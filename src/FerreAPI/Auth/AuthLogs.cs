namespace FerreAPI.Auth;

internal static partial class AuthLogs
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Inicio de sesión del usuario {UsuarioId}")]
    public static partial void LoginExitoso(this ILogger logger, int usuarioId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning,
        Message = "Intento de inicio de sesión fallido para {Email}")]
    public static partial void LoginFallido(this ILogger logger, string email);
}