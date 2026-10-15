namespace MyPlantCare.Models;

public enum TipoCuidado
{
    Riego,
    Fertilizacion,
    Poda,
    Rotacion,
    Trasplante,
    RevisionPlagas
}

/// <summary>
/// Traduce el enum de C# a los valores exactos del CHECK de la tabla `cuidado`
/// ('riego', 'fertilizacion', 'poda', 'rotacion', 'trasplante', 'revision_plagas').
/// </summary>
public static class TipoCuidadoDb
{
    public static string ToDb(TipoCuidado tipo) => tipo switch
    {
        TipoCuidado.Riego => "riego",
        TipoCuidado.Fertilizacion => "fertilizacion",
        TipoCuidado.Poda => "poda",
        TipoCuidado.Rotacion => "rotacion",
        TipoCuidado.Trasplante => "trasplante",
        TipoCuidado.RevisionPlagas => "revision_plagas",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de cuidado no soportado.")
    };

    public static TipoCuidado FromDb(string valor) => valor switch
    {
        "riego" => TipoCuidado.Riego,
        "fertilizacion" => TipoCuidado.Fertilizacion,
        "poda" => TipoCuidado.Poda,
        "rotacion" => TipoCuidado.Rotacion,
        "trasplante" => TipoCuidado.Trasplante,
        "revision_plagas" => TipoCuidado.RevisionPlagas,
        _ => throw new InvalidOperationException($"Valor de tipo de cuidado desconocido en la BD: '{valor}'.")
    };
}

public class Cuidado
{
    public int Id { get; set; }
    public int PlantaId { get; set; }
    public TipoCuidado Tipo { get; set; }
    public int FrecuenciaDias { get; set; }
    public DateTime? UltimaVezRealizado { get; set; }
    public DateTime ProximaFecha { get; set; }

    public Planta? Planta { get; set; }
}
