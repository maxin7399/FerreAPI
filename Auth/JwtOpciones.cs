namespace FerreAPI.Auth;

public sealed class JwtOpciones
{
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public string Clave { get; init; } = "";
    public int MinutosExpiracion { get; init; } = 60;
}