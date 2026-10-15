using System.ComponentModel.DataAnnotations;

namespace MyPlantCare.Dtos;

public record UsuarioRegistroInputDto(
    [Required, StringLength(100, MinimumLength = 2)] string Nombre,
    [Required, EmailAddress, StringLength(150)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password);

public record LoginInputDto(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record TokenResponseDto(string AccessToken, string TokenType, int ExpiresIn);
