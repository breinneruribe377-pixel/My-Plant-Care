using System.ComponentModel.DataAnnotations;
using MyPlantCare.Models;

namespace MyPlantCare.Dtos;

public record CuidadoInputDto(
    [EnumDataType(typeof(TipoCuidado))] TipoCuidado Tipo,
    [Range(1, 3650)] int FrecuenciaDias);

public record CuidadoPendienteDto(
    int Id, int PlantaId, string PlantaNombre, TipoCuidado Tipo,
    int FrecuenciaDias, DateTime ProximaFecha);
