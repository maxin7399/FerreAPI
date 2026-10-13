using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FerreAPI.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int ObtenerUsuarioId(this ClaimsPrincipal usuario) =>
        int.Parse(usuario.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("El token no contiene el claim 'sub'."));
}