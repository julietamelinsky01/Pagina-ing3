namespace LasMelis.Api.Models;

public class AsignacionTurno
{
    public int Id { get; set; }
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public int TipoTurnoId { get; set; }
    public TipoTurno TipoTurno { get; set; } = null!;
    public DateOnly Fecha { get; set; }
    public string? Observaciones { get; set; }
}
