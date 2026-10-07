using LasMelis.Api.Services;

namespace LasMelis.Tests;

public class TurnoHorasCalculatorTests
{
    // Un solo comportamiento ("cuántas horas dura el turno") con varios datos:
    // turnos normales, uno que cruza la medianoche, y los bordes (fin == inicio y media hora).
    [Theory]
    [InlineData("08:00", "16:00", 8)]     // turno normal
    [InlineData("14:00", "22:00", 8)]     // turno tarde
    [InlineData("22:00", "06:00", 8)]     // Noche: cruza la medianoche, no da -16
    [InlineData("06:00", "06:00", 24)]    // borde: fin == inicio se interpreta como 24 hs
    [InlineData("09:00", "09:30", 0.5)]   // borde: fracción de hora
    public void CalcularHoras_DevuelveLaDuracionDelTurno(string inicio, string fin, double esperado)
    {
        var horaInicio = TimeOnly.Parse(inicio);
        var horaFin = TimeOnly.Parse(fin);

        var horas = TurnoHorasCalculator.CalcularHoras(horaInicio, horaFin);

        Assert.Equal(esperado, horas);
    }

    [Fact]
    public void CalcularHoras_TurnoQueCruzaLaMedianoche_NuncaDaNegativo()
    {
        var horas = TurnoHorasCalculator.CalcularHoras(new TimeOnly(23, 0), new TimeOnly(1, 0));

        Assert.True(horas > 0);
        Assert.Equal(2, horas);
    }
}
