namespace LasMelis.Api.Services;

public static class TurnoHorasCalculator
{
    // Turnos como "Noche" (22:00–06:00) cruzan la medianoche: si la hora de fin
    // es menor o igual a la de inicio, se interpreta que termina al día siguiente.
    public static double CalcularHoras(TimeOnly horaInicio, TimeOnly horaFin)
    {
        var inicio = horaInicio.ToTimeSpan();
        var fin = horaFin.ToTimeSpan();

        var duracion = fin <= inicio
            ? fin.Add(TimeSpan.FromHours(24)) - inicio
            : fin - inicio;

        return Math.Round(duracion.TotalHours, 2);
    }
}
