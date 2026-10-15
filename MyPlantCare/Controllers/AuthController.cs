using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPlantCare.Data;
using MyPlantCare.Dtos;
using MyPlantCare.Models;
using MyPlantCare.Services;

namespace MyPlantCare.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly PlantCareDbContext _db;
    private readonly TokenService _tokens;
    private readonly PasswordHasher<Usuario> _hasher = new();

    public AuthController(PlantCareDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    // POST /auth/register
    [HttpPost("register")]
    public async Task<ActionResult<TokenResponseDto>> Registrar(UsuarioRegistroInputDto input)
    {
        var email = input.Email.Trim().ToLowerInvariant();

        if (await _db.Usuarios.AnyAsync(u => u.Email == email))
            return Conflict(new { code = "EMAIL_YA_REGISTRADO", message = "Ya existe un usuario con ese correo." });

        var usuario = new Usuario { Nombre = input.Nombre.Trim(), Email = email };
        usuario.PasswordHash = _hasher.HashPassword(usuario, input.Password);

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, _tokens.GenerarToken(usuario));
    }

    // POST /auth/login
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(LoginInputDto input)
    {
        var email = input.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        var valido = usuario is not null &&
            _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, input.Password) != PasswordVerificationResult.Failed;

        if (!valido)
            return Unauthorized(new { code = "CREDENCIALES_INVALIDAS", message = "Correo o contraseña incorrectos." });

        return Ok(_tokens.GenerarToken(usuario!));
    }
}
