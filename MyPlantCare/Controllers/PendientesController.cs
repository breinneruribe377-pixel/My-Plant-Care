using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPlantCare.Data;
using MyPlantCare.Dtos;
using MyPlantCare.Services;

namespace MyPlantCare.Controllers;

[ApiController]
[Route("pendientes")]
[Authorize]
public class PendientesController : ControllerBase
{
    private readonly PlantCareDbContext _db;
    public PendientesController(PlantCareDbContext db) => _db = db;

    // GET /pendientes — RF-05: cuidados del usuario cuya próxima fecha ya llegó o pasó
    [HttpGet]
    public async Task<ActionResult<List<CuidadoPendienteDto>>> ObtenerPendientes()
    {
        var usuarioId = User.ObtenerUsuarioId();
        var ahora = DateTime.UtcNow;

        var pendientes = await _db.Cuidados
            .AsNoTracking()
            .Where(c => c.Planta!.UsuarioId == usuarioId && c.ProximaFecha <= ahora)
            .OrderBy(c => c.ProximaFecha)
            .Select(c => new CuidadoPendienteDto(
                c.Id, c.PlantaId, c.Planta!.Nombre, c.Tipo, c.FrecuenciaDias, c.ProximaFecha))
            .ToListAsync();

        return Ok(pendientes);
    }
}
