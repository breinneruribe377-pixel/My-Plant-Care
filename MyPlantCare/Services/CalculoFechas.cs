namespace MyPlantCare.Services;

/// <summary>
/// RF-03: calcula la próxima fecha de un cuidado a partir de la última vez
/// realizado (o, si nunca se ha hecho, de una fecha base) más la frecuencia en días.
/// </summary>
public static class CalculoFechas
{
    public static DateTime CalcularProximaFecha(DateTime? ultimaVezRealizado, int frecuenciaDias, DateTime fechaBaseSiNunca)
    {
        var fechaBase = ultimaVezRealizado ?? fechaBaseSiNunca;
        return fechaBase.AddDays(frecuenciaDias);
    }
}
