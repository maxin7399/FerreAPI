namespace FerreAPI.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, DateTime ExpiraEn, string Nombre, string Rol);