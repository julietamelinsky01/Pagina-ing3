namespace LasMelis.Api.Models;

public class Empleado
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<AsignacionTurno> Asignaciones { get; set; } = new List<AsignacionTurno>();
}
