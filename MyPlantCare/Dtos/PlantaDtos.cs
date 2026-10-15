using System.ComponentModel.DataAnnotations;
using MyPlantCare.Models;

namespace MyPlantCare.Dtos;

public record PlantaInputDto(
    [Required, StringLength(100, MinimumLength = 1)] string Nombre,
    [StringLength(100)] string? Especie,
    [StringLength(100)] string? Ubicacion,
    [StringLength(255)] string? FotoUrl);

public record CuidadoDto(
    int Id, int PlantaId, TipoCuidado Tipo, int FrecuenciaDias,
    DateTime? UltimaVezRealizado, DateTime ProximaFecha);

public record PlantaDto(
    int Id, int UsuarioId, string Nombre, string? Especie, string? Ubicacion,
    string? FotoUrl, DateTime FechaCreacion, List<CuidadoDto> Cuidados);
