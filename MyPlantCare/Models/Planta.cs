namespace MyPlantCare.Models;

public class Planta
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Especie { get; set; }
    public string? Ubicacion { get; set; }
    public string? FotoUrl { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
    public ICollection<Cuidado> Cuidados { get; set; } = new List<Cuidado>();
}
