using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyPlantCare.Dtos;
using MyPlantCare.Models;

namespace MyPlantCare.Services;

public class TokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) => _config = config;

    public TokenResponseDto GenerarToken(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var minutos = _config.GetValue("Jwt:ExpiresMinutes", 60);
        var expiracion = DateTime.UtcNow.AddMinutes(minutos);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: credenciales);

        return new TokenResponseDto(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", minutos * 60);
    }
}
