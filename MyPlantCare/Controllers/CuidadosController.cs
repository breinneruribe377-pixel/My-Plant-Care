using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPlantCare.Data;
using MyPlantCare.Dtos;
using MyPlantCare.Models;
using MyPlantCare.Services;

namespace MyPlantCare.Controllers;

[ApiController]
[Authorize]
public class CuidadosController : ControllerBase
{
    private readonly PlantCareDbContext _db;
    public CuidadosController(PlantCareDbContext db) => _db = db;

    private int UsuarioIdActual => User.ObtenerUsuarioId();

    // POST /plantas/{plantaId}/cuidados — RF-02, RF-03
    [HttpPost("plantas/{plantaId:int}/cuidados")]
    public async Task<ActionResult<CuidadoDto>> Crear(int plantaId, CuidadoInputDto input)
    {
        var planta = await _db.Plantas
            .FirstOrDefaultAsync(p => p.Id == plantaId && p.UsuarioId == UsuarioIdActual);

        if (planta is null)
            return NotFound(new { code = "RECURSO_NO_ENCONTRADO", message = "La planta no existe o no te pertenece." });

        var cuidado = new Cuidado
        {
            PlantaId = plantaId,
            Tipo = input.Tipo,
            FrecuenciaDias = input.FrecuenciaDias,
            ProximaFecha = CalculoFechas.CalcularProximaFecha(null, input.FrecuenciaDias, DateTime.UtcNow)
        };

        _db.Cuidados.Add(cuidado);
        await _db.SaveChangesAsync();

        return Ok(ADto(cuidado));
    }

    // PUT /cuidados/{cuidadoId}/completar — RF-04 (dispara el recálculo de RF-03)
    [HttpPut("cuidados/{cuidadoId:int}/completar")]
    public async Task<ActionResult<CuidadoDto>> MarcarComoRealizado(int cuidadoId)
    {
        var cuidado = await _db.Cuidados
            .Include(c => c.Planta)
            .FirstOrDefaultAsync(c => c.Id == cuidadoId && c.Planta!.UsuarioId == UsuarioIdActual);

        if (cuidado is null)
            return NotFound(new { code = "RECURSO_NO_ENCONTRADO", message = "El cuidado no existe o no te pertenece." });

        var ahora = DateTime.UtcNow;
        cuidado.UltimaVezRealizado = ahora;
        cuidado.ProximaFecha = CalculoFechas.CalcularProximaFecha(ahora, cuidado.FrecuenciaDias, ahora);

        await _db.SaveChangesAsync();
        return Ok(ADto(cuidado));
    }

    private static CuidadoDto ADto(Cuidado c) =>
        new(c.Id, c.PlantaId, c.Tipo, c.FrecuenciaDias, c.UltimaVezRealizado, c.ProximaFecha);
}
