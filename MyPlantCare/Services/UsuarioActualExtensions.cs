using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyPlantCare.Services;

public static class UsuarioActualExtensions
{
    /// <summary>Id del usuario autenticado (claim "sub" del JWT).</summary>
    public static int ObtenerUsuarioId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
