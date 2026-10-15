using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPlantCare.Data;
using MyPlantCare.Dtos;
using MyPlantCare.Models;
using MyPlantCare.Services;

namespace MyPlantCare.Controllers;

[ApiController]
[Route("plantas")]
[Authorize]
public class PlantasController : ControllerBase
{
    private readonly PlantCareDbContext _db;
    public PlantasController(PlantCareDbContext db) => _db = db;

    private int UsuarioIdActual => User.ObtenerUsuarioId();

    // GET /plantas — RF-01 (listar)
    [HttpGet]
    public async Task<ActionResult<List<PlantaDto>>> ObtenerTodas()
    {
        var plantas = await _db.Plantas
            .AsNoTracking()
            .Include(p => p.Cuidados)
            .Where(p => p.UsuarioId == UsuarioIdActual)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return Ok(plantas.Select(MapearADto).ToList());
    }

    // GET /plantas/{plantaId} — RF-01 (detalle)
    [HttpGet("{plantaId:int}")]
    public async Task<ActionResult<PlantaDto>> ObtenerPorId(int plantaId)
    {
        var planta = await _db.Plantas
            .AsNoTracking()
            .Include(p => p.Cuidados)
            .FirstOrDefaultAsync(p => p.Id == plantaId && p.UsuarioId == UsuarioIdActual);

        if (planta is null) return NotFound(NoEncontrada());
        return Ok(MapearADto(planta));
    }

    // POST /plantas — RF-01 (crear)
    [HttpPost]
    public async Task<ActionResult<PlantaDto>> Crear(PlantaInputDto input)
    {
        var planta = new Planta
        {
            UsuarioId = UsuarioIdActual,
            Nombre = input.Nombre.Trim(),
            Especie = input.Especie,
            Ubicacion = input.Ubicacion,
            FotoUrl = input.FotoUrl
        };

        _db.Plantas.Add(planta);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { plantaId = planta.Id }, MapearADto(planta));
    }

    // PUT /plantas/{plantaId} — RF-01 (editar)
    [HttpPut("{plantaId:int}")]
    public async Task<ActionResult<PlantaDto>> Editar(int plantaId, PlantaInputDto input)
    {
        var planta = await _db.Plantas
            .Include(p => p.Cuidados)
            .FirstOrDefaultAsync(p => p.Id == plantaId && p.UsuarioId == UsuarioIdActual);

        if (planta is null) return NotFound(NoEncontrada());

        planta.Nombre = input.Nombre.Trim();
        planta.Especie = input.Especie;
        planta.Ubicacion = input.Ubicacion;
        planta.FotoUrl = input.FotoUrl;

        await _db.SaveChangesAsync();
        return Ok(MapearADto(planta));
    }

    // DELETE /plantas/{plantaId} — RF-01 (eliminar)
    [HttpDelete("{plantaId:int}")]
    public async Task<IActionResult> Eliminar(int plantaId)
    {
        var planta = await _db.Plantas
            .FirstOrDefaultAsync(p => p.Id == plantaId && p.UsuarioId == UsuarioIdActual);

        if (planta is null) return NotFound(NoEncontrada());

        _db.Plantas.Remove(planta); // Los cuidados se borran por ON DELETE CASCADE
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static object NoEncontrada() =>
        new { code = "RECURSO_NO_ENCONTRADO", message = "La planta solicitada no existe o no te pertenece." };

    private static PlantaDto MapearADto(Planta p) => new(
        p.Id, p.UsuarioId, p.Nombre, p.Especie, p.Ubicacion, p.FotoUrl, p.FechaCreacion,
        p.Cuidados
            .OrderBy(c => c.ProximaFecha)
            .Select(c => new CuidadoDto(c.Id, c.PlantaId, c.Tipo, c.FrecuenciaDias, c.UltimaVezRealizado, c.ProximaFecha))
            .ToList());
}
