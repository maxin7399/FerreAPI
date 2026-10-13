using System.Security.Claims;
using System.Text;
using FerreAPI.Usuarios;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FerreAPI.Auth;

public sealed class GeneradorTokens(IOptions<JwtOpciones> opciones)
{
    private readonly JwtOpciones _opciones = opciones.Value;

    public (string Token, DateTime ExpiraEn) Generar(Usuario usuario)
    {
        var expiraEn = DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new("name", usuario.Nombre),
            new("role", usuario.Rol.ToString())
        };

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiraEn,
            Issuer = _opciones.Issuer,
            Audience = _opciones.Audience,
            SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expiraEn);
    }
}